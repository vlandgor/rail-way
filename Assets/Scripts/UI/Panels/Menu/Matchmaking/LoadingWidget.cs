using DG.Tweening;
using UnityEngine;

namespace UI.Panels.Menu.Matchmaking
{
    public class LoadingWidget : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform iconTransform;
    
        [Header("Spin Settings")]
        [SerializeField] private float spinSpeed = 1f;
        [SerializeField] private bool isClockwise = true;
    
        private Tween spinTween;
    
        private void Awake()
        {
            if (iconTransform == null && transform.childCount > 0)
                iconTransform = transform.GetChild(0).GetComponent<RectTransform>();
        
            gameObject.SetActive(false);
        }
    
        public void Enable()
        {
            gameObject.SetActive(true);
            StartSpin();
        }
    
        public void Disable()
        {
            StopSpin();
            gameObject.SetActive(false);
        }
    
        private void StartSpin()
        {
            StopSpin();
            
            float angle = isClockwise ? -360f : 360f;
        
            spinTween = iconTransform
                .DORotate(new Vector3(0, 0, angle), spinSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }
    
        private void StopSpin()
        {
            spinTween?.Kill();
            if (iconTransform != null)
                iconTransform.localRotation = Quaternion.identity;
        }
    
        public void SetSpinSpeed(float speed)
        {
            spinSpeed = speed;
            if (spinTween != null && spinTween.IsActive())
            {
                StartSpin();
            }
        }
        
        public void SetClockwise(bool clockwise)
        {
            isClockwise = clockwise;
            if (spinTween != null && spinTween.IsActive())
            {
                StartSpin();
            }
        }
    
        private void OnDestroy()
        {
            StopSpin();
        }
    }
}