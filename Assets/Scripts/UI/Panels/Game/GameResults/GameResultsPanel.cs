using Core.Session;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Game.GameResults
{
    public class GameResultsPanel : MonoBehaviour
    {
        [SerializeField] private Button _homeButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _winnerText;

        private void Start()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _homeButton.onClick.AddListener(HandleHomeButtonClicked);

            if (TagGameManager.Instance != null)
            {
                TagGameManager.Instance.OnGameActiveChanged += HandleGameActiveChanged;
            }
        }

        private void OnDestroy()
        {
            _homeButton.onClick.RemoveListener(HandleHomeButtonClicked);

            if (TagGameManager.Instance != null)
            {
                TagGameManager.Instance.OnGameActiveChanged -= HandleGameActiveChanged;
            }
        }

        private void HandleGameActiveChanged(bool active)
        {
            if (!active && TagGameManager.Instance.MatchTimer <= 0)
            {
                ShowResults();
            }
        }

        private void ShowResults()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            var leaderboard = TagGameManager.Instance.GetLeaderboard();
            if (leaderboard != null && leaderboard.Count > 0)
            {
                var winner = leaderboard[0];
                _winnerText.text = $"WINNER: PLAYER {winner.ClientId}\nTIME AS IT: {Mathf.FloorToInt(winner.TimeAsTagger)}s";
            }
        }

        private void HandleHomeButtonClicked()
        {

        }
    }
}