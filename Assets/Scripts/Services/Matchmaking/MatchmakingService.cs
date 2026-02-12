using System.Collections.Generic;
using System.Threading.Tasks;
using Services.Loading;
using Services.Utilities;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services.Matchmaking
{
    public class MatchmakingService : Singleton<MatchmakingService>
    {
        private Lobby _currentLobby;
        private float _heartbeatTimer;

        private const int MaxPlayers = 2;
        private const float HeartbeatInterval = 15f;
        
        public Lobby CurrentLobby => _currentLobby;

        public bool IsHostPlayer =>
            _currentLobby != null &&
            _currentLobby.HostId == AuthenticationService.Instance.PlayerId;

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
                    _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);

                    if (IsHost() && _currentLobby.Players.Count == MaxPlayers)
                    {
                        StartGame();
                        break;
                    }

                    if (!IsHost() &&
                        _currentLobby.Data != null &&
                        _currentLobby.Data.TryGetValue("state", out var state) &&
                        state.Value == "starting")
                    {
                        //LoadingService.Instance.LoadScene("Game_Scene");
                        break;
                    }
                }
                catch (LobbyServiceException e)
                {
                    if (e.Reason == LobbyExceptionReason.RateLimited)
                    {
                        await Task.Delay(2000); 
                    }
                    else
                    {
                        break;
                    }
                }

                await Task.Delay(1500); // Increased from 500ms
            }
        }


        private bool IsHost()
        {
            return _currentLobby != null &&
                   _currentLobby.HostId == AuthenticationService.Instance.PlayerId;
        }
        
        public void RefreshLobby(Lobby lobby)
        {
            _currentLobby = lobby;
        }

        // =========================
        // Game Start
        // =========================

        private async void StartGame()
        {
            int seed = Random.Range(int.MinValue, int.MaxValue);

            Debug.Log($"Host starting game with seed {seed}");

            var data = new Dictionary<string, DataObject>
            {
                {
                    "seed",
                    new DataObject(
                        DataObject.VisibilityOptions.Member,
                        seed.ToString())
                },
                {
                    "state",
                    new DataObject(
                        DataObject.VisibilityOptions.Member,
                        "starting")
                }
            };

            await LobbyService.Instance.UpdateLobbyAsync(
                _currentLobby.Id,
                new UpdateLobbyOptions { Data = data });

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
