using System;
using Cysharp.Threading.Tasks;
using Services.Loading.Operations;

namespace Meta.Loading.Operations
{
    public class InitializeMetaOperation : ILoadingOperation
    {
        public string Description => "Initializing Meta...";

        public async UniTask Execute(IProgress<float> progress)
        {
            await MetaManager.Instance.Initialize();
            progress?.Report(1f);
        }
    }
}