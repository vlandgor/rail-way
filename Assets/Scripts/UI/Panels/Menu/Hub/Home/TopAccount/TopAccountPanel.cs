using Meta;
using Meta.Player;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.Hub.Home.TopAccount
{
    public class TopAccountPanel : BasePanel
    {
        [SerializeField] private Image _playerIcon;
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _playerLevelText;
        [SerializeField] private Button _settingsButton;
        
        private PlayerMeta _playerMeta;

        private void Start()
        {
            _settingsButton.onClick.AddListener(HandleSettingsButtonClicked);

            _playerMeta = MetaManager.Instance.PlayerMeta;
            SetPlayerInfo();
        }

        private void OnDestroy()
        {
            _settingsButton.onClick.AddListener(HandleSettingsButtonClicked);
        }

        private void SetPlayerInfo()
        {
            _playerNameText.text = _playerMeta.Name;
            _playerLevelText.text = $"Level {_playerMeta.Level.ToString()}";
        }

        private void HandleSettingsButtonClicked()
        {
            
        }
    }
}