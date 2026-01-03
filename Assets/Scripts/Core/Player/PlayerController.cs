using Core.Session;
using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(RailMover))]
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private GameObject _cameraObject;
        [SerializeField] private MeshRenderer _meshRenderer;
        
        private PlayerInput _input;
        private RailMover _mover;

        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _mover = GetComponent<RailMover>();
        }

        public override void OnNetworkSpawn()
        {
            if (_cameraObject != null) _cameraObject.SetActive(IsOwner);
        }

        private void Update()
        {
            UpdateVisuals();

            if (!IsOwner || _input == null || !_input.HasInput) return;

            var status = TagGameManager.Instance;
            if (status == null || !status.IsGameActive) return;

            if (status.IsTagCooldownActive && status.CurrentTaggerId == OwnerClientId)
            {
                return;
            }

            _mover.TryMove(_input.MoveDirection);
        }

        private void UpdateVisuals()
        {
            var status = TagGameManager.Instance;
            if (status == null || _meshRenderer == null) return;

            if (status.IsTagCooldownActive && status.CurrentTaggerId == OwnerClientId)
            {
                _meshRenderer.material.color = Color.white;
            }
            else
            {
                bool isTagger = status.CurrentTaggerId == OwnerClientId;
                _meshRenderer.material.color = isTagger ? Color.red : Color.blue;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerController>(out var otherPlayer))
            {
                if (IsOwner && _mover.IsMoving)
                {
                    _mover.HandleCollisionTurnAround();
                }

                if (IsServer)
                {
                    HandleServerTagLogic(otherPlayer);
                }
            }
        }

        private void HandleServerTagLogic(PlayerController otherPlayer)
        {
            var status = TagGameManager.Instance;
            if (status == null || !status.IsGameActive) return;

            ulong currentTaggerId = status.CurrentTaggerId;

            if (OwnerClientId == currentTaggerId || otherPlayer.OwnerClientId == currentTaggerId)
            {
                ulong newTaggerId = (OwnerClientId == currentTaggerId) ? otherPlayer.OwnerClientId : OwnerClientId;
                
                if (status is TagGameManager manager)
                {
                    manager.ReportTag(newTaggerId);
                }
            }
        }
    }
}