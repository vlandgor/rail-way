using System;
using TMPro;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.SearchOpponents
{
    public class SearchOpponentsPanel : BasePanel
    {
        [SerializeField] private TextMeshProUGUI _playersFoundText;
        [SerializeField] private TextMeshProUGUI _tipText;
        [SerializeField] private Button _cancelButton;

        private void Start()
        {
            _cancelButton.onClick.AddListener(HandleCancelButtonClicked);
        }

        private void OnDestroy()
        {
            _cancelButton.onClick.RemoveListener(HandleCancelButtonClicked);
        }

        private void HandleCancelButtonClicked()
        {
            
        }
    }
}