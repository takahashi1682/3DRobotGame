using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyUtils;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace Features.Unit.Enemy
{
    public class AIControl : MonoBehaviour
        , IUnitScopeInitializable
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

        public void OnResolve(IObjectResolver resolver)
        {
            _move.AddTo(this);
            _look.AddTo(this);
            _fly.AddTo(this);
            _boost.AddTo(this);
            _fire.AddTo(this);
            _lockOn.AddTo(this);

            var current = resolver.Resolve<UnitScopeRoot>();
            var unitManager = resolver.Resolve<UnitManager>();
            var trackingObservable = resolver.Resolve<IUnitTrackingObservable>();
            var trackingHandler = resolver.Resolve<IUnitTrackingHandler>();
            var unitStatus = resolver.Resolve<UnitStatus>();
            var unitSetting = resolver.Resolve<UnitSetting>();

            Observable.Interval(TimeSpan.FromSeconds(ThinkingInterval))
                .Subscribe(_ =>
                {
                    if (current.Running.CurrentValue)
                    {
                        if (unitStatus.HasFlag(EPlayerState.LockOn))
                        {
                            _move.Value = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

                            // 飛行を試みる
                            if (unitStatus.HasFlag(EPlayerState.Grounded))
                                TryPress(_fly, FlyRate, Random.Range(1f, 2f));

                            // ブーストを試みる(ロックオンの有無に関係なく共通)
                            TryPress(_boost, BoostRate, 0.1f);

                            // ターゲット見える場合は攻撃を試みる
                            if (trackingObservable.IsLookingAtTarget)
                            {
                                if (!_fire.Value) TryPress(_fire, FireRate, Random.Range(2f, 3f));
                            }
                        }
                        else
                        {
                            if (trackingObservable.Target.CurrentValue == null)
                            {
                                // 最も近い敵ユニットを検索する
                                var target = unitManager.FindClosestTargetUnit(
                                    unitSetting.Army,
                                    unitSetting.UnitPivot.position,
                                    float.MaxValue); // 範囲は無制限で検索する

                                if (target != null)
                                {
                                    trackingHandler.SetTarget(target, float.MaxValue);
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
                    }
                    else
                    {
                        // ユニットが停止中の場合は、すべての操作を解除する
                        _move.Value = Vector2.zero;
                        _look.Value = Vector2.zero;
                        _fly.Value = false;
                        _boost.Value = false;
                        _fire.Value = false;
                        _lockOn.Value = false;
                    }
                }).AddTo(this);
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