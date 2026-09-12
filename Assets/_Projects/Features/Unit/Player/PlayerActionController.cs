using R3;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Unit.Player
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
            var unitScopeRoot = resolver.Resolve<UnitScopeRoot>();
            var control = resolver.Resolve<IUnitControllable>();
            var playerStatus = resolver.Resolve<UnitStatus>();

            if (resolver.TryResolve(out _lookHandler))
            {
                control.Look
                    .SubscribeAwait(async (x, cts) =>
                        {
                            if (unitScopeRoot.Running.CurrentValue && playerStatus.CanLook)
                            {
                                await _lookHandler.OnValueChanged(x, cts);
                            }
                            else
                            {
                                await _lookHandler.OnValueChanged(Vector2.zero, cts);
                            }
                        }, AwaitOperation.Drop
                    ).AddTo(this);
            }
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