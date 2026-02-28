using System;
using System.Collections.Generic;
using Game.Session;

namespace Core.Session
{
    public interface ITagGameStatus
    {
        event Action<float> OnCountdownChanged;
        event Action<float> OnMatchTimerChanged;
        event Action<bool> OnGameActiveChanged;
        event Action<ulong> OnTaggerChanged;
        
        float CurrentCountdown { get; }
        float MatchTimer { get; }
        bool IsGameActive { get; }
        bool IsLocalPlayerTagger { get; }
        ulong CurrentTaggerId { get; }
        bool IsTagCooldownActive { get; }
        
        List<PlayerTagData> GetLeaderboard();
    }
}