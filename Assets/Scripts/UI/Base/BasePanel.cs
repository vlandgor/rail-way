using UnityEngine;

namespace UI.Base
{
    /// <summary>
    /// Base class for UI panels controlled via a CanvasGroup.
    /// Handles enabling/disabling visibility and interaction.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BasePanel : MonoBehaviour
    {
        [SerializeField] protected bool _enableOnStart = false;
        
        [Space]
        [SerializeField] protected CanvasGroup _canvasGroup;
        
        /// <summary>
        /// Indicates whether the panel is currently enabled.
        /// </summary>
        public bool IsEnabled { get; private set; }
        
        private bool _lastEditorEnableState;

        protected virtual void Awake()
        {
            SetupCanvasGroup();
            
            if (_enableOnStart)
                Enable();
            else
                Disable();
        }

        /// <summary>
        /// Keeps editor state in sync when toggling enable-on-start.
        /// </summary>
        protected virtual void OnValidate()
        {
            if (Application.isPlaying) return;

            SetupCanvasGroup();

            if (_enableOnStart != _lastEditorEnableState)
            {
                _lastEditorEnableState = _enableOnStart;
                
                if (_enableOnStart)
                    EditorEnable();
                else
                    EditorDisable();
            }
        }
        
        /// <summary>
        /// Ensures a CanvasGroup reference exists.
        /// </summary>
        private void SetupCanvasGroup()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Shows the panel and enables interaction.
        /// </summary>
        public virtual void Enable()
        {
            IsEnabled = true;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
        }

        /// <summary>
        /// Hides the panel and disables interaction.
        /// </summary>
        public virtual void Disable()
        {
            IsEnabled = false;
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        /// <summary>
        /// Enables the panel visually in the editor.
        /// </summary>
        private void EditorEnable()
        {
            if (_canvasGroup == null) return;
            
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
        }

        /// <summary>
        /// Disables the panel visually in the editor.
        /// </summary>
        private void EditorDisable()
        {
            if (_canvasGroup == null) return;
            
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }
    }
}