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

            GenerateLinkedIntersections(verts, tris, uvs);

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

        private void GenerateLinkedIntersections(List<Vector3> verts, List<int> tris, List<Vector2> uvs)
        {
            var splines = splineContainer.Splines;
            var linkCollection = splineContainer.KnotLinkCollection;
            if (linkCollection == null) return;

            HashSet<string> processedPairs = new HashSet<string>();

            for (int s1 = 0; s1 < splines.Count; s1++)
            {
                for (int k1 = 0; k1 < splines[s1].Count; k1++)
                {
                    SplineKnotIndex k1Idx = new SplineKnotIndex(s1, k1);
                    if (!linkCollection.TryGetKnotLinks(k1Idx, out IReadOnlyList<SplineKnotIndex> linkedKnots)) 
                        continue;

                    foreach (var k2Idx in linkedKnots)
                    {
                        if (k1Idx.Equals(k2Idx)) continue;

                        string pairId = k1Idx.Spline < k2Idx.Spline ? 
                            $"{k1Idx.Spline}_{k1Idx.Knot}_{k2Idx.Spline}_{k2Idx.Knot}" : 
                            $"{k2Idx.Spline}_{k2Idx.Knot}_{k1Idx.Spline}_{k1Idx.Knot}";
                        
                        if (processedPairs.Contains(pairId)) continue;
                        processedPairs.Add(pairId);

                        bool isK1End = (k1Idx.Knot == 0 || k1Idx.Knot == splines[k1Idx.Spline].Count - 1);
                        bool isK2End = (k2Idx.Knot == 0 || k2Idx.Knot == splines[k2Idx.Spline].Count - 1);

                        if (isK1End && isK2End)
                        {
                            CreateFilletBetweenEndpoints(k1Idx, k2Idx, verts, tris, uvs);
                        }
                        else if (!isK1End && !isK2End)
                        {
                            CreateFilletDirected(k1Idx.Spline, k1Idx.Knot, k2Idx.Spline, k2Idx.Knot, 1f, 1f, verts, tris, uvs);
                            CreateFilletDirected(k1Idx.Spline, k1Idx.Knot, k2Idx.Spline, k2Idx.Knot, 1f, -1f, verts, tris, uvs);
                            CreateFilletDirected(k1Idx.Spline, k1Idx.Knot, k2Idx.Spline, k2Idx.Knot, -1f, 1f, verts, tris, uvs);
                            CreateFilletDirected(k1Idx.Spline, k1Idx.Knot, k2Idx.Spline, k2Idx.Knot, -1f, -1f, verts, tris, uvs);
                        }
                        else
                        {
                            int mainS = isK1End ? k2Idx.Spline : k1Idx.Spline;
                            int branchS = isK1End ? k1Idx.Spline : k2Idx.Spline;
                            int mainK = isK1End ? k2Idx.Knot : k1Idx.Knot;
                            int branchK = isK1End ? k1Idx.Knot : k2Idx.Knot;

                            CreateFilletDirected(mainS, mainK, branchS, branchK, 1f, 0f, verts, tris, uvs);
                            CreateFilletDirected(mainS, mainK, branchS, branchK, -1f, 0f, verts, tris, uvs);
                        }
                    }
                }
            }
        }

        private void CreateFilletDirected(int sMainIdx, int kMainIdx, int sBranchIdx, int kBranchIdx, float mainSide, float branchSideOverride, List<Vector3> verts, List<int> tris, List<Vector2> uvs)
        {
            Spline sMain = splineContainer.Splines[sMainIdx];
            Spline sBranch = splineContainer.Splines[sBranchIdx];

            float tMainCenter = (float)kMainIdx / (sMain.Count - 1);
            float tBranchCenter = (float)kBranchIdx / (sBranch.Count - 1);

            float tMainStart = Mathf.Clamp01(tMainCenter + (filletDistance / sMain.GetLength() * mainSide));
            
            float branchDir;
            if (branchSideOverride != 0) branchDir = branchSideOverride;
            else branchDir = (kBranchIdx == 0) ? 1f : -1f;

            float tBranchStart = Mathf.Clamp01(tBranchCenter + (filletDistance / sBranch.GetLength() * branchDir));

            sMain.Evaluate(tMainStart, out var p0, out var tan0, out var up0);
            sBranch.Evaluate(tBranchStart, out var p3, out var tan3, out var up3);

            Vector3 m0 = Vector3.Normalize(tan0) * (-mainSide * filletDistance);
            Vector3 m3 = Vector3.Normalize(tan3) * (-branchDir * filletDistance);

            if (Mathf.Abs(Vector3.Dot(m0.normalized, m3.normalized)) > 0.95f) return;

            Vector3 p1 = (Vector3)p0 + m0 * 0.5f;
            Vector3 p2 = (Vector3)p3 + m3 * 0.5f;

            int offset = verts.Count;
            for (int i = 0; i <= filletResolution; i++)
            {
                float t = (float)i / filletResolution;
                Vector3 pos = GetCubicPoint((Vector3)p0, p1, p2, (Vector3)p3, t) + (Vector3)up0 * 0.01f;
                Vector3 tan = GetCubicTangent((Vector3)p0, p1, p2, (Vector3)p3, t);
                AddQuadAtPoint(pos, tan, up0, t, filletDistance * 2, offset, i, filletResolution, verts, tris, uvs);
            }
        }

        private void CreateFilletBetweenEndpoints(SplineKnotIndex k1, SplineKnotIndex k2, List<Vector3> verts, List<int> tris, List<Vector2> uvs)
        {
            Spline s1 = splineContainer.Splines[k1.Spline];
            Spline s2 = splineContainer.Splines[k2.Spline];

            float d1 = (k1.Knot == 0) ? 1f : -1f;
            float d2 = (k2.Knot == 0) ? 1f : -1f;

            float t1 = Mathf.Clamp01((float)k1.Knot / (s1.Count - 1) + (filletDistance / s1.GetLength() * d1));
            float t2 = Mathf.Clamp01((float)k2.Knot / (s2.Count - 1) + (filletDistance / s2.GetLength() * d2));

            s1.Evaluate(t1, out var p0, out var tan0, out var up0);
            s2.Evaluate(t2, out var p3, out var tan3, out var up3);

            Vector3 m0 = Vector3.Normalize(tan0) * (-d1 * filletDistance);
            Vector3 m3 = Vector3.Normalize(tan3) * (-d2 * filletDistance);

            Vector3 p1 = (Vector3)p0 + m0 * 0.5f;
            Vector3 p2 = (Vector3)p3 + m3 * 0.5f;

            int offset = verts.Count;
            for (int i = 0; i <= filletResolution; i++)
            {
                float t = (float)i / filletResolution;
                Vector3 pos = GetCubicPoint((Vector3)p0, p1, p2, (Vector3)p3, t) + (Vector3)up0 * 0.01f;
                Vector3 tan = GetCubicTangent((Vector3)p0, p1, p2, (Vector3)p3, t);
                AddQuadAtPoint(pos, tan, up0, t, filletDistance * 2, offset, i, filletResolution, verts, tris, uvs);
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
            if (fwd == Vector3.zero) fwd = transform.forward;
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