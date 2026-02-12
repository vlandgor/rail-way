using System;
using Cysharp.Threading.Tasks;
using Services.Account;

namespace Services.Loading.Operations
{
    public class AuthorizationOperation : ILoadingOperation
    {
        public string Description => "Checking authorization...";

        public async UniTask Execute(IProgress<float> progress)
        {
            progress?.Report(0.3f);

            bool autoAuthSuccess = await AccountService.Instance.TryAutoAuthorize();

            if (autoAuthSuccess)
            {
                progress?.Report(1f);
                return;
            }

            progress?.Report(0.5f);
            
            await AccountService.Instance.RequireAuthorization();
            
            progress?.Report(1f);
        }
    }
}