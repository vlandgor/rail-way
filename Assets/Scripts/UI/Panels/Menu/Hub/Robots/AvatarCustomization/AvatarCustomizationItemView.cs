using System;
using Game.Core.Player.Customization.Data;
using OneManEscapePlan.UIList.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.Menu.Hub.Robots.AvatarCustomization
{
    public class AvatarCustomizationItemView : UIListItemView<AvatarCustomizationItem>
    {
        public event Action<AvatarCustomizationItem> Selected;
        
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TextMeshProUGUI _avatarName;

        public bool IsOn => _toggle.isOn;
        
        private void Start()
        {
            _toggle.onValueChanged.AddListener(HandleToggleValueChanged);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _toggle.onValueChanged.RemoveListener(HandleToggleValueChanged);
        }

        public override void Refresh()
        {
            _avatarImage.sprite = model.AvatarSprite;
            _avatarName.text = model.AvatarName;
        }
        
        private void HandleToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                Selected?.Invoke(model);
            }
        }
    }
}