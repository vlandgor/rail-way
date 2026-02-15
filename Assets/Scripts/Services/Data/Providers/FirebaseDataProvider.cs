using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Services.Data.Providers
{
    public class FirebaseDataProvider : IDataProvider
    {
        public bool IsInitialized { get; private set; }

        public async UniTask InitializeAsync()
        {
            // TODO: Initialize Firebase
            Debug.LogWarning("[FirebaseDataProvider] Not yet implemented");
            await UniTask.Yield();
            IsInitialized = false;
        }

        public UniTask<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            Debug.LogWarning("[FirebaseDataProvider] Not yet implemented");
            return UniTask.FromResult(false);
        }

        public UniTask SaveTextAsync(string key, string text, CancellationToken ct = default)
        {
            Debug.LogWarning("[FirebaseDataProvider] Not yet implemented");
            return UniTask.CompletedTask;
        }

        public UniTask<string> LoadTextAsync(string key, CancellationToken ct = default)
        {
            Debug.LogWarning("[FirebaseDataProvider] Not yet implemented");
            return UniTask.FromResult<string>(null);
        }

        public UniTask DeleteAsync(string key, CancellationToken ct = default)
        {
            Debug.LogWarning("[FirebaseDataProvider] Not yet implemented");
            return UniTask.CompletedTask;
        }

        public string GetAbsolutePath(string key)
        {
            return $"firebase://not-implemented/{key}";
        }
    }
}