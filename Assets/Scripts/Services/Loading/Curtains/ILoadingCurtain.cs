using Cysharp.Threading.Tasks;

namespace Services.Loading.Curtains
{
    public interface ILoadingCurtain
    {
        public UniTask Show(bool instant = false);
        public UniTask Hide(bool instant = false);
        public void UpdateProgress(float progress, string description);
    }
}