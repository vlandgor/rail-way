using System.Threading.Tasks;
using Services.Matchmaking;
using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEngine;

namespace Core.Session
{
    public class SessionBootstrap : MonoBehaviour
    {
        [SerializeField] private Rail.RailGraphBuilder railGraphBuilder;

        private async void Start()
        {
            await InitializeSession();
        }

        private async Task InitializeSession()
        {
            var matchmaking = MatchmakingService.Instance;

            if (matchmaking == null)
            {
                Debug.LogError("MatchmakingService not found.");
                return;
            }

            Debug.Log($"RailGraphBuilder is null: {railGraphBuilder == null}");

            // 🔁 Wait for lobby + seed to be available
            var lobby = await WaitForSeed(matchmaking);

            if (lobby == null)
            {
                Debug.LogError("Failed to retrieve lobby seed.");
                return;
            }

            int seed = int.Parse(lobby.Data["seed"].Value);
            Debug.Log($"Game seed received: {seed}");

            // 1️⃣ Build deterministic level
            railGraphBuilder.SetSeed(seed);
            railGraphBuilder.Build();

            // 2️⃣ Start networking
            if (matchmaking.IsHostPlayer)
            {
                Debug.Log("Starting as Host");
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                Debug.Log("Starting as Client");
                NetworkManager.Singleton.StartClient();
            }
        }

        private async Task<Unity.Services.Lobbies.Models.Lobby> WaitForSeed(
            MatchmakingService matchmaking)
        {
            const int maxAttempts = 20;
            const int delayMs = 250;

            for (int i = 0; i < maxAttempts; i++)
            {
                var lobby = matchmaking.CurrentLobby;

                if (lobby != null &&
                    lobby.Data != null &&
                    lobby.Data.TryGetValue("seed", out var seedData))
                {
                    return lobby;
                }

                // 🔄 Re-fetch lobby to get latest data
                if (lobby != null)
                {
                    matchmaking.RefreshLobby(
                        await LobbyService.Instance.GetLobbyAsync(lobby.Id));
                }

                await Task.Delay(delayMs);
            }

            return null;
        }
    }
}
