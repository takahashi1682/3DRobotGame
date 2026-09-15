using MyUtils.Parameter.Basic;
using MyUtils.VContainerExtensions;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit.Battle
{
    /// <summary>
    /// ダメージを受け取るクラス
    /// </summary>
    public class DamageHandler : MonoBehaviour,
        IUnitScopeMember,
        IScopeRegisterable,
        IDamageable
    {
        [Inject] public IObjectResolver Owner { get; private set; }

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this);
        }

        public bool TakeDamage(IDamageSource source)
        {
            var sourceArmy = source.Owner.Resolve<UnitSetting>().Army;
            var targetArmy = Owner.Resolve<UnitSetting>().Army;
            if (!sourceArmy.CanHit(targetArmy))
            {
                return false;
            }

            var health = Owner.Resolve<Health>();

            if (health.IsEmpty.CurrentValue)
            {
                return false;
            }

            health.Sub(source.Damage);
            return true;
        }
    }
}