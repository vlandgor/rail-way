using System.Threading;
using Cysharp.Threading.Tasks;
using Services.Data;

namespace Meta.Player
{
    public class PlayerMeta
    {
        private const string SaveKey = "player_meta_data";
        
        private PlayerData _data;
        
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