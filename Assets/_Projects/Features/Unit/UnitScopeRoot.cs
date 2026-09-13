using System;
using _Projects.Features.Game;
using MyUtils.Parameter.Basic;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Unit
{
    public class UnitScopeRoot : AbstractScopeRoot<IUnitScopeMember>
        , IGameScopeMember
        , IScopeLaunchable
    {
        [SerializeField] private SerializableReactiveProperty<bool> _running = new();
        public ReadOnlyReactiveProperty<bool> Running => _running;

        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private GroundDetection _groundDetection;

        [field: SerializeField]
        public UnitSetting Setting { get; private set; }

        [field: SerializeField]
        public Health Health { get; private set; }

        [field: SerializeField]
        public Energy Energy { get; private set; }

        [Inject] private UnitManager _unitManager;
        [Inject] private GameJudge _gameJudge;

        private void Awake()
        {
            // 設定値を反映
            Health.SetMax(Setting.MaxHealth);
            Health.SetFull();
            Energy.SetMax(Setting.MaxEnergy);
            Energy.SetFull();
        }

        public void OnLaunch()
        {
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

        /// <summary>
        /// 自身が構築するPlayerスコープ(子コンテナ)への登録。
        /// Player配下のIUnitScopeMemberから解決される共有依存をここで登録する。
        /// </summary>
        protected override void ConfigureScope(IContainerBuilder builder)
        {
            base.ConfigureScope(builder);
            builder.RegisterComponent(this);
            builder.RegisterInstance(Setting);
            builder.RegisterComponent(_rigidbody);
            builder.RegisterComponent(_groundDetection);
            builder.RegisterComponent(Health);
            builder.RegisterComponent(Energy);
        }
    }
}