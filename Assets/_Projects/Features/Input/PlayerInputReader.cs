using _Projects.Features.Game;
using _Projects.Features.Unit;
using MyUtils;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Input
{
    /// <summary>
    ///     入力の読み取りを行うクラス。
    ///     Input Systemの自動更新はタイミングが不定なので手動更新(ProcessEventsManually)に切り替え、
    ///     Inputフェーズの先頭でこちらから更新することで、毎フレーム一番最初に入力を確定させる。
    /// </summary>
    public class PlayerInputReader : MonoBehaviour,
        InputSystem_Actions.IPlayerActions,
        IUnitControllable,
        IScopeRegisterable,
        IScopeLaunchable
    {
        [SerializeField] private SerializableReactiveProperty<Vector2> _move = new();

        [SerializeField] private SerializableReactiveProperty<Vector2> _look = new();

        [SerializeField] [ReadOnly] private SerializableReactiveProperty<string> _lookDeviceName = new();

        [SerializeField] private SerializableReactiveProperty<bool> _fire = new();

        [SerializeField] private SerializableReactiveProperty<bool> _fly = new();

        [SerializeField] private SerializableReactiveProperty<bool> _boost = new();

        [SerializeField] private SerializableReactiveProperty<bool> _lockOn = new();

        private InputSystem_Actions _actions;

        [Inject] private IUpdateObservable _updateObservable;
        public InputSystem_Actions.PlayerActions Player { get; private set; }

        private void Awake()
        {
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;

            _actions = new InputSystem_Actions();
            Player = _actions.Player;
            Player.AddCallbacks(this);
            Player.Enable();
        }

        private void OnEnable() => _actions.Enable();
        private void OnDisable() => _actions.Disable();

        private void OnDestroy() => _actions.Dispose();

        public void OnMove(InputAction.CallbackContext context) => _move.Value = context.ReadValue<Vector2>();

        public void OnLook(InputAction.CallbackContext context)
        {
            _look.Value = context.ReadValue<Vector2>();
            _lookDeviceName.Value = context.control?.device?.displayName ?? string.Empty;
        }

        public void OnFire(InputAction.CallbackContext context) => _fire.Value = context.ReadValueAsButton();
        public void OnFly(InputAction.CallbackContext context) => _fly.Value = context.ReadValueAsButton();
        public void OnBoost(InputAction.CallbackContext context) => _boost.Value = context.ReadValueAsButton();
        public void OnLockOn(InputAction.CallbackContext context) => _lockOn.Value = context.ReadValueAsButton();

        public void OnLaunch() => _updateObservable.OnUpdate(EUpdatePhase.Input)
            .Subscribe(_ => OnPhaseUpdate())
            .AddTo(this);

        public void OnRegister(IContainerBuilder builder) => builder.RegisterComponent(this).As<IUnitControllable>();
        public Observable<Vector2> Move => _move;
        public Observable<Vector2> Look => _look;
        public string LookDeviceName => _lookDeviceName.Value;
        public Observable<bool> Fire => _fire;
        public Observable<bool> Fly => _fly;
        public Observable<bool> Boost => _boost;
        public Observable<bool> LockOn => _lockOn;

        public void OnPhaseUpdate() => InputSystem.Update();
    }
}