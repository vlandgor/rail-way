using Services.Matchmaking;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.Home
{
    public class HomePanel : BasePanel
    {
        [SerializeField] private Button _playButton;

        private void Start()
        {
            _playButton.onClick.AddListener(HandlePlayButtonClicked);
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveListener(HandlePlayButtonClicked);
        }

        private async void HandlePlayButtonClicked()
        {
            _playButton.interactable = false;
            await MatchmakingService.Instance.PlayAsync();
        }
    }
}