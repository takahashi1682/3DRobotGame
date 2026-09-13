using System.Threading;
using Cysharp.Threading.Tasks;
using MyUtils;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit
{
    public interface ILockOnActionHandler : IUnitActionHandler<bool>
    {
    }

    public interface ILockOnActionObservable : IUnitActionObservable
    {
    }

    /// <summary>
    /// Targetの方向を向くよう、CurrentUnit(水平のみ)とShotPos(全方位)を回転させるロックオン制御。
    /// </summary>
    public class UnitLockOn : AbstractUnitAction,
        IUnitScopeMember,
        IScopeRegisterable,
        IScopeLaunchable,
        ILockOnActionHandler,
        ILockOnActionObservable
    {
        [Inject] private UnitSetting _unitSetting;
        [Inject] private UnitManager _unitManager;
        [Inject] private IUnitTrackingHandler _unitTracking;
        [Inject] private IUnitTrackingObservable _trackingObservable;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<ILockOnActionHandler, ILockOnActionObservable>();
        }

        public virtual void OnLaunch()
        {
            IsAction.AddTo(this);

            // Targetがnullになったらロックオンを解除する
            _trackingObservable.Target.Subscribe(target =>
            {
                if (target == null)
                {
                    CancelAction();
                }
            }).AddTo(this);
        }

        /// <summary>
        /// ボタン押下でロックオンをトグルする。呼び出し元(UnitActionController)は
        /// 押下時にのみvalue=trueで呼ぶため、解除はCancelActionで直接行う想定。
        /// </summary>
        public UniTask OnValueChanged(bool value, CancellationToken ct)
        {
            if (!value) return UniTask.CompletedTask;
            if (!IsAction.CurrentValue)
            {
                var target = _unitManager.FindClosestTargetUnit(
                    _unitSetting.Army,
                    _unitSetting.UnitPivot.position,
                    _unitSetting.MaxLockOnDistance);
                if (target != null)
                {
                    _isAction.Value = true;
                    _unitTracking.SetTarget(target, _unitSetting.MaxLockOnDistance);
                }
            }
            else
            {
                CancelAction();
            }

            return UniTask.CompletedTask;
        }

        public void CancelAction()
        {
            _unitTracking?.ClearTarget();
            _isAction.Value = false;
        }
    }
}