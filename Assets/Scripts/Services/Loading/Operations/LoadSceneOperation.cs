using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services.Loading.Operations
{
    public class LoadSceneOperation : ILoadingOperation
    {
        private readonly string _sceneName;
    
        public string Description => $"Loading {_sceneName}...";

        public LoadSceneOperation(string sceneName)
        {
            _sceneName = sceneName;
        }
    
        public async UniTask Execute(IProgress<float> progress)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_sceneName);
            if (asyncOperation != null)
            {
                asyncOperation.allowSceneActivation = false;

                while (asyncOperation.progress < 0.9f)
                {
                    progress?.Report(asyncOperation.progress / 0.9f);
                    await UniTask.Yield();
                }

                asyncOperation.allowSceneActivation = true;
            }

            progress?.Report(1f);
        }
    }
}