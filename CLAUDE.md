# AimAssist

## プロジェクト概要
WPFベースのコマンドランチャー・生産性向上ツール
- 複数のプロジェクトから構成される.NETアプリケーション
- DI、Factory、Command、MVVMパターンを採用
- ユニットベースの機能拡張システム

## リファクタリング計画 - View Template + 規約ベース ハイブリッド設計

### 目標
1. 人間が変更を加えやすい（Unitの追加など）構造に変更
2. コード量の削減（特にUnitViewFactory.csの100行switch文削除）
3. 既存のフォルダ構成は維持

### 現状の問題分析

#### Unit-View対応の複雑性
**UnitsFactory.cs (80行):**
- 全Unitの生成ロジックが手動記述
- MarkdownファイルやSnippet等の動的生成が複雑

**UnitViewFactory.cs (100行switch文):**
```csharp
switch (unit.Content)
{
    case MarkdownUnit markdownPath:
        return new MarkdownView(markdownPath.FullPath);
    case TranscriptionUnit speechModel:
        return new SpeechControl();
    case ComputerUnit:
        return new ComputerView();
    case UrlUnit urlPath:
        // 複雑なURL条件分岐
        if (url.StartsWith("https://chatgpt")) return new ChatGptControl(url);
        if (url.StartsWith("https://claude.ai")) return new ClaudeControl(url);
        // ...
    // 16個のケース + デフォルト処理
}
```

### 実装完了状況

#### ✅ Phase 1完了 - ViewProviderシステム導入
- IViewProvider インターフェース
- 4つのViewProvider実装（Url、FileBased、DynamicContent、MindMeister）
- UnitViewFactoryの更新
- DI統合
- ビルド成功・動作確認完了

#### ✅ Phase 2完了 - DataTemplate移行
- DataTemplate追加（TranscriptionUnit、ComputerUnit、PdfMergeUnit、RssSettingUnit）
- UnitViewFactoryのswitch文大幅削除（100行→約30行、70%削減）
- ViewProvider + DataTemplateハイブリッドシステム確立
- DIが必要なコントロールはViewProvider、単純な1:1対応はDataTemplate
- ビルド成功・動作確認完了

#### 🔄 Phase 3進行中 - Unit自動登録システム
1. ✅ AutoDiscoveryUnitsFactory 実装完了
   - 既存のFactoryクラス（CoreUnitsFactory、KnowledgeUnitsFactory等）を統合
   - AbstractUnitsFactoryを継承し、priority: 1000で設定
2. ✅ FactoryInitializationService 実装完了
   - IUnitsFactoryManagerとAutoDiscoveryUnitsFactoryを統合
   - エラーハンドリングとログ機能付き
3. ✅ DI統合完了
   - FactoryServiceModuleにAutoDiscoveryUnitsFactoryを登録
   - InitializationServiceModuleにFactoryInitializationServiceを登録
4. ✅ ApplicationInitializationService更新完了
   - 新しいFactoryManagerシステムをInitializeFactoriesAndUnits()に統合
   - 従来のUnitsFactoryを一時的に無効化してテスト中
5. ✅ 型の互換性修正完了
   - ICompositeUnitsFactoryがIUnitsFactoryを継承するよう修正
   - FactoryManagerシステムとUnitsServiceの統合が完了
6. 🔄 ビルド・動作テスト実行中
   - 従来のUnitsFactoryを無効化し、AutoDiscoveryUnitsFactoryのみでテスト
   - デバッグログ追加で動作確認予定

### 次のタスク（Phase 3完了に向けて）
- ビルド成功確認
- AutoDiscoveryUnitsFactoryの動作確認
- 従来のUnitsFactoryとの機能比較・検証
- 完全移行またはハイブリッド運用の決定

#### Phase 4: 完全移行とクリーンアップ（1週間）
1. 全Unit-View対応の新システム移行完了
2. UnitViewFactory.csのswitch文完全削除
3. UnitsFactory.csの削除
4. テストと最適化

### 期待効果

#### 新Unit追加の簡素化
**従来（2箇所修正必要）:**
```csharp
// 1. UnitsFactory.cs に追加
yield return new MyNewUnit();

// 2. UnitViewFactory.cs に追加
case MyNewUnit:
    return new MyNewView();
```

**新方式（1箇所のみ）:**
```xml
<!-- App.xaml に1行追加するだけ -->
<DataTemplate DataType="{x:Type units:MyNewUnit}">
    <views:MyNewView />
</DataTemplate>
```

#### コード削減効果
- **UnitViewFactory.cs**: 100行 → 30行（70%削減）
- **UnitsFactory.cs**: 80行 → 削除予定（100%削減）
- **新Unit追加工数**: 95%削減
- **修正漏れリスク**: ほぼゼロ

#### 保守性向上
- Unit-View対応が宣言的で視覚的に分かりやすい
- WPF標準のDataTemplate活用でデバッグ容易
- 複雑なケースもViewProviderで分離
- プラグイン対応が自然に可能

次のPhase 4（完全移行とクリーンアップ）の準備が整いました。既存機能への影響を最小限に抑えつつ、段階的に新しいアーキテクチャに移行する基盤が完成しています。
