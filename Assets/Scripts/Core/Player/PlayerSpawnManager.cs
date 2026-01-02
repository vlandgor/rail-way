using System.Collections.Generic;
using UnityEngine;
using Core.Rail;
using Unity.Netcode;

namespace Core.Player
{
    public class PlayerSpawnManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private RailSpawnPoints spawnPoints;
        [SerializeField] private RailMover playerPrefab;

        private readonly Dictionary<int, RailMover> _players = new();

        public void SpawnPlayer(ulong clientId)
        {
            if (_players.ContainsKey((int)clientId)) return;

            // Get a unique node for this client
            RailNode spawnNode = spawnPoints.GetSpawnNode((int)clientId);
            if (spawnNode == null) 
            {
                Debug.LogError($"[SpawnManager] No spawn node found for client {clientId}");
                return;
            }

            RailMover player = Instantiate(playerPrefab);
            
            // 1. SET THE DATA FIRST
            player.SetStartNode(spawnNode);

            // 2. THEN SPAWN ON NETWORK
            NetworkObject netObj = player.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId);

            _players.Add((int)clientId, player);
            Debug.Log($"[SpawnManager] Spawned Client {clientId} at Node {spawnNode.Id}");
        }

        public void DespawnPlayer(int playerId)
        {
            if (!_players.TryGetValue(playerId, out RailMover player)) return;

            NetworkObject netObj = player.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned) netObj.Despawn();
            else Destroy(player.gameObject);

            _players.Remove(playerId);
        }
    }
}