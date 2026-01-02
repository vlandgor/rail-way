using UnityEngine;

namespace Core.Rail
{
    public class RailSpawnPoints : MonoBehaviour
    {
        [SerializeField] private RailGraphBuilder graphBuilder;

        public RailNode GetSpawnNode(int playerIndex)
        {
            int nodeCount = graphBuilder.RuntimeNodes.Count;

            if (nodeCount == 0)
            {
                Debug.LogError("RailSpawnPoints: No runtime nodes available.");
                return null;
            }

            int nodeId = playerIndex % nodeCount;
            return graphBuilder.RuntimeNodes[nodeId];
        }
    }
}