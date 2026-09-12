using MyUtils.ObjectGroup;
using R3;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Game
{
    public class GameViewer : MonoBehaviour, IGameScopeInitializable
    {
        [SerializeField] private ObjectGroupSwitcher _gameStateSwitcher;

        public void OnRegister(IContainerBuilder builder)
        {
        }

        public void OnResolve(IObjectResolver resolver)
        {
            // GameJudgeのGameStateを監視し、ObjectGroupSwitcherで表示するオブジェクトを切り替える
            var judge = resolver.Resolve<GameJudge>();
            judge.State
                .Select(x => (int)x)
                .Subscribe(_gameStateSwitcher.SetActiveObject)
                .AddTo(this);
        }
    }
}