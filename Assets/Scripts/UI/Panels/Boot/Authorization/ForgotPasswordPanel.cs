using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Boot.Authorization
{
    public class ForgotPasswordPanel : BasePanel
    {
        [SerializeField] private TMP_InputField _emailInputField;
        
        [SerializeField] private Button _sendLinkButton;
        [SerializeField] private Button _backToSignInButton;

        private void Start()
        {
            _sendLinkButton.onClick.AddListener(HandleSendLinkButtonClicked);
            _backToSignInButton.onClick.AddListener(HandleBackToSignInButtonClicked);
        }
        
        private void OnDestroy()
        {
            _sendLinkButton.onClick.RemoveListener(HandleSendLinkButtonClicked);
            _backToSignInButton.onClick.RemoveListener(HandleBackToSignInButtonClicked);
        }

        private void HandleSendLinkButtonClicked()
        {
            
        }

        private void HandleBackToSignInButtonClicked()
        {
            
        }
    }
}