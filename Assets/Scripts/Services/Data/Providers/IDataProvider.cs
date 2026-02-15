using System.Threading;
using Cysharp.Threading.Tasks;

namespace Services.Data.Providers
{
    public interface IDataProvider
    {
        public bool IsInitialized { get; }
        public UniTask InitializeAsync();
        public void Cleanup();
        public UniTask<bool> ExistsAsync(string key, CancellationToken ct = default);
        public UniTask SaveTextAsync(string key, string text, CancellationToken ct = default);
        public UniTask<string> LoadTextAsync(string key, CancellationToken ct = default);
        public UniTask DeleteAsync(string key, CancellationToken ct = default);
        public string GetAbsolutePath(string key);
    }
}