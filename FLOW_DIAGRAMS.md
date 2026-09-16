# 3DRobotGame 起動フロー図

クラスの静的な構造は [CLASS_DIAGRAM.md](CLASS_DIAGRAM.md)、設計思想の解説は
[ARCHITECTURE.md](ARCHITECTURE.md) を参照。このファイルは**時間の流れ**、
つまり「実際に何が何の順番で呼ばれるか」だけに絞った2枚の図。

## 1. ゲームの起動の流れ

シーンを再生してから、全コンポーネントの `OnLaunch()` が呼び終わるまでの流れ。

```mermaid
sequenceDiagram
    autonumber
    participant Unity
    participant Root as RootLifetimeScope<br/>(親コンテナ, Configureは空)
    participant Scene as SceneLifetimeScope<br/>(MyUtils)
    participant Game as GameScopeRoot
    participant Units as UnitScopeRoot ×N

    Unity->>Root: Awake (親のVContainerコンテナを構築)
    Unity->>Scene: Awake (autoRun: 1)
    Scene->>Scene: Configure(builder)
    Scene->>Game: OnRegister(builder)
    Note right of Game: GameScopeRoot自身はOnRegister未実装<br/>(既定の空実装のまま)
    Scene->>Scene: builder.RegisterBuildCallback(...)
    Note over Scene: このコールバックはコンテナの<br/>ビルド完了後に実行される
    Scene->>Game: Build(resolver)
    Game->>Game: ResolveChildren(resolver, collector)
    Game->>Game: GetComponentsInChildren(IGameScopeMember)<br/>→ GameJudge, GameViewer, UpdateScheduler,<br/>CinemachineManualUpdater, UnitScopeRoot×N
    Game->>Game: ConfigureScope: Camera / UnitManager を登録
    Game->>Game: IScopeRegisterableなメンバーのOnRegisterを呼ぶ<br/>(GameJudge など)
    Game->>Game: 見つかった全メンバーへInject(target)
    Game->>Units: (IScopeRootでもあるため) ResolveChildren(gameContainer, collector)
    Note over Units: ここで機体ごとの初期化が走る<br/>→ 詳細は下の「2. Unitの初期化の流れ」
    Units-->>Game: 自身のメンバーをcollectorへ追加して返る
    Game->>Game: collector全員のOnLaunch()を呼ぶ(トップレベルのBuildの仕上げ)
    Game->>Game: GameJudge.OnLaunch / GameViewer.OnLaunch / ...
    Game->>Units: 各Unitメンバーの OnLaunch() も同じタイミングで呼ばれる
    Note over Unity,Units: ここからゲームが実際に動き出す<br/>(Update / FixedUpdate / UpdateSchedulerのフェーズが回り始める)
```

**ポイント**: 「登録+Inject解決」(`ResolveChildren`)と「OnLaunch実行」は完全に2段階に
分かれている。ツリー全体の依存解決が終わるまで、誰の`OnLaunch()`も呼ばれない。
そのため、ある`OnLaunch()`の中で「まだ`[Inject]`されていない依存」を参照してしまう
`NullReferenceException`が起きない。

## 2. Unitの初期化の流れ

上の図の「Units: ResolveChildren」1回分(機体1体分)を拡大したもの。

```mermaid
sequenceDiagram
    autonumber
    participant Unity
    participant Scope as UnitScopeRoot
    participant Members as Unit配下のコンポーネント群<br/>(Scriptsオブジェクト内)
    participant Health
    participant Energy

    Unity->>Scope: Awake()
    Scope->>Health: SetMax(Setting.MaxHealth) / SetFull()
    Scope->>Energy: SetMax(Setting.MaxEnergy) / SetFull()
    Note over Scope: Setting/Health/EnergyはSerializeFieldなので<br/>この時点でInjectなしに使える

    Note over Scope,Members: --- GameScopeRootからResolveChildrenが呼ばれる ---
    Scope->>Members: GetComponentsInChildren(IUnitScopeMember)<br/>で機体配下の全コンポーネントを収集
    Scope->>Scope: ConfigureScope: 自身/Setting/Rigidbody/<br/>GroundDetection/Health/Energyを登録
    Scope->>Members: IScopeRegisterableなメンバーのOnRegisterを呼ぶ<br/>(UnitFire, UnitMove, UnitTracking, DamageHandler...)
    Note right of Members: 例: UnitFireがIFireActionHandlerとして<br/>自分を登録→他コンポーネントから解決可能になる
    Scope->>Members: 見つかった全メンバーへInject(target)<br/>([Inject]フィールドがここで埋まる)
    Members-->>Scope: ネストしたIScopeRootではないので<br/>そのままcollectorに追加される

    Note over Unity,Members: --- 全ツリーのResolveChildrenが終わった後 ---
    Scope->>Scope: OnLaunch()<br/>Health.IsEmptyを購読し、Running/UnitManagerへの登録を管理
    Scope->>Members: 各コンポーネントのOnLaunch()<br/>(UnitActionControllerがハンドラーを配線、<br/>UnitMove/UnitFire等がFixedUpdate/フェーズ購読を開始 等)
    Note over Scope,Members: この時点でこの機体は完全に動作可能になる
```

**ポイント**: `UnitScopeRoot`は「`GameScopeRoot`から見つかる`IGameScopeMember`」であると
同時に「`IUnitScopeMember`を探索する別のスコープルート」でもある(`AbstractScopeRoot<T>`の
入れ子)。そのため、機体固有の依存(`Rigidbody`/`UnitSetting`/`Health`/`Energy`など)は
機体ごとの子コンテナにしか登録されず、他の機体のコンポーネントから誤って解決されることはない。

---

PDF化する場合は、このファイルを VS Code の Markdown Preview Enhanced で開き、
「Chrome (Puppeteer) : Export PDF」を実行してください(横に長い図なので、必要であれば
[CLASS_DIAGRAM.md](CLASS_DIAGRAM.md)で紹介したA3横用紙のFront Matterも活用してください)。
