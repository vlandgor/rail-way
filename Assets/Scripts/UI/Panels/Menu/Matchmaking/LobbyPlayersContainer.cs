using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.Matchmaking
{
    public class LobbyPlayersContainer : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private LobbyPlayerPanel _lobbyPlayerPanelPrefab;
        
        private void Start()
        {
            _scrollRect.onValueChanged.AddListener(HandleScrollRectValueChanged);
        }
        
        private void OnDestroy()
        {
            _scrollRect.onValueChanged.RemoveListener(HandleScrollRectValueChanged);
        }
        
        private void HandleScrollRectValueChanged(Vector2 position)
        {
            if (_scrollRect == null || _scrollRect.content == null) 
                return;
            
            RectTransform viewport = _scrollRect.viewport;
            RectTransform content = _scrollRect.content;
            
            bool needsScroll = content.rect.height > viewport.rect.height;
            _scrollRect.vertical = needsScroll;
        }
    }
}