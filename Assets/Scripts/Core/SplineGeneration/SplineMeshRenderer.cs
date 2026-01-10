using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEditor;

namespace Core.SplineGeneration
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SplineMeshBender : MonoBehaviour
    {
        public SplineContainer splineContainer;
        public float width = 1f;
        public float thickness = 0.2f;
        public int resolutionPerSpline = 50;
        public float filletDistance = 0.5f;
        public int filletResolution = 24;

        [Header("Debug")]
        public bool showKnotInfo = true;
        public Color debugColor = Color.yellow;

        private Mesh _generatedMesh;

        private void OnEnable()
        {
            Spline.Changed += OnSplineChanged;
            GenerateMesh();
        }

        private void OnDisable() => Spline.Changed -= OnSplineChanged;
        private void OnValidate() => GenerateMesh();
        private void OnSplineChanged(Spline spline, int knotIndex, SplineModification modification) => GenerateMesh();

        public void GenerateMesh()
        {
            if (splineContainer == null || splineContainer.Splines == null) return;

            if (_generatedMesh == null)
            {
                _generatedMesh = new Mesh { name = "GeneratedRail" };
                _generatedMesh.hideFlags = HideFlags.DontSave;
                GetComponent<MeshFilter>().sharedMesh = _generatedMesh;
            }

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            foreach (var spline in splineContainer.Splines)
            {
                GenerateSegmentMesh(spline, verts, tris, uvs, resolutionPerSpline);
            }

            GenerateProximityIntersections(verts, tris, uvs);

            _generatedMesh.Clear();
            _generatedMesh.SetVertices(verts);
            _generatedMesh.SetTriangles(tris, 0);
            _generatedMesh.SetUVs(0, uvs);
            _generatedMesh.RecalculateNormals();
            _generatedMesh.RecalculateBounds();
        }

        private void GenerateSegmentMesh(Spline spline, List<Vector3> verts, List<int> tris, List<Vector2> uvs, int res)
        {
            float length = spline.GetLength();
            if (length < 0.001f) return;

            int offset = verts.Count;
            for (int i = 0; i <= res; i++)
            {
                float t = (float)i / res;
                spline.Evaluate(t, out var pos, out var tan, out var up);
                AddQuadAtPoint(pos, tan, up, t, length, offset, i, res, verts, tris, uvs);
            }
        }

        private void GenerateProximityIntersections(List<Vector3> verts, List<int> tris, List<Vector2> uvs)
        {
            var splines = splineContainer.Splines;
            HashSet<string> processedPairs = new HashSet<string>();

            for (int s1 = 0; s1 < splines.Count; s1++)
            {
                for (int s2 = 0; s2 < splines.Count; s2++)
                {
                    if (s1 == s2) continue;

                    for (int k1 = 0; k1 < splines[s1].Count; k1++)
                    {
                        for (int k2 = 0; k2 < splines[s2].Count; k2++)
                        {
                            string pairId = s1 < s2 ? $"{s1}_{k1}_{s2}_{k2}" : $"{s2}_{k2}_{s1}_{k1}";
                            if (processedPairs.Contains(pairId)) continue;

                            float dist = Vector3.Distance((Vector3)splines[s1][k1].Position, (Vector3)splines[s2][k2].Position);
                            if (dist < 0.1f)
                            {
                                bool isK1End = (k1 == 0 || k1 == splines[s1].Count - 1);
                                bool isK2End = (k2 == 0 || k2 == splines[s2].Count - 1);

                                int mainS = isK1End && !isK2End ? s2 : s1;
                                int branchS = mainS == s1 ? s2 : s1;
                                int mainK = mainS == s1 ? k1 : k2;
                                int branchK = branchS == s1 ? k1 : k2;

                                CreateFilletDirected(mainS, mainK, branchS, branchK, 1f, verts, tris, uvs);
                                CreateFilletDirected(mainS, mainK, branchS, branchK, -1f, verts, tris, uvs);
                                
                                processedPairs.Add(pairId);
                            }
                        }
                    }
                }
            }
        }

        private void CreateFilletDirected(int sIdx1, int kIdx1, int sIdx2, int kIdx2, float sideSign, List<Vector3> verts, List<int> tris, List<Vector2> uvs)
        {
            Spline sMain = splineContainer.Splines[sIdx1];
            Spline sBranch = splineContainer.Splines[sIdx2];

            float tMainCenter = (float)kIdx1 / (sMain.Count - 1);
            float tBranchCenter = (float)kIdx2 / (sBranch.Count - 1);

            float tMainOffset = filletDistance / sMain.GetLength();
            float tBranchOffset = filletDistance / sBranch.GetLength();

            float tMainStart = Mathf.Clamp01(tMainCenter + (tMainOffset * sideSign));
            float tBranchStart = Mathf.Clamp01(tBranchCenter + (tBranchOffset * (kIdx2 == 0 ? 1 : -1)));

            sMain.Evaluate(tMainStart, out var p0, out var tan0, out var up0);
            sBranch.Evaluate(tBranchStart, out var p3, out var tan3, out var up3);

            Vector3 m0 = Vector3.Normalize(tan0) * (sideSign * -filletDistance);
            Vector3 m3 = Vector3.Normalize(tan3) * ((kIdx2 == 0 ? 1 : -1) * -filletDistance);

            if (Mathf.Abs(Vector3.Dot(m0.normalized, m3.normalized)) > 0.9f) return;

            Vector3 p1 = (Vector3)p0 + m0 * 0.5f;
            Vector3 p2 = (Vector3)p3 + m3 * 0.5f;

            int offset = verts.Count;
            for (int i = 0; i <= filletResolution; i++)
            {
                float t = (float)i / filletResolution;
                
                Vector3 pos = GetCubicPoint((Vector3)p0, p1, p2, (Vector3)p3, t);
                pos += (Vector3)up0 * 0.01f;

                Vector3 tan = GetCubicTangent((Vector3)p0, p1, p2, (Vector3)p3, t);
                Vector3 up = Vector3.Lerp(up0, up3, t);

                AddQuadAtPoint(pos, tan, up, t, filletDistance * 2, offset, i, filletResolution, verts, tris, uvs);
            }
        }

        private Vector3 GetCubicPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
        }

        private Vector3 GetCubicTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1 - t;
            return 3 * u * u * (p1 - p0) + 6 * u * t * (p2 - p1) + 3 * t * t * (p3 - p2);
        }

        private void AddQuadAtPoint(Vector3 pos, Vector3 tan, Vector3 up, float t, float len, int offset, int i, int res, List<Vector3> v, List<int> tr, List<Vector2> uv)
        {
            Vector3 fwd = Vector3.Normalize(tan);
            Vector3 side = Vector3.Cross(up, fwd).normalized;
            Vector3 correctUp = Vector3.Cross(fwd, side).normalized;

            float hw = width * 0.5f;
            v.Add(pos - (side * hw) + (correctUp * thickness));
            v.Add(pos + (side * hw) + (correctUp * thickness));
            v.Add(pos - (side * hw));
            v.Add(pos + (side * hw));

            float uvY = t * (len / width);
            for (int j = 0; j < 4; j++) uv.Add(new Vector2(j % 2, uvY));

            if (i < res)
            {
                int c = offset + (i * 4);
                int n = offset + ((i + 1) * 4);
                tr.AddRange(new int[] { c, n, c + 1, c + 1, n, n + 1 });
                tr.AddRange(new int[] { c + 2, c + 3, n + 2, n + 2, c + 3, n + 3 });
                tr.AddRange(new int[] { c, c + 2, n, n, c + 2, n + 2 });
                tr.AddRange(new int[] { c + 1, n + 1, c + 3, c + 3, n + 1, n + 3 });
            }
        }

        private void OnDrawGizmos()
        {
            if (!showKnotInfo || splineContainer == null) return;
            for (int s = 0; s < splineContainer.Splines.Count; s++)
            {
                for (int k = 0; k < splineContainer.Splines[s].Count; k++)
                {
                    Vector3 p = transform.TransformPoint((Vector3)splineContainer.Splines[s][k].Position);
                    Gizmos.color = debugColor;
                    Gizmos.DrawSphere(p, 0.05f);
                    #if UNITY_EDITOR
                    Handles.Label(p + Vector3.up * 0.15f, $"S:{s} K:{k}");
                    #endif
                }
            }
        }
    }
}