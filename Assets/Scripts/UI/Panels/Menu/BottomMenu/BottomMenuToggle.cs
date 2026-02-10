using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace UI.Panels.Menu.BottomMenu
{
    public class BottomMenuToggle : MonoBehaviour
    {
        public event Action<BottomMenuCategory> OnSelected;
        
        [SerializeField] private BottomMenuCategory _bottomMenuCategory;
        
        [Space]
        [SerializeField] private Toggle _toggle;
        [SerializeField] private LayoutElement _layoutElement;
        [SerializeField] private Image _backgroundSelectedImage;

        [Space]
        [SerializeField] private int _unselectedWidth;

        private Tweener _widthTweener;

        public bool IsOn
        {
            get => _toggle.isOn;
            set => _toggle.isOn = value;
        }
        
        public BottomMenuCategory BottomMenuCategory => _bottomMenuCategory;
        public int SiblingIndex => transform.GetSiblingIndex();

        private void Start()
        {
            _toggle.onValueChanged.AddListener(HandleToggleValueChanged);
        }

        private void OnDestroy()
        {
            _toggle.onValueChanged.RemoveListener(HandleToggleValueChanged);
            _widthTweener?.Kill();
        }

        private void HandleToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                OnSelected?.Invoke(_bottomMenuCategory);
            }
        }

        public void AnimateSelection(bool isOn, float delay, float duration, Ease ease)
        {
            _widthTweener?.Kill();

            if (isOn)
            {
                // Show background immediately
                _backgroundSelectedImage.gameObject.SetActive(true);
        
                // Expanding
                _layoutElement.flexibleWidth = 1;
                _widthTweener = DOTween.To(
                        () => _layoutElement.preferredWidth,
                        x => _layoutElement.preferredWidth = x,
                        -1,
                        duration
                    )
                    .SetDelay(delay)
                    .SetEase(ease);
            }
            else
            {
                // Hide background immediately
                _backgroundSelectedImage.gameObject.SetActive(false);
        
                // Contracting
                _layoutElement.flexibleWidth = -1;
                _widthTweener = DOTween.To(
                        () => _layoutElement.preferredWidth,
                        x => _layoutElement.preferredWidth = x,
                        _unselectedWidth,
                        duration
                    )
                    .SetDelay(delay)
                    .SetEase(ease);
            }
        }
    }
}