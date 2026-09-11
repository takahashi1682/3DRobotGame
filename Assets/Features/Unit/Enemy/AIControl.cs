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
            Observable.Interval(TimeSpan.FromSeconds(ThinkingInterval))
                .Where(_ => current.Running.CurrentValue)
                .Subscribe(_ =>
                {
                    if (unitStatus.HasFlag((int)EPlayerState.LockOn))
                    {
                        _move.Value = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

                        // 飛行を試みる
                        TryPress(_fly, FlyRate, 0.1f);

                        // ブーストを試みる(ロックオンの有無に関係なく共通)
                        TryPress(_boost, BoostRate, 0.1f);

                        // 一定時間の射撃を試みる
                        if (!_fire.Value) TryPress(_fire, FireRate, Random.Range(2f, 3f));
                    }
                    else
                    {
                        if (trackingObservable.Target.CurrentValue == null)
                        {
                            // 最も近い敵ユニットを検索する
                            var target = unitManager.FindClosestEnemyUnit(current, float.MaxValue);
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