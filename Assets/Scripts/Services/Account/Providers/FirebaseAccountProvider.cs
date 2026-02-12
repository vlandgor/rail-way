using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Services.Account.Providers
{
    public class FirebaseAccountProvider : IAccountProvider
    {
        public event Action<string> OnSignInSuccess;
        public event Action<string> OnSignInFailed;
        public event Action<string> OnSignUpSuccess;
        public event Action<string> OnSignUpFailed;
        public event Action OnSignOutSuccess;
        public event Action OnSignOutFailed;

        public bool IsSignedIn { get; private set; }
        public string PlayerId { get; private set; }
        public string PlayerName { get; private set; }
        public bool IsInitialized { get; private set; }

        public async UniTask<bool> InitializeAsync()
        {
            Debug.LogWarning("[FirebaseAccountProvider] Not implemented yet");
            await UniTask.Yield();
            return false;
        }

        public async UniTask<bool> SignInAnonymouslyAsync()
        {
            Debug.LogWarning("[FirebaseAccountProvider] Not implemented yet");
            await UniTask.Yield();
            return false;
        }

        public async UniTask<bool> SignInWithEmailPasswordAsync(string email, string password)
        {
            Debug.LogWarning("[FirebaseAccountProvider] Not implemented yet");
            await UniTask.Yield();
            return false;
        }
        
        public async UniTask<bool> SignUpWithEmailPasswordAsync(string email, string password)
        {
            Debug.LogWarning("[FirebaseAccountProvider] Not implemented yet");
            await UniTask.Yield();
            return false;
        }

        public async UniTask<bool> SignInWithUnityPlayerAccountAsync()
        {
            Debug.LogWarning("[FirebaseAccountProvider] Unity Player Account not supported with Firebase");
            await UniTask.Yield();
            OnSignInFailed?.Invoke("Unity Player Account is not supported with Firebase provider");
            return false;
        }

        public void SignOut()
        {
            Debug.LogWarning("[FirebaseAccountProvider] Not implemented yet");
        }

        public void ClearSessionToken()
        {
            Debug.LogWarning("[FirebaseAccountProvider] Not implemented yet");
        }

        public string GetPlayerInfo()
        {
            return "Firebase provider not implemented";
        }

        public void Cleanup()
        {
        }
    }
}