using UnityEngine;

namespace Core.LocalPlayer
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
            Debug.Log("[LocalPlayer] Start called");
            InitializePosition();
            _localPlayerInput.OnDirectionInput += LocalPlayerInput_OnDirectionInput;
            Debug.Log("[LocalPlayer] Input event subscribed");
        }

        private void OnDestroy()
        {
            Debug.Log("[LocalPlayer] OnDestroy - unsubscribing from input");
            _localPlayerInput.OnDirectionInput -= LocalPlayerInput_OnDirectionInput;
        }

        private void InitializePosition()
        {
            Debug.Log($"[LocalPlayer] InitializePosition - Starting stop point: {_startingStopPointId}");
            
            if (_railMap == null)
            {
                Debug.LogError("[LocalPlayer] RailMap is not assigned!");
                return;
            }
            
            _currentStopPointId = _startingStopPointId;
            Debug.Log($"[LocalPlayer] Current stop point set to: {_currentStopPointId}");
            
            Vector3 startPosition = _railMap.GetStopPointPosition(_currentStopPointId);
            Debug.Log($"[LocalPlayer] Start position from RailMap: {startPosition}");
            
            transform.position = startPosition;
            Debug.Log($"[LocalPlayer] Player position set to: {transform.position}");
            
            Vector3 initialDirection = _railMap.GetStopPointTangent(_currentStopPointId);
            if (initialDirection != Vector3.zero)
            {
                transform.forward = initialDirection;
                Debug.Log($"[LocalPlayer] Player forward direction set to: {transform.forward}");
            }
            
            Debug.Log("[LocalPlayer] InitializePosition complete");
        }
        
        private void LocalPlayerInput_OnDirectionInput(Vector2Int direction)
        {
            Debug.Log($"[LocalPlayer] Direction input received: {direction}");
            
            if (_movement.IsMoving)
            {
                Debug.LogWarning("[LocalPlayer] Already moving, ignoring input");
                return;
            }
                
            Debug.Log($"[LocalPlayer] Requesting next segment from stop {_currentStopPointId} in direction {direction}");
            var nextSegment = _railMap.GetNextSegment(_currentStopPointId, direction);
            
            if (nextSegment != null)
            {
                Debug.Log($"[LocalPlayer] Next segment found: {nextSegment.StartStopPointId}->{nextSegment.EndStopPointId}");
                Debug.Log($"[LocalPlayer] Starting movement along segment");
                
                _movement.MoveAlongSegment(
                    nextSegment, 
                    _railMap.GetSplineContainer(), 
                    OnReachedStopPoint
                );
            }
            else
            {
                Debug.LogWarning($"[LocalPlayer] No valid segment found for direction {direction} from stop {_currentStopPointId}");
            }
        }
        
        private void OnReachedStopPoint(int stopPointId)
        {
            Debug.Log($"[LocalPlayer] Reached stop point: {stopPointId} (previous: {_currentStopPointId})");
            _currentStopPointId = stopPointId;
            Debug.Log($"[LocalPlayer] Current stop point updated to: {_currentStopPointId}");
        }
    }
}