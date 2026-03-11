using Game.Core.Player.Avatar;
using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer.Player
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private PlayerAvatar _playerAvatar;
    }
}