using Game.Core.Player.Network;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Multiplayer.Session
{
    public class NetworkLifeCycleHandler : NetworkBehaviour
    {
        [SerializeField] private NetworkPlayerSpawner _networkPlayerSpawner;

        public override void OnNetworkSpawn()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnect;

            Debug.Log("<color=green>OnNetworkSpawn</color>");
            
            if (IsServer)
            {
                Debug.Log("<color=yellow>OnNetworkSpawn</color>");
                
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    OnClientConnected(client.ClientId);
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnect;
                
                if (IsServer)
                {
                    NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                }
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"<color=green>Client {clientId} connected</color>");
            _networkPlayerSpawner.SpawnPlayer(clientId);
        }

        private void OnDisconnect(ulong clientId)
        {
            if (clientId == NetworkManager.ServerClientId || (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId))
            {
                LeaveSession();
            }
        }

        public void LeaveSession()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }
            SceneManager.LoadScene("Menu_Scene");
        }
    }
}