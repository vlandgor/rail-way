using System;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;

namespace Services.Account.Providers
{
    public class UgsAccountProvider : IAccountProvider
    {
        // Events
        public event Action<string> OnSignInSuccess;
        public event Action<string> OnSignInFailed;
        public event Action<string> OnSignUpSuccess;
        public event Action<string> OnSignUpFailed;
        public event Action OnSignOutSuccess;
        public event Action OnSignOutFailed;

        // Properties
        public bool IsSignedIn => IsInitialized && AuthenticationService.Instance.IsSignedIn;
        public string PlayerId => IsInitialized ? AuthenticationService.Instance.PlayerId : string.Empty;
        public string PlayerName => IsInitialized ? AuthenticationService.Instance.PlayerName : string.Empty;
        public bool IsInitialized { get; private set; }

        private bool _eventsSubscribed;

        public UgsAccountProvider()
        {
            // Don't subscribe to events in constructor - wait until initialization
        }

        ~UgsAccountProvider()
        {
            Cleanup();
        }

        public void Cleanup()
        {
            UnsubscribeFromEvents();
        }

        public async UniTask<bool> InitializeAsync()
        {
            if (IsInitialized)
            {
                Debug.Log("[UgsAccountProvider] Already initialized");
                return true;
            }

            try
            {
                Debug.Log("[UgsAccountProvider] Initializing provider...");
                
                // UGS should already be initialized by InitializeUgsOperation
                ServicesInitializationState currentState = UnityServices.State;
                
                if (currentState != ServicesInitializationState.Initialized)
                {
                    Debug.LogError($"[UgsAccountProvider] Unity Services not initialized! State: {currentState}");
                    Debug.LogError("[UgsAccountProvider] Make sure InitializeUgsOperation runs before AccountService initialization");
                    return false;
                }
                
                IsInitialized = true;
                SubscribeToEvents();
                
                // Check if user was already signed in (session restored by UGS)
                if (IsSignedIn)
                {
                    Debug.Log($"[UgsAccountProvider] Session automatically restored for user: {PlayerId}");
                }
                
                Debug.Log("[UgsAccountProvider] Provider initialized successfully");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UgsAccountProvider] Failed to initialize provider: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        private void SubscribeToEvents()
        {
            if (_eventsSubscribed) return;

            AuthenticationService.Instance.SignedIn += OnSignedIn;
            AuthenticationService.Instance.SignedOut += OnSignedOut;
            AuthenticationService.Instance.SignInFailed += OnSignInFailedHandler;
            AuthenticationService.Instance.Expired += OnSessionExpired;
            
            _eventsSubscribed = true;
            Debug.Log("[UgsAccountProvider] Events subscribed");
        }

        private void UnsubscribeFromEvents()
        {
            if (!_eventsSubscribed || !IsInitialized) return;

            try
            {
                AuthenticationService.Instance.SignedIn -= OnSignedIn;
                AuthenticationService.Instance.SignedOut -= OnSignedOut;
                AuthenticationService.Instance.SignInFailed -= OnSignInFailedHandler;
                AuthenticationService.Instance.Expired -= OnSessionExpired;
                
                _eventsSubscribed = false;
                Debug.Log("[UgsAccountProvider] Events unsubscribed");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[UgsAccountProvider] Error unsubscribing from events: {e.Message}");
            }
        }

        public async UniTask<bool> SignInAnonymouslyAsync()
        {
            if (!IsInitialized)
            {
                Debug.LogError("[UgsAccountProvider] Provider not initialized");
                return false;
            }

            if (IsSignedIn)
            {
                Debug.Log($"[UgsAccountProvider] Already signed in as {PlayerId}");
                return true;
            }

            try
            {
                Debug.Log("[UgsAccountProvider] Signing in anonymously...");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UgsAccountProvider] Anonymous sign in failed: {e.Message}");
                OnSignInFailed?.Invoke(e.Message);
                return false;
            }
        }

        public async UniTask<bool> SignInWithEmailPasswordAsync(string email, string password)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[UgsAccountProvider] Provider not initialized");
                return false;
            }

            if (IsSignedIn)
            {
                Debug.Log($"[UgsAccountProvider] Already signed in as {PlayerId}");
                return true;
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Debug.LogError("[UgsAccountProvider] Email and password cannot be empty");
                OnSignInFailed?.Invoke("Email and password are required");
                return false;
            }

            try
            {
                Debug.Log($"[UgsAccountProvider] Signing in with email: {email}...");
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(email, password);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UgsAccountProvider] Sign in failed: {e.Message}");
                OnSignInFailed?.Invoke(e.Message);
                return false;
            }
        }
        
        public async UniTask<bool> SignUpWithEmailPasswordAsync(string email, string password)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[UgsAccountProvider] Provider not initialized");
                return false;
            }

            if (IsSignedIn)
            {
                Debug.Log($"[UgsAccountProvider] Already signed in as {PlayerId}");
                return true;
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Debug.LogError("[UgsAccountProvider] Email and password cannot be empty");
                OnSignUpFailed?.Invoke("Email and password are required");
                return false;
            }

            try
            {
                Debug.Log($"[UgsAccountProvider] Creating account with email: {email}...");
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(email, password);
                
                Debug.Log("[UgsAccountProvider] Account created successfully!");
                OnSignUpSuccess?.Invoke(PlayerId);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UgsAccountProvider] Sign up failed: {e.Message}");
                OnSignUpFailed?.Invoke(e.Message);
                return false;
            }
        }

        public async UniTask<bool> SignInWithUnityPlayerAccountAsync()
        {
            if (!IsInitialized)
            {
                Debug.LogError("[UgsAccountProvider] Provider not initialized");
                OnSignInFailed?.Invoke("Provider not initialized");
                return false;
            }

            if (IsSignedIn)
            {
                Debug.Log($"[UgsAccountProvider] Already signed in as {PlayerId}");
                return true;
            }

            try
            {
                Debug.Log("[UgsAccountProvider] Starting Unity Player Account sign in...");
                
                await PlayerAccountService.Instance.StartSignInAsync();
                
                if (!PlayerAccountService.Instance.IsSignedIn)
                {
                    Debug.LogError("[UgsAccountProvider] Unity Player Account sign-in was cancelled or failed");
                    OnSignInFailed?.Invoke("Unity Player Account sign-in cancelled or failed");
                    return false;
                }
                
                Debug.Log("[UgsAccountProvider] Unity Player Account sign-in successful, getting access token...");
                
                string accessToken = PlayerAccountService.Instance.AccessToken;
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    Debug.LogError("[UgsAccountProvider] Failed to get access token from Player Account Service");
                    OnSignInFailed?.Invoke("Failed to get access token");
                    return false;
                }
                
                Debug.Log("[UgsAccountProvider] Signing in with Unity token...");
                await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UgsAccountProvider] Unity Player Account sign in failed: {e.Message}\n{e.StackTrace}");
                OnSignInFailed?.Invoke(e.Message);
                return false;
            }
        }

        public void SignOut()
        {
            if (!IsSignedIn)
            {
                Debug.LogWarning("[UgsAccountProvider] Cannot sign out - no user is signed in");
                return;
            }

            try
            {
                Debug.Log("[UgsAccountProvider] Signing out...");
                AuthenticationService.Instance.SignOut();
                
                if (PlayerAccountService.Instance.IsSignedIn)
                {
                    PlayerAccountService.Instance.SignOut();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UgsAccountProvider] Error during sign out: {e.Message}");
                OnSignOutFailed?.Invoke();
            }
        }

        public void ClearSessionToken()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[UgsAccountProvider] Cannot clear session token - service not initialized");
                return;
            }
            
            AuthenticationService.Instance.ClearSessionToken();
            Debug.Log("[UgsAccountProvider] Session token cleared");
        }

        public string GetPlayerInfo()
        {
            if (!IsSignedIn)
            {
                return "Not signed in";
            }

            return $"PlayerId: {PlayerId}\nPlayerName: {PlayerName}\nIsSignedIn: {IsSignedIn}";
        }

        private void OnSignedIn()
        {
            Debug.Log($"[UgsAccountProvider] ✓ Signed in successfully! PlayerId: {PlayerId}");
            OnSignInSuccess?.Invoke(PlayerId);
        }

        private void OnSignedOut()
        {
            Debug.Log("[UgsAccountProvider] Signed out");
            OnSignOutSuccess?.Invoke();
        }

        private void OnSignInFailedHandler(RequestFailedException exception)
        {
            Debug.LogError($"[UgsAccountProvider] Sign in failed: {exception.Message}");
            OnSignInFailed?.Invoke(exception.Message);
        }

        private void OnSessionExpired()
        {
            Debug.LogWarning("[UgsAccountProvider] Session expired. User needs to sign in again.");
            OnSignInFailed?.Invoke("Session expired. Please sign in again.");
        }
    }
}