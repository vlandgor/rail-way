using Services.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services.Loading
{
    public class LoadingService : Singleton<LoadingService>
    {
        public async void LoadScene(string sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName);
        }
    }
}