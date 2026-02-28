namespace Utilities
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static T _instance;
        private static readonly object _lock = new();

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    _instance ??= new T();
                    return _instance;
                }
            }
        }

        protected Singleton()
        {
            
        }
    }
}