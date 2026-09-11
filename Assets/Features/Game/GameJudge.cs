using Features.Unit;
using Features.Unit.Battle;
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
        GameClear,
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

        private UnitManager _unitManager;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this);
        }

        public void OnResolve(IObjectResolver resolver)
        {
            _state.AddTo(this);

            _startTimer.IsPlay.Value = true;
            _gameTimer.IsPlay.Value = false;
            _unitManager = resolver.Resolve<UnitManager>();

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
            // 勝利条件: プレイヤー以外のユニットが全滅した場合
            _unitManager = unit.Container.Resolve<UnitManager>();
            var targetKilled = _unitManager.ChangedUnitList
                .Where(_ => _state.Value == EGameState.Playing)
                .Where(_ => _unitManager.GetTargetUnits(ArmyType.Player).Count == 0).Select(_ => true);

            // 敗北条件: 制限時間切れ
            var timeUp = _gameTimer.OnFinish.Select(_ => true);

            // 敗北条件: プレイヤーの体力が0になった場合
            var playerDie = unit.Container.Resolve<Health>().IsEmpty
                .Where(x => x)
                .Select(_ => false);

            // どちらが先に発火しても、勝敗判定は1回だけ
            targetKilled
                .Merge(timeUp)
                .Merge(playerDie)
                .Take(1)
                .Subscribe(playerWin =>
                {
                    _gameTimer.IsPlay.Value = false;
                    _state.Value = playerWin ? EGameState.GameClear : EGameState.GameOver;
                })
                .AddTo(this);
        }
    }
}