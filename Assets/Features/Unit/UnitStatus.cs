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

    public class UnitStatus : AbstractFlagsParameter<EPlayerState>, IUnitScopeInitializable
    {
        [field: SerializeField, ReadOnly] public bool CanMove { get; private set; }
        [field: SerializeField, ReadOnly] public bool CanLook { get; private set; }
        [field: SerializeField, ReadOnly] public bool CanLockOn { get; private set; }
        [field: SerializeField, ReadOnly] public bool CanFire { get; private set; }
        [field: SerializeField, ReadOnly] public bool CanBoost { get; private set; }
        [field: SerializeField, ReadOnly] public bool CanFly { get; private set; }

        public virtual void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this);
        }

        public virtual void OnResolve(IObjectResolver resolver)
        {
            if (resolver.TryResolve<ILookActionObservable>(out var look))
            {
                look.IsAction.Subscribe(x => SetFlag((int)EPlayerState.Look, x)).AddTo(this);
            }

            var move = resolver.Resolve<IMoveActionObservable>();
            move.IsAction.Subscribe(x => SetFlag((int)EPlayerState.Move, x)).AddTo(this);

            var ground = resolver.Resolve<GroundDetection>();
            ground.IsHit.Subscribe(x => SetFlag((int)EPlayerState.Grounded, x)).AddTo(this);

            var boost = resolver.Resolve<IBoostActionObservable>();
            boost.IsAction.Subscribe(x => SetFlag((int)EPlayerState.Boost, x)).AddTo(this);

            var lockOn = resolver.Resolve<ILockOnActionObservable>();
            lockOn.IsAction.Subscribe(x => SetFlag((int)EPlayerState.LockOn, x)).AddTo(this);

            var fire = resolver.Resolve<IFireActionObservable>();
            fire.IsAction.Subscribe(x => SetFlag((int)EPlayerState.Fire, x)).AddTo(this);

            var health = resolver.Resolve<Health>();
            health.IsEmpty.Subscribe(x => SetFlag((int)EPlayerState.Dead, x)).AddTo(this);

            var energy = resolver.Resolve<Energy>();
            energy.IsEmpty.Subscribe(x => SetFlag((int)EPlayerState.EnergyEmpty, x)).AddTo(this);

            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    // Moveできる条件
                    var canMove = true;
                    canMove &= !HasFlag((int)EPlayerState.Dead);
                    CanMove = canMove;

                    // Lookできる条件
                    var canLook = true;
                    canLook &= !HasFlag((int)EPlayerState.Dead);
                    canLook &= !HasFlag((int)EPlayerState.LockOn);
                    CanLook = canLook;

                    // Boostできる条件
                    var canBoost = true;
                    canBoost &= !HasFlag((int)EPlayerState.Boost);
                    canBoost &= !HasFlag((int)EPlayerState.Dead);
                    canBoost &= energy.CurrentValue > boost.BoostEnergy;
                    CanBoost = canBoost;

                    // Fireできる条件
                    var canFire = true;
                    canFire &= !HasFlag((int)EPlayerState.Dead);
                    CanFire = canFire;

                    // Flyできる条件
                    var canFly = true;
                    canFly &= !HasFlag((int)EPlayerState.EnergyEmpty);
                    canFly &= !HasFlag((int)EPlayerState.Dead);
                    CanFly = canFly;

                    // LockOnできる条件
                    var canLockOn = true;
                    canLockOn &= !HasFlag((int)EPlayerState.Dead);
                    CanLockOn = canLockOn;
                }).AddTo(this);
        }
    }
}