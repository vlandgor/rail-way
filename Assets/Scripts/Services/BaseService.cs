using Cysharp.Threading.Tasks;
using UnityEngine;
using Utilities;

namespace Services
{
    public class BaseService<T> : Singleton<T> where T : BaseService<T>
    {
        public async virtual UniTask Initialize()
        {
            Debug.Log($"[{typeof(T).Name}]: Service Initialized");
        }
    }
}