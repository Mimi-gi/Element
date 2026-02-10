# Element 仕様書

> 最終更新: 2026-02-10

## 1. ゲーム概要

**Element**は、様々なオブジェクトに乗り移って進む**2Dパズルプラットフォーマー**。

### コンセプト
- プレイヤーの本体は「**目（Dark）**」として表現される
- フォーカスモードで乗り移り可能なオブジェクトを確認し、最適なものに乗り移る
- 各オブジェクトは固有の能力を持ち（移動、ジャンプ、浮遊など）、それらを駆使してパズルを解く
- **メトロイドヴァニア**形式の探索型マップ
- マリオオデッセイの「キャプチャ」に近い乗り移りメカニクス

---

## 2. コアメカニクス

### 2.1 乗り移りシステム

#### フォーカスモード
- **起動**: ボタン長押し
- **効果**: 
  - ゲーム時間が減速（デフォルト0.3倍）
  - 乗り移り可能なオブジェクトが視覚的にハイライト表示
  - 移動などの通常入力は無効化
- **解除**: ボタンを離す

#### 乗り移り判定

フォーカスモード中、オブジェクトは3段階で表示される：

1. **フォーカス外** - 何も表示されない（乗り移り不可）
2. **候補** - 乗り移り可能だが最適ではない（スプライトで提示）
3. **最適ターゲット** - 現在乗り移り可能（強調表示/エフェクト）

**最適ターゲットの選定条件**：
- オブジェクトの**コア**がカメラから見えている
- **AND** 現在Possess中のコアに最も近い

#### 乗り移り実行
- フォーカスモード中に乗り移り入力
- 最適ターゲットに乗り移る
- 古いオブジェクトの状態を解除、新しいオブジェクトにPossess

### 2.2 Dark（プレイヤー本体）

- プレイヤーの本体である「目」
- デフォルトのIPossable、`IHorizontalMove`と`IGrounded`を実装
- 他のオブジェクトに乗り移ると、古いDarkは自動で破棄される
- **接地判定**: 両端（Inspector設定の相対位置）からRaycastAllで下方にレイを飛ばし、`IGround`を検出
- **落下**: 非接地時は`_fallSpeed`で一定速度落下、接地時はY速度0

#### リスポーン
- 任意のIPossableが死亡すると、**DarkSource**の位置に新しいDarkが生成される
- DarkSourceはステージごとに配置可能（複数存在可能）
- StageManagerが現在アクティブなDarkSourceを管理

### 2.3 接地システム

#### IGround（マーカーインターフェース）
接地判定の対象。コライダーを持つオブジェクトにアタッチするだけで、その上に乗れるようになる。

**現在の実装クラス**: `Tile`, `Box`

#### IGrounded
接地が必要なオブジェクトが実装。
```csharp
public interface IGrounded
{
    bool IsGrounded { get; }
    void ConfigureYVelocity(Rigidbody2D rb);
}
```

**接地判定の仕組み**（Darkの場合）:
- `Observable.EveryUpdate`で毎フレーム`CheckGround()`を実行
- オブジェクトの両端（`_leftRayOffset`, `_rightRayOffset`）から`_rayLength`だけ下方にRaycastAll
- ヒットしたすべてのコライダーで`IGround`をチェック
- Gizmosでレイを可視化（接地=緑、非接地=赤）

---

## 3. 技術設計

### 3.1 アーキテクチャ概要

```
GameLifetimeScope (唯一のMonoBehaviour、エントリーポイント)
├── Events (POCO Singleton)
│   ├── GameStateEvents
│   ├── PossessionEvents
│   ├── FocusEvents
│   ├── CameraEvents
│   └── StageEvents
├── Managers (POCO Singleton)
│   ├── StageManager
│   ├── GameStateManager
│   ├── TimeManager
│   ├── CameraController
│   ├── InputProcessor
│   └── PossessionManager
└── GameBootstrap (POCO、初期化実行)
```

**設計方針**:
- マネージャーはすべて**POCO**（`IDisposable`実装）
- MonoBehaviourは**IPossable実装クラスのみ**
- VContainerによる依存性注入、R3によるイベント駆動
- `Instantiate`で生成されたオブジェクトには`IObjectResolver.Inject()`で注入

### 3.2 インターフェース設計

#### IPossable（基本インターフェース）
```csharp
public interface IPossable
{
    void TryPossess();
    int Layer { get; }
    bool IsPossess { get; set; }
    Transform Core { get; }
    bool UseGravity { get; }
    bool IsKinematicWhenNotPossessed { get; }
    void Death();
}
```

#### 能力インターフェース
```csharp
public interface IHorizontalMove { void XMove(float direction); }
public interface IVerticalMove   { void YMove(float direction); }
```

### 3.3 イベントインターフェース

| インターフェース | 内容 | 発行者 | 購読者 |
|---|---|---|---|
| `IGameStateEvents` | `ReactiveProperty<GameState>` | GameStateManager | InputProcessor, PossessionManager |
| `IPossessionEvents` | `CurrentPossessed`, `OnPossessionChanged`, `OnPossessableDeath` | PossessionManager | CameraController |
| `IFocusEvents` | `OnFocusModeChanged` | InputProcessor | TimeManager, PossessionManager |
| `ICameraEvents` | `OnCameraTransition` | CameraController | TimeManager |
| `IStageEvents` | `ActiveDarkSource`, `OnActiveAreaChanged` | StageManager | PossessionManager, CameraController |

### 3.4 マネージャー一覧

| マネージャー | 責務 | 入力方式 |
|---|---|---|
| **InputProcessor** | 入力をR3ストリームで配信 | `Observable.EveryUpdate`でポーリング |
| **GameStateManager** | ゲーム状態管理 | メソッド呼び出し |
| **PossessionManager** | 乗り移り管理＋リスポーン | イベント購読 |
| **CameraController** | カメラ移動（LitMotion） | イベント購読 |
| **TimeManager** | `Time.timeScale`制御 | イベント購読 |
| **StageManager** | DarkSource管理 | メソッド呼び出し |

### 3.5 InputProcessor（EveryUpdateポーリング）

```csharp
Observable.EveryUpdate().Subscribe(_ =>
{
    _move.Value = _moveAction.ReadValue<Vector2>();           // 連続値
    if (_possessAction.WasPressedThisFrame()) ...             // トリガー
    if (_jumpAction.WasPressedThisFrame()) ...                // トリガー
    // Focus: IsPressed() + エッジ検出でホールド判定
});
```

**出力ストリーム**:
- `Move` → `ReadOnlyReactiveProperty<Vector2>`
- `PossessInput` → `Observable<Unit>`
- `Jump` → `Observable<Unit>`

### 3.6 VContainer登録（GameLifetimeScope）

**Inspector設定項目**:
| 項目 | 型 | 説明 |
|---|---|---|
| Main Camera | `Camera` | シーンのメインカメラ |
| Input Asset | `InputActionAsset` | Player.inputactions |
| Dark Prefab | `GameObject` | Dark.prefab |
| Dark Sources | `DarkSource[]` | シーン内のDarkSource |

**Instantiated オブジェクトへのDI**:
```csharp
GameObject darkObj = Object.Instantiate(_darkPrefab, spawnPos, Quaternion.identity);
Dark dark = darkObj.GetComponent<Dark>();
_resolver.Inject(dark);  // [Inject] Construct() が呼ばれる
```

---

## 4. ゲームフロー

### 初期化フロー
```
GameLifetimeScope.Awake()
  → VContainer Build（Configure()で全POCO登録）
  → GameBootstrap.InitializeGameAsync()
    → StageManager.RegisterDarkSources()
    → Instantiate(Dark) + Inject()
    → PossessionManager.PossessTo(dark)
    → CameraController.SetPosition()
    → GameStateManager.ChangeState(Playing)
```

### フォーカス → 乗り移り
```
フォーカスボタン長押し
  → InputProcessor → FocusEvents
  → TimeManager: 時間減速（0.3倍）
  → PossessionManager: 最適ターゲット選定
  → 乗り移り入力
  → PossessTo(target) → カメラ追従
```

### 死亡 → リスポーン
```
IPossable.Death()
  → PossessionManager.RespawnAsDark()
  → Instantiate(Dark) + Inject()
  → PossessTo(newDark)
```

---

## 5. オブジェクト実装

| オブジェクト | インターフェース | 説明 |
|---|---|---|
| **Dark** | `IPossable, IHorizontalMove, IGrounded` | プレイヤー本体。横移動＋接地判定。Layer=0 |
| **Box** | `IPossable, IGround` | 踏める箱。乗り移り可能。Layer=1 |
| **Tile** | `IGround` | 地形タイル。乗り移り不可 |

### 将来追加予定
| オブジェクト | 能力 |
|---|---|
| **Spring** | `IHorizontalMove, IJumpable` |
| **Elevator** | `IVerticalMove` |
| **Balloon** | `IHorizontalMove, IVerticalMove` |
| **Rock** | `IPossable`のみ（移動不可） |

---

## 6. 技術スタック

- **Unity 6000.3+** (URP)
- **VContainer** - 依存性注入（DIコンテナ）
- **R3** - リアクティブプログラミング（ReactiveProperty/Subject/Observable.EveryUpdate）
- **LitMotion** - カメラ移動のトゥイーン
- **Input System** - 入力管理（EveryUpdateでポーリング）
- **UniTask** - 非同期処理

---

## 7. 今後の拡張可能性

### 能力の追加
- `IJumpable` - ジャンプ
- `IDashable` - ダッシュ
- `ISwimmable` - 水中移動
- `IGlidable` - 滑空

### IDoublable（分身システム）
```csharp
public interface IDoublable : IPossable
{
    bool IsSplit { get; }
    void Split();
    void Merge();
    IPossable GetOtherHalf();
}
```

### ステージギミック
- ボタン/スイッチ
- 動く床
- ワープゾーン
- 時間制限エリア