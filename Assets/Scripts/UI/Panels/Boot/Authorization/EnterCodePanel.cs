using System.Collections.Generic;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Boot.Authorization
{
    public class EnterCodePanel : BasePanel
    {
        [SerializeField] private List<TMP_InputField> _codeInputFields;
        
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _resendCodeButton;
        [SerializeField] private Button _backToSignInButton;

        private void Start()
        {
            _confirmButton.onClick.AddListener(HandleConfirmButtonClicked);
            _resendCodeButton.onClick.AddListener(HandleResendLinkButtonClicked);
            _backToSignInButton.onClick.AddListener(HandleBackToSignInButtonClicked);
        }

        private void OnDestroy()
        {
            _confirmButton.onClick.RemoveListener(HandleConfirmButtonClicked);
            _resendCodeButton.onClick.RemoveListener(HandleResendLinkButtonClicked);
            _backToSignInButton.onClick.RemoveListener(HandleBackToSignInButtonClicked);
        }

        private void HandleConfirmButtonClicked()
        {
            
        }
        
        private void HandleResendLinkButtonClicked()
        {
            
        }

        private void HandleBackToSignInButtonClicked()
        {
            
        }
    }
}