using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Core.Session
{
    public class TagGameManager : NetworkBehaviour, ITagGameStatus
    {
        public static ITagGameStatus Instance { get; private set; }

        private NetworkVariable<float> _countdownTime = new NetworkVariable<float>(3f);
        private NetworkVariable<bool> _gameActive = new NetworkVariable<bool>(false);
        private NetworkVariable<ulong> _taggerId = new NetworkVariable<ulong>();
        private NetworkVariable<bool> _isTagCooldownActive = new NetworkVariable<bool>(false);

        public event Action<float> OnCountdownChanged;
        public event Action<bool> OnGameActiveChanged;
        public event Action<ulong> OnTaggerChanged;

        public float CurrentCountdown => _countdownTime.Value;
        public bool IsGameActive => _gameActive.Value;
        public ulong CurrentTaggerId => _taggerId.Value;
        public bool IsTagCooldownActive => _isTagCooldownActive.Value;

        private const float TAG_COOLDOWN_DURATION = 1.5f;

        public bool IsLocalPlayerTagger => NetworkManager.Singleton != null && 
                                          NetworkManager.Singleton.LocalClientId == _taggerId.Value;

        private void Awake()
        {
            if (Instance != null && Instance != (ITagGameStatus)this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            _countdownTime.OnValueChanged += (oldVal, newVal) => OnCountdownChanged?.Invoke(newVal);
            _gameActive.OnValueChanged += (oldVal, newVal) => OnGameActiveChanged?.Invoke(newVal);
            _taggerId.OnValueChanged += (oldVal, newVal) => OnTaggerChanged?.Invoke(newVal);

            if (IsServer) StartCoroutine(StartGameRoutine());
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == (ITagGameStatus)this) Instance = null;
        }

        private IEnumerator StartGameRoutine()
        {
            while (NetworkManager.Singleton.ConnectedClientsList.Count < 2) yield return null;

            _countdownTime.Value = 3f;
            while (_countdownTime.Value > 0)
            {
                yield return new WaitForSeconds(1f);
                _countdownTime.Value -= 1f;
            }

            int randomIndex = UnityEngine.Random.Range(0, NetworkManager.Singleton.ConnectedClientsList.Count);
            _taggerId.Value = NetworkManager.Singleton.ConnectedClientsList[randomIndex].ClientId;
            _gameActive.Value = true;
        }

        public void ReportTag(ulong newTaggerId)
        {
            if (!IsServer || _isTagCooldownActive.Value) return;

            _taggerId.Value = newTaggerId;
            StartCoroutine(TagCooldownRoutine());
        }

        private IEnumerator TagCooldownRoutine()
        {
            _isTagCooldownActive.Value = true;
            yield return new WaitForSeconds(TAG_COOLDOWN_DURATION);
            _isTagCooldownActive.Value = false;
        }
    }
}