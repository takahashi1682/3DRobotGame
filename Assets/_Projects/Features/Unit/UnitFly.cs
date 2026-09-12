using System.Threading;
using Cysharp.Threading.Tasks;
using MyUtils;
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

    public class UnitFly : MonoBehaviour,
        IUnitScopeInitializable,
        IFlyActionHandler,
        IFlyActionObservable
    {
        [Header("Settings")]
        public float FlyForce = 10f;
        public int FlyingEnergy = 1;

        private Rigidbody _rigidbody;
        private Energy _energy;

        [SerializeField, ReadOnly] private SerializableReactiveProperty<bool> _isAction = new();
        public ReadOnlyReactiveProperty<bool> IsAction => _isAction;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<IFlyActionHandler, IFlyActionObservable>();
        }

        public void OnResolve(IObjectResolver resolver)
        {
            IsAction.AddTo(this);

            _rigidbody = resolver.Resolve<Rigidbody>();
            _energy = resolver.Resolve<Energy>();
        }

        public UniTask OnValueChanged(bool value, CancellationToken ct)
        {
            _isAction.Value = value;
            return UniTask.CompletedTask;
        }

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

        /// <summary>
        /// 飛行中の処理。
        /// </summary>
        private void ApplyFly()
        {
            _rigidbody.linearVelocity += Vector3.up * (FlyForce * Time.fixedDeltaTime);
        }

        public void CancelAction()
        {
            _isAction.Value = false;
        }
    }
}