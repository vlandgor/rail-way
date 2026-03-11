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
        
        private RailGraph _railGraph;
        
        public int CurrentStopPointId { get; private set; }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
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
            SetStopPointId(startPointId);

            if (IsServer)
            {
                InitializeClientRpc(startPointId);
            }
        }

        [ClientRpc]
        private void InitializeClientRpc(int startPointId)
        {
            if (IsServer) return;

            RailGraph graph = FindFirstObjectByType<RailGraph>(); 
            _railGraph = graph;
            SetStopPointId(startPointId);
        }

        public void SetStopPointId(int stopPointId)
        {
            CurrentStopPointId = stopPointId;
        }
        
        private void PlayerInput_OnDirectionInput(Vector2Int direction)
        {
            if (_playerMovement.IsMoving || _railGraph == null)
            {
                return;
            }

            var nextSegment = _railGraph.GetNextSegment(CurrentStopPointId, direction);
            
            if (nextSegment != null)
            {
                _playerMovement.MoveAlongSegment(
                    nextSegment, 
                    _railGraph.GetSplineContainer(), 
                    OnReachedStopPoint
                );

                MoveRequestServerRpc(direction);
            }
        }

        [ServerRpc]
        private void MoveRequestServerRpc(Vector2Int direction)
        {
            var nextSegment = _railGraph.GetNextSegment(CurrentStopPointId, direction);
            if (nextSegment != null)
            {
                _playerMovement.MoveAlongSegment(
                    nextSegment, 
                    _railGraph.GetSplineContainer(), 
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