using System;
using Game.Core.Player;
using Unity.Netcode;
using UnityEngine;

namespace Game.Player
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private LocalPlayer _localPlayer;
        
        public bool IsChaser { get; private set; }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            if (other.CompareTag("Player"))
            {
                PlayCollisionEffectsClientRpc();
            }
        }
        
        public void Spawn()
        {
            
        }
        
        [ClientRpc]
        private void PlayCollisionEffectsClientRpc()
        {
            // _localPlayer.TriggerVisualEffects();
        }
    }
}