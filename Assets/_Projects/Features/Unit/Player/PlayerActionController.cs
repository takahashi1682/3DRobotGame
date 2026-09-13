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

        [Inject] private IObjectResolver _lookResolver;

        public override void OnLaunch()
        {
            base.OnLaunch();
            if (_control == null) return;

            _lookResolver.TryResolve(out _lookHandler);

            if (_lookHandler != null)
            {
                _control.Look
                    .SubscribeAwait(async (x, cts) =>
                        {
                            if (_unitScopeRoot.Running.CurrentValue && _playerStatus.CanLook)
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

        // 死亡・ゲーム終了時のキャンセルは、基底クラスのRunning購読からCancelAllActionsを
        // 通じて呼ばれる(重複購読を避けるため、ここでは個別にRunningを購読しない)。
        protected override void CancelAllActions()
        {
            base.CancelAllActions();
            _lookHandler?.CancelAction();
        }
    }
}