using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Multiplayer.Session.Data;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Game.Multiplayer.Session
{
    public class MultiplayerSession : MonoBehaviour
    {
        // //[SerializeField] private Rail.RailSplineMap railSplineMap;
        //
        // private MultiplayerSessionData _sessionData;
        //
        // private async void Start()
        // {
        //     await InitializeSession();
        // }
        //
        // public async UniTask Initialize(MultiplayerSessionData sessionData)
        // {
        //     _sessionData = sessionData;
        // }
        //
        // private async UniTask InitializeSession()
        // {
        //     var matchmaking = multiplayerMatchmaking;
        //
        //     if (matchmaking == null)
        //     {
        //         Debug.LogError("MatchmakingService not found.");
        //         return;
        //     }
        //
        //     Lobby lobby = await WaitForLobbyData(matchmaking);
        //
        //     if (lobby == null)
        //     {
        //         Debug.LogError("Failed to retrieve lobby data.");
        //         NetworkManager.Singleton.StartHost();
        //         return;
        //     }
        //
        //     if (matchmaking.IsHostPlayer)
        //     {
        //         Debug.Log("Starting as Host");
        //         NetworkManager.Singleton.StartHost();
        //     }
        //     else
        //     {
        //         Debug.Log("Starting as Client");
        //         NetworkManager.Singleton.StartClient();
        //     }
        // }
        //
        // private async UniTask<Lobby> WaitForLobbyData(MultiplayerMatchmaking multiplayerMatchmaking)
        // {
        //     const int maxAttempts = 20;
        //     const int delayMs = 250;
        //
        //     for (int i = 0; i < maxAttempts; i++)
        //     {
        //         Lobby lobby = multiplayerMatchmaking.CurrentLobby;
        //
        //         if (lobby != null)
        //         {
        //             return lobby;
        //         }
        //
        //         await Task.Delay(delayMs);
        //     }
        //
        //     return null;
        // }
    }
}