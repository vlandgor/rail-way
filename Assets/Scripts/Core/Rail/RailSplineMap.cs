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
                    float3 worldPos = splineContainer.transform.TransformPoint(localPos);
                    _runtimeNodes.Add(globalId, new KnotNode(globalId, (Vector3)worldPos));
                    globalId++;
                }
            }

            int offset = 0;
            for (int s = 0; s < splineContainer.Splines.Count; s++)
            {
                var spline = splineContainer.Splines[s];
                for (int k = 0; k < spline.Count; k++)
                {
                    int currentId = offset + k;
                    
                    if (k < spline.Count - 1) 
                        _runtimeNodes[currentId].ForwardId = offset + k + 1;
                    else if (spline.Closed) 
                        _runtimeNodes[currentId].ForwardId = offset;

                    if (k > 0) 
                        _runtimeNodes[currentId].BackwardId = offset + k - 1;
                    else if (spline.Closed) 
                        _runtimeNodes[currentId].BackwardId = offset + spline.Count - 1;
                }
                offset += spline.Count;
            }

            foreach (var nodeA in _runtimeNodes.Values)
            {
                foreach (var nodeB in _runtimeNodes.Values)
                {
                    if (nodeA.Id == nodeB.Id) continue;
                    if (Vector3.Distance(nodeA.Position, nodeB.Position) < 0.1f)
                    {
                        if (nodeA.LeftId == -1) 
                        {
                            nodeA.LeftId = nodeB.Id;
                            nodeB.RightId = nodeA.Id;
                        }
                    }
                }
            }
        }
    }

    public class KnotNode
    {
        public int Id;
        public Vector3 Position;
        public int ForwardId = -1;
        public int BackwardId = -1;
        public int LeftId = -1;
        public int RightId = -1;

        public KnotNode(int id, Vector3 pos) 
        { 
            Id = id; 
            Position = pos; 
        }
    }
}