# 3DRobotGame 命名規則

`Assets/_Projects/Features` 以下の既存コードから抽出した命名パターン。
新しくクラスやフィールドを追加するときは、ここに揃える。

## クラス・インターフェース

| 種別 | 規則 | 例 |
|---|---|---|
| クラス | PascalCase。`Unit`(全ユニット共通)/`Player`(プレイヤー専用の派生)で対象を明示する | `UnitMove`, `PlayerFire : UnitFire`, `UnitTracking` / `PlayerTracking : UnitTracking` |
| インターフェース | `I` + PascalCase | `IUnitScopeMember`, `IDamageable`, `IUnitControllable` |
| 抽象基底クラス | `Abstract` + PascalCase | `AbstractUnitAction`, `AbstractScopeRoot<T>` |
| enum | `E` + PascalCase。メンバーもPascalCase | `EGameState`, `EUnitState`, `EUpdatePhase` |
| アクション系インターフェース対 | `I` + 動詞 + `ActionHandler`(入力の受け口) / `I` + 動詞 + `ActionObservable`(状態の公開)を対で1アクションにつき2つ作る | `IFireActionHandler` / `IFireActionObservable`, `ILookActionHandler` / `ILookActionObservable` |
| 名前空間 | `_Projects.Features.<カテゴリ>[.<サブカテゴリ>]`。フォルダ構成と一致させる | `_Projects.Features.Unit.Player`, `_Projects.Features.Unit.Battle` |

## フィールド

| 種別 | 規則 | 例 |
|---|---|---|
| privateフィールド | `_camelCase` | `_rigidbody`, `_fireTime`, `_targetPivot` |
| `[Inject]`フィールド | 型名から意味が分かる`_camelCase`。DI対象であることは属性で表現し、名前に接頭辞は付けない | `[Inject] private Rigidbody _rigidbody;` |
| `[SerializeField]`フィールド | 同じく`_camelCase`。Inspector公開でも命名は変えない | `[SerializeField] private RectTransform _dot;` |
| publicフィールド(Inspectorで直接調整する設定値) | PascalCase、アンダースコアなし | `public float FireRate`, `public float CamSpeedX` |
| 定数 | PascalCase(SCREAMING_SNAKE_CASEは使わない) | `private const float MinDirectionSqrMagnitude` |
| bool状態を持つReactivePropertyの裏フィールド | `_isXxx`のprivateフィールド + 同じ意味の`IsXxx`publicプロパティで公開する | `_isAction` → `IsAction` |

## プロパティ

| 種別 | 規則 | 例 |
|---|---|---|
| 通常のプロパティ | PascalCase | `MoveDirection`, `TargetPosition` |
| bool判定プロパティ | `IsXxx`(状態) / `CanXxx`(許可) / `HasXxx`(フラグの有無) | `IsAction`, `CanMove`, `HasFlag(...)` |
| シリアライズ付き自動プロパティ | `[field: SerializeField] public Type Name { get; private set; }` | `[field: SerializeField] public int MaxHealth { get; private set; }` |

## メソッド

| 種別 | 規則 | 例 |
|---|---|---|
| 通常のメソッド | PascalCase(public/private問わず) | `ApplyGravity()`, `GetPooledBullet()` |
| イベントハンドラ・コールバック | `On` + PascalCase | `OnLaunch`, `OnRegister`, `OnPhaseUpdate`, `OnValueChanged`, `OnMove`/`OnLook`/`OnFire`(Input System由来) |
| 成否を返す/条件付きで実行する処理 | `TryXxx` | `TryGetDirection`, `TryPress`, `TryResolve` |
| 値の取得 | `GetXxx` | `GetPooledBullet`, `GetForwardPosition`, `GetScaledLook` |
| 値の適用・実行 | `ApplyXxx` | `ApplyGravity`, `ApplyHorizontalLook`, `ApplyMove` |
| 中断・解除 | `Cancel` / `Clear` / `Stop` + 対象 | `CancelAction`, `ClearTarget`, `StopAllActions` |
| ローカル変数・引数 | camelCase | `moveDirection`, `yawInput`, `deltaTime` |

## その他の慣習

- ファイル名はクラス名と完全一致させる。`.meta`とセットで管理されるため、リネームする際は
  ファイルと`.meta`を同時に(`git mv`で)移動し、guidを維持すること。
- 1ユニットの「実行中かどうか」は`IsAction`という統一名で、全アクションクラス
  (`UnitMove`, `UnitFire`, `UnitBoost`など)が`AbstractUnitAction`経由で共通実装する。
- DI解決は基本`[Inject]`だが、「無くてもよい依存」の場合だけ`IObjectResolver`を`[Inject]`し、
  `TryResolve`で取り出す(`UnitActionController`, `UnitStatus`, `PlayerActionController`)。

設計方針・アーキテクチャ全般については [ARCHITECTURE.md](ARCHITECTURE.md) を参照。
