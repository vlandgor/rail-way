using UnityEngine;

namespace Core.Player
{
    public class PlayerInput : MonoBehaviour
    {
        public Vector2Int MoveDirection { get; private set; }
        public bool HasInput { get; private set; }

        private void Update()
        {
            HasInput = false;
            MoveDirection = Vector2Int.zero;

            if (Input.GetKeyDown(KeyCode.W))
            {
                MoveDirection = Vector2Int.up;
                HasInput = true;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                MoveDirection = Vector2Int.down;
                HasInput = true;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                MoveDirection = Vector2Int.left;
                HasInput = true;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                MoveDirection = Vector2Int.right;
                HasInput = true;
            }
        }
    }
}