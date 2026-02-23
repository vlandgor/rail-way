using System;
 using Cysharp.Threading.Tasks;
 
 namespace Services.Loading.Operations
 {
     /// <summary>
     /// Generic delay operation for testing or artificial delays
     /// </summary>
     public class DelayOperation : ILoadingOperation
     {
         private readonly float _delaySeconds;
         private readonly string _customDescription;
 
         public string Description => _customDescription ?? $"Processing...";
 
         public DelayOperation(float delaySeconds, string customDescription = null)
         {
             _delaySeconds = delaySeconds;
             _customDescription = customDescription;
         }
 
         public async UniTask Execute(IProgress<float> progress)
         {
             float elapsed = 0f;
 
             while (elapsed < _delaySeconds)
             {
                 elapsed += UnityEngine.Time.deltaTime;
                 progress?.Report(elapsed / _delaySeconds);
                 await UniTask.Yield();
             }
 
             progress?.Report(1f);
         }
     }
 }