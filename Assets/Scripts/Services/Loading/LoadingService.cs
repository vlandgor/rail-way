using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Services.Loading.Curtains;
using Services.Loading.Operations;
using UnityEngine;

namespace Services.Loading
{
    public class LoadingService : BaseService<LoadingService>
    {
        [Header("Curtain References")]
        [SerializeField] private InitialLoadingCurtains initialLoadingCurtains;
        [SerializeField] private DefaultLoadingCurtains defaultLoadingCurtains;
        [SerializeField] private QuickLoadingCurtains quickLoadingCurtains;

        /// <summary>
        /// Get curtain by type
        /// </summary>
        private ILoadingCurtain GetCurtain(LoadingCurtainType type)
        {
            return type switch
            {
                LoadingCurtainType.Initial => initialLoadingCurtains,
                LoadingCurtainType.Default => defaultLoadingCurtains,
                LoadingCurtainType.Quick => quickLoadingCurtains,
                _ => defaultLoadingCurtains
            };
        }

        /// <summary>
        /// Execute a queue of loading operations with a specific curtain type
        /// </summary>
        public async UniTask Load(Queue<ILoadingOperation> operations, LoadingCurtainType curtainType = LoadingCurtainType.Default)
        {
            ILoadingCurtain curtain = GetCurtain(curtainType);
            await Load(operations, curtain);
        }

        /// <summary>
        /// Execute a queue of loading operations with a specific curtain instance
        /// </summary>
        public async UniTask Load(Queue<ILoadingOperation> operations, ILoadingCurtain curtain)
        {
            await curtain.Show();

            int totalOps = operations.Count;
            int currentOp = 0;

            while (operations.Count > 0)
            {
                ILoadingOperation operation = operations.Dequeue();

                Progress<float> progress = new Progress<float>(p =>
                {
                    float overallProgress = (currentOp + p) / totalOps;
                    curtain.UpdateProgress(overallProgress, operation.Description);
                });

                try
                {
                    await operation.Execute(progress);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Loading operation '{operation.Description}' failed: {ex.Message}");
                    throw;
                }

                currentOp++;
            }

            // Ensure we show 100% completion
            curtain.UpdateProgress(1f, "Complete");
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));

            await curtain.Hide();
        }

        /// <summary>
        /// Execute a single operation with specified curtain type
        /// </summary>
        public async UniTask LoadSingle(ILoadingOperation operation, LoadingCurtainType curtainType = LoadingCurtainType.Quick)
        {
            ILoadingCurtain curtain = GetCurtain(curtainType);
            await curtain.Show();

            try
            {
                Progress<float> progress = new Progress<float>(p =>
                {
                    curtain.UpdateProgress(p, operation.Description);
                });

                await operation.Execute(progress);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Loading operation '{operation.Description}' failed: {ex.Message}");
                throw;
            }
            finally
            {
                await curtain.Hide();
            }
        }

        /// <summary>
        /// Execute a simple async action with specified curtain type
        /// </summary>
        public async UniTask LoadQuick(Func<UniTask> action, string description = "Loading...", LoadingCurtainType curtainType = LoadingCurtainType.Quick)
        {
            ILoadingCurtain curtain = GetCurtain(curtainType);
            await curtain.Show();

            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Quick loading action '{description}' failed: {ex.Message}");
                throw;
            }
            finally
            {
                await curtain.Hide();
            }
        }
    }
}