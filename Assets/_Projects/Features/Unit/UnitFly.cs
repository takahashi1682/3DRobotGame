using System.Threading;
using Cysharp.Threading.Tasks;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit
{
    public interface IFlyActionHandler : IUnitActionHandler<bool>
    {
    }

    public interface IFlyActionObservable : IUnitActionObservable
    {
    }

    public class UnitFly : AbstractUnitAction,
        IScopeRegisterable,
        IScopeLaunchable,
        IFlyActionHandler,
        IFlyActionObservable
    {
        [Header("Settings")]
        public float FlyForce = 10f;
        public int FlyingEnergy = 1;

        [Inject] private Rigidbody _rigidbody;
        [Inject] private Energy _energy;

        private void FixedUpdate()
        {
            if (!IsAction.CurrentValue) return;

            if (_energy.CurrentValue <= 0)
            {
                CancelAction();
                return;
            }

            ApplyFly();
            _energy.Sub(FlyingEnergy);
        }

        public UniTask OnValueChanged(bool value, CancellationToken ct)
        {
            _isAction.Value = value;
            return UniTask.CompletedTask;
        }

        public void CancelAction() => _isAction.Value = false;

        public void OnLaunch() => IsAction.AddTo(this);

        public void OnRegister(IContainerBuilder builder)
            => builder.RegisterComponent(this).As<IFlyActionHandler, IFlyActionObservable>();

        /// <summary>
        ///     飛行中の処理。
        /// </summary>
        private void ApplyFly() => _rigidbody.linearVelocity += Vector3.up * (FlyForce * Time.fixedDeltaTime);
    }
}