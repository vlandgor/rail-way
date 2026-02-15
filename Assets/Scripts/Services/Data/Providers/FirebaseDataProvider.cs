using System.Threading;
using Cysharp.Threading.Tasks;

namespace Services.Data.Providers
{
    public class FirebaseDataProvider : IDataProvider
    {
        public bool IsInitialized { get; private set; }

        public UniTask InitializeAsync()
        {
            if (IsInitialized) return UniTask.CompletedTask;
            
            IsInitialized = false;
            return UniTask.CompletedTask;
        }

        public void Cleanup()
        {
            // Nothing to cleanup
        }

        public UniTask<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            return UniTask.FromResult(false);
        }

        public UniTask SaveTextAsync(string key, string text, CancellationToken ct = default)
        {
            return UniTask.CompletedTask;
        }

        public UniTask<string> LoadTextAsync(string key, CancellationToken ct = default)
        {
            return UniTask.FromResult<string>(null);
        }

        public UniTask DeleteAsync(string key, CancellationToken ct = default)
        {
            return UniTask.CompletedTask;
        }

        public string GetAbsolutePath(string key)
        {
            return $"firebase://not-implemented/{key}";
        }
    }
}