using Cysharp.Threading.Tasks;

namespace Game.Multiplayer.Matchmaking.Providers
{
    public interface IMatchmakingProvider
    {
        public UniTask<SearchResult> FindMatchAsync();
        public UniTask CancelAsync();
        public UniTask UpdateLobbyStateAsync(string lobbyId, string state);
        public void Dispose();
    }
}