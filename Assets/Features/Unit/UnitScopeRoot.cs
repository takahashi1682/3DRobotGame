using Features.Game;
using MyUtils.Parameter.Basic;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Features.Unit
{
    public class UnitScopeRoot : AbstractScopeRoot<IUnitScopeInitializable>
        , IGameScopeInitializable
    {
        [SerializeField] private SerializableReactiveProperty<bool> _running = new();
        public ReadOnlyReactiveProperty<bool> Running => _running;

        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private GroundDetection _groundDetection;
        [SerializeField] private Health _health;
        [SerializeField] private Energy _energy;

        public override void OnResolve(IObjectResolver resolver)
        {
            base.OnResolve(resolver);
            var unitManager = resolver.Resolve<UnitManager>();
            var health = resolver.Resolve<Health>();
            var gameJudge = resolver.Resolve<GameJudge>();

            // 体力が0でなく、かつゲーム進行中のみアクション可能
            health.IsEmpty.CombineLatest(gameJudge.State,
                    (healthEmpty, gameState) => !healthEmpty && gameState == EGameState.Playing)
                .Subscribe(x => _running.Value = x)
                .AddTo(this);

            // 体力が0になったらUnitManagerから削除する
            health.IsEmpty.Subscribe(x =>
            {
                if (x) unitManager.RemoveUnit(this);
                else unitManager.RegisterUnit(this);
            }).AddTo(this);
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