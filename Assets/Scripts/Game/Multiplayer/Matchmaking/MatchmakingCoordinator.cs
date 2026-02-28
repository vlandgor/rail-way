using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Multiplayer.Loading.Operations;
using Game.Multiplayer.Matchmaking.Providers;
using Services.Loading;
using Services.Loading.Operations;
using UnityEngine;

namespace Game.Multiplayer.Matchmaking
{
    public class MatchmakingCoordinator
    {
        private readonly IMatchmakingProvider _provider = new UnityLobbyProvider();
        
        private bool _isMatchmaking;

        public async UniTask StartMatchmaking()
        {
            if (_isMatchmaking) return;
            _isMatchmaking = true;

            SearchResult result = await _provider.FindMatchAsync();

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

        private async UniTask ProcessMatchResult(SearchResult result)
        {
            if (result.IsHost)
            {
                await _provider.UpdateLobbyStateAsync(result.LobbyId, "starting");
            }

            Queue<ILoadingOperation> operations = CreateMatchmakingOperations(result);
            
            _isMatchmaking = false;
            await LoadingService.Instance.Load(operations);
        }

        private Queue<ILoadingOperation> CreateMatchmakingOperations(SearchResult result)
        {
            Queue<ILoadingOperation> operations = new Queue<ILoadingOperation>();

            operations.Enqueue(new LoadSceneOperation("Game_Scene"));
            operations.Enqueue(new InitializeMultiplayerSession());
            operations.Enqueue(new StartNetworkOperation(result.IsHost));

            return operations;
        }

        public async UniTask CancelMatchmaking()
        {
            if (!_isMatchmaking) return;
            
            await _provider.CancelAsync();
            _isMatchmaking = false;
        }
    }
}