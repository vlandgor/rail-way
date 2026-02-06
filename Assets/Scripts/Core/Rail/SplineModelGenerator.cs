using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Splines;

namespace Core.Rail
{
    /// <summary>
    /// Generates and deforms models along a Unity Spline as a single combined mesh per spline
    /// </summary>
    public class SplineModelGenerator : MonoBehaviour
    {
        [Header("Setup")]
        [Tooltip("The SplineContainer to follow")]
        public SplineContainer splineContainer;
    
        [Tooltip("The model to repeat along the spline")]
        public GameObject model;
    
        [Header("Settings")]
        [Tooltip("Distance between each model instance")]
        public float spacing = 0.1f;
    
        [Tooltip("Offset from the spline path")]
        public Vector3 offset = Vector3.zero;
    
        [Tooltip("Additional rotation applied to each model")]
        public Vector3 rotationOffset = Vector3.zero;
    
        [Tooltip("Scale multiplier for generated models")]
        public Vector3 scale = Vector3.one;
    
        [Header("Deformation")]
        [Tooltip("Enable mesh deformation to bend along curves")]
        public bool enableDeformation = true;
    
        [Tooltip("Axis along which the model extends (usually Z for forward)")]
        public Vector3 modelLengthAxis = Vector3.forward;
    
        private const string CONTAINER_NAME = "Generated Models";
        private const string MESH_FOLDER = "Assets/GeneratedMeshes";
    
        [ContextMenu("Rebake")]
        public void Rebake()
        {
            Clear();
            Generate();
        }
    
        private void Generate()
        {
            if (!Validate())
            {
                return;
            }
        
            // Always clear before generating
            Clear();
        
            // Find or create container (always use the same one)
            Transform container = transform.Find(CONTAINER_NAME);
            if (container == null)
            {
                GameObject containerObj = new GameObject(CONTAINER_NAME);
                containerObj.transform.SetParent(transform);
                containerObj.transform.localPosition = Vector3.zero;
                containerObj.transform.localRotation = Quaternion.identity;
                container = containerObj.transform;
            }
        
#if UNITY_EDITOR
            // Create mesh folder if it doesn't exist (only in editor)
            if (!Application.isPlaying && enableDeformation)
            {
                if (!AssetDatabase.IsValidFolder(MESH_FOLDER))
                {
                    string parentFolder = "Assets";
                    string newFolder = "GeneratedMeshes";
                    AssetDatabase.CreateFolder(parentFolder, newFolder);
                }
            }
#endif
        
            // Get the base mesh and material from the model
            MeshFilter modelMeshFilter = model.GetComponent<MeshFilter>();
            MeshRenderer modelRenderer = model.GetComponent<MeshRenderer>();
            
            Mesh baseMesh = null;
            
            // Try to get mesh from MeshFilter first
            if (modelMeshFilter != null && modelMeshFilter.sharedMesh != null)
            {
                baseMesh = modelMeshFilter.sharedMesh;
            }
            // If no MeshFilter, try ProBuilder
            else
            {
                // ProBuilder objects store mesh in a different way
                var proBuilderMesh = model.GetComponent<ProBuilderMesh>();
                if (proBuilderMesh != null)
                {
#if UNITY_EDITOR
                    // Refresh the ProBuilder mesh to make sure it's built
                    proBuilderMesh.ToMesh();
                    proBuilderMesh.Refresh();
#endif
                    
                    // Get the mesh from ProBuilder - it should have a MeshFilter too
                    modelMeshFilter = proBuilderMesh.GetComponent<MeshFilter>();
                    modelRenderer = proBuilderMesh.GetComponent<MeshRenderer>();
                    if (modelMeshFilter != null && modelMeshFilter.sharedMesh != null)
                    {
                        baseMesh = modelMeshFilter.sharedMesh;
                    }
                }
            }
            
            if (baseMesh == null)
            {
                Debug.LogError("Model must have a mesh! If using ProBuilder, try clicking 'Reset Shape' or 'Rebuild' on the ProBuilder component.");
                return;
            }
        
            // Iterate through all splines in the container
            for (int splineIndex = 0; splineIndex < splineContainer.Splines.Count; splineIndex++)
            {
                Spline spline = splineContainer.Splines[splineIndex];
                float splineLength = spline.GetLength();
                int count = Mathf.Max(1, Mathf.CeilToInt(splineLength / spacing));
                
                // Create combined mesh for this spline
                Mesh combinedMesh = CombineMeshesAlongSpline(baseMesh, splineIndex, count);
                
                // Create GameObject for this spline's mesh
                GameObject splineObject = new GameObject($"Spline_{splineIndex}_Combined");
                splineObject.transform.SetParent(container);
                splineObject.transform.localPosition = Vector3.zero;
                splineObject.transform.localRotation = Quaternion.identity;
                splineObject.transform.localScale = Vector3.one;
                
                // Add MeshFilter and MeshRenderer
                MeshFilter meshFilter = splineObject.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = splineObject.AddComponent<MeshRenderer>();
                
                // Copy materials from original model
                if (modelRenderer != null)
                {
                    meshRenderer.sharedMaterials = modelRenderer.sharedMaterials;
                }
                
#if UNITY_EDITOR
                // Save as asset in edit mode
                if (!Application.isPlaying)
                {
                    string assetPath = $"{MESH_FOLDER}/Spline_{splineIndex}_Combined.asset";
                    
                    // Delete old asset if it exists
                    if (System.IO.File.Exists(assetPath))
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                    }
                    
                    // Create new asset
                    AssetDatabase.CreateAsset(combinedMesh, assetPath);
                    AssetDatabase.SaveAssets();
                    
                    // Load and assign
                    Mesh savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                    meshFilter.sharedMesh = savedMesh;
                }
                else
                {
                    meshFilter.mesh = combinedMesh;
                }
#else
                meshFilter.mesh = combinedMesh;
#endif
                
                Debug.Log($"Generated combined mesh for spline {splineIndex} with {count} segments and {baseMesh.subMeshCount} materials");
            }
        }
    
        private Mesh CombineMeshesAlongSpline(Mesh baseMesh, int splineIndex, int segmentCount)
        {
            Spline spline = splineContainer.Splines[splineIndex];
            float splineLength = spline.GetLength();
            
            // Support multiple submeshes (for multiple materials)
            int submeshCount = baseMesh.subMeshCount;
            
            List<Vector3> combinedVertices = new List<Vector3>();
            List<List<int>> combinedTrianglesPerSubmesh = new List<List<int>>();
            List<Vector3> combinedNormals = new List<Vector3>();
            List<Vector2> combinedUVs = new List<Vector2>();
            List<Vector4> combinedTangents = new List<Vector4>();
            
            // Initialize triangle lists for each submesh
            for (int i = 0; i < submeshCount; i++)
            {
                combinedTrianglesPerSubmesh.Add(new List<int>());
            }
            
            int vertexOffset = 0;
            
            // Get model bounds for deformation
            Bounds bounds = baseMesh.bounds;
            float modelLength = Vector3.Dot(bounds.size, modelLengthAxis.normalized);
            Vector3 modelCenter = bounds.center;
            
            for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                float distance = segmentIndex * spacing;
                float t = distance / splineLength;
                float nextT = Mathf.Min((distance + spacing) / splineLength, 1f);
                
                // Get spline position and rotation
                Vector3 position = splineContainer.EvaluatePosition(splineIndex, t);
                Vector3 forward = splineContainer.EvaluateTangent(splineIndex, t);
                Vector3 up = splineContainer.EvaluateUpVector(splineIndex, t);
                Quaternion rotation = Quaternion.LookRotation(forward, up);
                
                // Apply offsets
                position += rotation * offset;
                rotation *= Quaternion.Euler(rotationOffset);
                
                // Process each vertex of the base mesh
                Vector3[] baseVertices = baseMesh.vertices;
                Vector3[] baseNormals = baseMesh.normals;
                Vector2[] baseUVs = baseMesh.uv;
                Vector4[] baseTangents = baseMesh.tangents;
                
                for (int i = 0; i < baseVertices.Length; i++)
                {
                    Vector3 vertex = baseVertices[i];
                    
                    if (enableDeformation)
                    {
                        // Calculate vertex progress along model
                        float vertexProgress = Vector3.Dot(vertex - modelCenter, modelLengthAxis.normalized) / modelLength + 0.5f;
                        vertexProgress = Mathf.Clamp01(vertexProgress);
                        
                        // Interpolate spline position
                        float vertexT = Mathf.Lerp(t, nextT, vertexProgress);
                        
                        Vector3 splinePos = splineContainer.EvaluatePosition(splineIndex, vertexT);
                        Vector3 splineTangent = splineContainer.EvaluateTangent(splineIndex, vertexT);
                        Vector3 splineUp = splineContainer.EvaluateUpVector(splineIndex, vertexT);
                        Quaternion splineRot = Quaternion.LookRotation(splineTangent, splineUp);
                        splineRot *= Quaternion.Euler(rotationOffset);
                        
                        // Calculate offset from center line
                        Vector3 offsetFromCenter = vertex - (modelCenter + modelLengthAxis.normalized * (vertexProgress - 0.5f) * modelLength);
                        Vector3 worldOffset = splineRot * offsetFromCenter;
                        worldOffset += splineRot * offset;
                        
                        // Final vertex position
                        combinedVertices.Add(splinePos + worldOffset);
                        
                        // Deformed normal
                        if (i < baseNormals.Length)
                        {
                            Vector3 worldNormal = splineRot * baseNormals[i];
                            combinedNormals.Add(worldNormal);
                        }
                        
                        // Deformed tangent
                        if (i < baseTangents.Length)
                        {
                            Vector3 tangentDir = splineRot * (Vector3)baseTangents[i];
                            combinedTangents.Add(new Vector4(tangentDir.x, tangentDir.y, tangentDir.z, baseTangents[i].w));
                        }
                    }
                    else
                    {
                        // No deformation, just transform
                        Vector3 scaledVertex = Vector3.Scale(vertex, scale);
                        combinedVertices.Add(position + rotation * scaledVertex);
                        
                        if (i < baseNormals.Length)
                        {
                            combinedNormals.Add(rotation * baseNormals[i]);
                        }
                        
                        if (i < baseTangents.Length)
                        {
                            Vector3 tangentDir = rotation * (Vector3)baseTangents[i];
                            combinedTangents.Add(new Vector4(tangentDir.x, tangentDir.y, tangentDir.z, baseTangents[i].w));
                        }
                    }
                    
                    // UVs stay the same
                    if (i < baseUVs.Length)
                    {
                        combinedUVs.Add(baseUVs[i]);
                    }
                }
                
                // Add triangles with offset for each submesh
                for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
                {
                    int[] baseTriangles = baseMesh.GetTriangles(submeshIndex);
                    for (int i = 0; i < baseTriangles.Length; i++)
                    {
                        combinedTrianglesPerSubmesh[submeshIndex].Add(baseTriangles[i] + vertexOffset);
                    }
                }
                
                vertexOffset += baseVertices.Length;
            }
            
            // Create the combined mesh
            Mesh combined = new Mesh();
            combined.name = $"CombinedSpline_{splineIndex}";
            
            // Check if we need 32-bit indices
            if (combinedVertices.Count > 65535)
            {
                combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            
            combined.SetVertices(combinedVertices);
            combined.SetNormals(combinedNormals);
            combined.SetUVs(0, combinedUVs);
            combined.SetTangents(combinedTangents);
            
            // Set submeshes
            combined.subMeshCount = submeshCount;
            for (int i = 0; i < submeshCount; i++)
            {
                combined.SetTriangles(combinedTrianglesPerSubmesh[i], i);
            }
            
            combined.RecalculateBounds();
            
            return combined;
        }
    
        private void Clear()
        {
            // Find the container
            Transform container = transform.Find(CONTAINER_NAME);
            if (container == null)
            {
                return;
            }
        
            // Destroy all children in the container
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (Transform child in container)
            {
                toDestroy.Add(child.gameObject);
            }
        
            foreach (GameObject obj in toDestroy)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(obj);
                }
                else
                {
                    Destroy(obj);
                }
#else
                Destroy(obj);
#endif
            }
        
#if UNITY_EDITOR
            // Clean up old mesh assets
            if (!Application.isPlaying && AssetDatabase.IsValidFolder(MESH_FOLDER))
            {
                string[] meshAssets = AssetDatabase.FindAssets("t:Mesh", new[] { MESH_FOLDER });
                foreach (string guid in meshAssets)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    AssetDatabase.DeleteAsset(path);
                }
                AssetDatabase.SaveAssets();
            }
#endif
        }
    
        private bool Validate()
        {
            if (splineContainer == null)
            {
                Debug.LogError("SplineContainer is not assigned!");
                return false;
            }
        
            if (model == null)
            {
                Debug.LogError("Model is not assigned!");
                return false;
            }
        
            if (splineContainer.Splines == null || splineContainer.Splines.Count == 0)
            {
                Debug.LogError("SplineContainer has no splines!");
                return false;
            }
        
            return true;
        }
    }
}