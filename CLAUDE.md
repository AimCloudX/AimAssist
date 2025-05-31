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

### 次のステップ
- MainWindow.xaml.csのコードビハインド簡素化
- UI Behaviorsの分離
- UnitsServiceとの統合テスト
