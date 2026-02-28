using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Services.Data;
using Utilities;

namespace Game.Core.Player.Meta
{
    [Serializable]
    public class PlayerMetaData
    {
        public string Id;
        public string Name;
        public int Level = 1;
        public int Experience = 0;

        public PlayerMetaData()
        {
            Id = Guid.NewGuid().ToString().Substring(0, 8);
            Name = $"Player_{Id}";
        }
    }
    
    public class PlayerMeta : Singleton<PlayerMeta>
    {
        private const string SaveKey = "player_meta_data";
        
        private PlayerMetaData _metaData;
        
        public string Id =>  _metaData.Id;
        public string Name => _metaData.Name;
        public int Level => _metaData.Level;
        
        public async UniTask Initialize(CancellationToken ct = default)
        {
            _metaData = await DataService.Instance.LoadJsonOrDefaultAsync(SaveKey, new PlayerMetaData(), ct);
        }
        
        private async UniTask SaveAsync()
        {
            await DataService.Instance.SaveJsonAsync(SaveKey, _metaData);
        }
    }
}