using Features.Unit;
using MyUtils.VContainerExtensions;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Game
{
    public interface IGameScopeInitializable : IScopeInitializable
    {
    }

    public class GameScopeRoot : AbstractScopeRoot<IGameScopeInitializable>
    {
        [SerializeField] private Camera _mainCamera;

        protected override void ConfigureScope(IContainerBuilder builder)
        {
            base.ConfigureScope(builder);
            builder.RegisterComponent(_mainCamera);
            builder.Register<UnitManager>(Lifetime.Singleton).WithParameter(gameObject);
        }
    }
}