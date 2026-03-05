using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Multiplayer.Loading.Operations;
using Game.Multiplayer.Matchmaking.Data;
using Game.Multiplayer.Matchmaking.Providers;
using Services.Loading;
using Services.Loading.Operations;
using UnityEngine;

namespace Game.Multiplayer.Matchmaking
{
    public class MatchmakingSearch : MonoBehaviour
    {
        public event Action<SearchPlayer> OnPlayerJoined;
        public event Action<string> OnPlayerLeft;
        
        private readonly IMatchmakingProvider _provider = new UnityLobbyProvider();
        
        private Dictionary<string, SearchPlayer> _players = new();
        
        private bool _isMatchmaking;
        
        public int MaxPlayers { get; private set; }

        private void Start()
        {
            _provider.OnPlayerJoined += Provider_OnPlayerJoined;
            _provider.OnPlayerLeft += Provider_OnPlayerLeft;
        }

        private void OnDestroy()
        {
            _provider.OnPlayerJoined -= Provider_OnPlayerJoined;
            _provider.OnPlayerLeft -= Provider_OnPlayerLeft;
        }

        public async void StartMatchmakingSearch(int maxPlayers)
        {
            if (_isMatchmaking) return;
            _isMatchmaking = true;
            MaxPlayers = maxPlayers;

            SearchResult result = await _provider.FindMatchAsync(maxPlayers);

            if (result.Success)
            {
                await ProcessMatchResult(result);
            }
            else
            {
                _isMatchmaking = false;
                Debug.LogWarning("Matchmaking failed or was cancelled.");
            }
        }
        
        public async void CancelMatchmakingSearch()
        {
            if (!_isMatchmaking) return;
            
            await _provider.CancelAsync();
            _isMatchmaking = false;
        }

        private async UniTask ProcessMatchResult(SearchResult result)
        {
            if (result.IsHost)
            {
                await _provider.UpdateLobbyStateAsync(result.LobbyId, "starting");
            }

            Queue<ILoadingOperation> operations = new Queue<ILoadingOperation>();

            operations.Enqueue(new LoadSceneOperation("Game_Scene"));
            operations.Enqueue(new InitializeMultiplayerSession());
            operations.Enqueue(new StartNetworkOperation(result.IsHost));
            
            _isMatchmaking = false;
            await LoadingService.Instance.Load(operations);
        }
        
        private void Provider_OnPlayerJoined(SearchPlayer player)
        {
            _players.TryAdd(player.playerId, player);
            
            OnPlayerJoined?.Invoke(player);
        }

        private void Provider_OnPlayerLeft(string playerId)
        {
            _players.Remove(playerId);
            
            OnPlayerLeft?.Invoke(playerId);
        }
    }
}