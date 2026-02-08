using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    public class PlayerSpawnManager : MonoBehaviour
    {
        // [Header("Dependencies")]
        // [SerializeField] private RailSplineMap _railSplineMap;
        // [SerializeField] private PlayerController playerPrefab;
        //
        // private readonly Dictionary<ulong, PlayerController> _players = new();
        //
        // public void SpawnPlayer(ulong clientId)
        // {
        //     if (!NetworkManager.Singleton.IsServer)
        //         return;
        //
        //     if (_players.ContainsKey(clientId))
        //         return;
        //
        //     int spawnNodeId = (int)(clientId % (ulong)_railSplineMap.RuntimeNodes.Count);
        //
        //     PlayerController player = Instantiate(playerPrefab);
        //
        //     NetworkObject netObj = player.GetComponent<NetworkObject>();
        //     netObj.SpawnAsPlayerObject(clientId);
        //     player.SetStartNode(spawnNodeId);
        //
        //     _players.Add(clientId, player);
        // }
        //
        // public void DespawnPlayer(ulong clientId)
        // {
        //     if (!NetworkManager.Singleton.IsServer)
        //         return;
        //
        //     if (!_players.TryGetValue(clientId, out PlayerController player))
        //         return;
        //
        //     NetworkObject netObj = player.GetComponent<NetworkObject>();
        //
        //     if (netObj != null && netObj.IsSpawned)
        //     {
        //         netObj.Despawn();
        //     }
        //     else
        //     {
        //         Destroy(player.gameObject);
        //     }
        //
        //     _players.Remove(clientId);
        // }
    }
}