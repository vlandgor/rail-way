using UnityEngine.Splines;

namespace Core.Rail
{
    public struct RailSegment
    {
        public Spline Spline;
        public float StartT;
        public float EndT;

        public RailSegment(Spline spline, float startT, float endT)
        {
            Spline = spline;
            StartT = startT;
            EndT = endT;
        }
    }
}