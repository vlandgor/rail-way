using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Services.Utilities;

namespace Services.Account
{
    public class AccountService : Singleton<AccountService>
    {
        // Events
        public event Action<string> OnSignInSuccess;
        public event Action<string> OnSignInFailed;
        public event Action OnSignOutSuccess;

        // Properties
        public bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;
        public string PlayerId => AuthenticationService.Instance.PlayerId;
        public string PlayerName => AuthenticationService.Instance.PlayerName;

        private bool _isInitialized = false;

        protected override void Awake()
        {
            base.Awake();
            
            // Subscribe to authentication events
            AuthenticationService.Instance.SignedIn += OnSignedIn;
            AuthenticationService.Instance.SignedOut += OnSignedOut;
            AuthenticationService.Instance.SignInFailed += OnSignInFailedHandler;
            AuthenticationService.Instance.Expired += OnSessionExpired;
        }

        private void OnDestroy()
        {
            // Unsubscribe from authentication events
            if (AuthenticationService.Instance != null)
            {
                AuthenticationService.Instance.SignedIn -= OnSignedIn;
                AuthenticationService.Instance.SignedOut -= OnSignedOut;
                AuthenticationService.Instance.SignInFailed -= OnSignInFailedHandler;
                AuthenticationService.Instance.Expired -= OnSessionExpired;
            }
        }

        /// <summary>
        /// Initialize Unity Services and prepare for authentication
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            if (_isInitialized)
            {
                Debug.Log("[AccountService] Already initialized");
                return true;
            }

            try
            {
                Debug.Log("[AccountService] Initializing Unity Services...");
                await UnityServices.InitializeAsync();
                _isInitialized = true;
                Debug.Log("[AccountService] Unity Services initialized successfully");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AccountService] Failed to initialize Unity Services: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sign in anonymously as a guest
        /// </summary>
        public async Task<bool> SignInAnonymouslyAsync()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[AccountService] Cannot sign in - service not initialized. Initializing now...");
                bool initialized = await InitializeAsync();
                if (!initialized)
                {
                    return false;
                }
            }

            if (IsSignedIn)
            {
                Debug.Log($"[AccountService] Already signed in as {PlayerId}");
                return true;
            }

            try
            {
                Debug.Log("[AccountService] Signing in anonymously...");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                return true;
            }
            catch (AuthenticationException e)
            {
                Debug.LogError($"[AccountService] Authentication failed: {e.Message}");
                OnSignInFailed?.Invoke(e.Message);
                return false;
            }
            catch (RequestFailedException e)
            {
                Debug.LogError($"[AccountService] Request failed: {e.Message}");
                OnSignInFailed?.Invoke(e.Message);
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AccountService] Unexpected error during sign in: {e.Message}");
                OnSignInFailed?.Invoke(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Sign out the current player
        /// </summary>
        public void SignOut()
        {
            if (!IsSignedIn)
            {
                Debug.LogWarning("[AccountService] Cannot sign out - no user is signed in");
                return;
            }

            try
            {
                Debug.Log("[AccountService] Signing out...");
                AuthenticationService.Instance.SignOut();
            }
            catch (Exception e)
            {
                Debug.LogError($"[AccountService] Error during sign out: {e.Message}");
            }
        }

        /// <summary>
        /// Clear cached session token to force a fresh authentication
        /// </summary>
        public void ClearSessionToken()
        {
            AuthenticationService.Instance.ClearSessionToken();
            Debug.Log("[AccountService] Session token cleared");
        }

        // Event Handlers
        private void OnSignedIn()
        {
            Debug.Log($"[AccountService] ✓ Signed in successfully! PlayerId: {PlayerId}");
            OnSignInSuccess?.Invoke(PlayerId);
        }

        private void OnSignedOut()
        {
            Debug.Log("[AccountService] Signed out");
            OnSignOutSuccess?.Invoke();
        }

        private void OnSignInFailedHandler(RequestFailedException exception)
        {
            Debug.LogError($"[AccountService] Sign in failed: {exception.Message}");
            OnSignInFailed?.Invoke(exception.Message);
        }

        private async void OnSessionExpired()
        {
            Debug.LogWarning("[AccountService] Session expired. Attempting to re-authenticate...");
            await SignInAnonymouslyAsync();
        }

        // Utility Methods
        public string GetPlayerInfo()
        {
            if (!IsSignedIn)
            {
                return "Not signed in";
            }

            return $"PlayerId: {PlayerId}\nPlayerName: {PlayerName}\nIsSignedIn: {IsSignedIn}";
        }
    }
}