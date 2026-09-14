using System;
using System.Threading;
using _Projects.Features.Game;
using Cysharp.Threading.Tasks;
using MyUtils;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace _Projects.Features.Unit.Enemy
{
    public class AIControl : MonoBehaviour
        , IUnitScopeMember
        , IScopeRegisterable
        , IScopeLaunchable
        , IPhaseUpdatable
        , IUnitControllable
    {
        [SerializeField, ReadOnly] private SerializableReactiveProperty<Vector2> _move = new();
        public Observable<Vector2> Move => _move;

        [SerializeField, ReadOnly] private SerializableReactiveProperty<Vector2> _look = new();
        public Observable<Vector2> Look => _look;

        public string LookDeviceName => string.Empty;

        [SerializeField, ReadOnly] private SerializableReactiveProperty<bool> _fly = new();
        public Observable<bool> Fly => _fly;

        [SerializeField, ReadOnly] private SerializableReactiveProperty<bool> _boost = new();
        public Observable<bool> Boost => _boost;

        [SerializeField, ReadOnly] private SerializableReactiveProperty<bool> _fire = new();
        public Observable<bool> Fire => _fire;

        [SerializeField, ReadOnly] private SerializableReactiveProperty<bool> _lockOn = new();
        public Observable<bool> LockOn => _lockOn;

        [Header("Settings")]
        public float ThinkingInterval = 1f;
        public float FlyRate = 0.2f;
        public float BoostRate = 0.5f;
        public float FireRate = 0.9f;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<IUnitControllable>();
        }

        [Inject] private UnitScopeRoot _current;
        [Inject] private UnitManager _unitManager;
        [Inject] private IUnitTrackingObservable _trackingObservable;
        [Inject] private IUnitTrackingHandler _trackingHandler;
        [Inject] private UnitStatus _unitStatus;
        [Inject] private UnitSetting _unitSetting;
        [Inject] private UpdateDispatcher _dispatcher;
        private float _lastThinkTime;

        public void OnLaunch()
        {
            _move.AddTo(this);
            _look.AddTo(this);
            _fly.AddTo(this);
            _boost.AddTo(this);
            _fire.AddTo(this);
            _lockOn.AddTo(this);
            _lastThinkTime = Time.time;

            _dispatcher.Register(this);
        }

        private void OnDestroy()
        {
            _dispatcher.Unregister(this);
        }

        public UpdatePhase Phase => UpdatePhase.AI;

        public void OnPhaseUpdate()
        {
            if (Time.time - _lastThinkTime >= ThinkingInterval)
            {
                _lastThinkTime = Time.time;
                Think();
            }
        }

        /// <summary>
        /// 一定間隔で呼ばれるAIの思考処理。停止中はすべての操作を解除し、
        /// 稼働中はロックオンの有無で行動を切り替える。
        /// </summary>
        private void Think()
        {
            if (!_current.Running.CurrentValue)
            {
                StopAllActions();
                return;
            }

            if (_unitStatus.HasFlag(EPlayerState.LockOn))
            {
                DecideLockedOnBehavior();
            }
            else
            {
                DecideSearchBehavior();
            }
        }

        /// <summary>
        /// ロックオン中の行動。ランダムに動き回りつつ、状況に応じて飛行・ブースト・攻撃を試みる。
        /// </summary>
        private void DecideLockedOnBehavior()
        {
            _move.Value = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

            // 飛行を試みる
            if (_unitStatus.HasFlag(EPlayerState.Grounded))
                TryPress(_fly, FlyRate, Random.Range(1f, 2f));

            // ブーストを試みる(ロックオンの有無に関係なく共通)
            TryPress(_boost, BoostRate, 0.1f);

            // ターゲット見える場合は攻撃を試みる
            if (_trackingObservable.IsLookingAtTarget)
            {
                if (!_fire.Value) TryPress(_fire, FireRate, Random.Range(2f, 3f));
            }
        }

        /// <summary>
        /// ロックオン前の行動。ターゲットを探し、見つかっていれば前進しつつロックオンを試みる。
        /// </summary>
        private void DecideSearchBehavior()
        {
            if (_trackingObservable.Target.CurrentValue == null)
            {
                // 最も近い敵ユニットを検索する
                var target = _unitManager.FindClosestTargetUnit(
                    _unitSetting.Army,
                    _unitSetting.UnitPivot.position,
                    float.MaxValue); // 範囲は無制限で検索する

                if (target != null)
                {
                    _trackingHandler.SetTarget(target, float.MaxValue);
                }
            }
            else
            {
                // 前進する
                _move.Value = new Vector2(0, 1);

                // ブーストを試みる(ロックオンの有無に関係なく共通)
                TryPress(_boost, BoostRate, 0.1f);

                // ロックオンを試みる
                TryPress(_lockOn, 1f, 0.1f);
            }
        }

        /// <summary>
        /// ユニットが停止中の場合、すべての操作を解除する。
        /// </summary>
        private void StopAllActions()
        {
            _move.Value = Vector2.zero;
            _look.Value = Vector2.zero;
            _fly.Value = false;
            _boost.Value = false;
            _fire.Value = false;
            _lockOn.Value = false;
        }

        /// <summary>
        /// rateの確率でactionをduration秒だけtrueにする(ボタンの単発押下を模す)。
        /// </summary>
        private void TryPress(ReactiveProperty<bool> action, float rate, float duration)
        {
            if (Random.value < rate)
            {
                VirtualPress(action, duration, destroyCancellationToken).Forget();
            }
        }

        private static async UniTask VirtualPress(ReactiveProperty<bool> action, float duration, CancellationToken ct)
        {
            action.Value = true;
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: ct);
            action.Value = false;
        }
    }
}