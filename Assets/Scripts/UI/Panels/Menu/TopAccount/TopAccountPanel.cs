using System;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.TopAccount
{
    public class TopAccountPanel : BasePanel
    {
        [SerializeField] private Image _playerIcon;
        [SerializeField] private TextMeshProUGUI _playerUsernameText;
        [SerializeField] private TextMeshProUGUI _playerLevelText;
        [SerializeField] private Button _settingsButton;

        private void Start()
        {
            _settingsButton.onClick.AddListener(HandleSettingsButtonClicked);
        }

        private void OnDestroy()
        {
            _settingsButton.onClick.AddListener(HandleSettingsButtonClicked);
        }

        private void SetPlayerInfo()
        {
            
        }

        private void HandleSettingsButtonClicked()
        {
            
        }
    }
}