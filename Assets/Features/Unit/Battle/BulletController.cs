using MyUtils.Detector;
using UnityEngine;
using VContainer;

namespace Features.Unit.Battle
{
    /// <summary>
    /// 弾の移動・寿命(プーリング再利用)を管理する。
    /// 当たり判定とダメージ適用は、同一GameObjectのDamageApplierに委譲する。
    /// </summary>
    [RequireComponent(typeof(DamageApplier))]
    public class BulletController : AbstractSweepDetector
    {
        [Header("References")]
        [SerializeField] private DamageApplier _damageApplier;
        [SerializeField] private TrailRenderer _trailRenderer;

        [Header("Settings")]
        public float LifeTime = 2f;
        public float Speed = 900f;
        public float FireBlur = 3f;

        private float _lifeTimer;

        public void Initialize(IObjectResolver resolver, Vector3 position, Quaternion rotation)
        {
            _damageApplier.Build(resolver);
            ResetBullet(position, rotation);
        }

        public void ResetBullet(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;

            // 弾の回転値にランダムな回転値を加算（×と加算となる）
            float bx = Random.Range(-FireBlur, FireBlur);
            float by = Random.Range(-FireBlur, FireBlur);
            transform.rotation *= Quaternion.Euler(bx, by, 0);

            _lifeTimer = 0;

            _trailRenderer.Clear();
            gameObject.SetActive(true);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            transform.position += transform.forward * (Speed * Time.fixedDeltaTime);

            _lifeTimer += Time.fixedDeltaTime;
            if (_lifeTimer >= LifeTime)
            {
                gameObject.SetActive(false);
            }
        }

        protected override void OnHit(Collider hitCollider)
        {
            _damageApplier.TryApplyDamage(hitCollider);
        }
    }
}