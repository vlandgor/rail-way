using System.Collections.Generic;
using UnityEngine;

namespace Core.Rail
{
    public class RailGraphGenerator
    {
        private int _nextNodeId;
        private Dictionary<Vector2Int, RailNodeData> _posToNode;

        public RailGraphData Generate(int width, int height, int iterations, float spacing, int seed)
        {
            _nextNodeId = 0;
            _posToNode = new Dictionary<Vector2Int, RailNodeData>();
            Random.InitState(seed);

            RailGraphData graph = new RailGraphData();
            List<RectInt> rooms = new List<RectInt> { new RectInt(0, 0, width, height) };

            for (int i = 0; i < iterations; i++)
            {
                for (int j = rooms.Count - 1; j >= 0; j--)
                {
                    RectInt room = rooms[j];
                    if (room.width <= 1 || room.height <= 1) continue;

                    rooms.RemoveAt(j);
                    bool splitHorizontal = Random.value > 0.5f;
                    if (room.width > room.height * 1.2f) splitHorizontal = false;
                    else if (room.height > room.width * 1.2f) splitHorizontal = true;

                    if (splitHorizontal)
                    {
                        int split = Random.Range(1, room.height);
                        rooms.Add(new RectInt(room.x, room.y, room.width, split));
                        rooms.Add(new RectInt(room.x, room.y + split, room.width, room.height - split));
                    }
                    else
                    {
                        int split = Random.Range(1, room.width);
                        rooms.Add(new RectInt(room.x, room.y, split, room.height));
                        rooms.Add(new RectInt(room.x + split, room.y, room.width - split, room.height));
                    }
                }
            }

            foreach (var room in rooms)
            {
                Vector2Int p1 = new Vector2Int(room.xMin, room.yMin);
                Vector2Int p2 = new Vector2Int(room.xMax, room.yMin);
                Vector2Int p3 = new Vector2Int(room.xMax, room.yMax);
                Vector2Int p4 = new Vector2Int(room.xMin, room.yMax);

                AddEdge(graph, p1, p2, spacing);
                AddEdge(graph, p2, p3, spacing);
                AddEdge(graph, p3, p4, spacing);
                AddEdge(graph, p4, p1, spacing);
            }

            return graph;
        }

        private void AddEdge(RailGraphData graph, Vector2Int p1, Vector2Int p2, float spacing)
        {
            RailNodeData n1 = GetOrCreateNode(graph, p1, spacing);
            RailNodeData n2 = GetOrCreateNode(graph, p2, spacing);

            if (n1.ConnectedNodeIds.Contains(n2.Id)) return;

            n1.ConnectedNodeIds.Add(n2.Id);
            n2.ConnectedNodeIds.Add(n1.Id);
            graph.Edges.Add(new RailEdgeData { FromNodeId = n1.Id, ToNodeId = n2.Id });
        }

        private RailNodeData GetOrCreateNode(RailGraphData graph, Vector2Int pos, float spacing)
        {
            if (_posToNode.TryGetValue(pos, out RailNodeData existing)) return existing;

            RailNodeData newNode = new RailNodeData
            {
                Id = _nextNodeId++,
                Position = new Vector3(pos.x * spacing, 0, pos.y * spacing)
            };
            graph.Nodes.Add(newNode);
            _posToNode[pos] = newNode;
            return newNode;
        }
    }
}