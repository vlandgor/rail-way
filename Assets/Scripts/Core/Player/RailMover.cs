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

        public RailNode CurrentNode { get; private set; }
        public bool IsMoving { get; private set; }

        public void SetStartNode(RailNode node)
        {
            CurrentNode = node;
            transform.position = node.Position;
        }

        public bool TryMove(Vector2Int inputDir)
        {
            if (IsMoving || CurrentNode == null)
                return false;

            Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] TryMove called");
            
            RailNode target = GetTargetNodeRelative(inputDir);
            if (target == null)
                return false;

            StartCoroutine(MoveRoutine(target));
            return true;
        }

        private RailNode GetTargetNodeRelative(Vector2Int inputDir)
        {
            if (inputDir == Vector2Int.zero)
                return null;

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
            Quaternion startRot = transform.rotation;
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);

            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * moveSpeed;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                transform.rotation = Quaternion.Slerp(startRot, targetRot, t * rotationSpeed);
                yield return null;
            }

            transform.position = endPos;
            transform.rotation = targetRot;

            CurrentNode = target;
            IsMoving = false;
        }
    }
}