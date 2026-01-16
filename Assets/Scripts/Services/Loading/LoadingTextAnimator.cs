using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Services.Loading
{
    public class LoadingTextAnimator : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private float _interval = 0.35f;

        private CancellationTokenSource _cts;

        public void Play()
        {
            _cts = new CancellationTokenSource();
            AnimateAsync(_cts.Token).Forget();
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTaskVoid AnimateAsync(CancellationToken token)
        {
            const string baseText = "Loading";
            int dots = 0;

            while (!token.IsCancellationRequested)
            {
                _loadingText.text = baseText + new string('.', dots);
                dots = (dots + 1) % 4;

                await UniTask.Delay(System.TimeSpan.FromSeconds(_interval), cancellationToken: token);
            }
        }
    }
}