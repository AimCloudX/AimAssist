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

### 主な問題点
- MainWindow.xaml.csが500行以上で責任過多
- DI設定がApp.xaml.csに集中（ConfigureServicesが巨大）
- UnitsFactoryが複雑で新機能追加が困難
- 複数のOptionServiceが独立して存在
- エラーハンドリングとログ出力が散在

## リファクタリング計画

### 1. プレゼンテーション層の簡素化
**目標**: MainWindowのコードビハインドを薄くし、MVVM強化
- MainWindowViewModel.cs作成
- Behaviors分離（KeyboardBehavior、AnimationBehavior）
- UI Controls分割（UnitListControl、FilterControl等）

### 2. アプリケーション層の充実  
**目標**: ビジネスロジックをApplication層に集約
- ApplicationService.cs（アプリ全体制御）
- UnitManagementService.cs（ユニット管理統合）
- ConfigurationService.cs（設定管理統合）
- NavigationService.cs（画面遷移管理）

### 3. DI設定の整理
**目標**: ServiceRegistrationの分離とモジュール化
- ServiceRegistration.cs作成
- Modules分割（CoreModule、UIModule等）
- 拡張メソッドによる設定簡素化

### 4. ユニットシステム改善
**目標**: UnitsFactoryの分割と拡張性向上
- 個別ファクトリ作成（WorkToolsUnitFactory等）
- CompositeUnitsFactory実装
- UnitProvider抽象化

### 5. 設定管理統合
**目標**: 複数OptionServiceの統合
- ConfigurationManager.cs作成
- OptionProviders分離
- 設定検証機能追加

### 6. エラーハンドリング統一
**目標**: 例外処理とログの一貫性確保
- ErrorHandlingMiddleware.cs作成
- 統一例外ハンドラ実装

## 実装順序
1. DI設定分離とモジュール化
2. アプリケーション層サービス作成
3. ViewModel分離とMVVM強化
4. ユニットファクトリ分割
5. 設定管理統合
6. エラーハンドリング統一

## TODO

リファクタリング実施状況

### 完了したタスク

#### 1. DI設定分離とモジュール化 ✅
- ServiceRegistration.cs作成
- モジュール別にサービス登録を分割
- App.xaml.csのConfigureServicesを簡素化

#### 2. アプリケーション層サービス作成 ✅
- ApplicationService.cs: アプリ全体制御
- UnitManagementService.cs: ユニット管理統合
- ConfigurationService.cs: 設定管理統合
- NavigationService.cs: 画面遷移管理

#### 3. ViewModel分離とMVVM強化 ✅
- MainWindowViewModel.cs作成
- プレゼンテーション層のロジック分離

#### 4. ユニットファクトリ分割 ✅
- WorkToolsUnitsFactory.cs: ワークツール用ユニット
- SnippetUnitsFactory.cs: スニペット用ユニット
- KnowledgeUnitsFactory.cs: ナレッジ用ユニット
- CheatSheetUnitsFactory.cs: チートシート用ユニット
- OptionUnitsFactory.cs: オプション用ユニット
- CoreUnitsFactory.cs: 基本機能用ユニット
- CompositeUnitsFactory.cs: 統合ファクトリ

#### 5. エラーハンドリング統一 ✅
- ErrorHandlingMiddleware.cs作成
- 統一例外ハンドラ実装
- App.xaml.csでグローバル例外処理統合

#### 6. MainWindow リファクタリング完了 ✅
- **MainWindow.xaml.cs**: 600行から130行程度に大幅簡素化
- **MainWindowViewModel.cs**: 完全なMVVMアーキテクチャ実装  
- **Behaviors分離**: キーボード操作とフィルタリング処理を独立したBehaviorクラスに分離
  - `KeyboardNavigationBehavior`: キーボードナビゲーション処理
  - `DelayedFilterBehavior`: 遅延フィルタリング処理
- **型安全性向上**: WPF固有の型参照を明確化してコンパイルエラーを解決
- **XAML現代化**: Microsoft.Xaml.Behaviors使用による宣言的プログラミング
- **DI統合**: MainWindowViewModelがサービス層と適切に統合

### エラー修正と安定性向上
- WPFとWinFormsの型競合問題を解決
- 存在しないプロパティ参照エラーを修正  
- Microsoft.Xaml.Behaviors.Wpfパッケージを正しく統合
- ServiceRegistrationの型参照エラーを解決
- MainWindow.xamlの宣言的バインディングを最適化
- RelayCommandの型不一致エラーを解決（専用RelayCommandクラス作成）
- WindowHandleServiceの存在しないIsClosingプロパティ参照を修正

### アーキテクチャ改善の成果
1. **責任の分離**: UIロジック、ビジネスロジック、プレゼンテーションロジックが明確に分離
2. **テスタビリティ向上**: ViewModelが独立してテスト可能
3. **保守性向上**: コードビハインドが最小限で変更影響が局所化
4. **再利用性向上**: BehaviorとUserControlが他のウィンドウでも再利用可能
5. **宣言的UI**: XAMLベースの宣言的プログラミングスタイル

### 今後の改善点
- PickerWindowの同様なリファクタリング
- 統合テストの実装
- パフォーマンス最適化
- ユーザビリティ向上
