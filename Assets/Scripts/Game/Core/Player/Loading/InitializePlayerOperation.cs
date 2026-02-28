using System;
using Cysharp.Threading.Tasks;
using Game.Core.Player.Customization;
using Game.Core.Player.Meta;
using Services.Loading.Operations;

namespace Game.Core.Player.Loading
{
    public class InitializePlayerOperation : ILoadingOperation
    {
        public string Description => "Initialize player...";
        
        public async UniTask Execute(IProgress<float> progress)
        {
            progress?.Report(0.3f);
            await PlayerMeta.Instance.Initialize();
            
            progress?.Report(0.6f);
            await PlayerCustomization.Instance.Initialize();
            
            progress?.Report(1f);
        }
    }
}