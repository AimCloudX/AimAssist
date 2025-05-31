# AimAssist

## プロジェクト概要
WPFベースのコマンドランチャー・生産性向上ツール
- 複数のプロジェクトから構成される.NETアプリケーション
- DI、Factory、Command、MVVMパターンを採用
- ユニットベースの機能拡張システム

## コード全体像分析
### 現在のアーキテクチャ
1. **メインプロジェクト（AimAssist）**: WPFアプリケーション本体
2. **Core**: インターフェースとコアロジック  
3. **Units**: 機能単位の実装
4. **Services**: 各種サービス実装
5. **Infrastructure**: 外部依存との統合
6. **Common**: 共通UI・コンポーネント

## リファクタリング計画

### Phase 1: アーキテクチャの明確化と依存関係の整理

#### 1.1 レイヤード構造の確立
現在のプロジェクト構造は以下の6つの主要レイヤーで構成されている：
- **AimAssist**: WPFアプリケーション本体
- **AimAssist.Core**: インターフェースとコアロジック
- **AimAssist.Units**: 機能単位の実装
- **AimAssist.Services**: 各種サービス実装
- **AimAssist.Plugins**: プラグインシステム
- **Common**: 共通UI・コンポーネント

#### 1.2 依存関係の整理

**問題点:**
- Initializerクラスが多数の依存関係を持ちすぎている（11個のパラメータ）
- ServiceRegistrationが複雑になっている
- ファクトリーパターンの実装が分散している

**改善案:**
1. **Builder Pattern の導入**: Initializerの複雑な依存関係を整理
2. **Module分離**: ServiceRegistrationを機能別にモジュール化
3. **Factory統合**: CompositeUnitsFactoryを活用したファクトリー管理の統一

#### 1.3 命名規則の統一化
既存の命名不一致を解決：
- インターフェース：Iプレフィックス統一
- サービス：ServiceサフィックスまたはManagerサフィックスの統一
- メソッド：動詞開始の統一（Get, Create, Load, Save等）

### Phase 2: サービス層の再設計

#### 2.1 ApplicationServiceの責務分離
現在のApplicationServiceが初期化とシャットダウンの両方を担当している。以下に分離：
- **ApplicationLifecycleService**: アプリケーションのライフサイクル管理専用
- **ConfigurationService**: 設定管理専用
- **ModuleInitializationService**: モジュール初期化専用

#### 2.2 Factoryパターンの統合
現在複数存在するFactoryを統合：
- **AbstractUnitsFactory**: 基底ファクトリー
- **CompositeUnitsFactory**: 複合ファクトリー（現在の実装を拡張）
- **PluginUnitsFactory**: プラグイン用ファクトリー

#### 2.3 設定管理の統一
複数のOptionServiceを統合：
- **IConfigurationManager**: 全設定の統合管理
- **ConfigurationSection**: 設定セクション別の管理

### Phase 3: エラーハンドリングとログの強化

#### 3.1 統一エラーハンドリング
- **ErrorHandlingMiddleware**: 全体的なエラー処理
- **ErrorContext**: エラー情報の構造化
- **ErrorRecoveryStrategy**: エラー回復戦略の実装

#### 3.2 ログシステムの改善
- **StructuredLogging**: 構造化ログの導入
- **LoggingScope**: ログのスコープ管理
- **LoggingConfiguration**: ログ設定の統一

### Phase 4: UIとビジネスロジックの分離

#### 4.1 MVVM強化
- **ViewModelBase**: 共通ViewModelの基底クラス
- **CommandFactory**: コマンドの統一生成
- **EventAggregator**: イベント通信の統一

#### 4.2 UI責務の明確化
- **WindowManager**: ウィンドウ管理の統一
- **DialogService**: ダイアログ表示の統一
- **NavigationService**: 画面遷移の統一

### Phase 5: プラグインシステムの改善

#### 5.1 プラグインアーキテクチャの強化
- **PluginContract**: プラグイン契約の明確化
- **PluginLifecycle**: プラグインライフサイクル管理
- **PluginSecurity**: プラグインセキュリティ

#### 5.2 プラグイン発見メカニズム
- **PluginDiscovery**: プラグイン自動発見
- **PluginMetadata**: プラグインメタデータ管理

### 実装スケジュール

**Week 1-2: Phase 1（基盤整備）**
- 命名規則の統一
- ServiceRegistrationのモジュール化
- Initializerの責務分離

**Week 3-4: Phase 2（サービス層）**
- ApplicationServiceの分離
- Factoryパターンの統合
- 設定管理の統一

**Week 5-6: Phase 3（エラー処理・ログ）**
- エラーハンドリングの統一
- ログシステムの改善

**Week 7-8: Phase 4（UI改善）**
- MVVM強化
- UI責務の明確化

**Week 9-10: Phase 5（プラグイン）**
- プラグインシステムの改善
- 統合テスト・品質保証

### リスク管理

**高リスク要素:**
1. 広範囲な変更による予期しない影響
2. プラグインシステムの後方互換性
3. UI・UXへの影響

**軽減策:**
1. 段階的リファクタリング（フェーズ分割）
2. 十分なテスト実施
3. ロールバック計画の準備
4. プラグインAPIの段階的移行

## Phase 1 進捗状況

### 完了項目
1. **ServiceRegistrationのモジュール化**: ✅ 完了
   - IServiceModuleインターフェースの作成
   - 機能別モジュール分離（Core, Application, UI, Option, Factory, Plugin, Initialization）
   - モジュールベースの登録システム

2. **Initializerの責務分離**: ✅ 完了
   - IApplicationInitializationServiceの作成
   - FileInitializationServiceによるファイル初期化の分離
   - PluginInitializationServiceによるプラグイン初期化の分離
   - ApplicationInitializationServiceによる統合初期化

3. **依存関係の簡素化**: ✅ 完了
   - Initializerが単一のIApplicationInitializationServiceに依存
   - 11個のパラメータから1個に削減

### バグ修正
4. **計算機能の修正**: ✅ 完了
   - CalcUnitの新規実装
   - PickerWindowViewModelでのCalc機能の復旧
   - Calc→Snippet→Calcモード切り替え時の表示問題を修正
   - フィルタリング状態の適切な管理
   - SelectedUnitの同期問題の解決

### 技術的改善点
- **責務の明確化**: 各サービスが単一責任を持つように分離
- **モジュール性の向上**: DIコンテナの登録がモジュール単位で管理
- **テスタビリティの向上**: 各初期化サービスが独立してテスト可能
- **保守性の向上**: 新機能追加時にモジュールとして追加可能
- **UI状態管理の改善**: モード切り替え時の状態の一貫性を確保

### 修正内容詳細
- **CalcUnit**: 計算専用のUnitクラスを新規作成
- **HandleCalculationMode**: モード切り替え時のフィルタクリアとSelectedUnit同期
- **HandleSnippetMode**: フィルタ再適用とビューの強制更新
- **SelectedIndex**: インデックス変更時のSelectedUnit自動同期
- **NavigateUp/Down**: フィルタされたアイテムを考慮したナビゲーション

### 次のステップ
Phase 2のサービス層再設計に進む予定
