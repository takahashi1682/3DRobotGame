using _Projects.Features.Game;
using MyUtils.TalkUtils;
using R3;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Talk
{
    public class GameTalk : MonoBehaviour
    {
        [Inject] private TalkManager _talkManager;
        [Inject] private GameJudge _gameJudge;

        public void Start()
        {
            _gameJudge.State
                .Where(state => state == EGameState.Ready)
                .Subscribe(_ =>
                {
                    _talkManager.Talk("battle_start");
                })
                .AddTo(this);
        }
    }
}