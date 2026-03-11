using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Core.Rail
{
    public class RailMap : MonoBehaviour
    {
        [SerializeField] private SplineContainer splineContainer;
        
        private Dictionary<int, RailLink> _links = new();
        private Dictionary<int, List<RailSegment>> _segmentsByLink = new();

        public IReadOnlyDictionary<int, RailLink> Links => _links;

        private void Awake()
        {
            RailMapBuilder.Build(splineContainer, out _links, out _segmentsByLink);
        }

        public Vector3 GetLinkPosition(int linkId) 
            => _links.TryGetValue(linkId, out var link) ? link.WorldPosition : Vector3.zero;

        public RailSegment GetNextSegment(int currentLinkId, Vector2Int inputDir, Vector3 playerForward)
        {
            if (!_segmentsByLink.TryGetValue(currentLinkId, out var available)) 
                return null;

            Vector3 worldInput = Quaternion.LookRotation(playerForward, Vector3.up) * new Vector3(inputDir.x, 0, inputDir.y);
            
            RailSegment bestMatch = null;
            float bestDot = -1f;

            foreach (var segment in available)
            {
                float dot = Vector3.Dot(segment.GetTangent().normalized, worldInput.normalized);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestMatch = segment;
                }
            }
            return bestMatch;
        }

        public SplineContainer Container => splineContainer;
    }
}