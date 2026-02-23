using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Services.Data.Providers;
using UnityEngine;

namespace Services.Data
{
    public enum DataProviderType
    {
        Local,
        UGS,
        Firebase
    }

    public class DataService : BaseService<DataService>
    {
        [SerializeField] private DataProviderType _providerType = DataProviderType.Local;
        
        [Space]
        [SerializeField] private bool prettyPrint = true;
        
        private IDataProvider _provider;
        
        public bool IsInitialized => _provider?.IsInitialized ?? false;

        private void OnDestroy()
        {
            _provider?.Cleanup();
        }

        public override async UniTask Initialize()
        {
            if (IsInitialized)
            {
                return;
            }
            
            await base.Initialize();
            await InitializeProvider();
        }

        private async UniTask InitializeProvider()
        {
            switch (_providerType)
            {
                case DataProviderType.Local:
                    _provider = new LocalDataProvider();
                    break;
                
                case DataProviderType.UGS:
                    _provider = new UgsDataProvider();
                    break;
                
                case DataProviderType.Firebase:
                    _provider = new FirebaseDataProvider();
                    break;
                
                default:
                    _provider = new LocalDataProvider();
                    break;
            }
            
            await _provider.InitializeAsync();
        }

#if UNITY_EDITOR
        [ContextMenu("Open Saving Folder")]
        private void OpenFolderFromInspector()
        {
            if (_providerType != DataProviderType.Local)
            {
                return;
            }

            string folderPath = LocalDataProvider.GetRootFolderPath();
            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = folderPath,
                    UseShellExecute = true
                });
            }
        }
#endif

        public UniTask<bool> ExistsAsync(string key, CancellationToken ct = default)
            => _provider?.ExistsAsync(key, ct) ?? UniTask.FromResult(false);

        public async UniTask SaveJsonAsync<T>(string key, T data, CancellationToken ct = default)
        {
            if (_provider == null || data == null) return;
            
            var json = JsonUtility.ToJson(data, prettyPrint);
            await _provider.SaveTextAsync(key, json, ct);
        }

        public async UniTask<(bool success, T data)> TryLoadJsonAsync<T>(string key, CancellationToken ct = default)
        {
            if (_provider == null) return (false, default);

            var json = await _provider.LoadTextAsync(key, ct);
            if (string.IsNullOrEmpty(json)) return (false, default);

            try
            {
                var data = JsonUtility.FromJson<T>(json);
                return (!Equals(data, default(T)), data);
            }
            catch (Exception)
            {
                return (false, default);
            }
        }

        public async UniTask<T> LoadJsonOrDefaultAsync<T>(string key, T @default = default, CancellationToken ct = default)
        {
            var (ok, data) = await TryLoadJsonAsync<T>(key, ct);
            return ok ? data : @default;
        }

        public UniTask DeleteAsync(string key, CancellationToken ct = default)
            => _provider?.DeleteAsync(key, ct) ?? UniTask.CompletedTask;

        public string GetAbsolutePath(string key) => _provider?.GetAbsolutePath(key);
    }
}