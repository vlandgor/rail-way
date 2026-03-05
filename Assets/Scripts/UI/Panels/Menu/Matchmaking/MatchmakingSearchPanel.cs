using Game.Multiplayer.Matchmaking;
using Game.Multiplayer.Matchmaking.Data;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.Matchmaking
{
    public class MatchmakingSearchPanel : BasePanel
    {
        [SerializeField] private MatchmakingSearch _matchmakingSearch;
        
        [Header("UI Elements")]
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

            _matchmakingSearch.OnPlayerJoined += MatchmakingSearch_OnPlayerJoined;
            _matchmakingSearch.OnPlayerLeft += MatchmakingSearch_OnPlayerLeft;
        }

        private void OnDestroy()
        {
            _cancelButton.onClick.RemoveListener(HandleCancelButtonClicked);
        }

        public void StartSearch()
        {
            int maxPlayers = 7;
            
            _lobbyPlayersContainer.InitializeSlots(maxPlayers);
            _matchmakingSearch.StartMatchmakingSearch(maxPlayers);
            
            _loadingWidget.Enable();
            _titleText.text = matchSearchText;
        }

        public void CancelSearch()
        {
            _matchmakingSearch.CancelMatchmakingSearch();
        }

        private void HandleCancelButtonClicked()
        {
            CancelSearch();
            Disable();
        }
        
        private void MatchmakingSearch_OnPlayerJoined(SearchPlayer searchPlayer)
        {
            _lobbyPlayersContainer.AddPlayer(searchPlayer);
        }

        private void MatchmakingSearch_OnPlayerLeft(string playerId)
        {
            _lobbyPlayersContainer.RemovePlayer(playerId);
        }
    }
}