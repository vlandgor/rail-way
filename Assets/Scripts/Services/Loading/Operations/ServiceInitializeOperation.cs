using System;
using Cysharp.Threading.Tasks;

namespace Services.Loading.Operations
{
    public class ServiceInitializationOperation : ILoadingOperation
    {
        private readonly Func<IProgress<float>, UniTask> _initializeAction;
        private readonly string _serviceName;

        public string Description => $"Initializing {_serviceName}...";

        public ServiceInitializationOperation(string serviceName, Func<IProgress<float>, UniTask> initializeAction)
        {
            _serviceName = serviceName;
            _initializeAction = initializeAction;
        }

        public ServiceInitializationOperation(string serviceName, Func<UniTask> initializeAction)
        {
            _serviceName = serviceName;
            _initializeAction = async (progress) =>
            {
                await initializeAction();
                progress?.Report(1f);
            };
        }

        public async UniTask Execute(IProgress<float> progress)
        {
            await _initializeAction(progress);
        }
    }
}