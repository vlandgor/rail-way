using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.GameModes
{
    public class GameModeOption : MonoBehaviour
    {
        public event Action<GameModeType> OnSelected;
        
        [SerializeField] private GameModeType _gameModeType;
        
        [Space]
        [SerializeField] private Toggle _toggle;

        public bool IsOn
        {
            get => _toggle.isOn;
            set => _toggle.isOn = value;
        }
        
        public GameModeType GameModeType => _gameModeType;

        private void Start()
        {
            _toggle.onValueChanged.AddListener(HandleToggleValueChanged);
        }

        private void OnDestroy()
        {
            _toggle.onValueChanged.RemoveListener(HandleToggleValueChanged);
        }

        private void HandleToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                OnSelected?.Invoke(GameModeType.Classic);
            }
        }
    }
}