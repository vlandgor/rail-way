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

        public bool IsOccupied { get; private set; }

        public void SetPlayerPanelActive(Sprite playerIconImage, string playerUsername)
        {
            IsOccupied = true;
            _playerIconImage.sprite = playerIconImage;
            _playerUsername.text = playerUsername;
            
            _playerSearchingPanel.SetActive(false);
            _playerFoundPanel.SetActive(true);
        }

        public void SetPlayerPanelInactive()
        {
            IsOccupied = false;
            _playerUsername.text = string.Empty;
            
            _playerSearchingPanel.SetActive(true);
            _playerFoundPanel.SetActive(false);
        }
    }
}