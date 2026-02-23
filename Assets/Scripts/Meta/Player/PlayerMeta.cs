using System.Threading;
using Cysharp.Threading.Tasks;
using Services.Data;

namespace Meta.Player
{
    public class PlayerMeta
    {
        private const string SaveKey = "player_meta_data";
        
        private PlayerData _data;
        
        public string Id =>  _data.Id;
        public string Name => _data.Name;
        public int Level => _data.Level;
        
        public async UniTask Initialize(CancellationToken ct = default)
        {
            _data = await DataService.Instance.LoadJsonOrDefaultAsync(SaveKey, new PlayerData(), ct);
        }
        
        private async UniTask SaveAsync()
        {
            await DataService.Instance.SaveJsonAsync(SaveKey, _data);
        }
    }
}