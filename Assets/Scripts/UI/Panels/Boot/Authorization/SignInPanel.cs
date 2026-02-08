using System;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Boot.Authorization
{
    public class SignInPanel : BasePanel
    {
        [SerializeField] private TMP_InputField _emailInputField;
        [SerializeField] private TMP_InputField _passwordInputField;
        
        [SerializeField] private Button _signInButton;
        [SerializeField] private Button _signInWithGoogleButton;
        [SerializeField] private Button _continueAsGuestButton;
        [SerializeField] private Button _forgotPasswordButton;
        [SerializeField] private Button _createAccountButton;

        private void Start()
        {
            _signInButton.onClick.AddListener(HandleSignInButtonClicked);
            _signInWithGoogleButton.onClick.AddListener(HandleSignInWithGoogleButtonClicked);
            _continueAsGuestButton.onClick.AddListener(HandleContinueAsGuestButtonClicked);
            _forgotPasswordButton.onClick.AddListener(HandleForgotPasswordButtonClicked);
            _createAccountButton.onClick.AddListener(HandleCreateAccountButtonClicked);
        }

        private void OnDestroy()
        {
            _signInButton.onClick.RemoveListener(HandleSignInButtonClicked);
            _forgotPasswordButton.onClick.RemoveListener(HandleForgotPasswordButtonClicked);
            _createAccountButton.onClick.RemoveListener(HandleCreateAccountButtonClicked);
        }

        private void HandleSignInButtonClicked()
        {
            
        }
        
        private void HandleSignInWithGoogleButtonClicked()
        {
            
        }
        
        private void HandleContinueAsGuestButtonClicked()
        {
            
        }

        private void HandleForgotPasswordButtonClicked()
        {
            
        }

        private void HandleCreateAccountButtonClicked()
        {
            
        }
    }
}