using Game.Multiplayer.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Multiplayer.Session
{
    public class NetworkLifeCycleHandler : NetworkBehaviour
    {
        [SerializeField] private NetworkPlayerSpawner networkPlayerSpawner;

        public override void OnNetworkSpawn()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnect;

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

                if (NetworkManager.Singleton.IsServer)
                {
                    OnClientConnected(NetworkManager.ServerClientId);
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
            
        }

        private void OnDisconnect(ulong clientId)
        {
            if (clientId == NetworkManager.ServerClientId || clientId == NetworkManager.Singleton.LocalClientId)
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