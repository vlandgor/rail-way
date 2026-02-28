using System;
using Cysharp.Threading.Tasks;
using Services.Loading.Operations;

namespace Game.Multiplayer.Loading.Operations
{
    public class StartNetworkOperation : ILoadingOperation
    {
        private readonly bool _isHost;
        public string Description => _isHost ? "Starting Host..." : "Joining Game...";

        public StartNetworkOperation(bool isHost) => _isHost = isHost;

        public async UniTask Execute(IProgress<float> progress)
        {
            progress.Report(0.5f);
            if (_isHost)
                Unity.Netcode.NetworkManager.Singleton.StartHost();
            else
                Unity.Netcode.NetworkManager.Singleton.StartClient();
        
            await UniTask.Yield();
            progress.Report(1f);
        }
    }
}