using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Services.Data;
using Utilities;

namespace Game.Core.Player.Meta
{
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