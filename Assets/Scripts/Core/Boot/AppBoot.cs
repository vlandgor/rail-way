using Services.Loading;
using Services.Matchmaking;
using UnityEngine;

namespace Core.Boot
{
    public class AppBoot : MonoBehaviour
    {
        private void Start()
        {
            InitializeServices();

            LoadingService.Instance.LoadScene("Menu_Scene");
        }

        private void InitializeServices()
        {
            LoadingService.Instance.Initialize();
            MatchmakingService.Instance.Initialize();
        }
    }
}