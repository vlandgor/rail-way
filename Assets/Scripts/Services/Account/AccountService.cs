using System;
using Cysharp.Threading.Tasks;
using Services.Account.Providers;
using Services.Utilities;
using UnityEngine;

namespace Services.Account
{
    public enum AccountProviderType
    {
        Ugs,
        Firebase
    }
    
    public class AccountService : Singleton<AccountService>
    {
        public event Action<string> OnSignInSuccess
        {
            add
            {
                if (_provider != null)
                    _provider.OnSignInSuccess += value;
            }
            remove
            {
                if (_provider != null)
                    _provider.OnSignInSuccess -= value;
            }
        }
        public event Action<string> OnSignInFailed
        {
            add
            {
                if (_provider != null)
                    _provider.OnSignInFailed += value;
            }
            remove
            {
                if (_provider != null)
                    _provider.OnSignInFailed -= value;
            }
        }
        public event Action OnSignOutSuccess
        {
            add
            {
                if (_provider != null)
                    _provider.OnSignOutSuccess += value;
            }
            remove
            {
                if (_provider != null)
                    _provider.OnSignOutSuccess -= value;
            }
        }
        
        [SerializeField] private AccountProviderType _providerType = AccountProviderType.Ugs;

        private IAccountProvider _provider;
        
        public bool IsSignedIn => _provider?.IsSignedIn ?? false;
        public string PlayerId => _provider?.PlayerId ?? string.Empty;
        public string PlayerName => _provider?.PlayerName ?? string.Empty;
        public bool IsInitialized => _provider?.IsInitialized ?? false;

        private void OnDestroy()
        {
            _provider?.Cleanup();
        }

        public override void Initialize()
        {
            base.Initialize();
            
            InitializeProvider();
            InitializeServiceAsync().Forget();
        }

        public void SwitchProvider(AccountProviderType newProviderType)
        {
            if (_providerType == newProviderType)
            {
                Debug.LogWarning($"[AccountService] Already using {newProviderType} provider");
                return;
            }

            _provider?.Cleanup();

            _providerType = newProviderType;
            InitializeProvider();
            
            Debug.Log($"[AccountService] Switched to {newProviderType} provider");
        }

        // Public API - Delegate to provider
        public UniTask<bool> InitializeAsync() => _provider.InitializeAsync();
        public UniTask<bool> SignInAnonymouslyAsync() => _provider.SignInAnonymouslyAsync();
        public UniTask<bool> SignInWithEmailPasswordAsync(string email, string password) => _provider.SignInWithEmailPasswordAsync(email, password);
        public UniTask<bool> SignUpWithEmailPasswordAsync(string email, string password) => _provider.SignUpWithEmailPasswordAsync(email, password);
        public UniTask<bool> SignInWithUnityPlayerAccountAsync() => _provider.SignInWithUnityPlayerAccountAsync();
        public void SignOut() => _provider.SignOut();
        public void ClearSessionToken() => _provider.ClearSessionToken();
        public string GetPlayerInfo() => _provider.GetPlayerInfo();
        
        private async UniTaskVoid InitializeServiceAsync()
        {
            Debug.Log("[AccountService] Starting async initialization...");
            bool success = await _provider.InitializeAsync();
    
            if (success)
            {
                Debug.Log("[AccountService] Initialization completed successfully");
            }
            else
            {
                Debug.LogError("[AccountService] Initialization failed");
            }
        }
        
        private void InitializeProvider()
        {
            switch (_providerType)
            {
                case AccountProviderType.Ugs:
                    _provider = new UgsAccountProvider();
                    Debug.Log("[AccountService] Using UGS Account Provider");
                    break;
                
                case AccountProviderType.Firebase:
                    _provider = new FirebaseAccountProvider();
                    Debug.Log("[AccountService] Using Firebase Account Provider");
                    break;
                
                default:
                    Debug.LogError($"[AccountService] Unknown provider type: {_providerType}");
                    _provider = new UgsAccountProvider();
                    break;
            }
        }
    }
}