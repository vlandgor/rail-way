using Game.Core.Rail;
using UnityEngine;

namespace Game.Core.Player.Local
{
    public class LocalPlayerSpawner : MonoBehaviour
    {
        [SerializeField] private RailMap railMap;
        [SerializeField] private LocalPlayer _playerPrefab;

        private void Start()
        {
            SpawnPlayer();
        }

        public void SpawnPlayer()
        {
            int spawnNodeId = Random.Range(0, railMap.Links.Count);
            Vector3 spawnPosition = railMap.Links[spawnNodeId].WorldPosition;
        
            LocalPlayer player = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);
            player.Initialize(railMap, spawnNodeId);
        }
    }
}