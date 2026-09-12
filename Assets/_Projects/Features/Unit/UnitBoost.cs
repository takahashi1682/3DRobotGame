using System.Threading;
using Cysharp.Threading.Tasks;
using MyUtils;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit
{
    public interface IBoostActionHandler : IUnitActionHandler<bool>
    {
    }

    public interface IBoostActionObservable : IUnitActionObservable
    {
        float BoostPower { get; }
        int BoostEnergy { get; }
    }

    public class UnitBoost : MonoBehaviour,
        IUnitScopeInitializable,
        IBoostActionHandler,
        IBoostActionObservable
    {
        [Header("Settings")]
        public float BoostPower = 500f;
        public int BoostEnergy = 300;
        public int BoostDuration = 300;
        public int BoostInterval = 300;

        private Energy _energy;

        [SerializeField, ReadOnly] private SerializableReactiveProperty<bool> _isAction = new();
        public ReadOnlyReactiveProperty<bool> IsAction => _isAction;

        float IBoostActionObservable.BoostPower => BoostPower;
        int IBoostActionObservable.BoostEnergy => BoostEnergy;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<IBoostActionHandler, IBoostActionObservable>();
        }

        public void OnResolve(IObjectResolver resolver)
        {
            IsAction.AddTo(this);
            _energy = resolver.Resolve<Energy>();
        }

        public async UniTask OnValueChanged(bool value, CancellationToken ct)
        {
            if (IsAction.CurrentValue) return;
            _isAction.Value = true;

            _energy.Sub(BoostEnergy);

            await UniTask.Delay(BoostDuration, cancellationToken: ct);
            _isAction.Value = false;
            await UniTask.Delay(BoostInterval, cancellationToken: ct);
        }

        public void CancelAction()
        {
            _isAction.Value = false;
        }
    }
}