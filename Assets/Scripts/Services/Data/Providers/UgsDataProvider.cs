using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Services.Account;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

namespace Services.Data.Providers
{
    public class UgsDataProvider : IDataProvider
    {
        public bool IsInitialized { get; private set; }

        public UniTask InitializeAsync()
        {
            if (IsInitialized) return UniTask.CompletedTask;

            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                return UniTask.CompletedTask;
            }

            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        public void Cleanup()
        {
            // Nothing to cleanup
        }

        public async UniTask<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            if (!IsInitialized) return false;

            try
            {
                var data = await CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { key }
                );
                return data.ContainsKey(key);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async UniTask SaveTextAsync(string key, string text, CancellationToken ct = default)
        {
            if (!IsInitialized) return;

            try
            {
                var data = new Dictionary<string, object>
                {
                    { key, text ?? string.Empty }
                };

                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            }
            catch (Exception)
            {
                // Silent fail
            }
        }

        public async UniTask<string> LoadTextAsync(string key, CancellationToken ct = default)
        {
            if (!IsInitialized) return null;

            try
            {
                var data = await CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { key }
                );

                if (data.TryGetValue(key, out var item))
                {
                    return item.Value.GetAsString();
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async UniTask DeleteAsync(string key, CancellationToken ct = default)
        {
            if (!IsInitialized) return;

            try
            {
                await CloudSaveService.Instance.Data.Player.DeleteAsync(key);
            }
            catch (Exception)
            {
                // Silent fail
            }
        }

        public string GetAbsolutePath(string key)
        {
            var playerId = AccountService.Instance?.PlayerId ?? "unknown";
            return $"unity-cloud://{playerId}/{key}";
        }
    }
}