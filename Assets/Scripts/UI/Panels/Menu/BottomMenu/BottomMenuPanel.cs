using System;
using System.Collections.Generic;
using UI.Base;
using UnityEngine;

namespace UI.Panels.Menu.BottomMenu
{
    public class BottomMenuPanel : BasePanel
    {
        [SerializeField] private List<BottomMenuToggle> _menuToggles;

        private void Start()
        {
            foreach (BottomMenuToggle bottomMenuToggle in _menuToggles)
            {
                bottomMenuToggle.OnSelected += BottomMenuToggle_OnSelected;
            }
        }

        private void OnDestroy()
        {
            foreach (BottomMenuToggle bottomMenuToggle in _menuToggles)
            {
                bottomMenuToggle.OnSelected += BottomMenuToggle_OnSelected;
            }
        }

        private void BottomMenuToggle_OnSelected(BottomMenuCategory bottomMenuCategory)
        {
            
        }
    }
}