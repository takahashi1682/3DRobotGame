using Features.Unit;
using Features.Unit.Player;
using MyUtils;
using MyUtils.Parameter.Basic;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Game
{
    public enum EGameState
    {
        Ready,
        Playing,
        GameOver
    }

    /// <summary>
    /// ゲーム全体の進行(開始/終了)と勝敗を管理する。
    /// </summary>
    public class GameJudge : MonoBehaviour, IGameScopeInitializable
    {
        [Header("References")]
        [SerializeField] private UnitScopeRoot _player;
        [SerializeField] private BasicTimer _startTimer;
        [SerializeField] private BasicTimer _gameTimer;

        [SerializeField] private SerializableReactiveProperty<EGameState> _state = new(EGameState.Ready);
        public ReadOnlyReactiveProperty<EGameState> State => _state;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this);
        }

        public void OnResolve(IObjectResolver resolver)
        {
            _state.AddTo(this);
            
            _startTimer.IsPlay.Value = true;
            _gameTimer.IsPlay.Value = false;

            SubscribeGameStart();
            SubscribeGameEnd(_player);
        }

        /// <summary>
        /// カウントダウン終了後、ゲームを開始する。
        /// </summary>
        private void SubscribeGameStart()
        {
            _startTimer.OnFinish.Take(1)
                .Subscribe(_ =>
                {
                    _state.Value = EGameState.Playing;
                    _gameTimer.IsPlay.Value = true;
                })
                .AddTo(this);
        }

        /// <summary>
        /// 時間切れ、またはプレイヤーの体力切れのどちらか早い方でゲームを終了させ、勝敗を確定する。
        /// </summary>
        private void SubscribeGameEnd(UnitScopeRoot unit)
        {
            // 時間切れ
            var timeUp = _gameTimer.OnFinish.Select(_ => true);

            // プレイヤーの体力切れ
            var playerDie = unit.Container.Resolve<Health>().IsEmpty
                .Where(x => x)
                .Select(_ => false);

            // どちらが先に発火しても、勝敗判定は1回だけ
            playerDie.Merge(timeUp)
                .Take(1)
                .Subscribe(wins =>
                {
                    _gameTimer.IsPlay.Value = false;
                    _state.Value = EGameState.GameOver;
                    // GameClear = wins;
                })
                .AddTo(this);
        }
    }
}