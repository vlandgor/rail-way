using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace UI.Panels.Menu.Matchmaking
{
    public class CountdownWidget : MonoBehaviour
    {
        public event Action OnCountdownComplete;
        
        [Header("References")]
        [SerializeField] private TextMeshProUGUI countdownText;
        
        [Header("Settings")]
        [SerializeField] private string textFormat = "{0}";
        
        private CancellationTokenSource cancellationTokenSource;
        
        private void Awake()
        {
            if (countdownText == null)
                countdownText = GetComponent<TextMeshProUGUI>();
            
            gameObject.SetActive(false);
        }
        
        public void StartCountdown(int seconds)
        {
            StartCountdownAsync(seconds).Forget();
        }
        
        public async UniTask StartCountdownAsync(int seconds)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            
            gameObject.SetActive(true);
            
            try
            {
                for (int i = seconds; i > 0; i--)
                {
                    countdownText.text = string.Format(textFormat, i);
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationTokenSource.Token);
                }
                
                OnCountdownComplete?.Invoke();
                gameObject.SetActive(false);
            }
            catch (OperationCanceledException)
            {
                // Countdown was cancelled
            }
        }
        
        public void Stop()
        {
            cancellationTokenSource?.Cancel();
            gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }
    }
}