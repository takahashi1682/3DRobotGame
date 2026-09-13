using MyUtils.VContainerExtensions;
using R3;
using R3.Triggers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit
{
    /// <summary>
    /// 重力の適用を担当する。PlayerFly(IFlyActionObservable)が飛行中の間は重力をリセットし、
    /// 飛行しておらず、かつ接地していない間だけ重力を加算する。
    /// </summary>
    public class UnitGravity : MonoBehaviour,
        IUnitScopeMember,
        IScopeRegisterable,
        IScopeResolvable,
        IScopeStartable
    {
        [Header("Settings")]
        public float Gravity = -2.25f;
        public Vector3 GravityDirection = Vector3.down;

        private float _currentGravity;

        private Rigidbody _rigidbody;
        private GroundDetection _groundDetection;
        private IFlyActionObservable _flyActionObservable;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this);
        }

        public void OnResolve(IObjectResolver resolver)
        {
            _rigidbody = resolver.Resolve<Rigidbody>();
            _groundDetection = resolver.Resolve<GroundDetection>();
            _flyActionObservable = resolver.Resolve<IFlyActionObservable>();
        }

        public void OnStart()
        {
            this.FixedUpdateAsObservable()
                .Subscribe(_ =>
                {
                    if (_flyActionObservable.IsAction.CurrentValue ||
                        _groundDetection.IsHit.CurrentValue)
                    {
                        // 飛行中または接地中は重力をリセットする
                        _currentGravity = 0f;
                    }
                    else
                    {
                        ApplyGravity();
                    }
                })
                .AddTo(this);
        }

        /// <summary>
        /// 重力の処理。重力方向に力を加える。
        /// </summary>
        private void ApplyGravity()
        {
            _currentGravity -= Gravity * Time.fixedDeltaTime;
            _rigidbody.linearVelocity += GravityDirection * _currentGravity;
        }
    }
}