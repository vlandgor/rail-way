using UnityEngine;

namespace Services.Utilities
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new();
        private static bool _quitting;

        public static T Instance
        {
            get
            {
                if (_quitting)
                    return null;

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            T prefab = Resources.Load<T>(typeof(T).Name);
                            if (prefab != null)
                            {
                                GameObject instance = Instantiate(prefab.gameObject);
                                _instance = instance.GetComponent<T>();
                                DontDestroyOnLoad(instance);
                            }
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void Initialize()
        {
            Debug.Log($"[{typeof(T).Name}]: Service Initialized");
        }

        protected virtual void OnApplicationQuit()
        {
            _quitting = true;
            _instance = null;
        }
    }
}