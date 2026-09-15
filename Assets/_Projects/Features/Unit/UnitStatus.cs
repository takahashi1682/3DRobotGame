using _Projects.Features.Unit.Player;
using MyUtils.Parameter;
using MyUtils.Parameter.Basic;
using MyUtils.VContainerExtensions;
using R3;
using R3.Triggers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit
{
    public enum EUnitState
    {
        Move,
        Look,
        LockOn,
        Fire,
        Grounded,
        Boost,
        EnergyEmpty,
        Dead,
    }

    /// <summary>CanXxxプロパティに対応するフラグ。EUnitStateと同じビットフラグ機構で管理する。</summary>
    public enum ECanFlags
    {
        Move,
        Look,
        LockOn,
        Fire,
        Boost,
        Fly,
    }

    public class UnitStatus : MonoBehaviour,
        IUnitScopeMember,
        IScopeRegisterable,
        IScopeLaunchable
    {
        [SerializeField] private FlagsParameter<EUnitState> _stateFlags = new();
        [SerializeField] private FlagsParameter<ECanFlags> _canFlags = new();

        public bool HasFlag(EUnitState flag) => _stateFlags.HasFlag(flag);

        public bool CanMove => _canFlags.HasFlag(ECanFlags.Move);
        public bool CanLook => _canFlags.HasFlag(ECanFlags.Look);
        public bool CanLockOn => _canFlags.HasFlag(ECanFlags.LockOn);
        public bool CanFire => _canFlags.HasFlag(ECanFlags.Fire);
        public bool CanBoost => _canFlags.HasFlag(ECanFlags.Boost);
        public bool CanFly => _canFlags.HasFlag(ECanFlags.Fly);

        public virtual void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this);
        }

        private ILookActionObservable _look;
        [Inject] private IMoveActionObservable _move;
        [Inject] private GroundDetection _ground;
        [Inject] private IBoostActionObservable _boost;
        [Inject] private ILockOnActionObservable _lockOn;
        [Inject] private IFireActionObservable _fire;
        [Inject] private Health _health;
        [Inject] private Energy _energy;

        [Inject] private IObjectResolver _resolver;

        public virtual void OnLaunch()
        {
            _resolver.TryResolve(out _look);
            if (_look != null)
            {
                _look.IsAction.Subscribe(x => _stateFlags.SetFlag(EUnitState.Look, x)).AddTo(this);
            }

            _move.IsAction.Subscribe(x => _stateFlags.SetFlag(EUnitState.Move, x)).AddTo(this);
            _ground.IsHit.Subscribe(x => _stateFlags.SetFlag(EUnitState.Grounded, x)).AddTo(this);
            _boost.IsAction.Subscribe(x => _stateFlags.SetFlag(EUnitState.Boost, x)).AddTo(this);
            _lockOn.IsAction.Subscribe(x => _stateFlags.SetFlag(EUnitState.LockOn, x)).AddTo(this);
            _fire.IsAction.Subscribe(x => _stateFlags.SetFlag(EUnitState.Fire, x)).AddTo(this);
            _health.IsEmpty.Subscribe(x => _stateFlags.SetFlag(EUnitState.Dead, x)).AddTo(this);
            _energy.IsEmpty.Subscribe(x => _stateFlags.SetFlag(EUnitState.EnergyEmpty, x)).AddTo(this);

            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    // Moveできる条件
                    var canMove = true;
                    canMove &= !HasFlag(EUnitState.Dead);
                    _canFlags.SetFlag(ECanFlags.Move, canMove);

                    // Lookできる条件
                    var canLook = true;
                    canLook &= !HasFlag(EUnitState.Dead);
                    canLook &= !HasFlag(EUnitState.LockOn);
                    _canFlags.SetFlag(ECanFlags.Look, canLook);

                    // Boostできる条件
                    var canBoost = true;
                    canBoost &= HasFlag(EUnitState.Move);
                    canBoost &= !HasFlag(EUnitState.Boost);
                    canBoost &= !HasFlag(EUnitState.Dead);
                    canBoost &= _energy.CurrentValue > _boost.BoostEnergy;
                    _canFlags.SetFlag(ECanFlags.Boost, canBoost);

                    // Fireできる条件
                    var canFire = true;
                    canFire &= !HasFlag(EUnitState.Dead);
                    _canFlags.SetFlag(ECanFlags.Fire, canFire);

                    // Flyできる条件
                    var canFly = true;
                    canFly &= !HasFlag(EUnitState.EnergyEmpty);
                    canFly &= !HasFlag(EUnitState.Dead);
                    _canFlags.SetFlag(ECanFlags.Fly, canFly);

                    // LockOnできる条件
                    var canLockOn = true;
                    canLockOn &= !HasFlag(EUnitState.Dead);
                    _canFlags.SetFlag(ECanFlags.LockOn, canLockOn);
                }).AddTo(this);
        }
    }
}