using MyUtils.Parameter.Basic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit.Battle
{
    /// <summary>
    /// ダメージを受け取るクラス
    /// </summary>
    public class DamageHandler : MonoBehaviour,
        IUnitScopeInitializable,
        IDamageable
    {
        public IObjectResolver Owner { get; private set; }

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this);
        }

        public void OnResolve(IObjectResolver resolver)
        {
            Owner = resolver;
        }

        public bool TakeDamage(IDamageSource source)
        {
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