using MyUtils.ObjectGroup;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Game
{
    public class GameViewer : MonoBehaviour, IGameScopeMember, IScopeLaunchable
    {
        [SerializeField] private ObjectGroupSwitcher _gameStateSwitcher;

        [Inject] private GameJudge _judge;

        public void OnLaunch()
        {
            // GameJudgeのGameStateを監視し、ObjectGroupSwitcherで表示するオブジェクトを切り替える
            _judge.State
                .Select(x => (int)x)
                .Subscribe(_gameStateSwitcher.SetActiveObject)
                .AddTo(this);
        }
    }
}