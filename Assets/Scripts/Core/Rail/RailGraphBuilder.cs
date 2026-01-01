using System.Collections.Generic;
using UnityEngine;
using Core.Player;

namespace Core.Rail
{
    public class RailGraphBuilder : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject railPrefab;
        [SerializeField] private RailNode railNodePrefab;
        [SerializeField] private RailMover playerPrefab;

        private readonly Dictionary<int, RailNode> _runtimeNodes = new();

        private void Start()
        {
            RailGraphGenerator generator = new RailGraphGenerator();
            RailGraphData graph = generator.Generate(10, 3, 5, 3);

            Build(graph);
            SpawnPlayer(graph);
        }

        public void Build(RailGraphData graph)
        {
            // 1. Spawn nodes
            foreach (RailNodeData nodeData in graph.Nodes)
            {
                RailNode node = Instantiate(
                    railNodePrefab,
                    nodeData.Position,
                    Quaternion.identity,
                    transform
                );

                _runtimeNodes[nodeData.Id] = node;
            }

            // 2. Connect nodes + spawn rails
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

            // Simple axis-based connection
            if (Mathf.Abs(dir.z) > Mathf.Abs(dir.x))
            {
                if (dir.z > 0)
                {
                    a.Forward = b;
                    b.Backward = a;
                }
                else
                {
                    a.Backward = b;
                    b.Forward = a;
                }
            }
            else
            {
                if (dir.x > 0)
                {
                    a.Right = b;
                    b.Left = a;
                }
                else
                {
                    a.Left = b;
                    b.Right = a;
                }
            }
        }

        private void SpawnRail(Vector3 a, Vector3 b)
        {
            Vector3 mid = (a + b) * 0.5f;
            Vector3 dir = b - a;

            GameObject rail = Instantiate(railPrefab, mid, Quaternion.identity, transform);
            rail.transform.forward = dir.normalized;
            rail.transform.localScale = new Vector3(
                rail.transform.localScale.x,
                rail.transform.localScale.y,
                dir.magnitude
            );
        }

        private void SpawnPlayer(RailGraphData graph)
        {
            // Pick first node (later you can choose by tag/type)
            RailNodeData startNodeData = graph.Nodes[0];
            RailNode startNode = _runtimeNodes[startNodeData.Id];

            RailMover player = Instantiate(playerPrefab);
            player.SetStartNode(startNode);
        }
    }
}
