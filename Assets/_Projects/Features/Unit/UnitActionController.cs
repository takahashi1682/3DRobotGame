using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyUtils.VContainerExtensions;
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

    public class UnitActionController : MonoBehaviour, IUnitScopeMember, IScopeLaunchable
    {
        // 各アクションの実行役。派生クラス(PlayerActionControllerなど)からも使う
        protected IMoveActionHandler _moveHandler;
        protected IFlyActionHandler _flyHandler;
        protected IBoostActionHandler _boostHandler;
        protected IFireActionHandler _fireHandler;
        protected ILockOnActionHandler _lockOnHandler;

        protected IUnitControllable _control;

        // Running: 体力が0でなく、かつゲーム進行中かどうか。falseになったら全アクションを止める
        [Inject] protected UnitScopeRoot _unitScopeRoot;
        [Inject] protected UnitStatus _playerStatus;

        [Inject] private IObjectResolver _resolver;

        public virtual void OnLaunch()
        {
            // IUnitControllableが解決できない場合は、アクション制御を行わない
            if (!_resolver.TryResolve(out _control)) return;

            _resolver.TryResolve(out _moveHandler);
            _resolver.TryResolve(out _flyHandler);
            _resolver.TryResolve(out _boostHandler);
            _resolver.TryResolve(out _fireHandler);
            _resolver.TryResolve(out _lockOnHandler);

            BindValueAction(_control.Move, _moveHandler, () => _playerStatus.CanMove);
            BindValueAction(_control.Fly, _flyHandler, () => _playerStatus.CanFly);
            BindValueAction(_control.Fire, _fireHandler, () => _playerStatus.CanFire);
            BindValueAction(_control.Boost, _boostHandler, () => _playerStatus.CanBoost);
            BindValueAction(_control.LockOn, _lockOnHandler, () => _playerStatus.CanLockOn);

            // アクション不可状態になった場合、すべてのアクションをキャンセルする
            _unitScopeRoot.Running
                .Where(x => !x)
                .Subscribe(_ => CancelAllActions())
                .AddTo(this);
        }

        /// <summary>
        /// sourceの値を、Running中かつcanActを満たす間だけそのままhandlerへ渡し続ける。
        /// </summary>
        private void BindValueAction<T>(Observable<T> source, IUnitActionHandler<T> handler, Func<bool> canAct)
        {
            if (handler == null) return;

            source
                .Where(_ => _unitScopeRoot.Running.CurrentValue && canAct())
                .SubscribeAwait(async (value, cts) => await handler.OnValueChanged(value, cts), AwaitOperation.Drop)
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