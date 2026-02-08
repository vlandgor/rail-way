using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Boot.Authorization
{
    public class ResetPasswordPanel : BasePanel
    {
        [SerializeField] private TMP_InputField _passwordInputField;
        [SerializeField] private TMP_InputField _confirmPasswordInputField;
        
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _backToSignInButton;

        private void Start()
        {
            _confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
            _backToSignInButton.onClick.AddListener(HandleBackToSignInButtonClicked);
        }
        
        private void OnDestroy()
        {
            _confirmButton.onClick.RemoveListener(HandleConfirmButtonClicked);
            _backToSignInButton.onClick.RemoveListener(HandleBackToSignInButtonClicked);
        }

        private void HandleConfirmButtonClicked()
        {
            
        }

        private void HandleBackToSignInButtonClicked()
        {
            
        }
    }
}