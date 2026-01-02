using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core.Rail
{
    public class RailGraphGenerator
    {
        private int _nextNodeId;
        private System.Random _rng;

        public RailGraphData Generate(
            int nodeCount,
            float minDistance,
            float maxDistance,
            int maxConnectionsPerNode,
            int seed)
        {
            _nextNodeId = 0;
            _rng = new System.Random(seed);

            RailGraphData graph = new RailGraphData();

            // 1. Start node
            RailNodeData startNode = CreateNode(Vector3.zero);
            graph.Nodes.Add(startNode);

            // 2. Grow graph
            while (graph.Nodes.Count < nodeCount)
            {
                RailNodeData from = GetRandomNode(graph);

                if (from.ConnectedNodeIds.Count >= maxConnectionsPerNode)
                    continue;

                Vector3 dir = GetRandomDirection(graph, from);
                float dist = Lerp(minDistance, maxDistance, NextFloat());

                Vector3 pos = from.Position + dir * dist;

                RailNodeData newNode = CreateNode(pos);

                graph.Nodes.Add(newNode);
                ConnectNodes(graph, from, newNode);
            }

            return graph;
        }

        private RailNodeData CreateNode(Vector3 position)
        {
            return new RailNodeData
            {
                Id = _nextNodeId++,
                Position = position
            };
        }

        private void ConnectNodes(RailGraphData graph, RailNodeData a, RailNodeData b)
        {
            a.ConnectedNodeIds.Add(b.Id);
            b.ConnectedNodeIds.Add(a.Id);

            graph.Edges.Add(new RailEdgeData
            {
                FromNodeId = a.Id,
                ToNodeId = b.Id
            });
        }

        private RailNodeData GetRandomNode(RailGraphData graph)
        {
            return graph.Nodes[_rng.Next(graph.Nodes.Count)];
        }

        // ======================================================
        // Direction logic
        // ======================================================

        private Vector3 GetRandomDirection(RailGraphData graph, RailNodeData from)
        {
            bool allowForward = HasSideConnection(graph, from);

            List<Vector3> directions = new()
            {
                Vector3.left,
                Vector3.right,
                Vector3.back
            };

            if (allowForward)
                directions.Add(Vector3.forward);

            return directions[_rng.Next(directions.Count)];
        }

        private bool HasSideConnection(RailGraphData graph, RailNodeData node)
        {
            foreach (int connectedId in node.ConnectedNodeIds)
            {
                RailNodeData other = graph.Nodes.Find(n => n.Id == connectedId);
                if (other == null)
                    continue;

                Vector3 dir = (other.Position - node.Position).normalized;

                if (Vector3.Dot(dir, Vector3.left) > 0.9f ||
                    Vector3.Dot(dir, Vector3.right) > 0.9f)
                {
                    return true;
                }
            }

            return false;
        }

        // ======================================================
        // RNG helpers
        // ======================================================

        private float NextFloat()
        {
            return (float)_rng.NextDouble();
        }

        private float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}
