using System;

namespace Meta.Player
{
    [Serializable]
    public class PlayerData
    {
        public string Id;
        public string Name;
        public int Level = 1;
        public int Experience = 0;

        public PlayerData()
        {
            Id = Guid.NewGuid().ToString().Substring(0, 8);
            Name = $"Player_{Id}";
        }
    }
}