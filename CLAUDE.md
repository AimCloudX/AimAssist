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

#### 既存のUnit-View対応関係
1. **1:1対応（単純）**
   - `TranscriptionUnit` → `SpeechControl`
   - `ComputerUnit` → `ComputerView`
   - `PdfMergeUnit` → `PdfMergerControl`
   - `ClipboardUnit` → `ClipboardList`

2. **1:1対応（パラメータ付き）**
   - `MarkdownUnit` → `MarkdownView(filePath)`
   - `EditorUnit` → `AimEditor(filePath)`
   - `OptionUnit` → `AimEditor(multiple files)`

3. **条件分岐（複雑）**
   - `UrlUnit` → 4種類のWebViewControl（URL条件による分岐）

4. **動的生成**
   - `SnippetUnit` → `TextBox(code)`

### 新設計：View Template + 規約ベース + ViewProvider ハイブリッド

#### 1. Unit側の完全分離
```csharp
// Unit は UI を一切知らない純粋なビジネスモデル
public class TranscriptionUnit : IUnit
{
    public IMode Mode => AllInclusiveMode.Instance;
    public string Name => "音声認識";
    public string Description => "音声をテキストに変換";
    public string Category => "Audio";
    // UI の知識ゼロ
}

public class ComputerUnit : IUnit
{
    public IMode Mode => WorkToolsMode.Instance;
    public string Name => "PC情報";
    public string Description => "コンピューター情報の表示";
    public string Category => "System";
}

// パラメータ持ちUnitもUI無知
public class MarkdownUnit : IUnit
{
    public string FullPath { get; }
    public string Category { get; }
    // View作成ロジックなし
}
```

#### 2. WPF DataTemplate による自動対応（シンプルケース）
**App.xaml または専用ResourceDictionary:**
```xml
<Application.Resources>
    <ResourceDictionary>
        <!-- 1:1対応の簡単なケース -->
        <DataTemplate DataType="{x:Type units:TranscriptionUnit}">
            <speech:SpeechControl />
        </DataTemplate>
        
        <DataTemplate DataType="{x:Type units:ComputerUnit}">
            <computer:ComputerView />
        </DataTemplate>
        
        <DataTemplate DataType="{x:Type units:PdfMergeUnit}">
            <pdf:PdfMergerControl />
        </DataTemplate>
        
        <DataTemplate DataType="{x:Type units:ClipboardUnit}">
            <clipboard:ClipboardList EditorOptionService="{Binding EditorOptionService}" />
        </DataTemplate>
    </ResourceDictionary>
</Application.Resources>
```

#### 3. ViewProvider による複雑ケース対応
```csharp
public interface IViewProvider
{
    bool CanProvideView(Type unitType);
    UIElement CreateView(IUnit unit, IServiceProvider serviceProvider);
    int Priority { get; }
}

// URL条件分岐専用Provider
[ViewProvider(Priority = 100)]
public class UrlViewProvider : IViewProvider
{
    public bool CanProvideView(Type unitType) => unitType == typeof(UrlUnit);
    
    public UIElement CreateView(IUnit unit, IServiceProvider serviceProvider)
    {
        var urlUnit = (UrlUnit)unit;
        return urlUnit.Url switch
        {
            var url when url.StartsWith("https://chatgpt") => new ChatGptControl(url),
            var url when url.StartsWith("https://claude.ai") => new ClaudeControl(url),
            var url when url.StartsWith("https://www.amazon") => new AmazonWebViewControl(url),
            _ => new WebViewControl(urlUnit.Url)
        };
    }
}

// ファイルパス系Unit用Provider
[ViewProvider(Priority = 90)]
public class FileBasedViewProvider : IViewProvider
{
    public bool CanProvideView(Type unitType) => 
        unitType == typeof(MarkdownUnit) || 
        unitType == typeof(EditorUnit) || 
        unitType == typeof(OptionUnit);
    
    public UIElement CreateView(IUnit unit, IServiceProvider serviceProvider)
    {
        return unit switch
        {
            MarkdownUnit md => new MarkdownView(md.FullPath),
            EditorUnit editor => CreateEditor(editor.FullPath),
            OptionUnit option => CreateMultiFileEditor(option.OptionFilePaths),
            _ => null
        };
    }
}

// 動的コンテンツ用Provider
[ViewProvider(Priority = 80)]
public class DynamicContentViewProvider : IViewProvider
{
    public bool CanProvideView(Type unitType) => unitType == typeof(SnippetUnit);
    
    public UIElement CreateView(IUnit unit, IServiceProvider serviceProvider)
    {
        var code = unit switch
        {
            SnippetUnit snippet => snippet.Code,
            _ => string.Empty
        };
        
        return new TextBox
        {
            Text = code,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(0),
            IsReadOnly = true,
            TextWrapping = TextWrapping.Wrap,
            FontFamily = new FontFamily("Consolas"),
            Background = Brushes.LightGray
        };
    }
}
```

#### 4. 新しいUnitViewFactory（超シンプル）
```csharp
public class UnitViewFactory
{
    private readonly IEnumerable<IViewProvider> viewProviders;
    private readonly IServiceProvider serviceProvider;
    private readonly Dictionary<string, UIElement> cache = new();
    
    public UnitViewFactory(IEnumerable<IViewProvider> viewProviders, IServiceProvider serviceProvider)
    {
        this.viewProviders = viewProviders.OrderByDescending(p => p.Priority);
        this.serviceProvider = serviceProvider;
    }
    
    public UIElement Create(UnitViewModel unit, bool createNew = false)
    {
        if (!createNew && cache.TryGetValue(unit.Name, out var cached))
            return cached;
            
        var element = CreateViewForUnit(unit.Content);
        
        if (!createNew && element != null)
            cache[unit.Name] = element;
            
        return element;
    }
    
    private UIElement CreateViewForUnit(IUnit unit)
    {
        // 1. 専用ViewProviderをチェック
        var provider = viewProviders.FirstOrDefault(p => p.CanProvideView(unit.GetType()));
        if (provider != null)
        {
            return provider.CreateView(unit, serviceProvider);
        }
        
        // 2. WPFのDataTemplateに委譲（標準機能）
        var contentPresenter = new ContentPresenter 
        { 
            Content = unit,
            DataContext = serviceProvider // DIコンテナを提供
        };
        
        return contentPresenter;
    }
}
```

### 移行戦略

#### ✅ Phase 1: ViewProvider システム導入（完了）
1. ✅ IViewProvider インターフェース作成
2. ✅ 既存のswitch文ロジックをViewProviderに移行
   - ✅ UrlViewProvider（URL条件分岐専用）
   - ✅ FileBasedViewProvider（MarkdownUnit、EditorUnit、OptionUnit）
   - ✅ DynamicContentViewProvider（SnippetUnit）
3. ✅ DI登録とUnitViewFactoryの部分修正
4. ✅ 既存機能との並行動作確認
5. ✅ ビルド成功・動作確認完了

#### Phase 2: DataTemplate 移行（1週間）
1. App.xaml にDataTemplate追加
2. 簡単な1:1対応Unitから順次移行
3. ViewProviderから対応するケースを削除
4. 動作確認とテスト

#### Phase 3: Unit自動登録システム（1週間）
1. AutoDiscoveryUnitsFactory 実装
2. 既存UnitsFactory.csから段階的に移行
3. CompositeUnitsFactory との統合
4. 既存FactoryManagerシステムとの統合

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
- **UnitsFactory.cs**: 80行 → 削除（100%削減）
- **新Unit追加工数**: 95%削減
- **修正漏れリスク**: ほぼゼロ

#### 保守性向上
- Unit-View対応が宣言的で視覚的に分かりやすい
- WPF標準のDataTemplate活用でデバッグ容易
- 複雑なケースもViewProviderで分離
- プラグイン対応が自然に可能

#### テスタビリティ向上
- UnitからUIの依存を完全排除
- Unit単体テストが容易
- ViewProvider単体テストも可能

### 実装完了状況
✅ **Phase 1完了** - ViewProviderシステム導入
- IViewProvider インターフェース
- 4つのViewProvider実装（Url、FileBased、DynamicContent、MindMeister）
- UnitViewFactoryの更新
- DI統合
- ビルド成功・動作確認完了

✅ **Phase 2完了** - DataTemplate移行
- App.xamlにDataTemplate追加（TranscriptionUnit、ComputerUnit、PdfMergeUnit、RssSettingUnit）
- UnitViewFactoryのswitch文大幅削除（100行→約30行、70%削減）
- ViewProvider + DataTemplateハイブリッドシステム確立
- DIが必要なコントロールはViewProvider、単純な1:1対応はDataTemplate
- ビルド成功・動作確認完了

次のPhase 3（Unit自動登録システム）の準備が整いました。既存機能への影響を最小限に抑えつつ、段階的に新しいアーキテクチャに移行する基盤が完成しています。
Provider
[ViewProvider(Priority = 80)]
public class DynamicContentViewProvider : IViewProvider
{
    public bool CanProvideView(Type unitType) => 
        unitType == typeof(SnippetUnit) || unitType == typeof(SnippetModelUnit);
    
    public UIElement CreateView(IUnit unit, IServiceProvider serviceProvider)
    {
        var code = unit switch
        {
            SnippetUnit snippet => snippet.Code,
            SnippetModelUnit model => model.Code,
            _ => string.Empty
        };
        
        return new TextBox
        {
            Text = code,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(0)
        };
    }
}
```

#### 4. 新しいUnitViewFactory（超シンプル）
```csharp
public class UnitViewFactory
{
    private readonly IEnumerable<IViewProvider> viewProviders;
    private readonly IServiceProvider serviceProvider;
    private readonly Dictionary<string, UIElement> cache = new();
    
    public UnitViewFactory(IEnumerable<IViewProvider> viewProviders, IServiceProvider serviceProvider)
    {
        this.viewProviders = viewProviders.OrderByDescending(p => p.Priority);
        this.serviceProvider = serviceProvider;
    }
    
    public UIElement Create(UnitViewModel unit, bool createNew = false)
    {
        if (!createNew && cache.TryGetValue(unit.Name, out var cached))
            return cached;
            
        var element = CreateViewForUnit(unit.Content);
        
        if (!createNew && element != null)
            cache[unit.Name] = element;
            
        return element;
    }
    
    private UIElement CreateViewForUnit(IUnit unit)
    {
        // 1. 専用ViewProviderをチェック
        var provider = viewProviders.FirstOrDefault(p => p.CanProvideView(unit.GetType()));
        if (provider != null)
        {
            return provider.CreateView(unit, serviceProvider);
        }
        
        // 2. WPFのDataTemplateに委譲（標準機能）
        var contentPresenter = new ContentPresenter 
        { 
            Content = unit,
            DataContext = serviceProvider // DIコンテナを提供
        };
        
        return contentPresenter;
    }
}
```

#### 5. Unit自動登録システム（既存のFactoryManager活用）
```csharp
// 既存のAbstractUnitsFactory + FactoryManagerを活用
public class AutoDiscoveryUnitsFactory : AbstractUnitsFactory
{
    private readonly IServiceProvider serviceProvider;
    
    public AutoDiscoveryUnitsFactory(IServiceProvider serviceProvider) 
        : base("AutoDiscovery", priority: 1000)
    {
        this.serviceProvider = serviceProvider;
    }
    
    public override IEnumerable<IUnit> CreateUnits()
    {
        // 動的ファイル読み込み系
        foreach (var markdownUnit in CreateMarkdownUnits()) yield return markdownUnit;
        foreach (var snippetUnit in CreateSnippetUnits()) yield return snippetUnit;
        foreach (var workItemUnit in CreateWorkItemUnits()) yield return workItemUnit;
        
        // 静的Unit系（既存コードから移行）
        yield return new TranscriptionUnit();
        yield return new PdfMergeUnit();
        yield return new ComputerUnit();
        yield return new ClipboardUnit();
        yield return new MindMeisterUnit("最近開いたMap", "https://www.mindmeister.com/app/maps/recent");
        
        // CheatSheet系
        foreach (var cheatSheetUnit in CreateCheatSheetUnits()) yield return cheatSheetUnit;
        
        // Option系
        yield return CreateOptionUnit();
        yield return new ShortcutOptionUnit();
    }
}
```

### 移行戦略

#### Phase 1: ViewProvider システム導入（1週間）
1. IViewProvider インターフェース作成
2. 既存のswitch文ロジックをViewProviderに移行
3. DI登録とUnitViewFactoryの部分修正
4. 既存機能との並行動作確認

#### Phase 2: DataTemplate 移行（1週間）
1. App.xaml にDataTemplate追加
2. 簡単な1:1対応Unitから順次移行
3. ViewProviderから対応するケースを削除
4. 動作確認とテスト

#### Phase 3: Unit自動登録システム（1週間）
1. AutoDiscoveryUnitsFactory 実装
2. 既存UnitsFactory.csから段階的に移行
3. CompositeUnitsFactory との統合
4. 既存FactoryManagerシステムとの統合

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
- **UnitsFactory.cs**: 80行 → 削除（100%削減）
- **新Unit追加工数**: 95%削減
- **修正漏れリスク**: ほぼゼロ

#### 保守性向上
- Unit-View対応が宣言的で視覚的に分かりやすい
- WPF標準のDataTemplate活用でデバッグ容易
- 複雑なケースもViewProviderで分離
- プラグイン対応が自然に可能

#### テスタビリティ向上
- UnitからUIの依存を完全排除
- Unit単体テストが容易
- ViewProvider単体テストも可能

### 次の作業
Phase 1のViewProviderシステム導入から着手予定。既存機能への影響を最小限に抑えつつ、段階的に新しいアーキテクチャに移行する。
