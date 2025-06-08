# AimAssist

## プロジェクト概要
WPFベースのコマンドランチャー・生産性向上ツール
- 複数のプロジェクトから構成される.NETアプリケーション
- DI、Factory、Command、MVVMパターンを採用
- ユニットベースの機能拡張システム

## 完了したタスク
### GitHub Actions自動リリース機能
- `release.yml`: シンプルなリリースワークフロー
- `build-and-release.yml`: 高機能なビルド・リリースワークフロー
  - キャッシュ機能
  - テスト実行
  - 自動バージョニング
  - タグベース・ブランチベースリリース
  - セルフコンテインド実行ファイル生成
  - 詳細なリリースノート生成

#### 設定内容
- mainブランチpush時に自動リリース
- vタグpush時に正式リリース
- Windows x64向けセルフコンテインド実行ファイル
- 自動バージョニング（日付ベース + ビルド番号）
- ZIP形式でのアーティファクト配布

### AimAssistCommandsControl 動的コマンドボタン生成機能
- IAppCommandsインターフェースのすべてのコマンドを自動検出
- リフレクションを使用してコマンドプロパティを動的に取得
- 各コマンドに対応するボタンを自動生成
- 日本語表示名のマッピング機能
- シンプルなデフォルトスタイル

#### 実装内容
- ShutdownAimAssist、ToggleMainWindow、ShowPickerWindowの3つのコマンドをサポート
- 動的ボタン生成により新しいコマンド追加時の保守性向上
- シンプルなデフォルトボタンスタイルで統一
- ScrollViewer内での適切なレイアウト

### MainWindow DI インターフェース化
- IMainWindowインターフェースの作成
- MainWindowクラスでのインターフェース実装
- UIServiceModuleでのインターフェースベース登録
- ServiceProviderからインターフェース経由でMainWindow取得可能に

#### 実装内容
- IMainWindowインターフェースで主要メソッドを定義
- テスタビリティとコードの疎結合化を実現
- 既存コード互換性のため具象クラス登録も維持
- インターフェース経由での依存性注入が可能

## TODO
- なし

## 最新の変更
### MainWindowCommands.NextUnit/PreviousUnit コマンド修正
- ViewのSelectedIndexを直接操作するように変更
- 範囲チェックを追加して配列外アクセスを防止
- NextUnit: 最後の要素で最初に戻る（循環）
- PreviousUnit: 最初の要素で最後に戻る（循環）
- Items.Countが0の場合の安全チェックも追加

## アーキテクチャ
- WPFアプリケーション（.NET 8）
- マルチプロジェクト構成
- プラグインシステム
- 音声認識機能（Vosk/Whisper）
- Web統合機能
