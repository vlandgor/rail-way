using UnityEngine;
using UnityEngine.UI;

namespace UI.Utilities
{
    [ExecuteAlways]
    public class ToggleColorSwap : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Toggle _toggle;
        [SerializeField] private  Graphic _targetGraphic;

        [Header("Colors")]
        [SerializeField] private Color _onColor = Color.white;
        [SerializeField] private Color _offColor = Color.gray;

        private void OnEnable()
        {
            if (_toggle == null)
                _toggle = GetComponent<Toggle>();

            if (_toggle != null)
            {
                _toggle.onValueChanged.AddListener(OnToggleChanged);
                UpdateColor();
            }
        }

        private void OnDisable()
        {
            if (_toggle != null)
                _toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }

        private void OnValidate()
        {
            UpdateColor();
        }

        private void OnToggleChanged(bool isOn)
        {
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (_targetGraphic == null || _toggle == null)
                return;

            _targetGraphic.color = _toggle.isOn ? _onColor : _offColor;
        }
    }
}