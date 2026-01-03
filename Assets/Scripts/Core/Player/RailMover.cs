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

        private Coroutine _activeMoveRoutine;
        private RailNode _originNode; 
        private float _lastCollisionTime;

        public RailNode CurrentNode { get; private set; }
        public bool IsMoving { get; private set; }

        public override void OnNetworkSpawn()
        {
            if (_currentNodeId.Value != -1)
            {
                StartCoroutine(WaitAndSyncNode(_currentNodeId.Value));
            }

            _currentNodeId.OnValueChanged += (oldVal, newVal) => 
            {
                if (!IsOwner) StartCoroutine(WaitAndSyncNode(newVal));
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
                if(attempts > 100) yield break;
                yield return null; 
            }

            CurrentNode = builder.RuntimeNodes[id];
            
            if (!IsMoving)
            {
                transform.position = CurrentNode.Position;
            }
        }

        public void SetStartNode(RailNode node)
        {
            _currentNodeId.Value = node.Id;
            CurrentNode = node;
            transform.position = node.Position;
        }

        public bool TryMove(Vector2Int inputDir)
        {
            if (IsMoving || CurrentNode == null) return false;

            RailNode target = GetTargetNodeRelative(inputDir);
            if (target == null) return false;

            _originNode = CurrentNode; 
            _activeMoveRoutine = StartCoroutine(MoveRoutine(target));
            return true;
        }

        private IEnumerator MoveRoutine(RailNode target)
        {
            IsMoving = true;

            Vector3 startPos = transform.position;
            Vector3 endPos = target.Position;
            Vector3 moveDir = (endPos - startPos).normalized;
            
            float distance = Vector3.Distance(startPos, endPos);
            
            Quaternion targetRot = moveDir != Vector3.zero ? 
                Quaternion.LookRotation(moveDir, Vector3.up) : transform.rotation;

            float progress = 0f;
            while (progress < distance)
            {
                progress += Time.deltaTime * moveSpeed;
                float t = progress / distance;

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

        private RailNode GetTargetNodeRelative(Vector2Int inputDir)
        {
            if (inputDir == Vector2Int.zero) return null;
            
            Vector3 lookDir = transform.forward;
            Vector3 rightDir = transform.right;

            Vector3 worldDir = (lookDir * inputDir.y) + (rightDir * inputDir.x);
            worldDir.Normalize();

            return GetNodeByWorldDirection(worldDir, inputDir.y > 0);
        }

        private RailNode GetNodeByWorldDirection(Vector3 worldDir, bool isAttemptingForward)
        {
            if (isAttemptingForward && !HasSideConnections())
            {
                return null;
            }

            float bestDot = 0.5f;
            RailNode bestNode = null;

            TryCandidate(CurrentNode.Forward);
            TryCandidate(CurrentNode.Backward);
            TryCandidate(CurrentNode.Left);
            TryCandidate(CurrentNode.Right);

            return bestNode;

            void TryCandidate(RailNode node)
            {
                if (node == null) return;
                Vector3 dirToNode = (node.Position - CurrentNode.Position).normalized;
                float dot = Vector3.Dot(worldDir, dirToNode);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestNode = node;
                }
            }
        }

        private bool HasSideConnections()
        {
            return CurrentNode.Left != null || CurrentNode.Right != null;
        }

        [ServerRpc]
        private void UpdateNodeServerRpc(int newNodeId)
        {
            _currentNodeId.Value = newNodeId;
        }
    }
}