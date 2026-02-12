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

        // Properties
        public bool IsSignedIn => IsInitialized && AuthenticationService.Instance.IsSignedIn;
        public string PlayerId => IsInitialized ? AuthenticationService.Instance.PlayerId : string.Empty;
        public string PlayerName => IsInitialized ? AuthenticationService.Instance.PlayerName : string.Empty;
        public bool IsInitialized { get; private set; }

        private bool _eventsSubscribed;
        private UniTaskCompletionSource<bool> _initializationTask;

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

            if (_initializationTask != null)
            {
                Debug.Log("[UgsAccountProvider] Initialization already in progress, waiting...");
                return await _initializationTask.Task;
            }

            _initializationTask = new UniTaskCompletionSource<bool>();

            try
            {
                Debug.Log("[UgsAccountProvider] Initializing Unity Services...");
                
                var currentState = UnityServices.State;
                Debug.Log($"[UgsAccountProvider] Unity Services current state: {currentState}");
                
                if (currentState == ServicesInitializationState.Uninitialized)
                {
                    await UnityServices.InitializeAsync();
                }
                else if (currentState == ServicesInitializationState.Initializing)
                {
                    Debug.Log("[UgsAccountProvider] Unity Services is initializing, waiting...");
                    int maxWaitSeconds = 30;
                    int waitedSeconds = 0;
                    
                    while (UnityServices.State == ServicesInitializationState.Initializing && waitedSeconds < maxWaitSeconds)
                    {
                        await UniTask.Delay(100);
                        waitedSeconds++;
                    }
                    
                    if (UnityServices.State != ServicesInitializationState.Initialized)
                    {
                        throw new Exception($"Unity Services failed to initialize within {maxWaitSeconds} seconds. Current state: {UnityServices.State}");
                    }
                }
                
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    throw new Exception($"Unity Services is not in initialized state. Current state: {UnityServices.State}");
                }
                
                IsInitialized = true;
                SubscribeToEvents();
                
                Debug.Log("[UgsAccountProvider] Unity Services initialized successfully");
                _initializationTask.TrySetResult(true);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UgsAccountProvider] Failed to initialize Unity Services: {e.Message}\n{e.StackTrace}");
                _initializationTask.TrySetResult(false);
                return false;
            }
            finally
            {
                _initializationTask = null;
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
            if (!await EnsureInitialized())
                return false;

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
            if (!await EnsureInitialized())
                return false;

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
            if (!await EnsureInitialized())
                return false;

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
                
                // Trigger sign-up success (NOT sign-in success)
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
            Debug.Log("[UgsAccountProvider] SignInWithUnityPlayerAccountAsync called");
            
            if (!await EnsureInitialized())
            {
                Debug.LogError("[UgsAccountProvider] Failed to initialize before Unity Player Account sign in");
                OnSignInFailed?.Invoke("Failed to initialize Unity Services");
                return false;
            }

            if (IsSignedIn)
            {
                Debug.Log($"[UgsAccountProvider] Already signed in as {PlayerId}");
                return true;
            }

            try
            {
                Debug.Log("[UgsAccountProvider] Checking Unity Services state...");
                Debug.Log($"[UgsAccountProvider] Unity Services State: {UnityServices.State}");
                
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    Debug.LogError($"[UgsAccountProvider] Unity Services not properly initialized. State: {UnityServices.State}");
                    OnSignInFailed?.Invoke("Unity Services not initialized");
                    return false;
                }
                
                Debug.Log("[UgsAccountProvider] Starting Unity Player Account sign in...");
                await UniTask.Delay(100);
                
                await PlayerAccountService.Instance.StartSignInAsync();
                
                Debug.Log($"[UgsAccountProvider] PlayerAccountService.IsSignedIn: {PlayerAccountService.Instance.IsSignedIn}");
                
                if (PlayerAccountService.Instance.IsSignedIn)
                {
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
                else
                {
                    Debug.LogError("[UgsAccountProvider] Unity Player Account sign-in was cancelled or failed");
                    OnSignInFailed?.Invoke("Unity Player Account sign-in cancelled or failed");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UgsAccountProvider] Unexpected error during Unity Player Account sign in: {e.Message}\n{e.StackTrace}");
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

        private async UniTask<bool> EnsureInitialized()
        {
            if (IsInitialized && UnityServices.State == ServicesInitializationState.Initialized)
            {
                return true;
            }
            
            Debug.LogWarning("[UgsAccountProvider] Service not properly initialized. Initializing now...");
            return await InitializeAsync();
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

        private async void OnSessionExpired()
        {
            Debug.LogWarning("[UgsAccountProvider] Session expired. User needs to sign in again.");
            OnSignInFailed?.Invoke("Session expired. Please sign in again.");
        }
    }
}