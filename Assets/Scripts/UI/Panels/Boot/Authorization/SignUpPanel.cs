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
        [SerializeField] private SignInPanel _signInPanel;
        
        [Header("UI Elements")]
        [SerializeField] private TMP_InputField _emailInputField;
        [SerializeField] private TMP_InputField _passwordInputField;
        [SerializeField] private TMP_InputField _confirmPasswordInputField;
        
        [SerializeField] private Button _signUpButton;
        [SerializeField] private Button _backToSignInButton;
        
        [SerializeField] private GameObject _errorMessageContainer;
        [SerializeField] private TextMeshProUGUI _errorMessageText;
        [SerializeField] private Color _errorTextColor = new Color(0.8f, 0.2f, 0.2f);

        private bool _isProcessing;

        private void Start()
        {
            _signUpButton.onClick.AddListener(HandleSignUpButtonClicked);
            _backToSignInButton.onClick.AddListener(HandleBackToSignInButtonClicked);
            
            AccountService.Instance.OnSignUpSuccess += HandleSignUpSuccess;
            AccountService.Instance.OnSignUpFailed += HandleSignUpFailed;
            
            if (_errorMessageText != null)
            {
                _errorMessageText.color = _errorTextColor;
            }
            
            HideError();
        }
        
        private void OnDestroy()
        {
            _signUpButton.onClick.RemoveListener(HandleSignUpButtonClicked);
            _backToSignInButton.onClick.RemoveListener(HandleBackToSignInButtonClicked);
            
            if (AccountService.Instance != null)
            {
                AccountService.Instance.OnSignUpSuccess -= HandleSignUpSuccess;
                AccountService.Instance.OnSignUpFailed -= HandleSignUpFailed;
            }
        }

        private void HandleSignUpButtonClicked()
        {
            SignUpAsync().Forget();
        }

        private void HandleBackToSignInButtonClicked()
        {
            HideError();
            _signInPanel.Enable();
            Disable();
        }

        private async UniTaskVoid SignUpAsync()
        {
            if (_isProcessing) return;

            HideError();

            string email = _emailInputField.text.Trim();
            string password = _passwordInputField.text;
            string confirmPassword = _confirmPasswordInputField.text;

            if (string.IsNullOrEmpty(email))
            {
                ShowError("Please enter your email address");
                return;
            }

            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter a password");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Password must be at least 6 characters");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Passwords do not match");
                return;
            }

            try
            {
                _isProcessing = true;
                SetButtonsInteractable(false);

                bool success = await AccountService.Instance.SignUpWithEmailPasswordAsync(email, password);

                if (!success)
                {
                    Debug.LogWarning("[SignUpPanel] Sign up failed");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SignUpPanel] Exception: {e.Message}");
                ShowError("An unexpected error occurred. Please try again.");
            }
            finally
            {
                _isProcessing = false;
                SetButtonsInteractable(true);
            }
        }

        private void HandleSignUpSuccess(string playerId)
        {
            Debug.Log($"[SignUpPanel] Account created and signed in! Player ID: {playerId}");
            HideError();
            
            // Clear input fields
            _emailInputField.text = "";
            _passwordInputField.text = "";
            _confirmPasswordInputField.text = "";
            
            Disable();
        }

        private void HandleSignUpFailed(string error)
        {
            Debug.LogError($"[SignUpPanel] Sign up failed: {error}");
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
            _signUpButton.interactable = interactable;
            _backToSignInButton.interactable = interactable;
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