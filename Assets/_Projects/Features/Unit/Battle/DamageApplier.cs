using UnityEngine;
using VContainer;

namespace _Projects.Features.Unit.Battle
{
    /// <summary>
    /// ダメージ適用を行う。当たり判定(Rayスイープ)と移動・寿命の管理は、
    /// 同一GameObjectのBulletController(AbstractSweepDetector)が担当する。
    /// </summary>
    public class DamageApplier : MonoBehaviour, IDamageSource
    {
        [Header("Settings")]
        [field: SerializeField] public int Damage { get; set; } = 100;
        public IObjectResolver Owner { get; private set; }

        public void Build(IObjectResolver resolver)
        {
            Owner = resolver;
        }

        public void TryApplyDamage(Collider other)
        {
            // 自分自身(発射者)のRigidbodyは無視する
            if (Owner != null && Owner.TryResolve<Rigidbody>(out var rb))
            {
                if (other.attachedRigidbody == rb) return;
            }

            // ダメージを与えられるか判定
            if (other.TryGetComponent(out IDamageable damageable))
            {
                // ダメージを与える
                if (!damageable.TakeDamage(this)) return;
            }

            gameObject.SetActive(false); // 弾を非アクティブ化して再利用可能にする
        }
    }
}