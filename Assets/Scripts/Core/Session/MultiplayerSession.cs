using Core.Player;
using Unity.Netcode;
using UnityEngine;

namespace Core.Session
{
    public class MultiplayerSession : NetworkBehaviour
    {
        [SerializeField] private PlayerSpawnManager playerSpawnManager;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

                // Handle the Host/Server itself if it's already connected
                if (NetworkManager.Singleton.IsServer)
                {
                    OnClientConnected(NetworkManager.ServerClientId);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            playerSpawnManager.SpawnPlayer(clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            playerSpawnManager.DespawnPlayer((int)clientId);
        }
    }
}