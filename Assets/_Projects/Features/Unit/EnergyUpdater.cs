using MyUtils.Parameter;
using R3;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit
{
    public class EnergyUpdater : IntParameterUpdater, IUnitScopeInitializable
    {
        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this);
        }

        public void OnResolve(IObjectResolver resolver)
        {
            var groundDetection = resolver.Resolve<GroundDetection>();

            groundDetection.IsHit
                .Subscribe(x => IsEnable = x)
                .AddTo(this);
        }
    }
}