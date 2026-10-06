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
    /// 入力の読み取りを行うクラス。
    /// Input Systemの自動更新はタイミングが不定なので手動更新(ProcessEventsManually)に切り替え、
    /// Inputフェーズの先頭でこちらから更新することで、毎フレーム一番最初に入力を確定させる。
    /// </summary>
    public class PlayerInputReader : AbstractUnitControllable,
        InputSystem_Actions.IPlayerActions,
        IScopeRegisterable
    {
        [SerializeField, ReadOnly] private SerializableReactiveProperty<string> _lookDeviceName = new();
        public override string LookDeviceName => _lookDeviceName.Value;

        private InputSystem_Actions _actions;
        public InputSystem_Actions.PlayerActions Player { get; private set; }

        [Inject] private IUpdateObservable _updateObservable;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<IUnitControllable>();
        }

        private void Awake()
        {
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;

            _actions = new InputSystem_Actions();
            Player = _actions.Player;
            Player.AddCallbacks(this);

            _updateObservable.OnUpdate(EUpdatePhase.Input)
                .Subscribe(_ => OnPhaseUpdate())
                .AddTo(this);
        }
        
        private void OnEnable() => _actions.Enable();
        private void OnDisable() => _actions.Disable();

        private void OnDestroy()
        {
            Player.RemoveCallbacks(this);
            _actions.Dispose();
        }

        private static void OnPhaseUpdate() => InputSystem.Update();

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
    }
}