using System.Collections.Generic;
using Meta;
using Meta.Loading.Operations;
using Services.Account;
using Services.Data;
using Services.Loading;
using Services.Loading.Curtains;
using Services.Loading.Operations;
using UnityEngine;

namespace Game.Boot
{
    public class AppBoot : MonoBehaviour
    {
        private async void Start()
        {
            Queue<ILoadingOperation> appLoadOperations = new Queue<ILoadingOperation>();
            
            appLoadOperations.Enqueue(new InitializeUgsOperation());
            appLoadOperations.Enqueue(new InitializeServiceOperation("Loading Service", LoadingService.Instance.Initialize));
            appLoadOperations.Enqueue(new InitializeServiceOperation("Data Service" , DataService.Instance.Initialize));
            appLoadOperations.Enqueue(new InitializeServiceOperation("Account Service", AccountService.Instance.Initialize));
            appLoadOperations.Enqueue(new InitializeMetaOperation());
            appLoadOperations.Enqueue(new AuthorizationOperation());
            appLoadOperations.Enqueue(new DelayOperation(0.5f));
            appLoadOperations.Enqueue(new LoadSceneOperation("Menu_Scene"));
            
            await LoadingService.Instance.Load(appLoadOperations, LoadingCurtainType.Initial);
        }
    }
}