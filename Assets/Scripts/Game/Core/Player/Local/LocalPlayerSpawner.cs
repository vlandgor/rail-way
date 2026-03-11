using Game.Core.Rail;
using UnityEngine;

namespace Game.Core.Player.Local
{
    public class LocalPlayerSpawner : MonoBehaviour
    {
        [SerializeField] private RailGraph _railGraph;
        [SerializeField] private LocalPlayer _playerPrefab;

        private void Start()
        {
            SpawnPlayer();
        }

        public void SpawnPlayer()
        {
            int spawnNodeId = Random.Range(0, _railGraph.StopPoints.Count);
            Vector3 spawnPosition = _railGraph.StopPoints[spawnNodeId].WorldPosition;
        
            LocalPlayer player = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);
            player.Initialize(_railGraph, spawnNodeId);
        }
    }
}