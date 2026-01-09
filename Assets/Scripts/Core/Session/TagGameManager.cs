using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace Core.Session
{
    public class TagGameManager : NetworkBehaviour, ITagGameStatus
    {
        public static ITagGameStatus Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float _totalMatchDuration = 20f;
        [SerializeField] private float _initialCountdown = 3f;

        private readonly NetworkVariable<float> _countdownTime = new(0f);
        private readonly NetworkVariable<float> _matchTimer = new(0f);
        private readonly NetworkVariable<bool> _gameActive = new(false);
        private readonly NetworkVariable<ulong> _taggerId = new();
        private readonly NetworkVariable<bool> _isTagCooldownActive = new(false);

        public NetworkList<PlayerTagData> PlayerScores;

        public event Action<float> OnCountdownChanged;
        public event Action<float> OnMatchTimerChanged;
        public event Action<bool> OnGameActiveChanged;
        public event Action<ulong> OnTaggerChanged;

        public float CurrentCountdown => _countdownTime.Value;
        public float MatchTimer => _matchTimer.Value;
        public bool IsGameActive => _gameActive.Value;
        public ulong CurrentTaggerId => _taggerId.Value;
        public bool IsTagCooldownActive => _isTagCooldownActive.Value;

        private const float TAG_COOLDOWN_DURATION = 1.5f;

        public bool IsLocalPlayerTagger =>
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.LocalClientId == _taggerId.Value;

        // =========================
        // Lifecycle
        // =========================

        private void Awake()
        {
            if (Instance != null && Instance != (ITagGameStatus)this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            PlayerScores = new NetworkList<PlayerTagData>();
        }

        public override void OnNetworkSpawn()
        {
            _countdownTime.OnValueChanged += (_, v) => OnCountdownChanged?.Invoke(v);
            _matchTimer.OnValueChanged += (_, v) => OnMatchTimerChanged?.Invoke(v);
            _gameActive.OnValueChanged += (_, v) => OnGameActiveChanged?.Invoke(v);
            _taggerId.OnValueChanged += (_, v) => OnTaggerChanged?.Invoke(v);

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                StartCoroutine(StartGameRoutine());
            }
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == (ITagGameStatus)this)
                Instance = null;

            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            }
        }

        // =========================
        // Server logic
        // =========================

        private void HandleClientConnected(ulong clientId)
        {
            AddPlayerToScoreboard(clientId);
        }

        private void AddPlayerToScoreboard(ulong clientId)
        {
            for (int i = 0; i < PlayerScores.Count; i++)
            {
                if (PlayerScores[i].ClientId == clientId)
                    return;
            }

            PlayerScores.Add(new PlayerTagData
            {
                ClientId = clientId,
                TimeAsTagger = 0f
            });
        }

        private IEnumerator StartGameRoutine()
        {
            // 🔑 IMPORTANT CHANGE:
            // Wait for at least ONE player (solo host allowed)
            while (NetworkManager.Singleton.ConnectedClientsList.Count < 1)
                yield return null;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                AddPlayerToScoreboard(client.ClientId);
            }

            // Countdown
            _countdownTime.Value = _initialCountdown;
            while (_countdownTime.Value > 0f)
            {
                yield return new WaitForSeconds(1f);
                _countdownTime.Value -= 1f;
            }

            // Pick tagger safely (even with 1 player)
            int count = NetworkManager.Singleton.ConnectedClientsList.Count;
            int randomIndex = UnityEngine.Random.Range(0, count);
            _taggerId.Value = NetworkManager.Singleton.ConnectedClientsList[randomIndex].ClientId;

            _matchTimer.Value = _totalMatchDuration;
            _gameActive.Value = true;
        }

        private void Update()
        {
            if (!IsServer || !_gameActive.Value)
                return;

            if (_matchTimer.Value > 0f)
            {
                _matchTimer.Value -= Time.deltaTime;

                for (int i = 0; i < PlayerScores.Count; i++)
                {
                    if (PlayerScores[i].ClientId == _taggerId.Value)
                    {
                        var data = PlayerScores[i];
                        data.TimeAsTagger += Time.deltaTime;
                        PlayerScores[i] = data;
                        break;
                    }
                }
            }
            else
            {
                _matchTimer.Value = 0f;
                _gameActive.Value = false;
            }
        }

        // =========================
        // Tagging
        // =========================

        public void ReportTag(ulong newTaggerId)
        {
            if (!IsServer)
                return;

            if (_isTagCooldownActive.Value || !_gameActive.Value)
                return;

            _taggerId.Value = newTaggerId;
            StartCoroutine(TagCooldownRoutine());
        }

        private IEnumerator TagCooldownRoutine()
        {
            _isTagCooldownActive.Value = true;
            yield return new WaitForSeconds(TAG_COOLDOWN_DURATION);
            _isTagCooldownActive.Value = false;
        }

        // =========================
        // Public API
        // =========================

        public List<PlayerTagData> GetLeaderboard()
        {
            List<PlayerTagData> list = new List<PlayerTagData>();

            foreach (var player in PlayerScores)
            {
                list.Add(player);
            }

            list.Sort((a, b) => a.TimeAsTagger.CompareTo(b.TimeAsTagger));
            return list;
        }
    }
}
