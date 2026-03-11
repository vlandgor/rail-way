using System;
using Cysharp.Threading.Tasks;
using Game.Multiplayer.Matchmaking.Data;

namespace Game.Multiplayer.Matchmaking.Providers
{
    public interface IMatchmakingProvider : IDisposable
    {
        public event Action<SearchPlayer> OnPlayerJoined;
        public event Action<string> OnPlayerLeft;
        public UniTask<SearchResult> FindMatchAsync(int maxPlayers);
        public UniTask CancelAsync();
        public UniTask UpdateLobbyStateAsync(string lobbyId, string state);
    }
}