using Game.Core.Player.Avatar;
using Game.Core.Player.Behaviour;
using Game.Core.Player.Input;
using Game.Core.Rail;
using UnityEngine;

namespace Game.Core.Player.Local
{
    public class LocalPlayer : MonoBehaviour
    {
        [SerializeField] private PlayerAvatar _playerAvatar;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private RailGraph railGraph;
        
        [Header("Initialization")]
        [SerializeField] private int _startingStopPointId = 0;
        
        private int _currentStopPointId;

        private void Start()
        {
            InitializePosition();
            playerInput.OnDirectionInput += PlayerInputOnDirectionInput;
        }

        private void OnDestroy()
        {
            playerInput.OnDirectionInput -= PlayerInputOnDirectionInput;
        }
        
        private void InitializePosition()
        {
            if (railGraph == null)
                return;
            
            _currentStopPointId = _startingStopPointId;
            
            Vector3 startPosition = railGraph.GetStopPointPosition(_currentStopPointId);
            
            transform.position = startPosition;
            
            Vector3 initialDirection = railGraph.GetStopPointTangent(_currentStopPointId);
            if (initialDirection != Vector3.zero)
            {
                transform.forward = initialDirection;
            }
        }
        
        private void PlayerInputOnDirectionInput(Vector2Int direction)
        {
            if (_movement.IsMoving)
            {
                return;
            }
            
            var nextSegment = railGraph.GetNextSegment(_currentStopPointId, direction);
            
            if (nextSegment != null)
            {
                _movement.MoveAlongSegment(
                    nextSegment, 
                    railGraph.GetSplineContainer(), 
                    OnReachedStopPoint
                );
            }
        }
        
        private void OnReachedStopPoint(int stopPointId)
        {
            _currentStopPointId = stopPointId;
        }
    }
}