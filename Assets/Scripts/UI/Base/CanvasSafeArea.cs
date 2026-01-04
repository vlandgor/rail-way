namespace UI.Base
{
    using UnityEngine;
    using UnityEngine.Assertions;

    namespace _FLOWR.Scripts.UI.Base
    {
        /// <summary>
        /// Adjusts a RectTransform to match the device safe area.
        /// Ensures UI stays clear of notches and system UI.
        /// </summary>
        public class CanvasSafeArea : MonoBehaviour
        {
            private RectTransform _rectTransform;

            private void Awake()
            {
                _rectTransform = GetComponent<RectTransform>();
                Assert.IsNotNull(_rectTransform);
            }

            /// <summary>
            /// Updates the layout when the object becomes active.
            /// </summary>
            private void OnEnable()
            {
                UpdateSafeArea();
            }

#if UNITY_EDITOR
            /// <summary>
            /// Continuously updates in editor to reflect resolution changes.
            /// </summary>
            private void Update()
            {
                UpdateSafeArea();
            }
#endif

            /// <summary>
            /// Applies the current screen safe area to the RectTransform anchors.
            /// </summary>
            private void UpdateSafeArea()
            {
                Rect safeArea = Screen.safeArea;

                Rect normalizedSafeArea = new Rect(
                    safeArea.x / Screen.width,
                    safeArea.y / Screen.height,
                    safeArea.width / Screen.width,
                    safeArea.height / Screen.height
                );

                _rectTransform.anchorMin = normalizedSafeArea.min;
                _rectTransform.anchorMax = normalizedSafeArea.max;
            }
        }
    }
}