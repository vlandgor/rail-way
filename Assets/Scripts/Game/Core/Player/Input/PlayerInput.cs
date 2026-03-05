using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Core.Player.Input
{
    public class PlayerInput : MonoBehaviour
    {
        public event Action<Vector2Int> OnDirectionInput;
        
        [Header("Input Settings")]
        [SerializeField] private float swipeThreshold = 50f;
        
        private UnityEngine.InputSystem.PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction pointerPositionAction;
        private InputAction pointerPressAction;
        
        private Vector2 swipeStartPosition;
        private bool isSwipeStarted;
        
        private void Awake()
        {
            playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
            
            // Get references to actions
            moveAction = playerInput.actions["Move"];
            pointerPositionAction = playerInput.actions["PointerPosition"];
            pointerPressAction = playerInput.actions["PointerPress"];
            
            // Subscribe to events
            moveAction.performed += HandleMoveInput;
            pointerPressAction.started += OnPointerStart;
            pointerPressAction.canceled += OnPointerEnd;
        }
        
        private void OnEnable()
        {
            moveAction?.Enable();
            pointerPositionAction?.Enable();
            pointerPressAction?.Enable();
        }
        
        private void OnDisable()
        {
            moveAction?.Disable();
            pointerPositionAction?.Disable();
            pointerPressAction?.Disable();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            moveAction.performed -= HandleMoveInput;
            pointerPressAction.started -= OnPointerStart;
            pointerPressAction.canceled -= OnPointerEnd;
        }
        
        // Keyboard/Gamepad Input
        private void HandleMoveInput(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            
            // Determine direction based on strongest axis
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                // Horizontal movement
                OnDirectionInput?.Invoke(new Vector2Int(input.x > 0 ? 1 : -1, 0));
            }
            else if (input.y != 0)
            {
                // Vertical movement
                OnDirectionInput?.Invoke(new Vector2Int(0, input.y > 0 ? 1 : -1));
            }
        }
        
        // Pointer Input (Touch/Mouse)
        private void OnPointerStart(InputAction.CallbackContext context)
        {
            swipeStartPosition = pointerPositionAction.ReadValue<Vector2>();
            isSwipeStarted = true;
        }
        
        private void OnPointerEnd(InputAction.CallbackContext context)
        {
            if (!isSwipeStarted) return;
            
            Vector2 swipeEndPosition = pointerPositionAction.ReadValue<Vector2>();
            DetectSwipe(swipeStartPosition, swipeEndPosition);
            
            isSwipeStarted = false;
        }
        
        private void DetectSwipe(Vector2 startPos, Vector2 endPos)
        {
            Vector2 swipeDelta = endPos - startPos;
            
            // Check if swipe is long enough
            if (swipeDelta.magnitude < swipeThreshold)
                return;
            
            // Determine swipe direction based on angle
            float angle = Mathf.Atan2(swipeDelta.y, swipeDelta.x) * Mathf.Rad2Deg;
            
            // Normalize angle to 0-360
            if (angle < 0) angle += 360;
            
            // Determine direction based on angle
            // Right: 315-45, Up: 45-135, Left: 135-225, Down: 225-315
            if (angle >= 315 || angle < 45)
            {
                OnDirectionInput?.Invoke(new Vector2Int(1, 0)); // Right
            }
            else if (angle >= 45 && angle < 135)
            {
                OnDirectionInput?.Invoke(new Vector2Int(0, 1)); // Forward/Up
            }
            else if (angle >= 135 && angle < 225)
            {
                OnDirectionInput?.Invoke(new Vector2Int(-1, 0)); // Left
            }
            else if (angle >= 225 && angle < 315)
            {
                OnDirectionInput?.Invoke(new Vector2Int(0, -1)); // Back/Down
            }
        }
    }
}