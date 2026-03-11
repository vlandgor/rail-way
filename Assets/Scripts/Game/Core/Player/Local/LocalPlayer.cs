using Game.Core.Player.Avatar;
using Game.Core.Player.Behaviour;
using Game.Core.Player.Camera;
using Game.Core.Player.Input;
using Game.Core.Rail;
using UnityEngine;

namespace Game.Core.Player.Local
{
    public class LocalPlayer : MonoBehaviour
    {
        [SerializeField] private PlayerCamera _playerCamera;
        [SerializeField] private PlayerAvatar _playerAvatar;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerInput _playerInput;
        
        private RailGraph _railGraph;
        
        public int CurrentStopPointId { get; private set; }

        private void Start()
        {
            _playerInput.OnDirectionInput += PlayerInputOnDirectionInput;
        }

        private void OnDestroy()
        {
            _playerInput.OnDirectionInput -= PlayerInputOnDirectionInput;
        }

        public void Initialize(RailGraph railGraph, int startPointId)
        {
            _railGraph = railGraph;
            CurrentStopPointId  = startPointId;
        }
        
        private void PlayerInputOnDirectionInput(Vector2Int direction)
        {
            if (_playerMovement.IsMoving)
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
            }
        }
        
        private void OnReachedStopPoint(int stopPointId)
        {
            CurrentStopPointId = stopPointId;
        }
    }
}