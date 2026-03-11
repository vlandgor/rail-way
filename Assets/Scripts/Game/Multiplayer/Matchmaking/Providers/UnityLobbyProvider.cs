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
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
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
                    
                    await InitializeClientNetwork();
                }
                catch
                {
                    _currentLobby = await CreateLobby(player);
                    await InitializeHostNetwork();
                }

                await SubscribeToEvents(_currentLobby.Id);
                StartHeartbeat(_cts.Token).Forget();
                
                return await WaitForMatchReady(_cts.Token);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return new SearchResult { Success = false };
            }
        }

        private async UniTask InitializeHostNetwork()
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(_maxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            await UpdateLobbyWithRelay(joinCode);
            NetworkManager.Singleton.StartHost();
        }

        private async UniTask InitializeClientNetwork()
        {
            string joinCode = null;
            
            while (string.IsNullOrEmpty(joinCode))
            {
                _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);
                if (_currentLobby.Data != null && _currentLobby.Data.TryGetValue("RelayCode", out var code))
                {
                    joinCode = code.Value;
                }
                else
                {
                    await UniTask.Delay(1000);
                }
            }

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            
            transport.SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
        }

        private async UniTask<SearchResult> WaitForMatchReady(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (NetworkManager.Singleton.ConnectedClients.Count >= _maxPlayers)
                {
                    return new SearchResult 
                    { 
                        Success = true, 
                        IsHost = NetworkManager.Singleton.IsHost, 
                        LobbyId = _currentLobby.Id 
                    };
                }

                await UniTask.Delay(1000, cancellationToken: ct);
            }

            return new SearchResult { Success = false };
        }

        public async UniTask UpdateLobbyStateAsync(string lobbyId, string state)
        {
            var data = new Dictionary<string, DataObject>
            {
                { "state", new DataObject(DataObject.VisibilityOptions.Member, state) }
            };
            await LobbyService.Instance.UpdateLobbyAsync(lobbyId, new UpdateLobbyOptions { Data = data });
        }

        private async UniTask UpdateLobbyWithRelay(string joinCode)
        {
            var data = new Dictionary<string, DataObject>
            {
                { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
            };

            await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions { Data = data });
        }

        private Unity.Services.Lobbies.Models.Player CreateLocalPlayer()
        {
            return new Unity.Services.Lobbies.Models.Player(
                id: AccountService.Instance.PlayerId,
                data: new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, AccountService.Instance.PlayerName) }
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
            string name = player.Data != null && player.Data.TryGetValue("PlayerName", out var data) ? data.Value : "Unknown";
            return new SearchPlayer { playerId = player.Id, playerName = name };
        }

        public async UniTask CancelAsync()
        {
            _cts?.Cancel();
            if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
            if (_currentLobby != null)
            {
                try { await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, AccountService.Instance.PlayerId); } catch { }
            }
            if (_lobbyEvents != null) await _lobbyEvents.UnsubscribeAsync();
            _currentLobby = null;
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
            _cts?.Dispose();
            if (_lobbyEvents != null) _lobbyEvents.UnsubscribeAsync();
        }
    }
}