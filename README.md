# 3DRobotGame

Unity製の3Dメカ対戦ゲーム。VContainer(DI) + R3(Rx)ベースのアーキテクチャを採用している。
教材として、コード構成・設計方針を学べるように作られている。

## ゲーム概要

プレイヤー機体を操作し、敵機・ボスと戦う3Dシューティング。

- **勝利条件**: プレイヤー陣営以外の対象ユニットをすべて倒す
- **敗北条件**: 制限時間切れ、またはプレイヤーの体力が0になる

## 動作環境

- Unity **6000.6.0f1**
- Universal Render Pipeline (URP)
- 依存パッケージ(抜粋、詳細は `Packages/manifest.json`):
  - `com.takahashi.myutils`(ローカルパッケージ。DI基盤・共通ユーティリティ)
  - `jp.hadashikick.vcontainer`(DIコンテナ)
  - `org.nuget.r3` / `com.cysharp.r3`(リアクティブプログラミング)
  - `com.cysharp.unitask`(async/await)
  - `com.unity.cinemachine`(カメラ制御)
  - `com.unity.inputsystem`(新Input System)

`com.takahashi.myutils` はローカルパスで参照しているため、`MyUtils` リポジトリを
このプロジェクトと同階層(または`Packages/manifest.json`のパスに合わせた場所)にcloneしておく必要がある。

## セットアップ

1. `MyUtils` リポジトリを別途clone
   ```bash
   git clone https://github.com/takahashi1682/MyUtils.git
   ```
2. `Packages/manifest.json` の `com.takahashi.myutils` のパスが、cloneした場所と一致しているか確認
3. Unity Hubから Unity 6000.6.0f1 でこのプロジェクトを開く
4. `Assets/_Projects/Scenes/Game.unity` を開いてPlay

## 操作方法

| アクション | キーボード/マウス | ゲームパッド |
|---|---|---|
| 移動 | WASD / 矢印キー | 左スティック |
| 視点操作 | マウス移動 | 右スティック |
| 発射 | 左クリック | 右トリガー |
| 飛行 | Space | Aボタン(南) |
| ブースト | Shift | Xボタン(西) |
| ロックオン | E | 右スティック押し込み |

## ドキュメント

| ドキュメント | 内容 |
|---|---|
| [ARCHITECTURE.md](ARCHITECTURE.md) | DIアーキテクチャ・スコープツリー・各サブシステムの設計解説 |
| [FLOW_DIAGRAMS.md](FLOW_DIAGRAMS.md) | ゲーム起動〜Unit初期化までの時系列フロー図 |
| [NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md) | クラス・フィールド・メソッドの命名規則 |

DIの仕組み自体(`AbstractScopeRoot<T>`等)は本プロジェクト固有ではなく `MyUtils` 側の
汎用機能のため、使い方の解説は [MyUtils/DI_GUIDE.md](https://github.com/takahashi1682/MyUtils/blob/main/DI_GUIDE.md) を参照。

## プロジェクト構成

```
Assets/_Projects/
├── Features/   ゲームロジック本体(Root / Game / Unit / Input)
├── Scenes/     ゲームシーン
└── Settings/   URP等の設定
```

`Features` 配下の詳細は [ARCHITECTURE.md](ARCHITECTURE.md) を参照。

## ライセンス

個人・教材用プロジェクト。外部公開用のライセンス表記は特に設けていない。
