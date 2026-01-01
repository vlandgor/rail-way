using UnityEngine;

namespace Core.Rail
{
    public class RailNode : MonoBehaviour
    {
        [Header("Connections")]
        public RailNode Forward;
        public RailNode Backward;
        public RailNode Left;
        public RailNode Right;

        public Vector3 Position => transform.position;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.1f);

            DrawConnection(Forward, Color.green);
            DrawConnection(Backward, Color.red);
            DrawConnection(Left, Color.blue);
            DrawConnection(Right, Color.magenta);
        }

        private void DrawConnection(RailNode node, Color color)
        {
            if (node == null) return;
            Gizmos.color = color;
            Gizmos.DrawLine(transform.position, node.transform.position);
        }
#endif
    }
}