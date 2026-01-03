using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services.Matchmaking
{
    public class MatchmakingService : MonoBehaviour
    {
        public static MatchmakingService Instance { get; private set; }

        private Lobby _currentLobby;
        private float _heartbeatTimer;

        private const int MaxPlayers = 2;
        private const float HeartbeatInterval = 15f;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (_currentLobby == null || !IsHost())
                return;

            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer <= 0f)
            {
                _heartbeatTimer = HeartbeatInterval;
                _ = LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            }
        }

        // =========================
        // Public API
        // =========================

        public async Task PlayAsync()
        {
            await EnsureServicesReady();

            try
            {
                _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                Debug.Log("Joined lobby");
            }
            catch
            {
                _currentLobby = await CreateLobby();
                Debug.Log("Created lobby");
            }

            _heartbeatTimer = HeartbeatInterval;
            _ = ListenForLobbyUpdates();
        }

        // =========================
        // Lobby Logic
        // =========================

        private async Task<Lobby> CreateLobby()
        {
            var options = new CreateLobbyOptions
            {
                IsPrivate = false
            };

            return await LobbyService.Instance.CreateLobbyAsync(
                "QuickMatch",
                MaxPlayers,
                options);
        }

        private async Task ListenForLobbyUpdates()
        {
            while (_currentLobby != null)
            {
                try
                {
                    _currentLobby =
                        await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);

                    Debug.Log($"Lobby players: {_currentLobby.Players.Count}");

                    if (_currentLobby.Players.Count == MaxPlayers)
                    {
                        if (IsHost())
                            StartGame();

                        break;
                    }
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogWarning($"Lobby error: {e.Message}");
                    break;
                }

                await Task.Delay(1000);
            }
        }

        private bool IsHost()
        {
            return _currentLobby != null &&
                   _currentLobby.HostId == AuthenticationService.Instance.PlayerId;
        }

        // =========================
        // Game Start
        // =========================

        private void StartGame()
        {
            Debug.Log("Match ready. Starting game.");
            SceneManager.LoadScene("Game_Scene");
        }

        // =========================
        // Unity Services
        // =========================

        private async Task EnsureServicesReady()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
}
