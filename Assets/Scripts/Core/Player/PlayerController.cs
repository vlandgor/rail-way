using UnityEngine;

namespace Core.Player
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(RailMover))]
    public class PlayerController : MonoBehaviour
    {
        private PlayerInput _input;
        private RailMover _mover;

        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _mover = GetComponent<RailMover>();
        }

        private void Update()
        {
            if (!_input.HasInput)
                return;

            _mover.TryMove(_input.MoveDirection);
        }
    }
}