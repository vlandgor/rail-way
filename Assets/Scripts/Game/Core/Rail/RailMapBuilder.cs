using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Core.Rail
{
    public static class RailMapBuilder
    {
        public static void Build(
            SplineContainer container, 
            out Dictionary<int, RailLink> links, 
            out Dictionary<int, List<RailSegment>> segmentsByLink)
        {
            links = new Dictionary<int, RailLink>();
            segmentsByLink = new Dictionary<int, List<RailSegment>>();
            
            if (container == null) return;

            var knotLinks = container.KnotLinkCollection;
            var processedKnots = new HashSet<string>();
            int nextId = 0;

            for (int s = 0; s < container.Splines.Count; s++)
            {
                var spline = container.Splines[s];
                for (int k = 0; k < spline.Count; k++)
                {
                    if (processedKnots.Contains($"{s}_{k}")) continue;

                    var knotIndex = new SplineKnotIndex(s, k);
                    var knotLinkData = knotLinks.GetKnotLinks(knotIndex);
                    
                    bool isJunction = knotLinkData != null && knotLinkData.Count > 1;
                    bool isEnd = k == 0 || k == spline.Count - 1;

                    if (isJunction || isEnd)
                    {
                        var link = CreateRailLink(container, knotLinkData, s, k, ref nextId, processedKnots);
                        link.IsJunction = isJunction;
                        link.IsDeadEnd = isEnd && !isJunction;
                        links.Add(link.Id, link);
                    }
                }
            }

            foreach (var link in links.Values)
            {
                foreach (var node in link.Nodes)
                {
                    CreateSegmentsForNode(container, node, link.Id, links, segmentsByLink);
                }
            }
        }

        private static RailLink CreateRailLink(SplineContainer container, IReadOnlyList<SplineKnotIndex> knotLinkData, int s, int k, ref int nextId, HashSet<string> processed)
        {
            var railLink = new RailLink { Id = nextId++ };
            Vector3 sumPos = Vector3.zero;

            if (knotLinkData != null && knotLinkData.Count > 1)
            {
                foreach (var data in knotLinkData)
                {
                    var pos = (Vector3)container.Splines[data.Spline][data.Knot].Position;
                    var worldPos = container.transform.TransformPoint(pos);
                    sumPos += worldPos;
                    railLink.Nodes.Add(new RailNode { SplineIndex = data.Spline, KnotIndex = data.Knot, WorldPosition = worldPos });
                    processed.Add($"{data.Spline}_{data.Knot}");
                }
                railLink.WorldPosition = sumPos / knotLinkData.Count;
            }
            else
            {
                var worldPos = container.transform.TransformPoint(container.Splines[s][k].Position);
                railLink.WorldPosition = worldPos;
                railLink.Nodes.Add(new RailNode { SplineIndex = s, KnotIndex = k, WorldPosition = worldPos });
                processed.Add($"{s}_{k}");
            }
            return railLink;
        }

        private static void CreateSegmentsForNode(SplineContainer container, RailNode node, int linkId, Dictionary<int, RailLink> allLinks, Dictionary<int, List<RailSegment>> registry)
        {
            var spline = container.Splines[node.SplineIndex];
            
            TryWalk(container, node, linkId, 1, allLinks, registry);
            TryWalk(container, node, linkId, -1, allLinks, registry);

            if (spline.Closed)
            {
                if (node.KnotIndex == 0) TryWalk(container, node, linkId, -1, allLinks, registry);
                if (node.KnotIndex == spline.Count - 1) TryWalk(container, node, linkId, 1, allLinks, registry);
            }
        }

        private static void TryWalk(SplineContainer container, RailNode startNode, int startId, int dir, Dictionary<int, RailLink> allLinks, Dictionary<int, List<RailSegment>> registry)
        {
            var spline = container.Splines[startNode.SplineIndex];
            int current = startNode.KnotIndex + dir;

            while (current >= 0 && current < spline.Count)
            {
                var endLink = allLinks.Values.FirstOrDefault(l => l.Nodes.Any(n => n.SplineIndex == startNode.SplineIndex && n.KnotIndex == current));
                
                if (endLink != null)
                {
                    var segment = new RailSegment 
                    { 
                        SplineIndex = startNode.SplineIndex, 
                        StartKnotIndex = startNode.KnotIndex, 
                        EndKnotIndex = current, 
                        StartLinkId = startId, 
                        EndLinkId = endLink.Id, 
                        Direction = dir 
                    };
                    
                    segment.SetTangent(CalculateTangent(container, segment));
                    
                    if (!registry.ContainsKey(startId)) registry[startId] = new List<RailSegment>();
                    registry[startId].Add(segment);
                    return;
                }
                current += dir;
            }
        }

        private static Vector3 CalculateTangent(SplineContainer container, RailSegment segment)
        {
            var spline = container.Splines[segment.SplineIndex];
            float startT = (float)segment.StartKnotIndex / (spline.Count - 1);
            float endT = (float)segment.EndKnotIndex / (spline.Count - 1);
            spline.Evaluate(Mathf.Lerp(startT, endT, 0.1f), out _, out float3 tan, out _);
            Vector3 worldTan = container.transform.TransformDirection(tan);
            return segment.Direction < 0 ? -worldTan : worldTan;
        }
    }
}