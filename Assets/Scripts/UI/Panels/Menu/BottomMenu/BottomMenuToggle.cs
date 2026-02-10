using System;
using UnityEngine;
using UnityEngine.UI;

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

        public bool IsOn
        {
            get => _toggle.isOn;
            set => _toggle.isOn = value;
        }
        
        public BottomMenuCategory BottomMenuCategory => _bottomMenuCategory;

        private void Start()
        {
            _toggle.onValueChanged.AddListener(HandleToggleValueChanged);
        }

        private void OnDestroy()
        {
            _toggle.onValueChanged.AddListener(HandleToggleValueChanged);
        }

        private void HandleToggleValueChanged(bool isOn)
        {
            _layoutElement.flexibleWidth = isOn ? 1.8f : 1f;
            _backgroundSelectedImage.gameObject.SetActive(isOn);
            
            if (isOn)
            {
                OnSelected?.Invoke(_bottomMenuCategory);
            }
        }
    }
}