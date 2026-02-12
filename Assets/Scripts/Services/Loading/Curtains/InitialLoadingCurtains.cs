using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Services.Loading.Curtains
{
    public class InitialLoadingCurtains : MonoBehaviour, ILoadingCurtain
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("UI Elements")]
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TextMeshProUGUI _currentProcessText;
        [SerializeField] private TextMeshProUGUI _currentPercentageText;
        
        [Header("Fade")]
        [SerializeField] private float _fadeDuration = 0.35f;
        [SerializeField] private Ease _fadeEase = Ease.OutQuad;
        
        [Header("Progress Animation")]
        [SerializeField] private float _progressAnimationDuration = 0.3f;
        [SerializeField] private Ease _progressEase = Ease.OutCubic;

        private Tween _fadeTween;
        private Tween _progressTween;

        private void Awake()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            if (_progressSlider != null)
            {
                _progressSlider.value = 0f;
            }
        }

        public async UniTask Show(bool instant = false)
        {
            KillTweens();

            gameObject.SetActive(true);

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

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

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            if (instant)
            {
                _canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
                return;
            }

            _fadeTween = _canvasGroup
                .DOFade(0f, _fadeDuration)
                .SetEase(_fadeEase);

            await _fadeTween.AsyncWaitForCompletion();

            gameObject.SetActive(false);
        }

        public void UpdateProgress(float progress, string description)
        {
            progress = Mathf.Clamp01(progress);
            
            // Update progress bar with smooth animation
            if (_progressSlider != null)
            {
                _progressTween?.Kill();
                _progressTween = _progressSlider
                    .DOValue(progress, _progressAnimationDuration)
                    .SetEase(_progressEase);
            }
            
            // Update percentage text
            if (_currentPercentageText != null)
            {
                _currentPercentageText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
            
            // Update process description
            if (_currentProcessText != null && !string.IsNullOrEmpty(description))
            {
                _currentProcessText.text = description;
            }
        }

        private void KillTweens()
        {
            if (_fadeTween != null && _fadeTween.IsActive())
            {
                _fadeTween.Kill();
            }
            
            if (_progressTween != null && _progressTween.IsActive())
            {
                _progressTween.Kill();
            }
        }

        private void OnDestroy()
        {
            KillTweens();
        }
    }
}