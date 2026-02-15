using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

namespace Services.Data.Providers
{
    public class UnityCloudDataProvider : IDataProvider
    {
        public bool IsInitialized { get; private set; }

        public async UniTask InitializeAsync()
        {
            if (IsInitialized) return;

            try
            {
                await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"[UnityCloudDataProvider] Signed in as: {AuthenticationService.Instance.PlayerId}");
                }

                IsInitialized = true;
                Debug.Log("[UnityCloudDataProvider] Initialized successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityCloudDataProvider] Initialization failed: {e.Message}");
                throw;
            }
        }

        public async UniTask<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[UnityCloudDataProvider] Not initialized.");
                return false;
            }

            try
            {
                var data = await CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { key }
                );
                return data.ContainsKey(key);
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityCloudDataProvider] ExistsAsync failed for key '{key}': {e.Message}");
                return false;
            }
        }

        public async UniTask SaveTextAsync(string key, string text, CancellationToken ct = default)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[UnityCloudDataProvider] Not initialized.");
                return;
            }

            try
            {
                var data = new Dictionary<string, object>
                {
                    { key, text ?? string.Empty }
                };

                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
                
#if UNITY_EDITOR
                Debug.Log($"[UnityCloudDataProvider] Saved key: '{key}'");
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityCloudDataProvider] SaveTextAsync failed for key '{key}': {e.Message}");
                throw;
            }
        }

        public async UniTask<string> LoadTextAsync(string key, CancellationToken ct = default)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[UnityCloudDataProvider] Not initialized.");
                return null;
            }

            try
            {
                var data = await CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { key }
                );

                if (data.TryGetValue(key, out var item))
                {
                    var result = item.Value.GetAsString();
#if UNITY_EDITOR
                    Debug.Log($"[UnityCloudDataProvider] Loaded key: '{key}'");
#endif
                    return result;
                }

                Debug.LogWarning($"[UnityCloudDataProvider] Key '{key}' not found");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityCloudDataProvider] LoadTextAsync failed for key '{key}': {e.Message}");
                return null;
            }
        }

        public async UniTask DeleteAsync(string key, CancellationToken ct = default)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[UnityCloudDataProvider] Not initialized.");
                return;
            }

            try
            {
                await CloudSaveService.Instance.Data.Player.DeleteAsync(key);
                Debug.Log($"[UnityCloudDataProvider] Deleted key: '{key}'");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityCloudDataProvider] DeleteAsync failed for key '{key}': {e.Message}");
                throw;
            }
        }

        public string GetAbsolutePath(string key)
        {
            if (IsInitialized && AuthenticationService.Instance.IsSignedIn)
            {
                return $"unity-cloud://{AuthenticationService.Instance.PlayerId}/{key}";
            }
            return $"unity-cloud://not-initialized/{key}";
        }
    }
}