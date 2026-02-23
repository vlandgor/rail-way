using System;
using Cysharp.Threading.Tasks;
using Services.Account.Providers;
using UnityEngine;

namespace Services.Account
{
    public enum AccountProviderType
    {
        Ugs,
        Firebase
    }
    
    public class AccountService : BaseService<AccountService>
    {
        // Own events that can be subscribed to before initialization
        public event Action<string> OnSignInSuccess;
        public event Action<string> OnSignInFailed;
        public event Action<string> OnSignUpSuccess;
        public event Action<string> OnSignUpFailed;
        public event Action OnSignOutSuccess;
        public event Action OnSignOutFailed;
        public event Action OnAuthorizationRequired;
        
        [SerializeField] private AccountProviderType _providerType = AccountProviderType.Ugs;

        private IAccountProvider _provider;
        private UniTaskCompletionSource _authCompletionSource;
        
        public bool IsSignedIn => _provider?.IsSignedIn ?? false;
        public string PlayerId => _provider?.PlayerId ?? string.Empty;
        public string PlayerName => _provider?.PlayerName ?? string.Empty;
        public bool IsInitialized => _provider?.IsInitialized ?? false;

        private void OnDestroy()
        {
            UnsubscribeFromProvider();
            _provider?.Cleanup();
        }

        public override async UniTask Initialize()
        {
            if (IsInitialized)
            {
                Debug.Log("[AccountService] Already initialized");
                return;
            }
            
            await base.Initialize();
            await InitializeProvider();
        }
        
        public UniTask<bool> TryAutoAuthorize()
        {
            Debug.Log("[AccountService] Checking auto-authorization...");
            
            if (!IsInitialized)
            {
                Debug.LogError("[AccountService] Service not initialized!");
                return UniTask.FromResult(false);
            }

            if (IsSignedIn)
            {
                Debug.Log($"[AccountService] Auto-authorization successful! User: {PlayerId}");
                return UniTask.FromResult(true);
            }

            Debug.Log("[AccountService] No saved session - manual sign-in required");
            return UniTask.FromResult(false);
        }
        
        public async UniTask RequireAuthorization()
        {
            if (IsSignedIn)
            {
                Debug.Log("[AccountService] Already authorized");
                return;
            }

            Debug.Log("[AccountService] Waiting for manual authorization...");
            OnAuthorizationRequired?.Invoke();
            _authCompletionSource = new UniTaskCompletionSource();
            await _authCompletionSource.Task;
            Debug.Log("[AccountService] Manual authorization completed");
        }
        
        public void CompleteManualAuthorization()
        {
            if (!IsSignedIn)
            {
                Debug.LogWarning("[AccountService] CompleteManualAuthorization called but user not signed in");
                return;
            }

            Debug.Log("[AccountService] Completing manual authorization...");
            _authCompletionSource?.TrySetResult();
            _authCompletionSource = null;
        }

        public UniTask<bool> SignInAnonymouslyAsync() => _provider.SignInAnonymouslyAsync();
        
        public async UniTask<bool> SignInWithEmailPasswordAsync(string email, string password)
        {
            bool success = await _provider.SignInWithEmailPasswordAsync(email, password);
            if (success) CompleteManualAuthorization();
            return success;
        }
        
        public async UniTask<bool> SignUpWithEmailPasswordAsync(string email, string password)
        {
            bool success = await _provider.SignUpWithEmailPasswordAsync(email, password);
            
            if (success)
            {
                success = await _provider.SignInWithEmailPasswordAsync(email, password);
                if (success) CompleteManualAuthorization();
            }
            
            return success;
        }
        
        public async UniTask<bool> SignInWithUnityPlayerAccountAsync()
        {
            bool success = await _provider.SignInWithUnityPlayerAccountAsync();
            if (success) CompleteManualAuthorization();
            return success;
        }
        
        public void SignOut() => _provider.SignOut();
        public void ClearSessionToken() => _provider.ClearSessionToken();
        public string GetPlayerInfo() => _provider.GetPlayerInfo();
        
        private async UniTask InitializeProvider()
        {
            switch (_providerType)
            {
                case AccountProviderType.Ugs:
                    _provider = new UgsAccountProvider();
                    break;
                
                case AccountProviderType.Firebase:
                    _provider = new FirebaseAccountProvider();
                    break;
                
                default:
                    Debug.LogError($"[AccountService] Unknown provider type: {_providerType}");
                    _provider = new UgsAccountProvider();
                    break;
            }
            
            bool success = await _provider.InitializeAsync();
            
            if (success)
            {
                SubscribeToProvider();
                
                if (IsSignedIn)
                {
                    Debug.Log($"[AccountService] Session restored for user: {PlayerId}");
                }
            }
        }

        private void SubscribeToProvider()
        {
            if (_provider == null) return;

            _provider.OnSignInSuccess += (playerId) => OnSignInSuccess?.Invoke(playerId);
            _provider.OnSignInFailed += (error) => OnSignInFailed?.Invoke(error);
            _provider.OnSignUpSuccess += (playerId) => OnSignUpSuccess?.Invoke(playerId);
            _provider.OnSignUpFailed += (error) => OnSignUpFailed?.Invoke(error);
            _provider.OnSignOutSuccess += () => OnSignOutSuccess?.Invoke();
            _provider.OnSignOutFailed += () => OnSignOutFailed?.Invoke();
        }

        private void UnsubscribeFromProvider()
        {
            if (_provider == null) return;

            _provider.OnSignInSuccess -= (playerId) => OnSignInSuccess?.Invoke(playerId);
            _provider.OnSignInFailed -= (error) => OnSignInFailed?.Invoke(error);
            _provider.OnSignUpSuccess -= (playerId) => OnSignUpSuccess?.Invoke(playerId);
            _provider.OnSignUpFailed -= (error) => OnSignUpFailed?.Invoke(error);
            _provider.OnSignOutSuccess -= () => OnSignOutSuccess?.Invoke();
            _provider.OnSignOutFailed -= () => OnSignOutFailed?.Invoke();
        }
    }
}