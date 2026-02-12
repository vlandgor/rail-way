using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Services.Loading.Curtains
{
    public class QuickLoadingCurtains : MonoBehaviour, ILoadingCurtain
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("UI Elements")] 
        [SerializeField] private RectTransform _spinningIcon;
        
        [Header("Fade")]
        [SerializeField] private float _fadeDuration = 0.15f;
        [SerializeField] private Ease _fadeEase = Ease.Linear;
        
        [Header("Spin Animation")]
        [SerializeField] private float _spinDuration = 1f;
        [SerializeField] private Ease _spinEase = Ease.Linear;

        private Tween _fadeTween;
        private Tween _spinTween;

        private void Awake()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            if (_spinningIcon != null)
            {
                _spinningIcon.gameObject.SetActive(false);
            }
        }

        public async UniTask Show(bool instant = false)
        {
            KillTweens();

            gameObject.SetActive(true);

            _canvasGroup.interactable = false; // Quick loader shouldn't block interactions
            _canvasGroup.blocksRaycasts = false; // Transparent background allows clicks through
            
            if (_spinningIcon != null)
            {
                _spinningIcon.gameObject.SetActive(true);
                StartSpinAnimation();
            }

            if (instant)
            {
                _canvasGroup.alpha = 1f;
                return;
            }

            _fadeTween = _canvasGroup
                .DOFade(1f, _fadeDuration)
                .SetEase(_fadeEase);

            await _fadeTween.AsyncWaitForCompletion();
        }

        public async UniTask Hide(bool instant = false)
        {
            KillTweens();

            if (instant)
            {
                _canvasGroup.alpha = 0f;
                
                if (_spinningIcon != null)
                {
                    _spinningIcon.gameObject.SetActive(false);
                }
                
                gameObject.SetActive(false);
                return;
            }

            _fadeTween = _canvasGroup
                .DOFade(0f, _fadeDuration)
                .SetEase(_fadeEase);

            await _fadeTween.AsyncWaitForCompletion();

            if (_spinningIcon != null)
            {
                _spinningIcon.gameObject.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        public void UpdateProgress(float progress, string description)
        {
            // Quick loader doesn't show progress details
            // It's just a simple spinner for quick operations
        }

        private void StartSpinAnimation()
        {
            if (_spinningIcon == null) return;
            
            _spinTween = _spinningIcon
                .DORotate(new Vector3(0f, 0f, -360f), _spinDuration, RotateMode.FastBeyond360)
                .SetEase(_spinEase)
                .SetLoops(-1, LoopType.Restart);
        }

        private void KillTweens()
        {
            if (_fadeTween != null && _fadeTween.IsActive())
            {
                _fadeTween.Kill();
            }
            
            if (_spinTween != null && _spinTween.IsActive())
            {
                _spinTween.Kill();
            }
        }

        private void OnDestroy()
        {
            KillTweens();
        }
    }
}