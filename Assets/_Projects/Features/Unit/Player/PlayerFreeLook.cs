using System.Threading;
using Cysharp.Threading.Tasks;
using MyUtils;
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
    /// マウス/スティック入力を受けて、水平・垂直それぞれのTransformを回転させるカメラ制御。
    /// 水平方向はRigidbody本体をMoveRotationで回転させ、垂直方向はCameraTarget(物理に関与しない
    /// 子Transform)を直接回転させる。どちらも毎レンダーフレーム(Update)で反映する。
    /// MoveRotationはInterpolateと組み合わせても、呼び出し頻度自体がその間隔でしか目標値を
    /// 更新しないため、FixedUpdateで呼ぶと物理ティック単位の粗さが見た目のカクつきとして残る。
    /// Updateで毎フレーム呼ぶことでこれを避けている。
    /// </summary>
    public class PlayerFreeLook : MonoBehaviour,
        IUnitScopeInitializable,
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

        public Vector2 CurrentLook { get; private set; }

        [SerializeField, ReadOnly] private SerializableReactiveProperty<bool> _isAction = new();
        public ReadOnlyReactiveProperty<bool> IsAction => _isAction;

        private IUnitControllable _control;
        private Rigidbody _rigidbody;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<ILookActionHandler, ILookActionObservable>();
        }

        public void OnResolve(IObjectResolver resolver)
        {
            IsAction.AddTo(this);

            _control = resolver.Resolve<IUnitControllable>();
            _rigidbody = resolver.Resolve<Rigidbody>();
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

        private void Update()
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
            float yaw = _rigidbody.rotation.eulerAngles.y + yawInput * CamSpeedX;
            _rigidbody.MoveRotation(Quaternion.Euler(0, yaw, 0));
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