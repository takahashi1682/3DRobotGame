using System.Threading;
using _Projects.Features.Game;
using Cysharp.Threading.Tasks;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit.Player
{
    public interface ILookActionObservable : IUnitActionObservable
    {
    }

    public interface ILookActionHandler : IUnitActionHandler<Vector2>
    {
    }

    /// <summary>
    /// マウス/スティック入力でカメラを動かす。水平方向(Yaw)はRigidbody本体を回転させ、
    /// 垂直方向(Pitch)はCameraTargetを直接回転させる。
    /// </summary>
    public class PlayerFreeLook : AbstractUnitAction,
        IUnitScopeMember,
        IScopeRegisterable,
        IScopeLaunchable,
        ILookActionHandler,
        ILookActionObservable
    {
        [Header("References")]
        public Transform CameraTarget;

        [Header("Settings")]
        public float CamSpeedX = 0.1f;
        public float CamSpeedY = 0.1f;
        public float LookupLimit = -80f;
        public float LookdownLimit = 80f;

        [Header("Non-Mouse Input Settings")]
        public string MouseDeviceName = "Mouse";
        public float NonMouseLookScale = 1500f;

        private Vector2 CurrentLook { get; set; }
        private float _yaw;

        [Inject] private IUnitControllable _control;
        [Inject] private Rigidbody _rigidbody;
        [Inject] private IUpdateObservable _updateObservable;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<ILookActionHandler, ILookActionObservable>();
        }

        public void OnLaunch()
        {
            IsAction.AddTo(this);
            _updateObservable.OnUpdate(EUpdatePhase.CameraPrepare)
                .Subscribe(_ =>
                {
                    OnPhaseUpdate();
                }).AddTo(this);
        }

        public UniTask OnValueChanged(Vector2 value, CancellationToken ct = default)
        {
            _isAction.Value = value.sqrMagnitude > 0f;
            CurrentLook = value;
            return UniTask.CompletedTask;
        }

        public void CancelAction()
        {
            _isAction.Value = false;
            CurrentLook = Vector2.zero;
        }

        public void OnPhaseUpdate()
        {
            if (!IsAction.CurrentValue) return;

            var scaledLook = GetScaledLook();
            ApplyHorizontalLook(scaledLook.x);
            ApplyVerticalLook(scaledLook.y);
        }

        private Vector2 GetScaledLook()
        {
            var lookValue = CurrentLook;
            if (_control.LookDeviceName != MouseDeviceName)
            {
                lookValue *= Time.deltaTime * NonMouseLookScale;
            }

            return lookValue;
        }

        private void ApplyHorizontalLook(float yawInput)
        {
            // _rigidbody.rotationは物理演算の更新までしか変わらないので、そこから読み直すと
            // 入力が上書きされてカクつく。yawは自分で足し込んで管理する。
            _yaw = Mathf.Repeat(_yaw + yawInput * CamSpeedX, 360f);
            _rigidbody.MoveRotation(Quaternion.Euler(0, _yaw, 0));
        }

        private void ApplyVerticalLook(float pitchInput)
        {
            float pitch = NormalizePitchAngle(CameraTarget.localEulerAngles.x) - pitchInput * CamSpeedY;
            pitch = Mathf.Clamp(pitch, LookupLimit, LookdownLimit);
            CameraTarget.localRotation = Quaternion.Euler(pitch, 0, 0);
        }

        private static float NormalizePitchAngle(float angle)
        {
            if (angle > 180f) angle -= 360f;
            return angle;
        }
    }
}