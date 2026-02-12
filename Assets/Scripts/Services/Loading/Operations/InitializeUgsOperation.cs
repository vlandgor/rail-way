using System;
using Cysharp.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;

namespace Services.Loading.Operations
{
    public class InitializeUgsOperation : ILoadingOperation
    {
        public string Description => "Initializing Unity Gaming Services (UGS)...";
        
        public async UniTask Execute(IProgress<float> progress)
        {
            progress?.Report(0.2f);
            
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                Debug.Log("[InitializeUgsOperation] UGS already initialized");
                progress?.Report(1f);
                return;
            }

            if (UnityServices.State == ServicesInitializationState.Initializing)
            {
                Debug.Log("[InitializeUgsOperation] UGS is initializing, waiting...");
                progress?.Report(0.5f);
                
                int maxWaitSeconds = 30;
                int waitedSeconds = 0;
                
                while (UnityServices.State == ServicesInitializationState.Initializing && waitedSeconds < maxWaitSeconds)
                {
                    await UniTask.Delay(100);
                    waitedSeconds++;
                }
                
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    throw new Exception($"UGS failed to initialize within {maxWaitSeconds} seconds. State: {UnityServices.State}");
                }
                
                progress?.Report(1f);
                return;
            }

            try
            {
                Debug.Log("[InitializeUgsOperation] Initializing UGS...");
                progress?.Report(0.5f);
                
                await UnityServices.InitializeAsync();
                
                Debug.Log("[InitializeUgsOperation] UGS initialized successfully");
                progress?.Report(1f);
            }
            catch (Exception e)
            {
                Debug.LogError($"[InitializeUgsOperation] Failed to initialize UGS: {e.Message}");
                throw;
            }
        }
    }
}