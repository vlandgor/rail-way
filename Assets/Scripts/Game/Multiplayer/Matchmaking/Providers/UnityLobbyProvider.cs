using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Services.Account;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Game.Multiplayer.Matchmaking.Providers
{
    public class UnityLobbyProvider : IMatchmakingProvider
    {
        private Lobby _currentLobby;
        private CancellationTokenSource _cts;
        private const int MaxPlayers = 2;

        public async UniTask<SearchResult> FindMatchAsync()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                try
                {
                    _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                }
                catch
                {
                    _currentLobby = await CreateLobby();
                }

                StartHeartbeat(_cts.Token).Forget();
                return await WaitForMatchReady(_cts.Token);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Lobby Matchmaking failed: {ex.Message}");
                return new SearchResult { Success = false };
            }
        }
        
        public UniTask CancelAsync()
        {
            _cts?.Cancel();
            _currentLobby = null;
            return UniTask.CompletedTask;
        }
        
        public async UniTask UpdateLobbyStateAsync(string lobbyId, string state)
        {
            var data = new Dictionary<string, DataObject>
            {
                { "state", new DataObject(DataObject.VisibilityOptions.Member, state) }
            };

            await LobbyService.Instance.UpdateLobbyAsync(lobbyId, new UpdateLobbyOptions { Data = data });
        }

        public void Dispose() => _cts?.Cancel();

        private async UniTask<Lobby> CreateLobby()
        {
            var options = new CreateLobbyOptions { IsPrivate = false };
            return await LobbyService.Instance.CreateLobbyAsync("QuickMatch", MaxPlayers, options);
        }

        private async UniTaskVoid StartHeartbeat(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _currentLobby != null)
            {
                if (_currentLobby.HostId == AccountService.Instance.PlayerId)
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
                }
                await UniTask.Delay(TimeSpan.FromSeconds(15), cancellationToken: ct);
            }
        }

        private async UniTask<SearchResult> WaitForMatchReady(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);

                bool isHost = _currentLobby.HostId == AccountService.Instance.PlayerId;

                if (isHost && _currentLobby.Players.Count == MaxPlayers)
                {
                    return new SearchResult { Success = true, IsHost = true, LobbyId = _currentLobby.Id };
                }

                if (!isHost && _currentLobby.Data != null && 
                    _currentLobby.Data.TryGetValue("state", out var state) && state.Value == "starting")
                {
                    return new SearchResult { Success = true, IsHost = false, LobbyId = _currentLobby.Id };
                }

                await UniTask.Delay(1500, cancellationToken: ct);
            }

            return new SearchResult { Success = false };
        }
    }
}