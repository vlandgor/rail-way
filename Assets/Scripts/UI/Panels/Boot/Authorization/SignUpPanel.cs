using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Boot.Authorization
{
    public class SignUpPanel : BasePanel
    {
        [SerializeField] private TMP_InputField _emailInputField;
        [SerializeField] private TMP_InputField _passwordInputField;
        [SerializeField] private TMP_InputField _confirmPasswordInputField;
        
        [SerializeField] private Button _signUpButton;
        [SerializeField] private Button _backToSignInButton;

        private void Start()
        {
            _signUpButton.onClick.AddListener(HandleSignUpButtonClicked);
            _backToSignInButton.onClick.AddListener(HandleBackToSignInButtonClicked);
        }
        
        private void OnDestroy()
        {
            _signUpButton.onClick.RemoveListener(HandleSignUpButtonClicked);
            _backToSignInButton.onClick.RemoveListener(HandleBackToSignInButtonClicked);
        }

        private void HandleSignUpButtonClicked()
        {
            
        }

        private void HandleBackToSignInButtonClicked()
        {
            
        }
    }
}