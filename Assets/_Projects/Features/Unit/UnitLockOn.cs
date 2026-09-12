using System.Threading;
using Cysharp.Threading.Tasks;
using MyUtils;
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
    public class UnitLockOn : MonoBehaviour,
        IUnitScopeInitializable,
        ILockOnActionHandler,
        ILockOnActionObservable
    {
        [SerializeField, ReadOnly] private SerializableReactiveProperty<bool> _isAction = new();
        public ReadOnlyReactiveProperty<bool> IsAction => _isAction;

        private UnitSetting _unitSetting;
        private UnitManager _unitManager;
        private IUnitTrackingHandler _unitTracking;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<ILockOnActionHandler, ILockOnActionObservable>();
        }

        public virtual void OnResolve(IObjectResolver resolver)
        {
            IsAction.AddTo(this);
            _unitManager = resolver.Resolve<UnitManager>();
            _unitSetting = resolver.Resolve<UnitSetting>();
            _unitTracking = resolver.Resolve<IUnitTrackingHandler>();

            // Targetがnullになったらロックオンを解除する
            var trackingTargetObservable = resolver.Resolve<IUnitTrackingObservable>();
            trackingTargetObservable.Target.Subscribe(target =>
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