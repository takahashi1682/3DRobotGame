using System;
using Features.Unit.Battle;
using MyUtils.Parameter.Basic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Unit
{
    public class UnitSetting : MonoBehaviour, IUnitScopeInitializable
    {
        [Header("Stats")]
        [field: SerializeField] public int MaxHealth { get; private set; } = 5000;
        [field: SerializeField] public int MaxEnergy { get; private set; } = 1000;

        [Header("References")]
        [field: SerializeField] public Transform UnitPivot { get; private set; }
        [field: SerializeField] public Transform FirePoint { get; private set; }

        [Header("Combat")]
        [field: SerializeField] public float TrackingSpeed { get; private set; } = 0.1f;
        [field: SerializeField] public float MaxLockOnDistance { get; private set; } = 200f;
        [field: SerializeField] public ArmyType Army { get; private set; } = ArmyType.Player;

        /// <summary>
        /// 距離判定・ロックオンなどで使う、このUnitの基準座標。
        /// </summary>
        public Vector3 Pivot => UnitPivot.position;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this);
        }

        public void OnResolve(IObjectResolver resolver)
        {
            var health = resolver.Resolve<Health>();
            health.SetMax(MaxHealth);
            health.SetFull();

            var energy = resolver.Resolve<Energy>();
            energy.SetMax(MaxEnergy);
            energy.SetFull();
        }
    }
}