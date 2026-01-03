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

        private NetworkVariable<float> _countdownTime = new NetworkVariable<float>(3f);
        private NetworkVariable<float> _matchTimer = new NetworkVariable<float>(20f);
        private NetworkVariable<bool> _gameActive = new NetworkVariable<bool>(false);
        private NetworkVariable<ulong> _taggerId = new NetworkVariable<ulong>();
        private NetworkVariable<bool> _isTagCooldownActive = new NetworkVariable<bool>(false);

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
            PlayerScores = new NetworkList<PlayerTagData>();
        }

        public override void OnNetworkSpawn()
        {
            _countdownTime.OnValueChanged += (oldVal, newVal) => OnCountdownChanged?.Invoke(newVal);
            _matchTimer.OnValueChanged += (oldVal, newVal) => OnMatchTimerChanged?.Invoke(newVal);
            _gameActive.OnValueChanged += (oldVal, newVal) => OnGameActiveChanged?.Invoke(newVal);
            _taggerId.OnValueChanged += (oldVal, newVal) => OnTaggerChanged?.Invoke(newVal);

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                StartCoroutine(StartGameRoutine());
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            AddPlayerToScoreboard(clientId);
        }

        private void AddPlayerToScoreboard(ulong clientId)
        {
            foreach (var data in PlayerScores)
            {
                if (data.ClientId == clientId) return;
            }
            PlayerScores.Add(new PlayerTagData { ClientId = clientId, TimeAsTagger = 0f });
        }

        private void Update()
        {
            if (!IsServer || !_gameActive.Value) return;

            if (_matchTimer.Value > 0)
            {
                _matchTimer.Value -= Time.deltaTime;

                for (int i = 0; i < PlayerScores.Count; i++)
                {
                    if (PlayerScores[i].ClientId == _taggerId.Value)
                    {
                        var updatedData = PlayerScores[i];
                        updatedData.TimeAsTagger += Time.deltaTime;
                        PlayerScores[i] = updatedData;
                        break;
                    }
                }
            }
            else
            {
                _matchTimer.Value = 0;
                _gameActive.Value = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == (ITagGameStatus)this) Instance = null;
            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            }
        }

        private IEnumerator StartGameRoutine()
        {
            while (NetworkManager.Singleton.ConnectedClientsList.Count < 2) yield return null;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                AddPlayerToScoreboard(client.ClientId);
            }

            _countdownTime.Value = 3f;
            while (_countdownTime.Value > 0)
            {
                yield return new WaitForSeconds(1f);
                _countdownTime.Value -= 1f;
            }

            int randomIndex = UnityEngine.Random.Range(0, NetworkManager.Singleton.ConnectedClientsList.Count);
            _taggerId.Value = NetworkManager.Singleton.ConnectedClientsList[randomIndex].ClientId;
            _gameActive.Value = true;
            _matchTimer.Value = 60f;
        }

        public void ReportTag(ulong newTaggerId)
        {
            if (!IsServer || _isTagCooldownActive.Value || _matchTimer.Value <= 0) return;

            _taggerId.Value = newTaggerId;
            StartCoroutine(TagCooldownRoutine());
        }

        private IEnumerator TagCooldownRoutine()
        {
            _isTagCooldownActive.Value = true;
            yield return new WaitForSeconds(TAG_COOLDOWN_DURATION);
            _isTagCooldownActive.Value = false;
        }

        public List<PlayerTagData> GetLeaderboard()
        {
            List<PlayerTagData> list = new List<PlayerTagData>();
            foreach (var item in PlayerScores) list.Add(item);
            return list.OrderBy(x => x.TimeAsTagger).ToList();
        }
    }
}