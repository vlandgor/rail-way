using Cysharp.Threading.Tasks;
using Meta.Player;
using UnityEngine;
using Utilities;

namespace Meta
{
    public class MetaManager : Singleton<MetaManager>
    {
        public PlayerMeta PlayerMeta { get; private set; }
        
        public async UniTask Initialize()
        {
            PlayerMeta = new PlayerMeta();

            await PlayerMeta.Initialize();
            Debug.Log("[MetaManager] All Meta Systems Initialized.");
        }
    }
}