using System;

namespace Core.Session
{
    public interface ITagGameStatus
    {
        event Action<float> OnCountdownChanged;
        event Action<bool> OnGameActiveChanged;
        event Action<ulong> OnTaggerChanged;
        float CurrentCountdown { get; }
        bool IsGameActive { get; }
        bool IsLocalPlayerTagger { get; }
        ulong CurrentTaggerId { get; }
        bool IsTagCooldownActive { get; } 
    }
}