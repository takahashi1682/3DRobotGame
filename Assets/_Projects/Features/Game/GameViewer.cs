using MyUtils.ObjectGroup;
using R3;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Game
{
    public class GameViewer : MonoBehaviour
    {
        [SerializeField] private ObjectGroupSwitcher _gameStateSwitcher;

        [Inject] private GameJudge _judge;

        private void Awake()
        {
            // GameJudgeのGameStateを監視し、ObjectGroupSwitcherで表示するオブジェクトを切り替える
            _judge.State
                .Select(x => (int)x)
                .Subscribe(_gameStateSwitcher.SetActiveObject)
                .AddTo(this);
        }
    }
}