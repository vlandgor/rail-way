using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace Services.Loading
{
    public class LoadingCurtains : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LoadingTextAnimator _loadingTextAnimator;

        [Header("Animation")]
        [SerializeField] private float _fadeDuration = 0.35f;
        [SerializeField] private Ease _fadeEase = Ease.OutQuad;

        private Tween _fadeTween;

        private void Awake()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public async UniTask Show()
        {
            KillTween();

            gameObject.SetActive(true);

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            _loadingTextAnimator.Play();

            _fadeTween = _canvasGroup
                .DOFade(1f, _fadeDuration)
                .SetEase(_fadeEase);

            await _fadeTween.AsyncWaitForCompletion();
        }

        public async UniTask Hide()
        {
            KillTween();

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _fadeTween = _canvasGroup
                .DOFade(0f, _fadeDuration)
                .SetEase(_fadeEase);

            await _fadeTween.AsyncWaitForCompletion();

            _loadingTextAnimator.Stop();
            gameObject.SetActive(false);
        }

        private void KillTween()
        {
            if (_fadeTween != null && _fadeTween.IsActive())
            {
                _fadeTween.Kill();
            }
        }
    }
}