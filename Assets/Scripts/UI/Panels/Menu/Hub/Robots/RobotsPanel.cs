using System;
using Game.Core.Player.Customization.Data;
using UI.Base;
using UI.Panels.Menu.Hub.Robots.AvatarCustomization;
using UnityEngine;

namespace UI.Panels.Menu.Hub.Robots
{
    public class RobotsPanel : BasePanel
    {
        [SerializeField] private AvatarCustomizationCollection _avatarCustomizationCollection;
        [SerializeField] private AvatarCustomizationListController _avatarCustomizationListController;

        public override void Enable()
        {
            base.Enable();
            
            _avatarCustomizationListController.Data = _avatarCustomizationCollection.AvatarItems;
        }
    }
}