using Features.Unit;
using MyUtils;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Features.Input
{
    public class PlayerInputReader : MonoBehaviour,
        InputSystem_Actions.IPlayerActions,
        IUnitControllable,
        IUnitScopeInitializable
    {
        [SerializeField] private SerializableReactiveProperty<Vector2> _move = new();
        public Observable<Vector2> Move => _move;

        [SerializeField] private SerializableReactiveProperty<Vector2> _look = new();
        public Observable<Vector2> Look => _look;

        [SerializeField, ReadOnly] private SerializableReactiveProperty<string> _lookDeviceName = new();
        public string LookDeviceName => _lookDeviceName.Value;

        [SerializeField] private SerializableReactiveProperty<bool> _fire = new();
        public Observable<bool> Fire => _fire;

        [SerializeField] private SerializableReactiveProperty<bool> _fly = new();
        public Observable<bool> Fly => _fly;

        [SerializeField] private SerializableReactiveProperty<bool> _boost = new();
        public Observable<bool> Boost => _boost;

        [SerializeField] private SerializableReactiveProperty<bool> _lockOn = new();
        public Observable<bool> LockOn => _lockOn;

        private InputSystem_Actions _actions;
        public InputSystem_Actions.PlayerActions Player { get; private set; }

        private void Awake()
        {
            _actions = new InputSystem_Actions();
            Player = _actions.Player;
            Player.AddCallbacks(this);
            Player.Enable();
        }

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<IUnitControllable>();
        }

        public void OnResolve(IObjectResolver resolver)
        {
        }

        private void OnDestroy() => _actions.Dispose();
        private void OnEnable() => _actions.Enable();
        private void OnDisable() => _actions.Disable();

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