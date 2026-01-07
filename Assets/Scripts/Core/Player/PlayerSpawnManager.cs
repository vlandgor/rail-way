using System.Collections.Generic;
using UnityEngine;
using Core.Rail;
using Unity.Netcode;

namespace Core.Player
{
    public class PlayerSpawnManager : MonoBehaviour
    {
        [Header("Dependencies")] 
        [SerializeField] private RailSplineMap _railSplineMap;
        [SerializeField] private RailMover playerPrefab;

        private readonly Dictionary<int, RailMover> _players = new();

        public void SpawnPlayer(ulong clientId)
        {
            if (_players.ContainsKey((int)clientId)) return;

            int spawnNodeId = (int)clientId % _railSplineMap.RuntimeNodes.Count;

            RailMover player = Instantiate(playerPrefab);
            player.SetStartNode(spawnNodeId);

            NetworkObject netObj = player.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId);
            _players.Add((int)clientId, player);
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