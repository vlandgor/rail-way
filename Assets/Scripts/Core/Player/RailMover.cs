using System;
using Cysharp.Threading.Tasks;
using Core.Rail;
using UnityEngine;
using System.Threading;
using UnityEngine.Splines;

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

        private RailSegment _activeSegment;
        private float _currentT;

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
                transform.position = node.Position;
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
            KnotNode target = _map.RuntimeNodes[targetNodeId];

            _activeSegment = _map.GetSegment(_currentNode, target);
            StartMoveAlongSpline(_activeSegment).Forget();

            _currentNode = target;
            return true;
        }

        private async UniTaskVoid StartMoveAlongSpline(RailSegment segment)
        {
            CancelMove();

            _moveCts = new CancellationTokenSource();
            var token = _moveCts.Token;

            IsMoving = true;

            _currentT = segment.StartT;
            float dir = Mathf.Sign(segment.EndT - segment.StartT);

            try
            {
                while ((dir > 0 && _currentT < segment.EndT) ||
                       (dir < 0 && _currentT > segment.EndT))
                {
                    token.ThrowIfCancellationRequested();

                    float delta =
                        moveSpeed * Time.deltaTime / segment.Spline.GetLength();

                    _currentT += dir * delta;
                    _currentT = Mathf.Clamp(
                        _currentT,
                        Mathf.Min(segment.StartT, segment.EndT),
                        Mathf.Max(segment.StartT, segment.EndT)
                    );

                    Vector3 pos = segment.Spline.EvaluatePosition(_currentT);
                    Vector3 tangent = ((Vector3)segment.Spline.EvaluateTangent(_currentT)).normalized;

                    transform.position = pos;

                    if (tangent != Vector3.zero)
                    {
                        Quaternion rot = Quaternion.LookRotation(tangent, Vector3.up);
                        transform.rotation = Quaternion.Slerp(
                            transform.rotation,
                            rot,
                            rotationSpeed * Time.deltaTime
                        );
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                transform.position = segment.Spline.EvaluatePosition(segment.EndT);
            }
            catch (OperationCanceledException) { }
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
                var reverse = _map.GetSegment(_currentNode, _originNode);
                StartMoveAlongSpline(reverse).Forget();
            }
            else
            {
                CancelMove();
            }
        }

        private void CancelMove()
        {
            _moveCts?.Cancel();
        }

        private int GetTargetId(Vector2Int inputDir)
        {
            Vector3 worldDir =
                (transform.forward * inputDir.y + transform.right * inputDir.x).normalized;

            float bestDot = 0.5f;
            int bestId = -1;

            Check(_currentNode.ForwardId);
            Check(_currentNode.BackwardId);
            Check(_currentNode.LeftId);
            Check(_currentNode.RightId);

            return bestId;

            void Check(int id)
            {
                if (id == -1) return;

                Vector3 to =
                    (_map.RuntimeNodes[id].Position - _currentNode.Position).normalized;

                float dot = Vector3.Dot(worldDir, to);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestId = id;
                }
            }
        }
    }
}
