using Features.Game;
using MyUtils.Parameter.Basic;
using MyUtils.VContainerExtensions;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Unit
{
    public class UnitScopeRoot : AbstractScopeRoot<IUnitScopeInitializable>
        , IGameScopeInitializable
    {
        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private GroundDetection _groundDetection;
        [SerializeField] private Health _health;
        [SerializeField] private Energy _energy;

        private UnitManager _unitManager;

        public override void OnResolve(IObjectResolver resolver)
        {
            base.OnResolve(resolver);
            _unitManager = resolver.Resolve<UnitManager>();
        }

        private void OnEnable()
        {
            _unitManager.RegisterUnit(this);
        }

        private void OnDisable()
        {
            _unitManager.RemoveUnit(this);
        }

        /// <summary>
        /// 自身が構築するPlayerスコープ(子コンテナ)への登録。
        /// Player配下のIUnitScopeInitializableから解決される共有依存をここで登録する。
        /// </summary>
        protected override void ConfigureScope(IContainerBuilder builder)
        {
            base.ConfigureScope(builder);
            builder.RegisterComponent(this);
            builder.RegisterComponent(_rigidbody);
            builder.RegisterComponent(_groundDetection);
            builder.RegisterComponent(_health);
            builder.RegisterComponent(_energy);
        }
    }
}