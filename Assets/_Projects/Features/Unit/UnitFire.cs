using System.Collections.Generic;
using System.Threading;
using _Projects.Features.Unit.Battle;
using Cysharp.Threading.Tasks;
using MyUtils;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit
{
    public interface IFireActionHandler : IUnitActionHandler<bool>
    {
    }

    public interface IFireActionObservable : IUnitActionObservable
    {
    }

    /// <summary>
    /// 押下中(IsAction)の間、FireRate間隔でFirePointから弾を発射する。
    /// 弾はUnitFire自身がプールし、非アクティブな既存インスタンスがあれば再利用する。
    /// </summary>
    public class UnitFire : AbstractUnitAction,
        IUnitScopeMember,
        IScopeRegisterable,
        IScopeLaunchable,
        IFireActionHandler,
        IFireActionObservable
    {
        [Header("References")]
        public BulletController BulletPrefab; // 発射する弾のプレハブ
        public ParticleSystem FireEffect;

        [Header("Settings")]
        public float FireRate = 0.15f;

        /// <summary>派生クラス(PlayerFireなど)からもFirePoint等を参照できるようprotectedにしている。</summary>
        [Inject]
        protected UnitSetting Setting { get; private set; }

        private float _fireTime;
        [Inject] private IObjectResolver _resolver;
        private readonly List<BulletController> _bulletInstances = new();

        private void Awake()
        {
            SetEffectsActive(false);
        }

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<IFireActionHandler, IFireActionObservable>();
        }

        public virtual void OnLaunch()
        {
            IsAction.AddTo(this);
        }

        public UniTask OnValueChanged(bool value, CancellationToken ct)
        {
            _isAction.Value = value;
            SetEffectsActive(value);
            return UniTask.CompletedTask;
        }

        public void CancelAction()
        {
            _isAction.Value = false;
            SetEffectsActive(false);
        }

        private void SetEffectsActive(bool isActive)
        {
            if (isActive)
            {
                FireEffect.Play();
            }
            else
            {
                FireEffect.Stop();
            }
        }

        private void Update()
        {
            if (!IsAction.CurrentValue) return;

            // 前回の発射からFireRate以上の時間が経過している場合に発射する
            if (Time.time - _fireTime < FireRate) return;

            _fireTime = Time.time;
            Fire();
        }

        /// <summary>
        /// FirePointから弾を発射する。既存のプールに非アクティブな弾があれば再利用する。
        /// </summary>
        protected void Fire()
        {
            var bullet = GetPooledBullet();
            if (bullet != null)
            {
                bullet.ResetBullet(Setting.FirePoint.position, Setting.FirePoint.rotation);
                return;
            }

            bullet = Instantiate(BulletPrefab);
            bullet.Initialize(_resolver, Setting.FirePoint.position, Setting.FirePoint.rotation);
            _bulletInstances.Add(bullet);
        }

        /// <summary>非アクティブ(=使用済み)な弾があれば返す。無ければnull。</summary>
        private BulletController GetPooledBullet()
        {
            foreach (var bullet in _bulletInstances)
            {
                if (!bullet.isActiveAndEnabled) return bullet;
            }

            return null;
        }

        private void OnDestroy()
        {
            // 弾のインスタンスを破棄する
            foreach (var bullet in _bulletInstances)
            {
                if (bullet != null)
                {
                    Destroy(bullet.gameObject);
                }
            }
        }
    }
}
