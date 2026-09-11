using MyUtils;
using MyUtils.Parameter.Basic;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Unit
{
    public interface IUnitTrackingHandler
    {
        void SetTarget(UnitScopeRoot target, float maxDistance);
        void ClearTarget();
    }

    public interface IUnitTrackingObservable
    {
        ReadOnlyReactiveProperty<UnitScopeRoot> Target { get; }
        bool IsLookingAtTarget { get; }
        float Distance { get; }
        float MaxDistance { get; }
    }

    /// <summary>
    /// 設定されたTargetの方向を向くよう、本体(水平のみ)とShotPos(全方位)を毎フレーム回転させる。
    /// ロックオンのオン/オフやターゲット選定(UnitLockOn)とは責務を分離しており、
    /// このクラスは「与えられたTargetを向き続ける」ことだけを担当する。
    /// </summary>
    public class UnitTracking : MonoBehaviour,
        IUnitScopeInitializable,
        IUnitTrackingHandler,
        IUnitTrackingObservable
    {
        [Header("Target")]
        [SerializeField, ReadOnly]
        private SerializableReactiveProperty<UnitScopeRoot> _target = new();
        public ReadOnlyReactiveProperty<UnitScopeRoot> Target => _target;

        public bool IsLookingAtTarget { get; private set; }
        public float Distance { get; private set; }
        public float MaxDistance { get; private set; }

        private Rigidbody _rigidbody;
        private UnitSetting _unitSetting;
        private Health _targetHealth;
        private UnitSetting _targetSetting;
        private Vector3 _lastTargetPosition;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<IUnitTrackingHandler, IUnitTrackingObservable>();
        }

        public void OnResolve(IObjectResolver resolver)
        {
            _target.AddTo(this);
            _rigidbody = resolver.Resolve<Rigidbody>();
            _unitSetting = resolver.Resolve<UnitSetting>();
        }

        public void SetTarget(UnitScopeRoot target, float maxDistance)
        {
            MaxDistance = maxDistance;

            // ターゲットの情報を取得する
            _target.Value = target;
            _targetHealth = _target.Value.Container.Resolve<Health>();
            _targetSetting = _target.Value.Container.Resolve<UnitSetting>();
            _lastTargetPosition = _targetSetting.Pivot;
        }

        public void ClearTarget()
        {
            IsLookingAtTarget = false;
            Distance = 0;
            MaxDistance = 0;
            _target.Value = null;
            _targetHealth = null;
            _targetSetting = null;
            _lastTargetPosition = Vector3.zero;
        }

        protected virtual void Update()
        {
            if (!IsTargetValid())
            {
                ClearTarget();
                return;
            }
            
            UpdateIsLookingAtTarget();

            // CurrentUnit(水平のみ)とShotPos(全方位)を、少し遅れてTargetの方向に向ける。
            _lastTargetPosition = Vector3.Lerp(_lastTargetPosition, _targetSetting.Pivot, _unitSetting.TrackingSpeed);
            LookAtTarget(_lastTargetPosition);
        }

        private bool IsTargetValid()
        {
            if (_target.Value == null) return false;
            if (_targetHealth.IsEmpty.CurrentValue) return false;

            Distance = Vector3.Distance(_unitSetting.Pivot, _targetSetting.Pivot);
            return Distance < MaxDistance;
        }

        /// <summary>
        /// 自身からTargetまでの間に、ObstacleLayerMaskに属する障害物がなければ「直視できている」とする。
        /// </summary>
        private void UpdateIsLookingAtTarget()
        {
            var direction = (_targetSetting.Pivot - _unitSetting.Pivot).normalized;
            IsLookingAtTarget = !Physics.Raycast(
                _unitSetting.Pivot, direction, Distance, _unitSetting.ObstacleLayerMask);
        }

        protected virtual void LookAtTarget(Vector3 targetPos)
        {
            // 本体は水平方向(Yaw)のみTargetを向く。
            if (TryGetLookRotation(targetPos, _rigidbody.position, out var bodyRotation))
            {
                _rigidbody.MoveRotation(Quaternion.Euler(0f, bodyRotation.eulerAngles.y, 0f));
            }

            // 銃口は親の向きに関係なく、ワールド回転で直接Targetを狙う。
            if (TryGetLookRotation(targetPos, _unitSetting.FirePoint.position, out var shotRotation))
            {
                _unitSetting.FirePoint.rotation = shotRotation;
            }
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
    }
}