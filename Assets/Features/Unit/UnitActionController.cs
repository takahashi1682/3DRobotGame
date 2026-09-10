using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Game;
using MyUtils.Parameter.Basic;
using R3;
using UnityEngine;
using VContainer;

namespace Features.Unit
{
    public interface IUnitActionObservable
    {
        SerializableReactiveProperty<bool> IsAction { get; }
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
        protected ReadOnlyReactiveProperty<bool> CanAction { get; private set; }
        protected IMoveActionHandler MoveHandler;
        protected IFlyActionHandler FlyHandler;
        protected IBoostActionHandler BoostHandler;
        protected IFireActionHandler FireHandler;
        protected ILockOnActionHandler LockOnHandler;

        public virtual void OnRegister(IContainerBuilder builder)
        {
        }

        public virtual void OnResolve(IObjectResolver resolver)
        {
            var control = resolver.Resolve<IUnitControllable>();
            var playerStatus = resolver.Resolve<UnitStatus>();
            var health = resolver.Resolve<Health>();
            var gameJudge = resolver.Resolve<GameJudge>();

            CanAction = Observable.CombineLatest(
                    health.IsEmpty, gameJudge.State, // 体力が0でなく、かつゲーム進行中のみアクション可能
                    (healthEmpty, gameState) => !healthEmpty && gameState == EGameState.Playing)
                .ToReadOnlyReactiveProperty()
                .AddTo(this);

            MoveHandler = resolver.Resolve<IMoveActionHandler>();
            control.Move
                .Where(_ => CanAction.CurrentValue && playerStatus.CanMove)
                .SubscribeAwait(async (x, cts) => await MoveHandler.OnValueChanged(x, cts), AwaitOperation.Drop)
                .AddTo(this);

            FlyHandler = resolver.Resolve<IFlyActionHandler>();
            control.Fly
                .Where(_ => CanAction.CurrentValue && playerStatus.CanFly)
                .SubscribeAwait(async (press, cts) => await FlyHandler.OnValueChanged(press, cts), AwaitOperation.Drop)
                .AddTo(this);

            BoostHandler = resolver.Resolve<IBoostActionHandler>();
            control.Boost
                .Where(press => press && CanAction.CurrentValue && playerStatus.CanBoost)
                .SubscribeAwait(async (_, cts) => await BoostHandler.OnValueChanged(true, cts), AwaitOperation.Drop)
                .AddTo(this);

            FireHandler = resolver.Resolve<IFireActionHandler>();
            control.Fire
                .Where(_ => CanAction.CurrentValue && playerStatus.CanFire)
                .SubscribeAwait(async (press, cts) => await FireHandler.OnValueChanged(press, cts),
                    AwaitOperation.Drop)
                .AddTo(this);

            LockOnHandler = resolver.Resolve<ILockOnActionHandler>();
            control.LockOn
                .Where(press => press && CanAction.CurrentValue && playerStatus.CanLockOn)
                .SubscribeAwait(async (_, cts) => await LockOnHandler.OnValueChanged(true, cts),
                    AwaitOperation.Drop)
                .AddTo(this);

            // アクション不可状態になった場合、すべてのアクションをキャンセルする
            CanAction.Where(x => !x).Subscribe(_ =>
            {
                CancelAllActions();
            }).AddTo(this);
        }

        protected virtual void CancelAllActions()
        {
            MoveHandler?.CancelAction();
            FlyHandler?.CancelAction();
            BoostHandler?.CancelAction();
            FireHandler?.CancelAction();
            LockOnHandler?.CancelAction();
        }
    }
}