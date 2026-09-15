using System;
using _Projects.Features.Unit;
using _Projects.Features.Unit.Battle;
using Cysharp.Threading.Tasks;
using MyUtils;
using MyUtils.FadeScreen;
using MyUtils.SceneLoader;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Game
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
    public class GameJudge : MonoBehaviour,
        IGameScopeMember,
        IScopeRegisterable,
        IScopeLaunchable
    {
        [Header("References")]
        [SerializeField] private UnitScopeRoot _player;
        [SerializeField] private BasicTimer _startTimer;
        [SerializeField] private BasicTimer _gameTimer;

        [SerializeField] private SerializableReactiveProperty<EGameState> _state = new(EGameState.Ready);
        public ReadOnlyReactiveProperty<EGameState> State => _state;

        [Inject] private UnitManager _unitManager;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this);
        }

        public void OnLaunch()
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
            // 勝利条件: プレイヤー以外のユニットが全滅した場合
            var targetKilled = _unitManager.OnRemovedUnit
                .Where(_ => _state.Value == EGameState.Playing)
                .Where(_ => _unitManager.GetTargetUnits(ArmyType.Player).Count == 0).Select(_ => true);

            // 敗北条件: 制限時間切れ
            var timeUp = _gameTimer.OnFinish.Select(_ => true);

            // 敗北条件: プレイヤーの体力が0になった場合
            var playerDie = unit.Health.IsEmpty
                .Where(x => x)
                .Select(_ => false);

            // どちらが先に発火しても、勝敗判定は1回だけ
            targetKilled
                .Merge(timeUp)
                .Merge(playerDie)
                .Take(1)
                .SubscribeAwait(async (playerWin, cts) =>
                {
                    _gameTimer.IsPlay.Value = false;
                    _state.Value = playerWin ? EGameState.GameClear : EGameState.GameOver;

                    await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: cts);

                    // シーンを再読み込み（フェードアウトしてから）
                    var currentScene = SceneManager.GetActiveScene();
                    await SceneLoaderUtils.LoadSceneAsync(currentScene.name, FadeSetting.Default);
                })
                .AddTo(this);
        }
    }
}