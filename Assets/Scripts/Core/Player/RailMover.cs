using System.Collections;
using Core.Rail;
using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    public class RailMover : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 12f;

        private readonly NetworkVariable<int> _currentNodeId = new NetworkVariable<int>(-1);

        public RailNode CurrentNode { get; private set; }
        public bool IsMoving { get; private set; }

        public override void OnNetworkSpawn()
        {
            Debug.Log($"[RailMover] Spawned. ID in NetVar: {_currentNodeId.Value}. IsOwner: {IsOwner}");

            if (_currentNodeId.Value != -1)
            {
                StartCoroutine(WaitAndSyncNode(_currentNodeId.Value));
            }

            _currentNodeId.OnValueChanged += (oldVal, newVal) => 
            {
                StartCoroutine(WaitAndSyncNode(newVal));
            };
        }

        private IEnumerator WaitAndSyncNode(int id)
        {
            RailGraphBuilder builder = null;
            int attempts = 0;

            while (builder == null || builder.RuntimeNodes == null || !builder.RuntimeNodes.ContainsKey(id))
            {
                builder = Object.FindFirstObjectByType<RailGraphBuilder>();
                attempts++;
                if(attempts > 100) 
                {
                    Debug.LogError($"[RailMover] Failed to find Node {id} after 100 frames!");
                    yield break;
                }
                yield return null; 
            }

            CurrentNode = builder.RuntimeNodes[id];
            
            if (!IsMoving)
            {
                transform.position = CurrentNode.Position;
            }
            
            Debug.Log($"[RailMover] Successfully synced to Node {id} (Name: {CurrentNode.name})");
        }

        public void SetStartNode(RailNode node)
        {
            _currentNodeId.Value = node.Id;
            CurrentNode = node;
            transform.position = node.Position;
            Debug.Log($"[RailMover] Server set start node to {node.Id}");
        }

        public bool TryMove(Vector2Int inputDir)
        {
            if (IsMoving || CurrentNode == null)
            {
                Debug.LogWarning($"[RailMover] Move denied. IsMoving: {IsMoving}, CurrentNode: {CurrentNode}");
                return false;
            }

            RailNode target = GetTargetNodeRelative(inputDir);
            if (target == null) return false;

            StartCoroutine(MoveRoutine(target));
            return true;
        }

        private RailNode GetTargetNodeRelative(Vector2Int inputDir)
        {
            if (inputDir == Vector2Int.zero) return null;
            Vector3 worldDir = transform.forward * inputDir.y + transform.right * inputDir.x;
            worldDir.y = 0f;
            worldDir.Normalize();
            return GetNodeByWorldDirection(worldDir);
        }

        private RailNode GetNodeByWorldDirection(Vector3 worldDir)
        {
            float bestDot = 0.5f;
            RailNode bestNode = null;

            TryCandidate(CurrentNode.Forward, Vector3.forward);
            TryCandidate(CurrentNode.Backward, Vector3.back);
            TryCandidate(CurrentNode.Left, Vector3.left);
            TryCandidate(CurrentNode.Right, Vector3.right);

            return bestNode;

            void TryCandidate(RailNode node, Vector3 nodeDir)
            {
                if (node == null) return;
                float dot = Vector3.Dot(worldDir, nodeDir);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestNode = node;
                }
            }
        }

        private IEnumerator MoveRoutine(RailNode target)
        {
            IsMoving = true;

            Vector3 startPos = transform.position;
            Vector3 endPos = target.Position;
            Vector3 moveDir = (endPos - startPos).normalized;
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * moveSpeed;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, t * rotationSpeed);
                yield return null;
            }

            transform.position = endPos;
            transform.rotation = targetRot;
            CurrentNode = target;

            if (IsOwner)
            {
                UpdateNodeServerRpc(target.Id);
            }

            IsMoving = false;
        }

        [ServerRpc]
        private void UpdateNodeServerRpc(int newNodeId)
        {
            _currentNodeId.Value = newNodeId;
        }
    }
}