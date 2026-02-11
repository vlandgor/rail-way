using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Services.Account.Providers
{
    public interface IAccountProvider
    {
        public event Action<string> OnSignInSuccess;
        public event Action<string> OnSignInFailed;
        public event Action OnSignOutSuccess;

        // Properties
        public bool IsSignedIn { get; }
        public string PlayerId { get; }
        public string PlayerName { get; }
        public bool IsInitialized { get; }

        // Methods
        public UniTask<bool> InitializeAsync();
        public UniTask<bool> SignInAnonymouslyAsync();
        public UniTask<bool> SignInWithEmailPasswordAsync(string email, string password);
        public UniTask<bool> SignUpWithEmailPasswordAsync(string email, string password);
        public UniTask<bool> SignInWithUnityPlayerAccountAsync();
        public void SignOut();
        public void ClearSessionToken();
        public string GetPlayerInfo();
        public void Cleanup();
    }
}