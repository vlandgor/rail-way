using System;
using Cysharp.Threading.Tasks;
using Services.Loading.Operations;

namespace Game.Multiplayer.Loading.Operations
{
    public class InitializeMultiplayerSession : ILoadingOperation
    {
        public string Description =>  "Initialize Multiplayer Session";

        public InitializeMultiplayerSession()
        {
            
        }
        
        public async UniTask Execute(IProgress<float> progress)
        {
            
        }
    }
}