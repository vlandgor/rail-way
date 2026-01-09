using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace Core.Rail
{
    public class RailSplineMap : MonoBehaviour
    {
        [SerializeField] private SplineContainer splineContainer;

        private readonly Dictionary<int, KnotNode> _runtimeNodes = new();
        public IReadOnlyDictionary<int, KnotNode> RuntimeNodes => _runtimeNodes;

        private void Awake()
        {
            BuildMap();
        }

        private void BuildMap()
        {
            _runtimeNodes.Clear();
            int globalId = 0;

            for (int s = 0; s < splineContainer.Splines.Count; s++)
            {
                var spline = splineContainer.Splines[s];

                for (int k = 0; k < spline.Count; k++)
                {
                    float3 localPos = spline[k].Position;
                    Vector3 worldPos = splineContainer.transform.TransformPoint(localPos);

                    _runtimeNodes.Add(
                        globalId,
                        new KnotNode(globalId, worldPos, spline, k)
                    );

                    globalId++;
                }
            }

            int offset = 0;
            for (int s = 0; s < splineContainer.Splines.Count; s++)
            {
                var spline = splineContainer.Splines[s];

                for (int k = 0; k < spline.Count; k++)
                {
                    int id = offset + k;

                    if (k < spline.Count - 1)
                        _runtimeNodes[id].ForwardId = offset + k + 1;
                    else if (spline.Closed)
                        _runtimeNodes[id].ForwardId = offset;

                    if (k > 0)
                        _runtimeNodes[id].BackwardId = offset + k - 1;
                    else if (spline.Closed)
                        _runtimeNodes[id].BackwardId = offset + spline.Count - 1;
                }

                offset += spline.Count;
            }
        }

        public RailSegment GetSegment(KnotNode from, KnotNode to)
        {
            float startT = from.KnotIndex / (float)(from.Spline.Count - 1);
            float endT = to.KnotIndex / (float)(to.Spline.Count - 1);

            return new RailSegment(from.Spline, startT, endT);
        }
    }

    public class KnotNode
    {
        public int Id;
        public Vector3 Position;
        public Spline Spline;
        public int KnotIndex;

        public int ForwardId = -1;
        public int BackwardId = -1;
        public int LeftId = -1;
        public int RightId = -1;

        public KnotNode(int id, Vector3 pos, Spline spline, int knotIndex)
        {
            Id = id;
            Position = pos;
            Spline = spline;
            KnotIndex = knotIndex;
        }
    }
}
