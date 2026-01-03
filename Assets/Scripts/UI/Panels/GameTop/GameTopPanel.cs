using TMPro;
using UnityEngine;
using Core.Session;

namespace UI.Panels.GameTop
{
    public class GameTopPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI uiStatusText;

        private void Start()
        {
            if (TagGameManager.Instance != null)
            {
                TagGameManager.Instance.OnCountdownChanged += HandleCountdownChanged;
                TagGameManager.Instance.OnGameActiveChanged += HandleGameActiveChanged;
                TagGameManager.Instance.OnTaggerChanged += HandleTaggerChanged;
                
                RefreshUI();
            }
        }

        private void OnDestroy()
        {
            if (TagGameManager.Instance != null)
            {
                TagGameManager.Instance.OnCountdownChanged -= HandleCountdownChanged;
                TagGameManager.Instance.OnGameActiveChanged -= HandleGameActiveChanged;
                TagGameManager.Instance.OnTaggerChanged -= HandleTaggerChanged;
            }
        }

        private void HandleCountdownChanged(float time) => RefreshUI();
        private void HandleGameActiveChanged(bool active) => RefreshUI();
        private void HandleTaggerChanged(ulong id) => RefreshUI();

        private void RefreshUI()
        {
            var manager = TagGameManager.Instance;
            if (uiStatusText == null || manager == null) return;

            if (!manager.IsGameActive)
            {
                uiStatusText.text = manager.CurrentCountdown > 0 
                    ? $"STARTING IN: {Mathf.CeilToInt(manager.CurrentCountdown)}" 
                    : "WAITING FOR PLAYERS";
            }
            else
            {
                uiStatusText.text = manager.IsLocalPlayerTagger ? "YOU ARE IT! TAG SOMEONE!" : "RUN!";
            }
        }
    }
}