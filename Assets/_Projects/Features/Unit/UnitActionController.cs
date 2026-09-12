using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Unit
{
    public interface IUnitActionObservable
    {
        ReadOnlyReactiveProperty<bool> IsAction { get; }
    }

    public interface IUnitActionHandler<in T>
    {
        UniTask OnValueChanged(T value, CancellationToken ct);
        void CancelAction();
    }

    public class UnitActionController : MonoBehaviour, IUnitScopeInitializable
    {
        /// <summary>
        /// 体力が0でなく、かつゲーム進行中かどうか。派生クラス(PlayerActionControllerのLookなど)も
        /// 同じインスタンスを参照することで、死亡・ゲーム終了時の停止処理を連動させる。
        /// </summary>
        protected IMoveActionHandler _moveHandler;
        protected IFlyActionHandler _flyHandler;
        protected IBoostActionHandler _boostHandler;
        protected IFireActionHandler _fireHandler;
        protected ILockOnActionHandler _lockOnHandler;

        public virtual void OnRegister(IContainerBuilder builder)
        {
        }

        public virtual void OnResolve(IObjectResolver resolver)
        {
            // IUnitControllableが解決できない場合は、アクション制御を行わない
            if (!resolver.TryResolve<IUnitControllable>(out var control)) return;

            var unitScopeRoot = resolver.Resolve<UnitScopeRoot>();
            var playerStatus = resolver.Resolve<UnitStatus>();

            if (resolver.TryResolve(out _moveHandler))
            {
                control.Move
                    .Where(_ => unitScopeRoot.Running.CurrentValue && playerStatus.CanMove)
                    .SubscribeAwait(async (x, cts) => await _moveHandler.OnValueChanged(x, cts),
                        AwaitOperation.Drop)
                    .AddTo(this);
            }

            if (resolver.TryResolve(out _flyHandler))
            {
                control.Fly
                    .Where(_ => unitScopeRoot.Running.CurrentValue && playerStatus.CanFly)
                    .SubscribeAwait(async (x, cts) => await _flyHandler.OnValueChanged(x, cts), AwaitOperation.Drop)
                    .AddTo(this);
            }

            if (resolver.TryResolve(out _boostHandler))
            {
                control.Boost
                    .Where(press => press && unitScopeRoot.Running.CurrentValue && playerStatus.CanBoost)
                    .SubscribeAwait(async (_, cts) => await _boostHandler.OnValueChanged(true, cts),
                        AwaitOperation.Drop)
                    .AddTo(this);
            }

            if (resolver.TryResolve(out _fireHandler))
            {
                control.Fire
                    .Where(_ => unitScopeRoot.Running.CurrentValue && playerStatus.CanFire)
                    .SubscribeAwait(async (press, cts) => await _fireHandler.OnValueChanged(press, cts),
                        AwaitOperation.Drop)
                    .AddTo(this);
            }

            if (resolver.TryResolve(out _lockOnHandler))
            {
                control.LockOn
                    .Where(press => press && unitScopeRoot.Running.CurrentValue && playerStatus.CanLockOn)
                    .SubscribeAwait(async (_, cts) => await _lockOnHandler.OnValueChanged(true, cts),
                        AwaitOperation.Drop)
                    .AddTo(this);
            }

            // アクション不可状態になった場合、すべてのアクションをキャンセルする
            unitScopeRoot.Running
                .Where(x => !x)
                .Subscribe(_ => CancelAllActions())
                .AddTo(this);
        }

        protected virtual void CancelAllActions()
        {
            _moveHandler?.CancelAction();
            _flyHandler?.CancelAction();
            _boostHandler?.CancelAction();
            _fireHandler?.CancelAction();
            _lockOnHandler?.CancelAction();
        }
    }
}