using System.Threading;
using Cysharp.Threading.Tasks;
using MyUtils.VContainerExtensions;
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

    public class UnitBoost : AbstractUnitAction,
        IUnitScopeMember,
        IScopeRegisterable,
        IScopeLaunchable,
        IBoostActionHandler,
        IBoostActionObservable
    {
        [Header("Settings")]
        public float BoostPower = 500f;
        public int BoostEnergy = 300;
        public int BoostDuration = 300;
        public int BoostInterval = 300;

        [Inject] private Energy _energy;

        float IBoostActionObservable.BoostPower => BoostPower;
        int IBoostActionObservable.BoostEnergy => BoostEnergy;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<IBoostActionHandler, IBoostActionObservable>();
        }

        public void OnLaunch()
        {
            IsAction.AddTo(this);
        }

        public async UniTask OnValueChanged(bool value, CancellationToken ct)
        {
            if (!value) return;
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