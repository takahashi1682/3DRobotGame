using _Projects.Features.Unit;
using MyUtils.VContainerExtensions;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Game
{
    public class GameScopeRoot : AbstractScopeRoot
    {
        [SerializeField] private Camera _camera;

        protected override void ConfigureScope(IContainerBuilder builder)
        {
            base.ConfigureScope(builder);
            builder.RegisterComponent(_camera);
            builder.Register<UnitManager>(Lifetime.Singleton);
        }
    }
}