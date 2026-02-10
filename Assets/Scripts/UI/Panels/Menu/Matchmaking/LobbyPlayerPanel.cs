using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.Matchmaking
{
    public class LobbyPlayerPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _playerSearchingPanel;
        [SerializeField] private GameObject _playerFoundPanel;

        [SerializeField] private Image _playerIconImage;
        [SerializeField] private TextMeshProUGUI _playerUsername;

        public void SetPlayerPanelActive(Sprite playerIconImage, string playerUsername)
        {
            _playerIconImage.sprite = playerIconImage;
            _playerUsername.text = playerUsername;
            
            _playerFoundPanel.SetActive(true);
        }

        public void SetPlayerPanelInactive()
        {
            _playerFoundPanel.SetActive(false);
        }
    }
}