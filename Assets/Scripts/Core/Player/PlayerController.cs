using Core.Session;
using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(RailMover))]
    public class PlayerController : NetworkBehaviour
    {
        // [Header("Refs")]
        // [SerializeField] private PlayerInput _input;
        // [SerializeField] private RailMover _mover;
        // [SerializeField] private GameObject _cameraObject;
        // [SerializeField] private MeshRenderer _meshRenderer;
        //
        // private readonly NetworkVariable<int> _currentNodeId = new(-1);
        //
        // public override void OnNetworkSpawn()
        // {
        //     if (_cameraObject != null)
        //         _cameraObject.SetActive(IsOwner);
        //
        //     _currentNodeId.OnValueChanged += (_, newId) =>
        //     {
        //         _mover.SyncNode(newId);
        //     };
        // }
        //
        // private void Update()
        // {
        //     UpdateVisuals();
        //
        //     if (!IsOwner || !_input.HasInput)
        //         return;
        //
        //     var status = TagGameManager.Instance;
        //     if (status == null || !status.IsGameActive)
        //         return;
        //
        //     if (status.IsTagCooldownActive && status.CurrentTaggerId == OwnerClientId)
        //         return;
        //
        //     if (_mover.TryMove(_input.MoveDirection, out int targetNodeId))
        //     {
        //         MoveRequestServerRpc(targetNodeId);
        //     }
        // }
        //
        // public void SetStartNode(int nodeId)
        // {
        //     if (!IsServer)
        //         return;
        //
        //     _currentNodeId.Value = nodeId;
        // }
        //
        //
        // private void UpdateVisuals()
        // {
        //     ITagGameStatus status = TagGameManager.Instance;
        //     if (status == null || _meshRenderer == null)
        //         return;
        //
        //     if (status.IsTagCooldownActive && status.CurrentTaggerId == OwnerClientId)
        //     {
        //         _meshRenderer.material.color = Color.white;
        //     }
        //     else
        //     {
        //         bool isTagger = status.CurrentTaggerId == OwnerClientId;
        //         _meshRenderer.material.color = isTagger ? Color.red : Color.blue;
        //     }
        // }
        //
        // private void OnTriggerEnter(Collider other)
        // {
        //     if (!other.TryGetComponent(out PlayerController otherPlayer))
        //         return;
        //
        //     // local movement response
        //     if (IsOwner && _mover.IsMoving)
        //     {
        //         _mover.HandleCollisionTurnAround();
        //     }
        //
        //     // server-only tag logic
        //     if (IsServer)
        //     {
        //         HandleServerTagLogic(otherPlayer);
        //     }
        // }
        //
        // private void HandleServerTagLogic(PlayerController otherPlayer)
        // {
        //     ITagGameStatus status = TagGameManager.Instance;
        //     if (status == null || !status.IsGameActive)
        //         return;
        //
        //     ulong currentTaggerId = status.CurrentTaggerId;
        //
        //     if (OwnerClientId == currentTaggerId || otherPlayer.OwnerClientId == currentTaggerId)
        //     {
        //         ulong newTaggerId =
        //             OwnerClientId == currentTaggerId
        //                 ? otherPlayer.OwnerClientId
        //                 : OwnerClientId;
        //
        //         if (status is TagGameManager manager)
        //         {
        //             manager.ReportTag(newTaggerId);
        //         }
        //     }
        // }
        //
        // [ServerRpc]
        // private void MoveRequestServerRpc(int targetNodeId)
        // {
        //     _currentNodeId.Value = targetNodeId;
        // }
    }
}
