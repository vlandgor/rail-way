using System;
using Cysharp.Threading.Tasks;
using Core.Rail;
using UnityEngine;
using System.Threading;

namespace Core.Player
{
    public class RailMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 12f;

        private KnotNode _currentNode;
        private KnotNode _originNode;
        private RailSplineMap _map;

        private CancellationTokenSource _moveCts;
        private float _lastCollisionTime;

        public bool IsMoving { get; private set; }

        private void Awake()
        {
            _map = FindFirstObjectByType<RailSplineMap>();
        }

        public void SyncNode(int nodeId)
        {
            if (_map == null || !_map.RuntimeNodes.TryGetValue(nodeId, out var node))
                return;

            _currentNode = node;

            if (!IsMoving)
            {
                transform.position = node.Position;
            }
        }

        public bool TryMove(Vector2Int inputDir, out int targetNodeId)
        {
            targetNodeId = -1;

            if (IsMoving || _currentNode == null)
                return false;

            targetNodeId = GetTargetId(inputDir);
            if (targetNodeId == -1)
                return false;

            _originNode = _currentNode;
            StartMove(_map.RuntimeNodes[targetNodeId]).Forget();
            return true;
        }

        private async UniTaskVoid StartMove(KnotNode target)
        {
            CancelMove();

            _moveCts = new CancellationTokenSource();
            CancellationToken token = _moveCts.Token;

            IsMoving = true;

            Vector3 endPos = target.Position;

            try
            {
                while (Vector3.Distance(transform.position, endPos) > 0.01f)
                {
                    token.ThrowIfCancellationRequested();

                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        endPos,
                        moveSpeed * Time.deltaTime
                    );

                    Vector3 moveDir = (endPos - transform.position).normalized;
                    if (moveDir != Vector3.zero)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                        transform.rotation = Quaternion.Slerp(
                            transform.rotation,
                            targetRot,
                            rotationSpeed * Time.deltaTime
                        );
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                transform.position = endPos;
                _currentNode = target;
            }
            catch (OperationCanceledException)
            {
                // expected on collision or reversal
            }
            finally
            {
                IsMoving = false;
                _moveCts?.Dispose();
                _moveCts = null;
            }
        }

        public void HandleCollisionTurnAround()
        {
            if (Time.time < _lastCollisionTime + 0.2f)
                return;

            _lastCollisionTime = Time.time;

            if (_originNode != null)
            {
                StartMove(_originNode).Forget();
            }
            else
            {
                CancelMove();
            }
        }

        private void CancelMove()
        {
            if (_moveCts == null)
                return;

            _moveCts.Cancel();
        }

        private int GetTargetId(Vector2Int inputDir)
        {
            Vector3 worldDir =
                (transform.forward * inputDir.y + transform.right * inputDir.x).normalized;

            float bestDot = 0.5f;
            int bestId = -1;

            CheckCandidate(_currentNode.ForwardId);
            CheckCandidate(_currentNode.BackwardId);
            CheckCandidate(_currentNode.LeftId);
            CheckCandidate(_currentNode.RightId);

            return bestId;

            void CheckCandidate(int id)
            {
                if (id == -1)
                    return;

                Vector3 toNode =
                    (_map.RuntimeNodes[id].Position - _currentNode.Position).normalized;

                float dot = Vector3.Dot(worldDir, toNode);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestId = id;
                }
            }
        }
    }
}
