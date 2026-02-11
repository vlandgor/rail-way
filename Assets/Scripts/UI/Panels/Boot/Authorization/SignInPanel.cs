using System;
using Cysharp.Threading.Tasks;
using Services.Account;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Boot.Authorization
{
    public class SignInPanel : BasePanel
    {
        [Header("Panels")]
        [SerializeField] private SignUpPanel _signUpPanel;
        [SerializeField] private ForgotPasswordPanel _forgotPasswordPanel;
        
        [Header("UI Elements")]
        [SerializeField] private TMP_InputField _emailInputField;
        [SerializeField] private TMP_InputField _passwordInputField;
        
        [SerializeField] private Button _signInButton;
        [SerializeField] private Button _signInWithGoogleButton;
        [SerializeField] private Button _continueAsGuestButton;
        [SerializeField] private Button _forgotPasswordButton;
        [SerializeField] private Button _createAccountButton;

        private bool _isProcessing;

        private void Start()
        {
            _signInButton.onClick.AddListener(HandleSignInButtonClicked);
            _signInWithGoogleButton.onClick.AddListener(HandleSignInWithGoogleButtonClicked);
            _continueAsGuestButton.onClick.AddListener(HandleContinueAsGuestButtonClicked);
            _forgotPasswordButton.onClick.AddListener(HandleForgotPasswordButtonClicked);
            _createAccountButton.onClick.AddListener(HandleCreateAccountButtonClicked);
            
            // Subscribe to account service events
            AccountService.Instance.OnSignInSuccess += HandleSignInSuccess;
            AccountService.Instance.OnSignInFailed += HandleSignInFailed;
        }

        private void OnDestroy()
        {
            _signInButton.onClick.RemoveListener(HandleSignInButtonClicked);
            _signInWithGoogleButton.onClick.RemoveListener(HandleSignInWithGoogleButtonClicked);
            _continueAsGuestButton.onClick.RemoveListener(HandleContinueAsGuestButtonClicked);
            _forgotPasswordButton.onClick.RemoveListener(HandleForgotPasswordButtonClicked);
            _createAccountButton.onClick.RemoveListener(HandleCreateAccountButtonClicked);
            
            // Unsubscribe from account service events
            if (AccountService.Instance != null)
            {
                AccountService.Instance.OnSignInSuccess -= HandleSignInSuccess;
                AccountService.Instance.OnSignInFailed -= HandleSignInFailed;
            }
        }

        private void HandleSignInButtonClicked()
        {
            SignInWithEmailPasswordAsync().Forget();
        }
        
        private void HandleSignInWithGoogleButtonClicked()
        {
            SignInWithUnityPlayerAccountAsync().Forget();
        }
        
        private void HandleContinueAsGuestButtonClicked()
        {
            SignInAnonymouslyAsync().Forget();
        }

        private void HandleForgotPasswordButtonClicked()
        {
            _forgotPasswordPanel.Enable();
            Disable();
        }

        private void HandleCreateAccountButtonClicked()
        {
            _signUpPanel.Enable();
            Disable();
        }

        private async UniTaskVoid SignInWithEmailPasswordAsync()
        {
            if (_isProcessing) return;

            string email = _emailInputField.text.Trim();
            string password = _passwordInputField.text;

            // Validation
            if (string.IsNullOrEmpty(email))
            {
                Debug.LogWarning("[SignInPanel] Please enter your email address");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("[SignInPanel] Please enter your password");
                return;
            }

            if (!IsValidEmail(email))
            {
                Debug.LogWarning("[SignInPanel] Please enter a valid email address");
                return;
            }

            try
            {
                _isProcessing = true;
                SetButtonsInteractable(false);

                Debug.Log($"[SignInPanel] Signing in with email: {email}");
                
                // Initialize if needed
                if (!AccountService.Instance.IsInitialized)
                {
                    await AccountService.Instance.InitializeAsync();
                }

                bool success = await AccountService.Instance.SignInWithEmailPasswordAsync(email, password);

                if (!success)
                {
                    Debug.LogWarning("[SignInPanel] Sign in with email/password failed");
                    // Error will be handled by HandleSignInFailed event
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SignInPanel] Exception during sign in: {e.Message}");
            }
            finally
            {
                _isProcessing = false;
                SetButtonsInteractable(true);
            }
        }

        private async UniTaskVoid SignInWithUnityPlayerAccountAsync()
        {
            if (_isProcessing) return;

            try
            {
                _isProcessing = true;
                SetButtonsInteractable(false);

                Debug.Log("[SignInPanel] Signing in with Unity Player Account");
                
                // Initialize if needed
                if (!AccountService.Instance.IsInitialized)
                {
                    await AccountService.Instance.InitializeAsync();
                }

                bool success = await AccountService.Instance.SignInWithUnityPlayerAccountAsync();

                if (!success)
                {
                    Debug.LogWarning("[SignInPanel] Sign in with Unity Player Account failed");
                    // Error will be handled by HandleSignInFailed event
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SignInPanel] Exception during Unity Player Account sign in: {e.Message}");
            }
            finally
            {
                _isProcessing = false;
                SetButtonsInteractable(true);
            }
        }

        private async UniTaskVoid SignInAnonymouslyAsync()
        {
            if (_isProcessing) return;

            try
            {
                _isProcessing = true;
                SetButtonsInteractable(false);

                Debug.Log("[SignInPanel] Signing in anonymously");
                
                // Initialize if needed
                if (!AccountService.Instance.IsInitialized)
                {
                    await AccountService.Instance.InitializeAsync();
                }

                bool success = await AccountService.Instance.SignInAnonymouslyAsync();

                if (!success)
                {
                    Debug.LogWarning("[SignInPanel] Anonymous sign in failed");
                    // Error will be handled by HandleSignInFailed event
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SignInPanel] Exception during anonymous sign in: {e.Message}");
            }
            finally
            {
                _isProcessing = false;
                SetButtonsInteractable(true);
            }
        }

        // Event Handlers
        private void HandleSignInSuccess(string playerId)
        {
            Debug.Log($"[SignInPanel] Sign in successful! Player ID: {playerId}");
            
            // TODO: Navigate to main game or next screen
            // Example: SceneManager.LoadScene("MainGame");
            // Or: PanelManager.Instance.ShowPanel<MainMenuPanel>();
        }

        private void HandleSignInFailed(string error)
        {
            Debug.LogError($"[SignInPanel] Sign in failed: {error}");
        }

        // UI Helper Methods
        private void SetButtonsInteractable(bool interactable)
        {
            _signInButton.interactable = interactable;
            _signInWithGoogleButton.interactable = interactable;
            _continueAsGuestButton.interactable = interactable;
            _forgotPasswordButton.interactable = interactable;
            _createAccountButton.interactable = interactable;
        }

        // Validation Helper
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}