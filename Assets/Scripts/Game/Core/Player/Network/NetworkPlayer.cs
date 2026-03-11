using Cysharp.Threading.Tasks;
using Game.Core.Player.Avatar;
using Game.Core.Player.Behaviour;
using Game.Core.Player.Camera;
using Game.Core.Player.Input;
using Game.Core.Rail;
using Unity.Netcode;
using UnityEngine;

namespace Game.Core.Player.Network
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private NetworkObject _networkObject;
        
        [Space]
        [SerializeField] private PlayerCamera _playerCamera;
        [SerializeField] private PlayerAvatar _playerAvatar;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerInput _playerInput;
        
        private RailMap _railMap;

        public int CurrentStopPointId;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            EnsureRailGraphConnection();
            
            if (IsOwner)
            {
                _playerCamera.gameObject.SetActive(true);
                _playerInput.enabled = true;
                _playerInput.OnDirectionInput += PlayerInput_OnDirectionInput;
            }
            else
            {
                _playerCamera.gameObject.SetActive(false);
                _playerInput.enabled = false;
                _playerMovement.enabled = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (IsOwner)
            {
                _playerInput.OnDirectionInput -= PlayerInput_OnDirectionInput;
            }
        }

        public void Initialize(int startPointId)
        {
            CurrentStopPointId = startPointId;

            if (IsServer)
            {
                InitializeClientRpc(startPointId);
            }
        }

        [ClientRpc]
        private void InitializeClientRpc(int startPointId)
        {
            CurrentStopPointId = startPointId;
            EnsureRailGraphConnection();
        }

        private void EnsureRailGraphConnection()
        {
            if (_railMap == null)
            {
                _railMap = FindFirstObjectByType<RailMap>();
                
                if (_railMap == null)
                {
                    Debug.LogWarning($"[NetworkPlayer] RailGraph not found in scene yet for Player {OwnerClientId}");
                }
            }
        }
        
        private void PlayerInput_OnDirectionInput(Vector2Int direction)
        {
            EnsureRailGraphConnection();

            if (_railMap == null || _playerMovement.IsMoving)
            {
                return;
            }

            var nextSegment = _railMap.GetNextSegment(CurrentStopPointId, direction, transform.forward);
            
            if (nextSegment != null)
            {
                _playerMovement.MoveAlongSegment(
                    nextSegment, 
                    _railMap.Container, 
                    OnReachedStopPoint
                );
            }
        }
        
        private void OnReachedStopPoint(int stopPointId)
        {
            CurrentStopPointId = stopPointId;
        }
    }
}