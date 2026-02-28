using System;

namespace Game.Core.Player.Meta
{
    public interface IRemovePlayerMetaData
    {
        public string Id { get; }
        public string Name { get; }
        public int Level { get; }
    }
    
    [Serializable]
    public class PlayerMetaData : IRemovePlayerMetaData
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public int Level { get; private set; } = 1;
        public int Experience = 0;

        public PlayerMetaData()
        {
            Id = Guid.NewGuid().ToString().Substring(0, 8);
            Name = $"Player_{Id}";
        }
    }
}