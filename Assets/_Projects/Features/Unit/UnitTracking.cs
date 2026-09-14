using _Projects.Features.Game;
using MyUtils;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit
{
    public interface IUnitTrackingHandler
    {
        void SetTarget(UnitScopeRoot target, float maxDistance);
        void ClearTarget();
    }

    public interface IUnitTrackingObservable : IUnitActionObservable
    {
        ReadOnlyReactiveProperty<UnitScopeRoot> Target { get; }
        bool IsLookingAtTarget { get; }
        Vector3 TargetPosition { get; }
        float Distance { get; }
    }

    /// <summary>
    /// 設定されたTargetの方向を向くよう、本体(水平のみ)とFirePoint(全方位)を毎フレーム回転させる。
    /// ロックオンのオン/オフやターゲット選定(UnitLockOn)とは責務を分離しており、
    /// このクラスは「与えられたTargetを向き続ける」ことだけを担当する。
    /// </summary>
    public class UnitTracking : MonoBehaviour,
        IUnitScopeMember,
        IScopeRegisterable,
        IScopeLaunchable,
        IUnitTrackingHandler,
        IUnitTrackingObservable,
        IPhaseUpdatable
    {
        private const float MinDirectionSqrMagnitude = 0.0001f;
        private const float UnlockedAimDistance = 200f;

        [SerializeField, ReadOnly] protected SerializableReactiveProperty<bool> _isAction = new();
        public ReadOnlyReactiveProperty<bool> IsAction => _isAction;
        [SerializeField, ReadOnly] protected SerializableReactiveProperty<UnitScopeRoot> _target = new();
        public ReadOnlyReactiveProperty<UnitScopeRoot> Target => _target;
        public bool IsLookingAtTarget { get; private set; }
        public Vector3 TargetPosition { get; protected set; }
        public float Distance { get; private set; }
        private float MaxDistance { get; set; }

        [Inject] protected Rigidbody _rigidbody;
        [Inject] protected UnitSetting _unitSetting;
        [Inject] private UpdateDispatcher _dispatcher;
        protected Transform _targetPivot;

        public UpdatePhase Phase => UpdatePhase.Movement;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<IUnitTrackingHandler, IUnitTrackingObservable>();
        }

        public void OnLaunch()
        {
            _target.AddTo(this);
            _dispatcher.Register(this);
        }

        private void OnDestroy()
        {
            _dispatcher.Unregister(this);
        }

        public void SetTarget(UnitScopeRoot target, float maxDistance)
        {
            MaxDistance = maxDistance;

            // ターゲットの情報を取得する
            _target.Value = target;
            _targetPivot = _target.Value.Setting.UnitPivot;
            TargetPosition = _targetPivot.position;

            _isAction.Value = true;
        }

        public void ClearTarget()
        {
            // 銃口を初期位置(正面)に戻す
            _unitSetting.FirePoint.localRotation = Quaternion.identity;

            IsLookingAtTarget = false;
            Distance = 0;
            MaxDistance = 0;

            _target.Value = null;
            _targetPivot = null;

            _isAction.Value = false;
        }

        public virtual void OnPhaseUpdate()
        {
            if (!IsTargetValid())
            {
                ClearTarget();
            }

            if (_target.Value != null)
            {
                TargetPosition = Vector3.MoveTowards(TargetPosition, _targetPivot.position,
                    _unitSetting.RotationSpeed * Time.deltaTime);

                // ターゲットが有効な場合は、毎フレーム回転を更新する
                RotateBodyTowardsTarget(TargetPosition);

                // ターゲットが視界に入っているかどうかを判定する
                UpdateIsLookingAtTarget(TargetPosition);
            }
            else
            {
                TargetPosition = GetUnlockedTargetPosition();
            }

            // 銃口(FirePoint)は、ターゲットが有効であればターゲット方向へ、無効であれば正面方向へ回転させる
            RotateFirePointTowardsTarget(TargetPosition);
        }

        /// <summary>
        /// ロックオン対象がない間、銃口を正面方向に向けておくための仮想ターゲット位置。
        /// </summary>
        protected virtual Vector3 GetUnlockedTargetPosition()
        {
            return GetForwardPosition(_unitSetting.UnitPivot);
        }

        protected static Vector3 GetForwardPosition(Transform origin, float distance = UnlockedAimDistance)
        {
            return origin.position + origin.forward * distance;
        }

        protected bool IsTargetValid()
        {
            if (!_isAction.CurrentValue) return false;
            if (_target.Value == null) return false;
            if (!_target.CurrentValue.Running.CurrentValue) return false;

            Distance = Vector3.Distance(_unitSetting.Pivot, _targetPivot.position);
            return Distance < MaxDistance;
        }

        /// <summary>
        /// 自身からTargetまでの間に、ObstacleLayerMaskに属する障害物がなければ「直視できている」とする。
        /// </summary>
        private void UpdateIsLookingAtTarget(Vector3 targetPosition)
        {
            var direction = (targetPosition - _unitSetting.Pivot).normalized;
            IsLookingAtTarget = !Physics.Raycast(
                _unitSetting.Pivot, direction, Distance, _unitSetting.ObstacleLayerMask);
        }

        /// <summary>
        /// 本体を水平方向(Yaw)のみ、現在の角度からターゲット方向へ一定速度で回転させる。
        /// </summary>
        private void RotateBodyTowardsTarget(Vector3 targetPosition)
        {
            if (!TryGetDirection(_rigidbody.position, targetPosition, out var diff)) return;

            var targetY = Mathf.Atan2(diff.x, diff.z) * Mathf.Rad2Deg;
            _rigidbody.MoveRotation(Quaternion.Euler(0f, targetY, 0f));
        }

        /// <summary>
        /// 銃口(FirePoint)を、親の向きに関係なくワールド回転で直接ターゲットへ、一定速度で回転させる。
        /// </summary>
        private void RotateFirePointTowardsTarget(Vector3 targetPosition)
        {
            if (!TryGetDirection(_unitSetting.FirePoint.position, targetPosition, out var diff)) return;

            var targetRotation = Quaternion.LookRotation(diff);
            _unitSetting.FirePoint.rotation = targetRotation;
        }

        /// <summary>
        /// fromからtoへの方向ベクトルを求める。距離が近すぎて方向が定まらない場合はfalseを返す。
        /// </summary>
        private static bool TryGetDirection(Vector3 from, Vector3 to, out Vector3 direction)
        {
            direction = to - from;
            return direction.sqrMagnitude >= MinDirectionSqrMagnitude;
        }
    }
}