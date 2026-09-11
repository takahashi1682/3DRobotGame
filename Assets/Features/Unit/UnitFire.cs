using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Features.Unit.Battle;
using MyUtils;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Unit
{
    public interface IFireActionHandler : IUnitActionHandler<bool>
    {
    }

    public interface IFireActionObservable : IUnitActionObservable
    {
    }

    public class UnitFire : MonoBehaviour,
        IUnitScopeInitializable,
        IFireActionHandler,
        IFireActionObservable
    {
        [Header("References")]
        public BulletController BulletPrefab; // 発射する弾のプレハブ
        public ParticleSystem FireEffect;

        [Header("Settings")]
        public float FireRate = 0.15f;

        private float _fireTime;
        private IObjectResolver _resolver;
        private UnitSetting _setting;

        [SerializeField, ReadOnly] private SerializableReactiveProperty<bool> _isAction = new();
        public SerializableReactiveProperty<bool> IsAction => _isAction;

        private readonly List<BulletController> _bulletInstances = new();

        private void Awake()
        {
            SetEffectsActive(false);
        }

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<IFireActionHandler, IFireActionObservable>();
        }

        public virtual void OnResolve(IObjectResolver resolver)
        {
            IsAction.AddTo(this);
            _resolver = resolver;
            _setting = resolver.Resolve<UnitSetting>();
        }

        public UniTask OnValueChanged(bool value, CancellationToken ct)
        {
            IsAction.Value = value;
            SetEffectsActive(value);
            return UniTask.CompletedTask;
        }

        public void CancelAction()
        {
            IsAction.Value = false;
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
            if (Time.time - _fireTime >= FireRate)
            {
                _fireTime = Time.time;
                Fire();
            }
        }

        /// <summary>
        /// 全FirePointから、カメラの正面方向を狙って弾を発射する。
        /// </summary>
        protected void Fire()
        {
            BulletController bullet = null;

            // 使い終わった弾を再利用するため、非アクティブな弾を探す
            foreach (var b in _bulletInstances)
            {
                if (!b.isActiveAndEnabled)
                {
                    bullet = b;
                    bullet.ResetBullet(_setting.FirePoint.position, _setting.FirePoint.rotation);
                    break;
                }
            }

            // 非アクティブな弾が見つからなかった場合は、新しい弾を生成する
            if (bullet == null)
            {
                bullet = Instantiate(BulletPrefab);
                bullet.Initialize(_resolver, _setting.FirePoint.position, _setting.FirePoint.rotation);
                _bulletInstances.Add(bullet);
            }
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