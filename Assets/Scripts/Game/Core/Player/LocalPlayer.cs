using Game.Player.Behaviour;
using Game.Rail;
using UnityEngine;

namespace Game.Core.Player
{
    public class LocalPlayer : MonoBehaviour
    {
        [SerializeField] private LocalPlayerInput _localPlayerInput;
        [SerializeField] private LocalPlayerMovement _movement;
        [SerializeField] private RailGraph railGraph;
        
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
        
        private void LocalPlayerInput_OnDirectionInput(Vector2Int direction)
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