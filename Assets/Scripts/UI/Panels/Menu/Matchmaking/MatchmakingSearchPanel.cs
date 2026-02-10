using System;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.Matchmaking
{
    public class MatchmakingSearchPanel : BasePanel
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private LobbyPlayersContainer _lobbyPlayersContainer;
        [SerializeField] private LoadingWidget _loadingWidget;
        [SerializeField] private CountdownWidget _countdownWidget;
        [SerializeField] private Button _cancelButton;

        private readonly string matchSearchText = "Looking for opponents...";
        private readonly string matchfoundText = "Match found!";

        private void Start()
        {
            _cancelButton.onClick.AddListener(HandleCancelButtonClicked);
        }

        private void OnDestroy()
        {
            _cancelButton.onClick.RemoveListener(HandleCancelButtonClicked);
        }
        
        private void HandleCancelButtonClicked()
        {
            
        }
    }
}