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
        
        private KnotNode _currentNode;
        private KnotNode _originNode; 
        private RailSplineMap _map;
        private Coroutine _activeMoveRoutine;
        private float _lastCollisionTime;

        public bool IsMoving { get; private set; }

        public override void OnNetworkSpawn()
        {
            _map = Object.FindFirstObjectByType<RailSplineMap>();

            if (_currentNodeId.Value != -1)
            {
                SyncNode(_currentNodeId.Value);
            }

            _currentNodeId.OnValueChanged += (oldVal, newVal) => 
            {
                SyncNode(newVal);
            };
        }

        private void SyncNode(int id)
        {
            if (_map == null || !_map.RuntimeNodes.TryGetValue(id, out var node)) return;

            _currentNode = node;
            
            if (!IsMoving)
            {
                transform.position = node.Position;
            }
        }

        public void SetStartNode(int nodeId)
        {
            _currentNodeId.Value = nodeId;
        }

        public bool TryMove(Vector2Int inputDir)
        {
            if (IsMoving || _currentNode == null) return false;

            int targetId = GetTargetId(inputDir);
            if (targetId == -1) return false;

            _originNode = _currentNode; 
            _activeMoveRoutine = StartCoroutine(MoveRoutine(_map.RuntimeNodes[targetId]));
            return true;
        }

        private IEnumerator MoveRoutine(KnotNode target)
        {
            IsMoving = true;

            Vector3 startPos = transform.position;
            Vector3 endPos = target.Position;
            
            float distance = Vector3.Distance(startPos, endPos);
            
            while (Vector3.Distance(transform.position, endPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, endPos, moveSpeed * Time.deltaTime);
                
                Vector3 moveDir = (endPos - transform.position).normalized;
                if (moveDir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                }
                yield return null;
            }

            transform.position = endPos;
            _currentNode = target;

            if (IsOwner)
            {
                UpdateNodeServerRpc(target.Id);
            }

            IsMoving = false;
            _activeMoveRoutine = null;
        }

        public void HandleCollisionTurnAround()
        {
            if (Time.time < _lastCollisionTime + 0.2f) return;
            _lastCollisionTime = Time.time;

            if (_activeMoveRoutine != null)
            {
                StopCoroutine(_activeMoveRoutine);
            }

            if (_originNode != null)
            {
                _activeMoveRoutine = StartCoroutine(MoveRoutine(_originNode));
            }
            else
            {
                IsMoving = false;
            }
        }

        private int GetTargetId(Vector2Int inputDir)
        {
            Vector3 worldDir = (transform.forward * inputDir.y + transform.right * inputDir.x).normalized;
            float bestDot = 0.5f;
            int bestId = -1;

            CheckCandidate(_currentNode.ForwardId);
            CheckCandidate(_currentNode.BackwardId);
            CheckCandidate(_currentNode.LeftId);
            CheckCandidate(_currentNode.RightId);

            return bestId;

            void CheckCandidate(int id)
            {
                if (id == -1) return;
                Vector3 toNode = (_map.RuntimeNodes[id].Position - _currentNode.Position).normalized;
                float dot = Vector3.Dot(worldDir, toNode);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestId = id;
                }
            }
        }

        [ServerRpc]
        private void UpdateNodeServerRpc(int newNodeId)
        {
            _currentNodeId.Value = newNodeId;
        }
    }
}