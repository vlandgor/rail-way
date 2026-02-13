using System;
using System.Collections.Generic;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.GameModes
{
    public class GameModesPanel : BasePanel
    {
        [SerializeField] private Button _fullCloseButton;
        [SerializeField] private Button _smallCloseButton;
        
        [SerializeField] private List<GameModeOption>  _gameModeOptions;

        private void Start()
        {
            _fullCloseButton.onClick.AddListener(HandleCloseButtonClicked);

            foreach (GameModeOption gameModeOption in _gameModeOptions)
            {
                gameModeOption.OnSelected += GameModeOption_OnSelected;
            }
        }
        
        private void OnDestroy()
        {
            _fullCloseButton.onClick.RemoveListener(HandleCloseButtonClicked);
            
            foreach (GameModeOption gameModeOption in _gameModeOptions)
            {
                gameModeOption.OnSelected -= GameModeOption_OnSelected;
            }
        }

        private void HandleCloseButtonClicked()
        {
            Disable();
        }
        
        private void GameModeOption_OnSelected(GameModeType gameModeType)
        {
            
        }
    }
}