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
        public bool IsEnabled = true;

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

            var unitStatus = resolver.Resolve<UnitStatus>();
            Observable.Interval(TimeSpan.FromSeconds(ThinkingInterval))
                .Where(_ => IsEnabled)
                .Subscribe(_ =>
                {
                    _move.Value = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

                    if (Random.value < FlyRate)
                    {
                        VirtualPress(_fly, 0.1f, destroyCancellationToken).Forget();
                    }

                    if (Random.value < BoostRate)
                    {
                        VirtualPress(_boost, 0.1f, destroyCancellationToken).Forget();
                    }

                    // ロックオン状態であれば、一定確率で射撃する。ロックオン状態でなければ、ロックオンする。
                    if (unitStatus.HasFlag((int)EPlayerState.LockOn))
                    {
                        _fire.Value = Random.value < FireRate;
                    }
                    else
                    {
                        _fire.Value = false;
                        VirtualPress(_lockOn, 0.1f, destroyCancellationToken).Forget();
                    }
                }).AddTo(this);
        }

        private static async UniTask VirtualPress(ReactiveProperty<bool> action, float duration, CancellationToken ct)
        {
            action.Value = true;
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: ct);
            action.Value = false;
        }
    }
}