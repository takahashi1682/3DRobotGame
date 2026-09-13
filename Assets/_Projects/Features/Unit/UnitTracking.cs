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
        IScopeResolvable,
        IScopeStartable,
        IUnitTrackingHandler,
        IUnitTrackingObservable
    {
        private const float MinDirectionSqrMagnitude = 0.0001f;

        [SerializeField, ReadOnly] protected SerializableReactiveProperty<bool> _isAction = new();
        public ReadOnlyReactiveProperty<bool> IsAction => _isAction;
        [SerializeField, ReadOnly] protected SerializableReactiveProperty<UnitScopeRoot> _target = new();
        public ReadOnlyReactiveProperty<UnitScopeRoot> Target => _target;
        public bool IsLookingAtTarget { get; private set; }
        public float Distance { get; private set; }
        private float MaxDistance { get; set; }

        protected Rigidbody _rigidbody;
        protected UnitSetting _unitSetting;
        protected Transform _targetPivot;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<IUnitTrackingHandler, IUnitTrackingObservable>();
        }

        public void OnResolve(IObjectResolver resolver)
        {
            _rigidbody = resolver.Resolve<Rigidbody>();
            _unitSetting = resolver.Resolve<UnitSetting>();
        }

        public void OnStart()
        {
            _target.AddTo(this);
        }

        public void SetTarget(UnitScopeRoot target, float maxDistance)
        {
            MaxDistance = maxDistance;

            // ターゲットの情報を取得する
            _target.Value = target;
            _targetPivot = _target.Value.Setting.UnitPivot;

            _isAction.Value = true;
        }

        public void ClearTarget()
        {
            Debug.Log(2);
            // 銃口を初期位置(正面)に戻す
            _unitSetting.FirePoint.localRotation = Quaternion.identity;

            IsLookingAtTarget = false;
            Distance = 0;
            MaxDistance = 0;

            _target.Value = null;
            _targetPivot = null;

            _isAction.Value = false;
        }

        protected virtual void Update()
        {
            if (!IsTargetValid())
            {
                // ターゲットが無効な場合は銃口を初期位置に戻す
                ClearTarget();
                return;
            }

            // ターゲットが有効な場合は、毎フレーム回転を更新する
            RotateBodyTowardsTarget();
            RotateFirePointTowardsTarget();
        }

        protected virtual void FixedUpdate()
        {
            if (!_isAction.CurrentValue) return;

            // ターゲットが視界に入っているかどうかを判定する
            UpdateIsLookingAtTarget();
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
        private void UpdateIsLookingAtTarget()
        {
            var direction = (_targetPivot.position - _unitSetting.Pivot).normalized;
            IsLookingAtTarget = !Physics.Raycast(
                _unitSetting.Pivot, direction, Distance, _unitSetting.ObstacleLayerMask);
        }

        /// <summary>
        /// 本体を水平方向(Yaw)のみ、現在の角度からターゲット方向へ一定速度で回転させる。
        /// </summary>
        private void RotateBodyTowardsTarget()
        {
            if (!TryGetDirection(_rigidbody.position, _targetPivot.position, out var diff)) return;

            // diffはワールド座標のベクトルなので、Atan2にそのまま渡せば絶対角度(目標のY角度)が求まる。
            var currentY = _rigidbody.rotation.eulerAngles.y;
            var targetY = Mathf.Atan2(diff.x, diff.z) * Mathf.Rad2Deg;
            var newY = Mathf.MoveTowardsAngle(currentY, targetY, _unitSetting.RotationSpeed * Time.deltaTime);
            _rigidbody.MoveRotation(Quaternion.Euler(0f, newY, 0f));
        }

        /// <summary>
        /// 銃口(FirePoint)を、親の向きに関係なくワールド回転で直接ターゲットへ、一定速度で回転させる。
        /// </summary>
        private void RotateFirePointTowardsTarget()
        {
            if (!TryGetDirection(_unitSetting.FirePoint.position, _targetPivot.position, out var diff)) return;

            var targetRotation = Quaternion.LookRotation(diff);
            _unitSetting.FirePoint.rotation = Quaternion.RotateTowards(
                _unitSetting.FirePoint.rotation, targetRotation, _unitSetting.RotationSpeed * Time.deltaTime);
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
