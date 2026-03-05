using Game.Multiplayer;
using Game.Multiplayer.Matchmaking;
using UI.Base;
using UI.Panels.Menu.GameModes;
using UI.Panels.Menu.Matchmaking;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.Hub.Home
{
    public class HomePanel : BasePanel
    {
        [SerializeField] private MatchmakingSearch _matchmakingSearch;
        
        [Header("Panels")]
        [SerializeField] private GameModesPanel _gameModesPanel;
        [SerializeField] private MatchmakingSearchPanel _matchmakingSearchPanel;
        
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

        private void HandlePlayButtonClicked()
        {
            _matchmakingSearchPanel.Enable();
            _matchmakingSearchPanel.StartSearch();
        }
        
        private void HandleGameModesButtonClicked()
        {
            _gameModesPanel.Enable();
        }
    }
}