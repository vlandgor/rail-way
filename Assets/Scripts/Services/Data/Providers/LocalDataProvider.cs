using System.IO;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Services.Data.Providers
{
    public class LocalDataProvider : IDataProvider
    {
        private const string Extension = ".json";

        public bool IsInitialized { get; private set; } = true; // Local provider is always ready

        public static string GetRootFolderPath()
        {
            return Application.persistentDataPath;
        }
        
        private static string MakePath(string key)
        {
            var safeKey = key.Replace("\\", "/");
            var path = Path.Combine(Application.persistentDataPath, safeKey) + Extension;
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return path;
        }

        public UniTask<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            return UniTask.FromResult(File.Exists(MakePath(key)));
        }

        public async UniTask SaveTextAsync(string key, string text, CancellationToken ct = default)
        {
            var path = MakePath(key);
            var bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
            
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await fs.WriteAsync(bytes, 0, bytes.Length, ct);
                await fs.FlushAsync(ct);
            }
            
#if UNITY_EDITOR
            Debug.Log($"[LocalDataProvider] Saved → {path}");
#endif
        }

        public async UniTask<string> LoadTextAsync(string key, CancellationToken ct = default)
        {
            var path = MakePath(key);
            if (!File.Exists(path)) return null;
            
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            using (var sr = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                return await sr.ReadToEndAsync();
            }
        }

        public UniTask DeleteAsync(string key, CancellationToken ct = default)
        {
            var path = MakePath(key);
            if (File.Exists(path))
            {
                File.Delete(path);
#if UNITY_EDITOR
                Debug.Log($"[LocalDataProvider] Deleted → {path}");
#endif
            }
            return UniTask.CompletedTask;
        }

        public string GetAbsolutePath(string key)
        {
            return MakePath(key);
        }
    }
}