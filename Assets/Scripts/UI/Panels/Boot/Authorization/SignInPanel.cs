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
        
        [SerializeField] private GameObject _errorMessageContainer;
        [SerializeField] private TextMeshProUGUI _errorMessageText;
        [SerializeField] private Color _errorTextColor = new Color(0.8f, 0.2f, 0.2f);

        private bool _isProcessing;

        private void Start()
        {
            _signInButton.onClick.AddListener(HandleSignInButtonClicked);
            _signInWithGoogleButton.onClick.AddListener(HandleSignInWithGoogleButtonClicked);
            _continueAsGuestButton.onClick.AddListener(HandleContinueAsGuestButtonClicked);
            _forgotPasswordButton.onClick.AddListener(HandleForgotPasswordButtonClicked);
            _createAccountButton.onClick.AddListener(HandleCreateAccountButtonClicked);

            AccountService.Instance.OnAuthorizationRequired += HandleAuthorizationRequired;
            AccountService.Instance.OnSignInSuccess += HandleSignInSuccess;
            AccountService.Instance.OnSignInFailed += HandleSignInFailed;
            
            if (_errorMessageText != null)
            {
                _errorMessageText.color = _errorTextColor;
            }
            
            HideError();
        }

        private void OnDestroy()
        {
            _signInButton.onClick.RemoveListener(HandleSignInButtonClicked);
            _signInWithGoogleButton.onClick.RemoveListener(HandleSignInWithGoogleButtonClicked);
            _continueAsGuestButton.onClick.RemoveListener(HandleContinueAsGuestButtonClicked);
            _forgotPasswordButton.onClick.RemoveListener(HandleForgotPasswordButtonClicked);
            _createAccountButton.onClick.RemoveListener(HandleCreateAccountButtonClicked);
            
            if (AccountService.Instance != null)
            {
                AccountService.Instance.OnAuthorizationRequired -= HandleAuthorizationRequired;
                AccountService.Instance.OnSignInSuccess -= HandleSignInSuccess;
                AccountService.Instance.OnSignInFailed -= HandleSignInFailed;
            }
        }

        private void HandleSignInButtonClicked()
        {
            SignInWithEmailPasswordAsync().Forget();
            //HideError();
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
            HideError();
            _forgotPasswordPanel.Enable();
            Disable();
        }

        private void HandleCreateAccountButtonClicked()
        {
            HideError();
            _signUpPanel.Enable();
            Disable();
        }

        private async UniTaskVoid SignInWithEmailPasswordAsync()
        {
            if (_isProcessing) return;
            
            HideError();

            string email = _emailInputField.text.Trim();
            string password = _passwordInputField.text;

            if (string.IsNullOrEmpty(email))
            {
                ShowError("Please enter your email address");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your password");
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address");
                return;
            }

            try
            {
                _isProcessing = true;
                SetButtonsInteractable(false);

                bool success = await AccountService.Instance.SignInWithEmailPasswordAsync(email, password);

                if (!success)
                {
                    Debug.LogWarning("[SignInPanel] Sign in failed");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SignInPanel] Exception: {e.Message}");
                ShowError("An unexpected error occurred. Please try again.");
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

            HideError();

            try
            {
                _isProcessing = true;
                SetButtonsInteractable(false);

                bool success = await AccountService.Instance.SignInWithUnityPlayerAccountAsync();

                if (!success)
                {
                    Debug.LogWarning("[SignInPanel] Unity Player Account sign in failed");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SignInPanel] Exception: {e.Message}");
                ShowError("Unity Player Account sign-in failed. Please try again.");
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

                bool success = await AccountService.Instance.SignInAnonymouslyAsync();

                if (!success)
                {
                    Debug.LogWarning("[SignInPanel] Anonymous sign in failed");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SignInPanel] Exception: {e.Message}");
                ShowError("Guest sign-in failed. Please try again.");
            }
            finally
            {
                _isProcessing = false;
                SetButtonsInteractable(true);
            }
        }
        
        private void HandleAuthorizationRequired()
        {
            Enable();
        }
        
        private void HandleSignInSuccess(string playerId)
        {
            Debug.Log($"[SignInPanel] Sign in successful! Player ID: {playerId}");
            HideError();
            Disable();
        }

        private void HandleSignInFailed(string error)
        {
            Debug.LogError($"[SignInPanel] Sign in failed: {error}");
            ShowError(error);
        }

        private void ShowError(string message)
        {
            if (_errorMessageText != null)
            {
                _errorMessageText.text = message;
                _errorMessageText.gameObject.SetActive(true);
            }

            if (_errorMessageContainer != null)
            {
                _errorMessageContainer.SetActive(true);
            }
        }

        private void HideError()
        {
            if (_errorMessageText != null)
            {
                _errorMessageText.gameObject.SetActive(false);
            }

            if (_errorMessageContainer != null)
            {
                _errorMessageContainer.SetActive(false);
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            _signInButton.interactable = interactable;
            _signInWithGoogleButton.interactable = interactable;
            _continueAsGuestButton.interactable = interactable;
            _forgotPasswordButton.interactable = interactable;
            _createAccountButton.interactable = interactable;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                System.Net.Mail.MailAddress addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}