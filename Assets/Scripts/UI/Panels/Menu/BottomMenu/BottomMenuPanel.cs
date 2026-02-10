using System;
using System.Collections.Generic;
using UI.Base;
using UnityEngine;
using DG.Tweening;

namespace UI.Panels.Menu.BottomMenu
{
    public class BottomMenuPanel : BasePanel
    {
        [SerializeField] private List<BottomMenuToggle> _menuToggles;
        
        [Header("Animation Settings")]
        [SerializeField] private float _totalAnimationDuration = 0.4f;
        [SerializeField] private float _delayPerToggle = 0.05f;
        [SerializeField] private Ease _animationEase = Ease.OutCubic;

        private BottomMenuToggle _currentSelected;

        private void Start()
        {
            foreach (BottomMenuToggle bottomMenuToggle in _menuToggles)
            {
                bottomMenuToggle.OnSelected += BottomMenuToggle_OnSelected;
            }
            
            // Set initial selection
            _currentSelected = _menuToggles.Find(t => t.IsOn);
        }

        private void OnDestroy()
        {
            foreach (BottomMenuToggle bottomMenuToggle in _menuToggles)
            {
                bottomMenuToggle.OnSelected -= BottomMenuToggle_OnSelected; // Fixed: was +=
            }
        }

        private void BottomMenuToggle_OnSelected(BottomMenuCategory bottomMenuCategory)
        {
            var newSelected = _menuToggles.Find(t => t.BottomMenuCategory == bottomMenuCategory);
            
            if (newSelected == null || newSelected == _currentSelected)
                return;

            AnimateTransition(_currentSelected, newSelected);
            _currentSelected = newSelected;
        }

        private void AnimateTransition(BottomMenuToggle from, BottomMenuToggle to)
        {
            if (from == null || to == null)
                return;

            int fromIndex = from.SiblingIndex;
            int toIndex = to.SiblingIndex;
            
            // Determine direction (left to right or right to left)
            bool movingRight = toIndex > fromIndex;
            int start = Mathf.Min(fromIndex, toIndex);
            int end = Mathf.Max(fromIndex, toIndex);

            // Animate all toggles in the wave
            for (int i = 0; i < _menuToggles.Count; i++)
            {
                var toggle = _menuToggles[i];
                int toggleIndex = toggle.SiblingIndex;
                bool shouldBeSelected = (toggleIndex == toIndex);
                float delay = 0f;

                // Calculate delay based on distance from the start of the wave
                if (toggleIndex >= start && toggleIndex <= end)
                {
                    int distanceFromStart = movingRight ? (toggleIndex - start) : (end - toggleIndex);
                    delay = distanceFromStart * _delayPerToggle;
                }

                toggle.AnimateSelection(shouldBeSelected, delay, _totalAnimationDuration, _animationEase);
            }
        }
    }
}