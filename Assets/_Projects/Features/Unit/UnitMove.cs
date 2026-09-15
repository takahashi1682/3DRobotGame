using System.Threading;
using Cysharp.Threading.Tasks;
using MyUtils;
using MyUtils.VContainerExtensions;
using R3;
using R3.Triggers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit
{
    public interface IMoveActionHandler : IUnitActionHandler<Vector2>
    {
    }

    public interface IMoveActionObservable : IUnitActionObservable
    {
    }

    public class UnitMove : AbstractUnitAction,
        IUnitScopeMember,
        IScopeRegisterable,
        IScopeLaunchable,
        IMoveActionHandler,
        IMoveActionObservable
    {
        [Header("References")]
        public Transform Target;

        [Header("Settings")]
        public float MovePower = 70f;
        public float PowerDownTime = 10f;
        public float StoppingPower = 2f;

        [field: SerializeField, ReadOnly] public Vector3 MoveDirection { get; private set; }

        private float _currentPower;
        [Inject] private Rigidbody _rigidbody;
        [Inject] private GroundDetection _groundDetection;
        [Inject] private IBoostActionObservable _playerBoost;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<IMoveActionHandler, IMoveActionObservable>();
        }

        public void OnLaunch()
        {
            IsAction.AddTo(this);

            _currentPower = MovePower;

            this.FixedUpdateAsObservable()
                .Subscribe(_ =>
                {
                    var moveDirection = GetGroundAlignedMoveDirection();
                    ApplyMove(moveDirection);
                    ApplyAutoStop();
                })
                .AddTo(this);
        }

        public UniTask OnValueChanged(Vector2 value, CancellationToken ct = default)
        {
            _isAction.Value = value.sqrMagnitude > 0f;
            var clampMagnitude = Vector2.ClampMagnitude(value, 1);
            MoveDirection = new Vector3(clampMagnitude.x, 0f, clampMagnitude.y);
            return UniTask.CompletedTask;
        }

        public void CancelAction()
        {
            _isAction.Value = false;
            MoveDirection = Vector3.zero;
        }

        private Vector3 GetGroundAlignedMoveDirection()
        {
            // 入力値の方向をキャラクターの向きに合わせる
            var moveDirection = Target.TransformDirection(MoveDirection);

            // 地面に設置しているときは移動方向を地面の法線に沿って調整する
            if (_groundDetection.IsHit.CurrentValue)
            {
                var groundNormal = _groundDetection.HitObject.CurrentValue.normal;
                moveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;
            }

            return moveDirection;
        }

        private void ApplyMove(Vector3 moveDirection)
        {
            // ブースト中はPlayerBoost.BoostPowerを目標に、それ以外はMovePowerを目標に近づける
            var targetPower = _playerBoost.IsAction.CurrentValue ? _playerBoost.BoostPower : MovePower;
            _currentPower = Mathf.Lerp(_currentPower, targetPower, PowerDownTime * Time.fixedDeltaTime);

            // 加速
            _rigidbody.linearVelocity += moveDirection * (_currentPower * Time.fixedDeltaTime);
        }

        private void ApplyAutoStop()
        {
            // 自動で止まる
            _rigidbody.linearVelocity =
                Vector3.Lerp(
                    _rigidbody.linearVelocity,
                    Vector3.zero,
                    StoppingPower * Time.fixedDeltaTime
                );
        }
    }
}