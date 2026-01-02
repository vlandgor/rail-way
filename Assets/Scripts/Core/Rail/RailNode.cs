using UnityEngine;

namespace Core.Rail
{
    public class RailNode : MonoBehaviour
    {
        public int Id;
        public Vector3 Position => transform.position;

        public RailNode Forward;
        public RailNode Backward;
        public RailNode Left;
        public RailNode Right;
    }
}