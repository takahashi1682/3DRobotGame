using _Projects.Features.Game;
using MyUtils.Parameter.Basic;
using MyUtils.TalkUtils;
using R3;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Unit.Player
{
    public class PlayerTalk : MonoBehaviour
    {
        [Inject] protected TalkManager _talkManager;
        [Inject] protected Health _health;
        [Inject] private GameJudge _gameJudge;

        private void Awake()
        {
            _gameJudge.State
                .Where(state => state == EGameState.Playing)
                .Take(1)
                .Subscribe(_ => _talkManager.Talk("player_battle_start"))
                .AddTo(this);

            _gameJudge.State
                .Where(state => state == EGameState.GameClear)
                .Take(1)
                .Subscribe(_ => _talkManager.Talk("player_game_clear"))
                .AddTo(this);

            _gameJudge.State
                .Where(state => state == EGameState.GameOver)
                .Take(1)
                .Subscribe(_ => _talkManager.Talk("player_game_over"))
                .AddTo(this);

            _health.CurrentRate
                .Where(rate => rate < 0.5f)
                .Take(1)
                .Subscribe(_ => _talkManager.Talk("player_health_50"))
                .AddTo(this);

            _health.CurrentRate
                .Where(rate => rate < 0.8f)
                .Take(1)
                .Subscribe(_ => _talkManager.Talk("player_health_80"))
                .AddTo(this);
        }
    }
}