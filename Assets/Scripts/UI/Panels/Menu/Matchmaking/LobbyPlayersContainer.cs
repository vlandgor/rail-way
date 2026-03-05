using System.Collections.Generic;
using Game.Multiplayer.Matchmaking.Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.Matchmaking
{
    public class LobbyPlayersContainer : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private LobbyPlayerPanel _lobbyPlayerPanelPrefab;
        
        private readonly List<LobbyPlayerPanel> _pool = new();
        private readonly Dictionary<string, LobbyPlayerPanel> _activePlayers = new();

        public void InitializeSlots(int count)
        {
            Clear();

            for (int i = 0; i < count; i++)
            {
                LobbyPlayerPanel panel = Instantiate(_lobbyPlayerPanelPrefab, _scrollRect.content);
                panel.SetPlayerPanelInactive();
                _pool.Add(panel);
            }
            
            HandleScrollRectValueChanged(Vector2.zero);
        }

        public void SyncPlayers(IEnumerable<SearchPlayer> players)
        {
            foreach (var panel in _pool)
            {
                panel.SetPlayerPanelInactive();
            }

            int index = 0;
            foreach (var player in players)
            {
                if (index >= _pool.Count) break;
        
                _pool[index].SetPlayerPanelActive(null, player.playerId); //TODO: change to player name
                index++;
            }
        }

        public void Clear()
        {
            foreach (var panel in _pool)
            {
                Destroy(panel.gameObject);
            }
            _pool.Clear();
            _activePlayers.Clear();
        }

        private void HandleScrollRectValueChanged(Vector2 position)
        {
            if (_scrollRect == null || _scrollRect.content == null) return;
            _scrollRect.vertical = _scrollRect.content.rect.height > _scrollRect.viewport.rect.height;
        }
    }
}