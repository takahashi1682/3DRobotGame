using System;
using _Projects.Features.Game;
using MyUtils.Parameter.Basic;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit
{
    public class UnitScopeRoot : AbstractScopeRoot<IUnitScopeMember>
        , IGameScopeMember
    {
        [SerializeField] private SerializableReactiveProperty<bool> _running = new();
        public ReadOnlyReactiveProperty<bool> Running => _running;

        public bool IsVisible { get; private set; }

        /// <summary>
        /// このユニットの設定値(Pivot、Army、RotationSpeedなど)。
        /// 他ユニットのスコープから直接Resolveする代わりに、ここを経由して参照する。
        /// </summary>
        public UnitSetting Setting { get; private set; }

        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private GroundDetection _groundDetection;
        [field: SerializeField]
        public Health Health { get; private set; }

        [field: SerializeField]
        public Energy Energy { get; private set; }

        private UnitManager _unitManager;
        private GameJudge _gameJudge;

        public override void OnResolve(IObjectResolver resolver)
        {
            base.OnResolve(resolver);
            Setting = resolver.Resolve<UnitSetting>();
            _unitManager = resolver.Resolve<UnitManager>();
            _gameJudge = resolver.Resolve<GameJudge>();
        }

        public override void OnStart()
        {
            base.OnStart();

            // 体力が0でなく、かつゲーム進行中のみアクション可能
            Health.IsEmpty.CombineLatest(_gameJudge.State,
                    (healthEmpty, gameState) => !healthEmpty && gameState == EGameState.Playing)
                .Subscribe(x => _running.Value = x)
                .AddTo(this);

            // 体力が0になったらUnitManagerから削除する
            Health.IsEmpty.Subscribe(x =>
            {
                if (x) _unitManager.RemoveUnit(this);
                else _unitManager.RegisterUnit(this);
            }).AddTo(this);
        }
        
        protected void OnBecameVisible() => IsVisible = true;
        protected void OnBecameInvisible() => IsVisible = false;
        
        /// <summary>
        /// 自身が構築するPlayerスコープ(子コンテナ)への登録。
        /// Player配下のIUnitScopeMemberから解決される共有依存をここで登録する。
        /// </summary>
        protected override void ConfigureScope(IContainerBuilder builder)
        {
            base.ConfigureScope(builder);
            builder.RegisterComponent(this);
            builder.RegisterComponent(_rigidbody);
            builder.RegisterComponent(_groundDetection);
            builder.RegisterComponent(Health);
            builder.RegisterComponent(Energy);
        }
    }
}