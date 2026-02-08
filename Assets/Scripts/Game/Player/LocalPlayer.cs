using Game.Player.Behaviour;
using UnityEngine;

namespace Game.Player
{
    public class LocalPlayer : MonoBehaviour
    {
        [SerializeField] private LocalPlayerInput _localPlayerInput;
        [SerializeField] private LocalPlayerMovement _movement;
        [SerializeField] private Core.Rail.RailMap _railMap;
        
        [Header("Initialization")]
        [SerializeField] private int _startingStopPointId = 0;
        
        private int _currentStopPointId;

        private void Start()
        {
            InitializePosition();
            _localPlayerInput.OnDirectionInput += LocalPlayerInput_OnDirectionInput;
        }

        private void OnDestroy()
        {
            _localPlayerInput.OnDirectionInput -= LocalPlayerInput_OnDirectionInput;
        }

        private void InitializePosition()
        {
            if (_railMap == null)
                return;
            
            _currentStopPointId = _startingStopPointId;
            
            Vector3 startPosition = _railMap.GetStopPointPosition(_currentStopPointId);
            
            transform.position = startPosition;
            
            Vector3 initialDirection = _railMap.GetStopPointTangent(_currentStopPointId);
            if (initialDirection != Vector3.zero)
            {
                transform.forward = initialDirection;
            }
        }
        
        private void LocalPlayerInput_OnDirectionInput(Vector2Int direction)
        {
            if (_movement.IsMoving)
            {
                return;
            }
            
            var nextSegment = _railMap.GetNextSegment(_currentStopPointId, direction);
            
            if (nextSegment != null)
            {
                _movement.MoveAlongSegment(
                    nextSegment, 
                    _railMap.GetSplineContainer(), 
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