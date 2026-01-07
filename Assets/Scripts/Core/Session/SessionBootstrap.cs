using System.Threading.Tasks;
using Services.Matchmaking;
using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEngine;

namespace Core.Session
{
    public class SessionBootstrap : MonoBehaviour
    {
        [SerializeField] private Rail.RailSplineMap railSplineMap;

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

            var lobby = await WaitForLobbyData(matchmaking);

            if (lobby == null)
            {
                Debug.LogError("Failed to retrieve lobby data.");
                return;
            }

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

        private async Task<Unity.Services.Lobbies.Models.Lobby> WaitForLobbyData(
            MatchmakingService matchmaking)
        {
            const int maxAttempts = 20;
            const int delayMs = 250;

            for (int i = 0; i < maxAttempts; i++)
            {
                var lobby = matchmaking.CurrentLobby;

                if (lobby != null)
                {
                    return lobby;
                }

                await Task.Delay(delayMs);
            }

            return null;
        }
    }
}