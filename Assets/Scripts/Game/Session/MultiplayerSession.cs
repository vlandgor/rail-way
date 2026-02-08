using Core.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Session
{
    public class MultiplayerSession : NetworkBehaviour, IMultiplayerSession
    {
        [SerializeField] private PlayerSpawnManager playerSpawnManager;
        
        public static IMultiplayerSession Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

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
            if (Instance == (IMultiplayerSession)this) Instance = null;

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
            //playerSpawnManager.SpawnPlayer(clientId);
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