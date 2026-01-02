using System.Collections.Generic;
using UnityEngine;

namespace Core.Rail
{
    public class RailGraphBuilder : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject railPrefab;
        [SerializeField] private RailNode railNodePrefab;

        [Header("Grid Layout")]
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private int subdivisions = 5;
        [SerializeField] private float spacing = 5f;

        [Header("Seed")]
        [SerializeField] private int seed = 12345;

        private readonly Dictionary<int, RailNode> _runtimeNodes = new();
        public IReadOnlyDictionary<int, RailNode> RuntimeNodes => _runtimeNodes;

        private void Awake()
        {
            BuildGraph();
        }

        private void BuildGraph()
        {
            RailGraphGenerator generator = new RailGraphGenerator();
            RailGraphData graph = generator.Generate(gridWidth, gridHeight, subdivisions, spacing, seed);
            Build(graph);
        }

        private void Build(RailGraphData graph)
        {
            foreach (Transform child in transform) Destroy(child.gameObject);
            _runtimeNodes.Clear();

            foreach (RailNodeData nodeData in graph.Nodes)
            {
                RailNode node = Instantiate(railNodePrefab, nodeData.Position, Quaternion.identity, transform);
                node.Id = nodeData.Id;
                _runtimeNodes[nodeData.Id] = node;
            }

            foreach (RailEdgeData edge in graph.Edges)
            {
                RailNode a = _runtimeNodes[edge.FromNodeId];
                RailNode b = _runtimeNodes[edge.ToNodeId];
                ConnectNodes(a, b);
                SpawnRail(a.transform.position, b.transform.position);
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

        private void SpawnRail(Vector3 a, Vector3 b)
        {
            Vector3 mid = (a + b) * 0.5f;
            Vector3 dir = b - a;
            GameObject rail = Instantiate(railPrefab, mid, Quaternion.identity, transform);
            rail.transform.forward = dir.normalized;
            rail.transform.localScale = new Vector3(rail.transform.localScale.x, rail.transform.localScale.y, dir.magnitude);
        }
    }
}