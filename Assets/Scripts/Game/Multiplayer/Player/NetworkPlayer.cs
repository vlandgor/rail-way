using Game.Core.Player.Avatar;
using Game.Core.Player.Behaviour;
using Game.Core.Player.Camera;
using Game.Core.Player.Input;
using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer.Player
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private NetworkObject _networkObject;
        
        [Space]
        [SerializeField] private PlayerCamera _playerCamera;
        [SerializeField] private PlayerAvatar _playerAvatar;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerInput _playerInput;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsOwner)
            {
                _playerCamera.gameObject.SetActive(true);
                _playerInput.enabled = true;
            }
            else
            {
                _playerCamera.gameObject.SetActive(false);
                _playerInput.enabled = false;
                _playerMovement.enabled = false;
            }
        }
    }
}