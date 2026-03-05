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

        public void AddPlayer(SearchPlayer player)
        {
            if (_activePlayers.ContainsKey(player.playerId)) return;

            foreach (var panel in _pool)
            {
                if (!panel.IsOccupied)
                {
                    panel.SetPlayerPanelActive(null, player.playerId); //TODO: Change it to use PlayerName
                    _activePlayers.Add(player.playerId, panel);
                    return;
                }
            }
        }

        public void RemovePlayer(string playerId)
        {
            if (_activePlayers.TryGetValue(playerId, out LobbyPlayerPanel panel))
            {
                panel.SetPlayerPanelInactive();
                _activePlayers.Remove(playerId);
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