using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Multiplayer.Loading.Operations;
using Game.Multiplayer.Matchmaking.Data;
using Game.Multiplayer.Matchmaking.Providers;
using Services.Loading;
using Services.Loading.Operations;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            }
        }
        
        public async void CancelMatchmakingSearch()
        {
            if (!_isMatchmaking) return;
            
            await _provider.CancelAsync();
            _isMatchmaking = false;
        }
        
        public IEnumerable<SearchPlayer> GetCurrentPlayers() => _players.Values;

        private async UniTask ProcessMatchResult(SearchResult result)
        {
            if (result.IsHost)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Game_Scene", LoadSceneMode.Single);
            }
            
            // Queue<ILoadingOperation> operations = new Queue<ILoadingOperation>();
            //
            // operations.Enqueue(new LoadSceneOperation("Game_Scene"));
            //
            // _isMatchmaking = false;
            // await LoadingService.Instance.Load(operations);
        }
        
        private void Provider_OnPlayerJoined(SearchPlayer player)
        {
            _players[player.playerId] = player;
            OnPlayerJoined?.Invoke(player);
        }

        private void Provider_OnPlayerLeft(string playerId)
        {
            _players.Remove(playerId);
            OnPlayerLeft?.Invoke(playerId);
        }
    }
}