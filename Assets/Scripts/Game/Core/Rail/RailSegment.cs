using UnityEngine;

namespace Game.Core.Rail
{
    public class RailSegment
    {
        public int SplineIndex;
        public int StartKnotIndex;
        public int EndKnotIndex;
        public int StartLinkId;
        public int EndLinkId;
        public int Direction;
        
        private Vector3 _cachedTangent;

        public void SetTangent(Vector3 dir) => _cachedTangent = dir;
        public Vector3 GetTangent() => _cachedTangent;
    }
}