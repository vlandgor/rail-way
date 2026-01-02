using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(RailMover))]
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private GameObject _cameraObject;
        
        private PlayerInput _input;
        private RailMover _mover;

        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _mover = GetComponent<RailMover>();
        }

        public override void OnNetworkSpawn()
        {
            if (_cameraObject != null)
            {
                Debug.Log("Camera Object is " + _cameraObject);
                _cameraObject.SetActive(IsOwner);
            }

            if (!IsOwner)
            {
                enabled = false;
            }
        }

        private void Update()
        {
            if (!IsOwner || !_input.HasInput)
                return;

            _mover.TryMove(_input.MoveDirection);
        }
    }
}