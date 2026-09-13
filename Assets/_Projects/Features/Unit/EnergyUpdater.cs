using MyUtils.Parameter;
using MyUtils.VContainerExtensions;
using R3;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit
{
    public class EnergyUpdater : IntParameterUpdater,
        IUnitScopeMember,
        IScopeRegisterable,
        IScopeLaunchable
    {
        [Inject] private GroundDetection _groundDetection;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this);
        }

        public void OnLaunch()
        {
            _groundDetection.IsHit
                .Subscribe(x => IsEnable = x)
                .AddTo(this);
        }
    }
}