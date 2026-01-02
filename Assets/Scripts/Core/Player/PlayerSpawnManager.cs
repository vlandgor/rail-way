using System.Collections.Generic;
using System.Linq;
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

            RailNode spawnNode = spawnPoints.GetSpawnNode((int)clientId);

            if (spawnNode == null) return;

            RailMover player = Instantiate(playerPrefab);
            
            player.SetStartNode(spawnNode);

            NetworkObject netObj = player.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId);

            _players.Add((int)clientId, player);
        }

        public void DespawnPlayer(int playerId)
        {
            if (!_players.TryGetValue(playerId, out RailMover player))
            {
                return;
            }

            NetworkObject netObj = player.GetComponent<NetworkObject>();
            
            if (netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn(); 
            }
            else
            {
                Destroy(player.gameObject);
            }

            _players.Remove(playerId);
        }

        public void SyncPlayers(IReadOnlyList<int> activePlayerIds)
        {
            List<int> toRemove = new();

            foreach (int existingId in _players.Keys)
            {
                if (!activePlayerIds.Contains(existingId))
                    toRemove.Add(existingId);
            }

            foreach (int id in toRemove)
                DespawnPlayer(id);

            foreach (int id in activePlayerIds)
            {
                if (!_players.ContainsKey(id))
                    SpawnPlayer((ulong)id);
            }
        }

        public RailMover GetPlayer(int playerId)
        {
            _players.TryGetValue(playerId, out RailMover player);
            return player;
        }

        public void DespawnAll()
        {
            foreach (int id in _players.Keys.ToList())
            {
                DespawnPlayer(id);
            }

            _players.Clear();
        }
    }
}