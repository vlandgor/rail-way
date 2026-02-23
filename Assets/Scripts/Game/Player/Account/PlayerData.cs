using System;
using UnityEngine;

namespace Game.Player.Account
{
    [Serializable]
    public class PlayerData
    {
        private string playerId;
        private string username;
        private int coins;
        private int level;
        private int experience;
        private string createdAt;

        public string PlayerId
        {
            get => playerId;
            set => playerId = value;
        }

        public string Username
        {
            get => username;
            set => username = value;
        }

        public int Coins
        {
            get => coins;
            set => coins = value;
        }

        public int Level
        {
            get => level;
            set => level = value;
        }

        public int Experience
        {
            get => experience;
            set => experience = value;
        }

        public DateTime CreatedAt
        {
            get => string.IsNullOrEmpty(createdAt) ? DateTime.Now : DateTime.Parse(createdAt);
            set => createdAt = value.ToString("o");
        }

        public PlayerData()
        {
        }

        public PlayerData(string username)
        {
            playerId = GeneratePlayerId();
            this.username = username;
            coins = 0;
            level = 1;
            experience = 0;
            createdAt = DateTime.Now.ToString("o");
        }

        // Generate unique player ID
        private string GeneratePlayerId()
        {
            return "PLR_" + Guid.NewGuid().ToString().Substring(0, 12).ToUpper();
        }
    }
}