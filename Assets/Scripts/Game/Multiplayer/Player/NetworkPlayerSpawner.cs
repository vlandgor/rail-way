using System.Collections.Generic;
using Game.Core.Player.Customization;
using Game.Core.Player.Meta;
using Game.Core.Rail;
using Game.Rail;
using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer.Player
{
    public class NetworkPlayerSpawner : NetworkBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private RailGraph railGraph;
        [SerializeField] private NetworkPlayer playerPrefab;
        
        private readonly Dictionary<ulong, NetworkPlayer> _players = new();
        
        public void SpawnPlayer(ulong clientId, IRemovePlayerMetaData playerMetaData, IRemotePlayerCustomizationData playerCustomizationData)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;
        
            if (_players.ContainsKey(clientId))
                return;
        
            int spawnNodeId = (int)(clientId % (ulong)railGraph.StopPoints.Count);
        
            NetworkPlayer player = Instantiate(playerPrefab);
        
            NetworkObject netObj = player.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId);
            //player.Spawn(spawnNodeId);
        
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