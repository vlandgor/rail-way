using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace Core.Rail
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SplineMeshBender : MonoBehaviour
    {
        public SplineContainer splineContainer;
        public Mesh sourceMesh;
        public float meshLength = 1f;

        void Update()
        {
            if (splineContainer == null || sourceMesh == null) return;
            DeformMesh();
        }

        void DeformMesh()
        {
            float splineLength = splineContainer.CalculateLength();
            int repetitions = Mathf.Max(1, Mathf.FloorToInt(splineLength / meshLength));
            float actualStep = splineLength / repetitions;

            Mesh deformedMesh = new Mesh();
            List<Vector3> newVerts = new List<Vector3>();
            List<Vector2> newUvs = new List<Vector2>();
            List<int> newTris = new List<int>();
        
            Vector3[] sourceVerts = sourceMesh.vertices;
            Vector2[] sourceUvs = sourceMesh.uv;
            int[] sourceTris = sourceMesh.triangles;

            for (int i = 0; i < repetitions; i++)
            {
                float startDist = i * actualStep;

                for (int v = 0; v < sourceVerts.Length; v++)
                {
                    Vector3 vPos = sourceVerts[v];
                    float distanceOnSpline = startDist + (vPos.z * (actualStep / meshLength));
                    float t = distanceOnSpline / splineLength;

                    splineContainer.Evaluate(t, out var pos, out var tangent, out var up);
                    Quaternion rot = Quaternion.LookRotation(tangent, up);

                    Vector3 bentVert = (Vector3)pos + (rot * new Vector3(vPos.x, vPos.y, 0));
                    newVerts.Add(transform.InverseTransformPoint(bentVert));
                
                    if (sourceUvs.Length > v) newUvs.Add(sourceUvs[v]);
                }

                int vOffset = i * sourceVerts.Length;
                for (int t = 0; t < sourceTris.Length; t++)
                {
                    newTris.Add(sourceTris[t] + vOffset);
                }
            }

            deformedMesh.vertices = newVerts.ToArray();
            deformedMesh.uv = newUvs.ToArray();
            deformedMesh.triangles = newTris.ToArray();
            deformedMesh.RecalculateNormals();
            deformedMesh.RecalculateBounds();
            GetComponent<MeshFilter>().mesh = deformedMesh;
        }
    }
}