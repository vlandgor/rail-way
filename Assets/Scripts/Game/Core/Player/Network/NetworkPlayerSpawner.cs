using System.Collections.Generic;
using Game.Core.Rail;
using Unity.Netcode;
using UnityEngine;

namespace Game.Core.Player.Network
{
    public class NetworkPlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private RailMap railMap;
        [SerializeField] private NetworkPlayer playerPrefab;
        
        private readonly Dictionary<ulong, NetworkPlayer> _players = new();
        
        public void SpawnPlayer(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;
        
            if (_players.ContainsKey(clientId))
                return;
        
            int spawnNodeId = (int)(clientId % (ulong)railMap.Links.Count);
            Vector3 spawnPosition = railMap.Links[spawnNodeId].WorldPosition;
        
            NetworkPlayer player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            NetworkObject netObj = player.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId);
        
            player.Initialize(spawnNodeId);
            
            _players.Add(clientId, player);
        }
        
        public void DespawnPlayer(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;
        
            if (!_players.TryGetValue(clientId, out NetworkPlayer player))
                return;
        
            NetworkObject netObj = player.GetComponent<NetworkObject>();
        
            if (netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn();
            }
            else
            {
                Destroy(player.gameObject);
            }
        
            _players.Remove(clientId);
        }
    }
}