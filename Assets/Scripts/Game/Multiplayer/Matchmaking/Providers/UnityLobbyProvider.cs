using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Multiplayer.Matchmaking.Data;
using Services.Account;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Game.Multiplayer.Matchmaking.Providers
{
    public class UnityLobbyProvider : IMatchmakingProvider
    {
        public event Action<SearchPlayer> OnPlayerJoined;
        public event Action<string> OnPlayerLeft;

        private Lobby _currentLobby;
        private ILobbyEvents _lobbyEvents;
        private CancellationTokenSource _cts;
        private int _maxPlayers;

        private readonly Dictionary<int, string> _indexToIdMap = new();

        public async UniTask<SearchResult> FindMatchAsync(int maxPlayers)
        {
            _maxPlayers = maxPlayers;
            
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                Unity.Services.Lobbies.Models.Player player = CreateLocalPlayer();

                try
                {
                    QuickJoinLobbyOptions joinOptions = new() { Player = player };
                    _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(joinOptions);
                }
                catch
                {
                    _currentLobby = await CreateLobby(player);
                }

                await SubscribeToEvents(_currentLobby.Id);
                StartHeartbeat(_cts.Token).Forget();
                
                return await WaitForMatchReady(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                return new SearchResult { Success = false };
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return new SearchResult { Success = false };
            }
        }

        private async UniTask<SearchResult> WaitForMatchReady(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);

                bool isHost = _currentLobby.HostId == AccountService.Instance.PlayerId;

                if (isHost && _currentLobby.Players.Count == _maxPlayers)
                {
                    string joinCode = await CreateRelayAllocation();
                    await UpdateLobbyWithRelay(joinCode);

                    return new SearchResult 
                    { 
                        Success = true, 
                        IsHost = true, 
                        LobbyId = _currentLobby.Id,
                        RelayJoinCode = joinCode
                    };
                }

                if (!isHost && _currentLobby.Data != null && 
                    _currentLobby.Data.TryGetValue("RelayCode", out var code) && 
                    !string.IsNullOrEmpty(code.Value))
                {
                    return new SearchResult 
                    { 
                        Success = true, 
                        IsHost = false, 
                        LobbyId = _currentLobby.Id,
                        RelayJoinCode = code.Value
                    };
                }

                await UniTask.Delay(1500, cancellationToken: ct);
            }

            return new SearchResult { Success = false };
        }

        private async UniTask<string> CreateRelayAllocation()
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(_maxPlayers);
            return await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }

        private async UniTask UpdateLobbyWithRelay(string joinCode)
        {
            var data = new Dictionary<string, DataObject>
            {
                { "state", new DataObject(DataObject.VisibilityOptions.Member, "starting") },
                { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
            };

            await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions { Data = data });
        }

        private Unity.Services.Lobbies.Models.Player CreateLocalPlayer()
        {
            string nameToSend = AccountService.Instance.PlayerName;
            return new Unity.Services.Lobbies.Models.Player(
                id: AccountService.Instance.PlayerId,
                data: new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, nameToSend) }
                }
            );
        }

        private async UniTask SubscribeToEvents(string lobbyId)
        {
            _indexToIdMap.Clear();
    
            foreach (var p in _currentLobby.Players)
            {
                _indexToIdMap[_currentLobby.Players.IndexOf(p)] = p.Id;
                OnPlayerJoined?.Invoke(MapToSearchPlayer(p));
            }

            LobbyEventCallbacks callbacks = new LobbyEventCallbacks();
            callbacks.PlayerJoined += (players) =>
            {
                foreach (LobbyPlayerJoined changes in players)
                {
                    if (!_indexToIdMap.ContainsValue(changes.Player.Id))
                    {
                        _indexToIdMap[changes.PlayerIndex] = changes.Player.Id;
                        OnPlayerJoined?.Invoke(MapToSearchPlayer(changes.Player));
                    }
                }
            };

            callbacks.PlayerLeft += (playerIndices) =>
            {
                foreach (int index in playerIndices)
                {
                    if (_indexToIdMap.TryGetValue(index, out string playerId))
                    {
                        OnPlayerLeft?.Invoke(playerId);
                        _indexToIdMap.Remove(index);
                    }
                }
            };

            _lobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyId, callbacks);
        }

        private SearchPlayer MapToSearchPlayer(Unity.Services.Lobbies.Models.Player player)
        {
            string name = "Unknown";
            if (player.Data != null && player.Data.TryGetValue("PlayerName", out var data))
            {
                name = data.Value;
            }

            return new SearchPlayer { playerId = player.Id, playerName = name };
        }

        public async UniTask CancelAsync()
        {
            _cts?.Cancel();
            if (_currentLobby != null)
            {
                try
                {
                    await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, AccountService.Instance.PlayerId);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex.Message);
                }
            }
            if (_lobbyEvents != null)
            {
                await _lobbyEvents.UnsubscribeAsync();
                _lobbyEvents = null;
            }
            _currentLobby = null;
        }

        public async UniTask UpdateLobbyStateAsync(string lobbyId, string state)
        {
            var data = new Dictionary<string, DataObject>
            {
                { "state", new DataObject(DataObject.VisibilityOptions.Member, state) }
            };
            await LobbyService.Instance.UpdateLobbyAsync(lobbyId, new UpdateLobbyOptions { Data = data });
        }

        private async UniTask<Lobby> CreateLobby(Unity.Services.Lobbies.Models.Player player)
        {
            CreateLobbyOptions options = new() { IsPrivate = false, Player = player };
            return await LobbyService.Instance.CreateLobbyAsync("QuickMatch", _maxPlayers, options);
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

        public void Dispose()
        {
            _cts?.Cancel();
            _lobbyEvents?.UnsubscribeAsync();
        }
    }
}