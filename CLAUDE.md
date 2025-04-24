# AimAssistリファクタリング計画

AimAssistのリファクタリング計画と進捗状況を記録するドキュメントです。

## リファクタリングの目的

- コードの可読性・保守性の向上
- アーキテクチャの改善と明確化
- 依存関係の適切な管理
- テスト容易性の向上
- パフォーマンスの最適化

## アーキテクチャの再構築

### クリーンアーキテクチャの導入

```
AimAssist/
├── Infrastructure/ (インフラストラクチャ層)
│   ├── Persistence/ (データ永続化)
│   ├── ExternalServices/ (外部サービス)
│   └── UI/ (ユーザーインターフェース)
├── Application/ (アプリケーション層)
│   ├── Services/ (サービス実装)
│   ├── Commands/ (コマンドハンドラー)
│   └── ViewModels/ (ビューモデル)
├── Domain/ (ドメイン層)
│   ├── Models/ (ドメインモデル)
│   ├── Services/ (ドメインサービス)
│   └── Interfaces/ (リポジトリやサービスのインターフェース)
└── Plugins/ (プラグイン拡張)
```

### 依存性の逆転の適用

- 上位モジュールを抽象化に依存させる
- 具象クラスへの直接依存を排除

## コード品質の改善

### 命名規則の統一

- 誤字の修正 (例: `UnitsService.Instnace` → `UnitsService.Instance`)
- ハードコードされた値の定数化
- 一貫した命名パターンの適用

### 例外処理の改善

- 適切なログ記録メカニズムの導入
- エラー回復処理の実装
- UIスレッドでの例外ハンドリングの強化

### コメントの改善と文書化

- XMLドキュメントコメントの追加
- 複雑なロジックへの説明コメント
- 公開APIドキュメントの充実

## パフォーマンスの最適化

### 非同期処理の改善

- `async/await`パターンの適切な使用
- `async void`の排除（テスト容易性向上）
- UIスレッドのブロッキング防止

### リソース管理の改善

- `IDisposable`の適切な実装
- `using`ステートメントの徹底
- メモリリークの防止

## テストの導入

### 単体テストの追加

- xUnit/NUnit/MSTestの導入
- 重要なビジネスロジックのテスト作成
- 境界条件のテスト

### モックフレームワークの導入

- Moq/NSubstituteの導入
- 依存関係のモック化
- テスト環境の構築

## プラグインシステムの改善

### プラグインローダーの強化

- エラー処理の改善
- プラグインのライフサイクル管理
- ロギングの強化

### プラグインのバージョン管理

- バージョン情報の追加
- 互換性チェックの実装
- プラグインメタデータの拡充

## ユーザーインターフェースの改善

### MVVMパターンの徹底

- ViewModel層とModel層の明確な分離
- コマンドパターンの一貫した使用
- データバインディングの適切な利用

### リソースとスタイルの整理

- アプリケーション全体のスタイルの統一
- リソースディクショナリの整理
- テーマ対応の改善

## 依存性注入コンテナの導入

- Microsoft.Extensions.DependencyInjection/Autofacの導入
- サービスの登録と解決
- コンストラクタインジェクションの実装

## 設定管理の改善

- 設定管理の一元化
- 型安全な設定クラスの導入
- 設定変更の通知メカニズム

## 国際化対応の改善

- リソースファイルの活用
- 文字列のハードコード排除
- 多言語対応の基盤整備

## リファクタリング実施計画

1. **準備フェーズ** 
   - [x] コードベースの詳細分析
   - [ ] テスト環境構築
   - [ ] 優先順位付け

2. **基盤的リファクタリング**
   - [x] 依存性注入コンテナ導入
   - [x] インターフェース抽出
   - [ ] ロギングフレームワーク導入

3. **構造的リファクタリング**
   - [ ] アーキテクチャ再構築
   - [ ] MVVMパターン適用
   - [x] 設定管理改善

4. **機能別リファクタリング**
   - [ ] プラグインシステム改善
   - [ ] UI改善
   - [ ] 国際化対応

5. **最適化**
   - [ ] パフォーマンス最適化
   - [ ] メモリ使用量最適化
   - [ ] エラー処理改善

6. **テストと品質保証**
   - [ ] 単体テスト追加
   - [ ] 統合テスト追加
   - [ ] 手動テスト実施

## 進捗状況

| タスク | 状態 | 担当者 | 完了日 |
|-------|------|-------|-------|
| コードベース分析 | 完了 | - | 2025-04-24 |
| 依存性注入導入 | 完了 | - | 2025-04-25 |
| インターフェース抽出 | 完了 | - | 2025-04-25 |
| 設定管理改善 | 完了 | - | 2025-04-25 |
| テスト環境構築 | 未着手 | - | - |
| PluginsServiceのDI対応 | 完了 | - | 2025-04-25 |
| ClipboardPluginのDI対応 | 完了 | - | 2025-04-25 |
| 命名の一貫性改善 | 進行中 | - | - |

## 現在の課題とノート

- PluginsServiceとClipboardPluginをDIパターンに完全対応させました
- IUnitsFacotry → IUnitsFactoryに修正し、名前の誤字を修正しました
- UnitToUIElementDicotionary → UnitToUIElementDictionaryに修正しました
- EditorCash → EditorCacheに修正しました
- BookmarkUnitsFacotry → BookmarkUnitsFactoryに修正しました
- UnitViewModel.csの参照を確認し、キャッシュの実装を改善する必要があります
- エラーハンドリングの改善を進めています
- アプリケーション全体の命名の一貫性を確保するために引き続き誤字を修正しています UnitsServiceの依存性注入を実装しました
- アプリケーションのDIコンテナを導入しました（Microsoft.Extensions.DependencyInjection）
- インターフェースの抽出とシングルトンパターンの改善を進めています
- 命名の修正（UnitsService.Instnace → UnitsService.Instance、WaitHowKeysWindow → WaitHotKeysWindow）
- CommandServiceを静的クラスからDIを使用したインスタンスクラスに変更しました
- CommandServiceのインターフェース（ICommandService）を作成しました
- ApplicationLogServiceをDIパターンに修正しました
- ApplicationLogServiceのインターフェース（IApplicationLogService）を作成しました
- App.xaml.csのDI設定を更新しました
- Initializerクラスをインジェクションされたサービスを使用するように修正しました
- WaitHotKeysWindowを修正してDIからCommandServiceを取得するようにしました
- サービスロケーターパターン（((App)App.Current)._serviceProvider.GetRequiredService）を除去しました
- CustomizeKeyboardShortcutsSettingsをDIパターンに対応させました
- UnitViewFactoryをDIパターンに対応させました
- MainWindowがDIからUnitViewFactoryを取得するよう修正しました
- PickerServiceを静的クラスからDIパターンに変更しました
- WindowHandleServiceを静的クラスからDIパターンに変更しました
- AppCommandsクラスを静的クラスからDIパターンに対応したインスタンスクラスに変更しました
- CheatSheetControllerをDIパターンに対応させ、WindowHandleServiceのインスタンスを使用するように修正しました
- SettingManagerのインターフェース（ISettingManager）を作成し、DIパターンに対応させました
- KeySequenceManagerのインターフェース（IKeySequenceManager）を作成し、DIパターンに対応させました
- EditorOptionServiceのインターフェース（IEditorOptionService）を作成し、静的クラスからインスタンスベースのDIパターンに変更しました
- EditorOptionServiceの変更に伴うエラーを修正しました（UnitsFactory、PickerWindow、FileModel、MainWindow）
- SystemTrayRegisterでの静的AppCommandsの使用をDIパターンに変更しました
- MainWindow.xaml.csにGetRequiredServiceメソッドを使用するためのMicrosoft.Extensions.DependencyInjection名前空間を追加しました
- AimEditor.xaml.csをDIパターンに対応させ、IEditorOptionServiceをコンストラクタで注入するように変更しました
- UnitViewFactoryにIEditorOptionServiceを注入し、AimEditorの生成時に渡すように修正しました
- ClipboardAnalyzerプロジェクトにAimAssist.Coreへの参照を追加し、IEditorOptionServiceを使用できるようにしました
- ClipboardListにIEditorOptionServiceを注入するように修正しました
- ClipboardPluginをDIパターンに対応させ、GetUIElementConvertersメソッドでIEditorOptionServiceを取得して使用するように修正しました
- ユニットテストプロジェクト（AimAssist.Tests）を追加しました
- DI設定のテストクラスを作成しました（DependencyInjectionTests）
- サービスクラスのテストクラスを作成しました（SettingManagerTests、KeySequenceManagerTests）
- SnippetOptionServiceのインターフェース（ISnippetOptionService）を作成し、DIパターンに対応させました
- WorkItemOptionServiceのインターフェース（IWorkItemOptionService）を作成し、DIパターンに対応させました
- AimAssist.CoreプロジェクトにAimAssist.Unitプロジェクトへの参照を追加しました
- UnitsFactoryクラスにSnippetOptionServiceとWorkItemOptionServiceをコンストラクタで注入するように変更しました
- Initializerクラスが新しい形式のUnitsFactoryを使用するように修正しました

## 次のステップ

1. 残りのサービスクラスに対してインターフェースを抽出し、DIに対応させる
   - ✓ SettingManager
   - ✓ KeySequenceManager
   - ✓ EditorOptionService
   - ✓ SnippetOptionService
   - ✓ WorkItemOptionService
   - ✓ PluginsService
2. ✓ AppCommandsクラスをDIに完全対応させる
3. ✓ ユニットテストプロジェクトの追加
   - ✓ プロジェクト作成
   - ✓ DIコンテナのテスト
   - ✓ サービスの単体テスト
   - ✗ 統合テストの追加
4. リファクタリングの詳細計画の策定
   - ✓ PluginsServiceをDIパターンに対応
   - ✗ 命名の一貫性確保（例: GetConverters vs GetConterters）
   - ✗ エラーハンドリングの改善
   - ✗ UI層の改善
   - ✗ 国際化対応
5. クリーンアーキテクチャの適用
   - ✗ ドメイン層の整理
   - ✗ アプリケーション層の整理
   - ✗ インフラストラクチャ層の整理
   - ✗ プレゼンテーション層の整理
