using System;
using Cysharp.Threading.Tasks;

namespace Services.Loading.Operations
{
    public interface ILoadingOperation
    {
        public string Description { get; }
        public UniTask Execute(IProgress<float> progress);
    }
}