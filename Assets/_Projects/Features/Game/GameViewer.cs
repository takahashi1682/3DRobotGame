using MyUtils.ObjectGroup;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Game
{
    public class GameViewer : MonoBehaviour, IGameScopeMember, IScopeResolvable, IScopeStartable
    {
        [SerializeField] private ObjectGroupSwitcher _gameStateSwitcher;

        private GameJudge _judge;

        public void OnResolve(IObjectResolver resolver)
        {
            _judge = resolver.Resolve<GameJudge>();
        }

        public void OnStart()
        {
            // GameJudgeのGameStateを監視し、ObjectGroupSwitcherで表示するオブジェクトを切り替える
            _judge.State
                .Select(x => (int)x)
                .Subscribe(_gameStateSwitcher.SetActiveObject)
                .AddTo(this);
        }
    }
}