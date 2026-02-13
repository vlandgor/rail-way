using Services.Matchmaking;
using UI.Base;
using UI.Panels.Menu.GameModes;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.Hub.Home
{
    public class HomePanel : BasePanel
    {
        [Header("Panels")]
        [SerializeField] private GameModesPanel _gameModesPanel;
        
        [Header("UI Elements")]
        [SerializeField] private Button _customLobbyButton;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _gameModesButton;

        private void Start()
        {
            _playButton.onClick.AddListener(HandlePlayButtonClicked);
            _customLobbyButton.onClick.AddListener(HandleCustomLobbyButtonClicked);
            _gameModesButton.onClick.AddListener(HandleGameModesButtonClicked);
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveListener(HandlePlayButtonClicked);
            _customLobbyButton.onClick.RemoveListener(HandleCustomLobbyButtonClicked);
            _gameModesButton.onClick.RemoveListener(HandleGameModesButtonClicked);
        }
        
        private void HandleCustomLobbyButtonClicked()
        {
            
        }

        private async void HandlePlayButtonClicked()
        {
            _playButton.interactable = false;
            await MatchmakingService.Instance.PlayAsync();
        }
        
        private void HandleGameModesButtonClicked()
        {
            _gameModesPanel.Enable();
        }
    }
}