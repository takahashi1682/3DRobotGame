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
        IScopeResolvable,
        IScopeStartable
    {
        private GroundDetection _groundDetection;

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this);
        }

        public void OnResolve(IObjectResolver resolver)
        {
            _groundDetection = resolver.Resolve<GroundDetection>();
        }

        public void OnStart()
        {
            _groundDetection.IsHit
                .Subscribe(x => IsEnable = x)
                .AddTo(this);
        }
    }
}