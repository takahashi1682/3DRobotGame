using _Projects.Features.Game;
using MyUtils.Parameter.Basic;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit
{
    public class UnitScopeRoot : AbstractScopeRoot<IUnitScopeInitializable>
        , IGameScopeInitializable
    {
        [SerializeField] private SerializableReactiveProperty<bool> _running = new();
        public ReadOnlyReactiveProperty<bool> Running => _running;

        /// <summary>
        /// このユニットの設定値(Pivot、Army、TrackingSpeedなど)。
        /// 他ユニットのスコープから直接Resolveする代わりに、ここを経由して参照する。
        /// </summary>
        public UnitSetting Setting { get; private set; }

        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private GroundDetection _groundDetection;
        [SerializeField] private Health _health;
        [SerializeField] private Energy _energy;

        /// <summary>すでに自身が直接参照を持っているため、Resolveを介さずそのまま公開する。</summary>
        public Health Health => _health;

        /// <summary>すでに自身が直接参照を持っているため、Resolveを介さずそのまま公開する。</summary>
        public Energy Energy => _energy;

        public override void OnResolve(IObjectResolver resolver)
        {
            base.OnResolve(resolver);
            Setting = resolver.Resolve<UnitSetting>();
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