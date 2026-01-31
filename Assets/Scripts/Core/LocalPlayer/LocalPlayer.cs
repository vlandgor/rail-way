using System;
using UnityEngine;

namespace Core.LocalPlayer
{
    public class LocalPlayer : MonoBehaviour
    {
        [SerializeField] private LocalPlayerInput _localPlayerInput;

        private void Start()
        {
            _localPlayerInput.OnDirectionInput += LocalPlayerInput_OnDirectionInput;
        }

        private void OnDestroy()
        {
            _localPlayerInput.OnDirectionInput -= LocalPlayerInput_OnDirectionInput;
        }
        
        private void LocalPlayerInput_OnDirectionInput(Vector2Int direction)
        {
            Debug.Log(direction);
        }
    }
}