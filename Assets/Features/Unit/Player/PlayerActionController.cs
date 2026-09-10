using R3;
using VContainer;

namespace Features.Unit.Player
{
    /// <summary>
    /// UnitActionControllerにLook(Player固有のアクション)を追加したPlayer専用コントローラ。
    /// PlayerFire : UnitFireなどと同じく、Unit共通の挙動をPlayerScopeで拡張する構成に揃えている。
    /// </summary>
    public class PlayerActionController : UnitActionController
    {
        private ILookActionHandler _lookHandler;

        public override void OnResolve(IObjectResolver resolver)
        {
            base.OnResolve(resolver);
            var control = resolver.Resolve<IUnitControllable>();
            var playerStatus = resolver.Resolve<UnitStatus>();

            _lookHandler = resolver.Resolve<ILookActionHandler>();
            control.Look
                .Where(_ => CanAction.CurrentValue && playerStatus.CanLook)
                .SubscribeAwait(async (x, cts) => await _lookHandler.OnValueChanged(x, cts), AwaitOperation.Drop)
                .AddTo(this);
        }

        // 死亡・ゲーム終了時のキャンセルは、基底クラスのCanAction購読からCancelAllActionsを
        // 通じて呼ばれる(重複購読を避けるため、ここでは個別にCanActionを購読しない)。
        protected override void CancelAllActions()
        {
            base.CancelAllActions();
            _lookHandler?.CancelAction();
        }
    }
}