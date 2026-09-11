using Features.Unit.Player;
using MyUtils;
using MyUtils.Parameter;
using MyUtils.Parameter.Basic;
using R3;
using R3.Triggers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Unit
{
    public enum EPlayerState
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

    /// <summary>CanXxxプロパティに対応するフラグ。EPlayerStateと同じビットフラグ機構で管理する。</summary>
    public enum ECanFlags
    {
        Move,
        Look,
        LockOn,
        Fire,
        Boost,
        Fly,
    }

    public class UnitStatus : MonoBehaviour, IUnitScopeInitializable
    {
        [SerializeField] private FlagsParameter<EPlayerState> _stateFlags = new();
        [SerializeField] private FlagsParameter<ECanFlags> _canFlags = new();

        public bool HasFlag(EPlayerState flag) => _stateFlags.HasFlag(flag);

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

        public virtual void OnResolve(IObjectResolver resolver)
        {
            if (resolver.TryResolve<ILookActionObservable>(out var look))
            {
                look.IsAction.Subscribe(x => _stateFlags.SetFlag(EPlayerState.Look, x)).AddTo(this);
            }

            var move = resolver.Resolve<IMoveActionObservable>();
            move.IsAction.Subscribe(x => _stateFlags.SetFlag(EPlayerState.Move, x)).AddTo(this);

            var ground = resolver.Resolve<GroundDetection>();
            ground.IsHit.Subscribe(x => _stateFlags.SetFlag(EPlayerState.Grounded, x)).AddTo(this);

            var boost = resolver.Resolve<IBoostActionObservable>();
            boost.IsAction.Subscribe(x => _stateFlags.SetFlag(EPlayerState.Boost, x)).AddTo(this);

            var lockOn = resolver.Resolve<ILockOnActionObservable>();
            lockOn.IsAction.Subscribe(x => _stateFlags.SetFlag(EPlayerState.LockOn, x)).AddTo(this);

            var fire = resolver.Resolve<IFireActionObservable>();
            fire.IsAction.Subscribe(x => _stateFlags.SetFlag(EPlayerState.Fire, x)).AddTo(this);

            var health = resolver.Resolve<Health>();
            health.IsEmpty.Subscribe(x => _stateFlags.SetFlag(EPlayerState.Dead, x)).AddTo(this);

            var energy = resolver.Resolve<Energy>();
            energy.IsEmpty.Subscribe(x => _stateFlags.SetFlag(EPlayerState.EnergyEmpty, x)).AddTo(this);

            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    // Moveできる条件
                    var canMove = true;
                    canMove &= !HasFlag(EPlayerState.Dead);
                    _canFlags.SetFlag(ECanFlags.Move, canMove);

                    // Lookできる条件
                    var canLook = true;
                    canLook &= !HasFlag(EPlayerState.Dead);
                    canLook &= !HasFlag(EPlayerState.LockOn);
                    _canFlags.SetFlag(ECanFlags.Look, canLook);

                    // Boostできる条件
                    var canBoost = true;
                    canBoost &= !HasFlag(EPlayerState.Boost);
                    canBoost &= !HasFlag(EPlayerState.Dead);
                    canBoost &= energy.CurrentValue > boost.BoostEnergy;
                    _canFlags.SetFlag(ECanFlags.Boost, canBoost);

                    // Fireできる条件
                    var canFire = true;
                    canFire &= !HasFlag(EPlayerState.Dead);
                    _canFlags.SetFlag(ECanFlags.Fire, canFire);

                    // Flyできる条件
                    var canFly = true;
                    canFly &= !HasFlag(EPlayerState.EnergyEmpty);
                    canFly &= !HasFlag(EPlayerState.Dead);
                    _canFlags.SetFlag(ECanFlags.Fly, canFly);

                    // LockOnできる条件
                    var canLockOn = true;
                    canLockOn &= !HasFlag(EPlayerState.Dead);
                    _canFlags.SetFlag(ECanFlags.LockOn, canLockOn);
                }).AddTo(this);
        }
    }
}
