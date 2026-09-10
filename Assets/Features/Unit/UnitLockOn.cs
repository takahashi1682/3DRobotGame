using System.Threading;
using Cysharp.Threading.Tasks;
using MyUtils;
using MyUtils.Parameter.Basic;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Unit
{
    public interface ILockOnActionHandler : IUnitActionHandler<bool>
    {
    }

    public interface ILockOnActionObservable : IUnitActionObservable
    {
        ReadOnlyReactiveProperty<UnitScopeRoot> Target { get; }
        float Distance { get; }
    }

    /// <summary>
    /// Targetの方向を向くよう、CurrentUnit(水平のみ)とShotPos(全方位)を回転させるロックオン制御。
    /// </summary>
    public class UnitLockOn : MonoBehaviour,
        IUnitScopeInitializable,
        ILockOnActionHandler,
        ILockOnActionObservable
    {
        [Header("Target")]
        [SerializeField, ReadOnly]
        private SerializableReactiveProperty<UnitScopeRoot> _target = new();
        public ReadOnlyReactiveProperty<UnitScopeRoot> Target => _target;
        public float Distance { get; private set; }

        [Header("References")]
        public Transform ShotPos;

        [SerializeField, ReadOnly] private SerializableReactiveProperty<bool> _isAction = new();
        public SerializableReactiveProperty<bool> IsAction => _isAction;

        private Rigidbody _rigidbody;
        private UnitManager _unitManager;
        private UnitScopeRoot _currentUnit;
        private UnitSetting _unitSetting;
        private Health _targetHealth;
        private UnitSetting _targetSetting;
        private Vector3 _lastTargetPosition;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<ILockOnActionHandler, ILockOnActionObservable>();
        }

        public virtual void OnResolve(IObjectResolver resolver)
        {
            IsAction.AddTo(this);
            _target.AddTo(this);

            _rigidbody = resolver.Resolve<Rigidbody>();
            _unitManager = resolver.Resolve<UnitManager>();
            _currentUnit = resolver.Resolve<UnitScopeRoot>();
            _unitSetting = resolver.Resolve<UnitSetting>();
        }

        /// <summary>
        /// ボタン押下でロックオンをトグルする。呼び出し元(UnitActionController)は
        /// 押下時にのみvalue=trueで呼ぶため、解除はCancelActionで直接行う想定。
        /// </summary>
        public UniTask OnValueChanged(bool value, CancellationToken ct)
        {
            if (!IsAction.CurrentValue)
            {
                _target.Value = _unitManager.FindClosestEnemyUnit(_currentUnit, _unitSetting.MaxLockOnDistance);
                if (_target.Value != null)
                {
                    // ターゲットの情報を取得する
                    _targetHealth = _target.Value.Container.Resolve<Health>();
                    _targetSetting = _target.Value.Container.Resolve<UnitSetting>();
                    _lastTargetPosition = _targetSetting.Pivot;
                    IsAction.Value = true;
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
            _target.Value = null;
            IsAction.Value = false;
        }

        protected static bool TryGetLookRotation(Vector3 targetPosition, Vector3 fromPosition, out Quaternion rotation)
        {
            Vector3 direction = targetPosition - fromPosition;
            if (direction.sqrMagnitude < 0.0001f)
            {
                rotation = Quaternion.identity;
                return false;
            }

            rotation = Quaternion.LookRotation(direction);
            return true;
        }

        protected virtual void Update()
        {
            if (!IsTargetValid())
            {
                CancelAction();
                return;
            }

            // CurrentUnit(水平のみ)とShotPos(全方位)を、少し遅れてTargetの方向に向ける。
            _lastTargetPosition =
                Vector3.Lerp(_lastTargetPosition, _targetSetting.Pivot, _unitSetting.TrackingSpeed);
            LookAtTarget(_lastTargetPosition);
        }

        private bool IsTargetValid()
        {
            if (_target.Value != null)
            {
                if (_targetHealth.IsEmpty.CurrentValue) return false;
                
                Distance = Vector3.Distance(_unitSetting.Pivot, _targetSetting.Pivot);
                return Distance <= _unitSetting.MaxLockOnDistance;
            }
            
            return false;
        }

        protected virtual void LookAtTarget(Vector3 targetPos)
        {
            // 本体は水平方向(Yaw)のみTargetを向く。
            if (TryGetLookRotation(targetPos, _rigidbody.position, out var bodyRotation))
            {
                _rigidbody.MoveRotation(Quaternion.Euler(0f, bodyRotation.eulerAngles.y, 0f));
            }

            // 銃口は親の向きに関係なく、ワールド回転で直接Targetを狙う。
            if (TryGetLookRotation(targetPos, ShotPos.position, out var shotRotation))
            {
                ShotPos.rotation = shotRotation;
            }
        }
    }
}