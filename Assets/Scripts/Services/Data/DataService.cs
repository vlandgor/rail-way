using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Services.Data.Providers;
using Services.Utilities;
using UnityEngine;

namespace Services.Data
{
    public enum DataProviderType
    {
        Local,
        UGS,
        Firebase
    }

    public class DataService : Singleton<DataService>
    {
        [SerializeField] private DataProviderType providerType = DataProviderType.Local;
        
        [Space]
        [SerializeField] private bool prettyPrint = true;
        
        private IDataProvider _provider;
        
        public bool IsInitialized => _provider?.IsInitialized ?? false;

        public override async UniTask Initialize()
        {
            await base.Initialize();
            await InitializeProviderAsync();
        }

        private async UniTask InitializeProviderAsync()
        {
            if (IsInitialized) return;

            switch (providerType)
            {
                case DataProviderType.Local:
                    _provider = new LocalDataProvider();
                    break;

                case DataProviderType.UGS:
                    var ugsProvider = new UnityCloudDataProvider();
                    await ugsProvider.InitializeAsync();
                    _provider = ugsProvider;
                    break;

                case DataProviderType.Firebase:
                    var firebaseProvider = new FirebaseDataProvider();
                    await firebaseProvider.InitializeAsync();
                    _provider = firebaseProvider;
                    break;
            }

            Debug.Log($"[DataService] Initialized with {providerType} provider");
        }

#if UNITY_EDITOR
        [ContextMenu("Open Saving Folder")]
        private void OpenFolderFromInspector()
        {
            if (providerType != DataProviderType.Local)
            {
                Debug.LogWarning($"[DataService] 'Open Folder' only works with Local provider");
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
            else
            {
                Debug.LogWarning($"[DataService] Save folder does not exist: {folderPath}");
            }
        }
#endif

        public void SetProvider(IDataProvider provider) => _provider = provider;

        public UniTask<bool> ExistsAsync(string key, CancellationToken ct = default)
            => _provider != null ? _provider.ExistsAsync(key, ct) : UniTask.FromResult(false);

        public async UniTask SaveJsonAsync<T>(string key, T data, CancellationToken ct = default)
        {
            if (_provider == null) 
            { 
                Debug.LogWarning("[DataService] No provider set."); 
                return; 
            }
            if (data == null) 
            { 
                Debug.LogWarning("[DataService] SaveJsonAsync called with null data."); 
                return; 
            }
            
            var json = JsonUtility.ToJson(data, prettyPrint);
            await _provider.SaveTextAsync(key, json, ct);
        }

        public async UniTask<(bool success, T data)> TryLoadJsonAsync<T>(string key, CancellationToken ct = default)
        {
            if (_provider == null)
            {
                Debug.LogWarning("[DataService] No provider set.");
                return (false, default);
            }

            var json = await _provider.LoadTextAsync(key, ct);
            if (string.IsNullOrEmpty(json)) return (false, default);

            try
            {
                var data = JsonUtility.FromJson<T>(json);
                return (!Equals(data, default(T)), data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataService] Failed to parse JSON for key '{key}': {e}");
                return (false, default);
            }
        }

        public async UniTask<T> LoadJsonOrDefaultAsync<T>(string key, T @default = default, CancellationToken ct = default)
        {
            var (ok, data) = await TryLoadJsonAsync<T>(key, ct);
            return ok ? data : @default;
        }

        public UniTask DeleteAsync(string key, CancellationToken ct = default)
            => _provider != null ? _provider.DeleteAsync(key, ct) : UniTask.CompletedTask;

        public string GetAbsolutePath(string key) => _provider?.GetAbsolutePath(key);
    }
}