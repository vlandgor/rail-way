using Core.Session;
using TMPro;
using UnityEngine;

namespace UI.Panels.Game.GameTop
{
    public class GameTopPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI uiStatusText;
        [SerializeField] private TextMeshProUGUI uiTimerText;

        private void Start()
        {
            var status = TagGameManager.Instance;
            if (status != null)
            {
                status.OnCountdownChanged += HandleCountdownChanged;
                status.OnGameActiveChanged += HandleGameActiveChanged;
                status.OnTaggerChanged += HandleTaggerChanged;
                status.OnMatchTimerChanged += HandleMatchTimerChanged;
                
                RefreshUI();
            }
        }

        private void OnDestroy()
        {
            var status = TagGameManager.Instance;
            if (status != null)
            {
                status.OnCountdownChanged -= HandleCountdownChanged;
                status.OnGameActiveChanged -= HandleGameActiveChanged;
                status.OnTaggerChanged -= HandleTaggerChanged;
                status.OnMatchTimerChanged -= HandleMatchTimerChanged;
            }
        }

        private void HandleCountdownChanged(float time) => RefreshUI();
        private void HandleGameActiveChanged(bool active) => RefreshUI();
        private void HandleTaggerChanged(ulong id) => RefreshUI();
        private void HandleMatchTimerChanged(float time) => UpdateTimerUI(time);

        private void UpdateTimerUI(float time)
        {
            if (uiTimerText != null)
            {
                uiTimerText.text = $"TIME: {Mathf.CeilToInt(time)}s";
            }
        }

        private void RefreshUI()
        {
            var status = TagGameManager.Instance;
            if (uiStatusText == null || status == null) return;

            if (!status.IsGameActive)
            {
                if (status.MatchTimer <= 0 && status.CurrentCountdown <= 0)
                {
                    uiStatusText.text = "GAME OVER!";
                }
                else
                {
                    uiStatusText.text = status.CurrentCountdown > 0 
                        ? $"STARTING IN: {Mathf.CeilToInt(status.CurrentCountdown)}" 
                        : "WAITING FOR PLAYERS";
                }
            }
            else
            {
                uiStatusText.text = status.IsLocalPlayerTagger ? "YOU ARE IT! TAG SOMEONE!" : "RUN!";
            }
        }
    }
}