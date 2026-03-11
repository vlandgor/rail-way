using System;
using Cysharp.Threading.Tasks;
using Services.Loading.Operations;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Game.Multiplayer.Loading.Operations
{
    public class StartNetworkOperation : ILoadingOperation
    {
        private readonly bool _isHost;
        private readonly string _joinCode;

        public string Description => _isHost ? "Starting Host via Relay..." : "Joining Game via Relay...";

        public StartNetworkOperation(bool isHost, string joinCode)
        {
            _isHost = isHost;
            _joinCode = joinCode;
        }

        public async UniTask Execute(IProgress<float> progress)
        {
            progress.Report(0.1f);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            try
            {
                if (_isHost)
                {
                    Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
                    
                    transport.SetHostRelayData(
                        allocation.RelayServer.IpV4,
                        (ushort)allocation.RelayServer.Port,
                        allocation.AllocationIdBytes,
                        allocation.Key,
                        allocation.ConnectionData
                    );

                    progress.Report(0.5f);
                    NetworkManager.Singleton.StartHost();
                }
                else
                {
                    JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(_joinCode);

                    transport.SetClientRelayData(
                        joinAllocation.RelayServer.IpV4,
                        (ushort)joinAllocation.RelayServer.Port,
                        joinAllocation.AllocationIdBytes,
                        joinAllocation.Key,
                        joinAllocation.ConnectionData,
                        joinAllocation.HostConnectionData
                    );

                    progress.Report(0.5f);
                    NetworkManager.Singleton.StartClient();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Relay error: {e.Message}");
            }

            await UniTask.Yield();
            progress.Report(1f);
        }
    }
}