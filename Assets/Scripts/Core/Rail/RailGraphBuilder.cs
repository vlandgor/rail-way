using System.Collections.Generic;
using UnityEngine;

namespace Core.Rail
{
    public class RailGraphBuilder : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private RailNode railNodePrefab;
        [SerializeField] private GameObject railVisualPrefab;

        [Space] 
        [SerializeField] private Transform _railNodesTransform;
        [SerializeField] private Transform _railVisualsTransform;

        [Header("Grid Layout")]
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private int subdivisions = 5;

        [Header("Seed")]
        [SerializeField] private int seed = 54321;

        private readonly Dictionary<int, RailNode> _runtimeNodes = new();
        public IReadOnlyDictionary<int, RailNode> RuntimeNodes => _runtimeNodes;

        private void Awake()
        {
            BuildGraph();
        }

        private void BuildGraph()
        {
            RailGraphGenerator generator = new RailGraphGenerator();
            RailGraphData graph = generator.Generate(gridWidth, gridHeight, subdivisions, seed);
            Build(graph);
        }

        private void Build(RailGraphData graph)
        {
            if (_railNodesTransform != null)
            {
                foreach (Transform child in _railNodesTransform) Destroy(child.gameObject);
            }

            if (_railVisualsTransform != null)
            {
                foreach (Transform child in _railVisualsTransform) Destroy(child.gameObject);
            }

            _runtimeNodes.Clear();

            // This offset moves the (0,0) corner to (-5,-5), centering the (10,10) grid at (0,0)
            Vector3 centerOffset = new Vector3(gridWidth * 0.5f, 0, gridHeight * 0.5f);

            foreach (RailNodeData nodeData in graph.Nodes)
            {
                // Subtract the offset before converting to world space
                Vector3 localPos = nodeData.Position - centerOffset;
                Vector3 worldPos = transform.TransformPoint(localPos);

                RailNode node = Instantiate(railNodePrefab, worldPos, Quaternion.identity, _railNodesTransform);
                node.Id = nodeData.Id;
                node.name = $"Node_{node.Id}";
                _runtimeNodes[nodeData.Id] = node;
            }

            foreach (RailEdgeData edge in graph.Edges)
            {
                if (_runtimeNodes.TryGetValue(edge.FromNodeId, out RailNode a) && 
                    _runtimeNodes.TryGetValue(edge.ToNodeId, out RailNode b))
                {
                    ConnectNodes(a, b);
                    SpawnVisualRail(a.transform.position, b.transform.position);
                }
            }
        }

        private void ConnectNodes(RailNode a, RailNode b)
        {
            Vector3 dir = (b.transform.position - a.transform.position).normalized;

            if (Mathf.Abs(dir.z) > 0.9f)
            {
                if (dir.z > 0) { a.Forward = b; b.Backward = a; }
                else { a.Backward = b; b.Forward = a; }
            }
            else if (Mathf.Abs(dir.x) > 0.9f)
            {
                if (dir.x > 0) { a.Right = b; b.Left = a; }
                else { a.Left = b; b.Right = a; }
            }
        }

        private void SpawnVisualRail(Vector3 posA, Vector3 posB)
        {
            Vector3 midPoint = (posA + posB) / 2f;
            float distance = Vector3.Distance(posA, posB);

            GameObject visualRail = Instantiate(railVisualPrefab, midPoint, Quaternion.identity, _railVisualsTransform);
            visualRail.transform.forward = (posB - posA).normalized;

            Vector3 scale = visualRail.transform.localScale;
            scale.z = distance;
            visualRail.transform.localScale = scale;
        }
    }
}