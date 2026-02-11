using System;
using Cysharp.Threading.Tasks;
using Services.Account;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Boot.Authorization
{
    public class SignUpPanel : BasePanel
    {
        [Header("Panels")]
        [SerializeField] private SignInPanel _signIpPanel;
        
        [Header("UI Elements")]
        [SerializeField] private TMP_InputField _emailInputField;
        [SerializeField] private TMP_InputField _passwordInputField;
        [SerializeField] private TMP_InputField _confirmPasswordInputField;
        
        [SerializeField] private Button _signUpButton;
        [SerializeField] private Button _backToSignInButton;

        private bool _isProcessing;

        private void Start()
        {
            _signUpButton.onClick.AddListener(HandleSignUpButtonClicked);
            _backToSignInButton.onClick.AddListener(HandleBackToSignInButtonClicked);
            
            // Subscribe to account service events
            AccountService.Instance.OnSignInSuccess += HandleSignUpSuccess;
            AccountService.Instance.OnSignInFailed += HandleSignUpFailed;
        }
        
        private void OnDestroy()
        {
            _signUpButton.onClick.RemoveListener(HandleSignUpButtonClicked);
            _backToSignInButton.onClick.RemoveListener(HandleBackToSignInButtonClicked);
            
            // Unsubscribe from account service events
            if (AccountService.Instance != null)
            {
                AccountService.Instance.OnSignInSuccess -= HandleSignUpSuccess;
                AccountService.Instance.OnSignInFailed -= HandleSignUpFailed;
            }
        }

        private void HandleSignUpButtonClicked()
        {
            SignUpAsync().Forget();
        }

        private void HandleBackToSignInButtonClicked()
        {
            _signIpPanel.Enable();
            Disable();
        }

        private async UniTaskVoid SignUpAsync()
        {
            if (_isProcessing) return;

            string email = _emailInputField.text.Trim();
            string password = _passwordInputField.text;
            string confirmPassword = _confirmPasswordInputField.text;

            // Validation
            if (string.IsNullOrEmpty(email))
            {
                Debug.LogWarning("[SignUpPanel] Please enter your email address");
                return;
            }

            if (!IsValidEmail(email))
            {
                Debug.LogWarning("[SignUpPanel] Please enter a valid email address");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("[SignUpPanel] Please enter a password");
                return;
            }

            if (password.Length < 6)
            {
                Debug.LogWarning("[SignUpPanel] Password must be at least 6 characters");
                return;
            }

            if (password != confirmPassword)
            {
                Debug.LogWarning("[SignUpPanel] Passwords do not match");
                return;
            }

            try
            {
                _isProcessing = true;
                SetButtonsInteractable(false);

                Debug.Log($"[SignUpPanel] Creating account with email: {email}");
        
                // Initialize if needed
                if (!AccountService.Instance.IsInitialized)
                {
                    await AccountService.Instance.InitializeAsync();
                }

                // Use the new SignUp method instead of SignIn
                bool success = await AccountService.Instance.SignUpWithEmailPasswordAsync(email, password);

                if (!success)
                {
                    Debug.LogWarning("[SignUpPanel] Account creation failed");
                    // Error will be handled by HandleSignUpFailed event
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SignUpPanel] Exception during sign up: {e.Message}");
            }
            finally
            {
                _isProcessing = false;
                SetButtonsInteractable(true);
            }
        }

        // Event Handlers
        private void HandleSignUpSuccess(string playerId)
        {
            Debug.Log($"[SignUpPanel] Account created successfully! Player ID: {playerId}");
            
            // TODO: Navigate to main game or next screen
            // Example: SceneManager.LoadScene("MainGame");
            // Or: PanelManager.Instance.ShowPanel<MainMenuPanel>();
        }

        private void HandleSignUpFailed(string error)
        {
            Debug.LogError($"[SignUpPanel] Sign up failed: {error}");
        }

        // UI Helper Methods
        private void SetButtonsInteractable(bool interactable)
        {
            _signUpButton.interactable = interactable;
            _backToSignInButton.interactable = interactable;
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