using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using AimAssist.Core.Interfaces;
using System.Windows.Forms;

namespace AimAssist.Units.Implementation.Git
{
    public class GitViewModel : INotifyPropertyChanged
    {
        private readonly IGitService _gitService;
        private ObservableCollection<GitRepositoryViewModel> _repositories = new();
        private GitRepositoryViewModel? _selectedRepository;
        private bool _isLoading;
        private string _statusMessage = string.Empty;

        public GitViewModel(IGitService gitService)
        {
            _gitService = gitService;
            LoadRepositoriesCommand = new RelayCommand(async _ => await LoadRepositoriesAsync());
            RefreshCommand = new RelayCommand(async _ => await RefreshSelectedRepositoryAsync(), _ => SelectedRepository != null);
            ExportDiffCommand = new RelayCommand(async _ => await ExportDiffAsync(), _ => SelectedRepository?.CanExportDiff == true);
            AddRepositoryCommand = new RelayCommand(async _ => await AddRepositoryAsync());
            RemoveRepositoryCommand = new RelayCommand(async _ => await RemoveRepositoryAsync(), _ => SelectedRepository != null);
            
            _ = LoadRepositoriesAsync();
        }

        public ObservableCollection<GitRepositoryViewModel> Repositories
        {
            get => _repositories;
            set { _repositories = value; OnPropertyChanged(); }
        }

        public GitRepositoryViewModel? SelectedRepository
        {
            get => _selectedRepository;
            set 
            { 
                _selectedRepository = value;
                OnPropertyChanged();
                ((RelayCommand)RefreshCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ExportDiffCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RemoveRepositoryCommand).RaiseCanExecuteChanged();
                if (value != null)
                {
                    _ = LoadRepositoryDetailsAsync(value);
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoadRepositoriesCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExportDiffCommand { get; }
        public ICommand AddRepositoryCommand { get; }
        public ICommand RemoveRepositoryCommand { get; }

        private async Task LoadRepositoriesAsync()
        {
            IsLoading = true;
            StatusMessage = "リポジトリを検索中...";

            try
            {
                var reposPaths = await _gitService.GetRepositoriesAsync();
                var repoViewModels = reposPaths.Select(path => new GitRepositoryViewModel(path, _gitService)).ToList();
                
                Repositories.Clear();
                foreach (var repo in repoViewModels)
                {
                    Repositories.Add(repo);
                }

                StatusMessage = $"{Repositories.Count}個のリポジトリが見つかりました";
            }
            catch (Exception ex)
            {
                StatusMessage = $"エラー: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshSelectedRepositoryAsync()
        {
            if (SelectedRepository == null) return;
            await LoadRepositoryDetailsAsync(SelectedRepository);
        }

        private async Task LoadRepositoryDetailsAsync(GitRepositoryViewModel repository)
        {
            repository.IsLoading = true;
            StatusMessage = "リポジトリ詳細を読み込み中...";

            try
            {
                await repository.LoadDetailsAsync();
                StatusMessage = "リポジトリ詳細を読み込み完了";
            }
            catch (Exception ex)
            {
                StatusMessage = $"エラー: {ex.Message}";
            }
            finally
            {
                repository.IsLoading = false;
            }
        }

        private async Task ExportDiffAsync()
        {
            var commitPair = SelectedRepository?.GetSelectedCommitPair();
            if (commitPair == null)
                return;

            var folderDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "エクスポート先フォルダを選択",
                FileName = "選択してください",
                Filter = "フォルダ選択|*.folder"
            };

            if (folderDialog.ShowDialog() != true) return;

            var outputDir = Path.GetDirectoryName(folderDialog.FileName);
            if (string.IsNullOrEmpty(outputDir)) return;

            IsLoading = true;
            StatusMessage = "ファイル差分をエクスポート中...";

            try
            {
                var (fromCommit, toCommit) = commitPair.Value;
                var success = await _gitService.ExportFilesDiffAsync(
                    SelectedRepository.Path,
                    fromCommit.Hash,
                    toCommit.Hash,
                    outputDir);

                StatusMessage = success ? "エクスポート完了" : "エクスポートに失敗しました";
                
                if (success)
                {
                    System.Windows.MessageBox.Show($"差分ファイルを以下にエクスポートしました:\n{outputDir}\n\n変更前: {fromCommit.ShortHash}\n変更後: {toCommit.ShortHash}", "エクスポート完了");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"エラー: {ex.Message}";
                System.Windows.MessageBox.Show($"エクスポートエラー: {ex.Message}", "エラー");
            }
            finally
            {
                IsLoading = false;
            }
        }


        private async Task AddRepositoryAsync()
        {
            try
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "Gitリポジトリフォルダを選択してください"
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    IsLoading = true;
                    StatusMessage = "リポジトリを検証中...";

                    var selectedPath = dialog.SelectedPath;
                    if (await _gitService.IsValidGitRepositoryAsync(selectedPath))
                    {
                        var success = await _gitService.AddRepositoryAsync(selectedPath);
                        if (success)
                        {
                            await LoadRepositoriesAsync();
                            StatusMessage = "リポジトリを追加しました";
                        }
                        else
                        {
                            StatusMessage = "リポジトリの追加に失敗しました";
                        }
                    }
                    else
                    {
                        StatusMessage = "選択されたフォルダはGitリポジトリではありません";
                        System.Windows.MessageBox.Show("選択されたフォルダはGitリポジトリではありません", "エラー");
                    }

                    IsLoading = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"エラー: {ex.Message}";
                System.Windows.MessageBox.Show($"リポジトリ追加エラー: {ex.Message}", "エラー");
                IsLoading = false;
            }
        }

        private async Task RemoveRepositoryAsync()
        {
            if (SelectedRepository == null) return;

            try
            {
                var result = System.Windows.MessageBox.Show(
                    $"リポジトリ '{SelectedRepository.Name}' をリストから削除しますか？\n(リポジトリ自体は削除されません)",
                    "確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsLoading = true;
                    StatusMessage = "リポジトリを削除中...";

                    var success = await _gitService.RemoveRepositoryAsync(SelectedRepository.Path);
                    if (success)
                    {
                        await LoadRepositoriesAsync();
                        StatusMessage = "リポジトリを削除しました";
                        SelectedRepository = null;
                    }
                    else
                    {
                        StatusMessage = "リポジトリの削除に失敗しました";
                    }

                    IsLoading = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"エラー: {ex.Message}";
                System.Windows.MessageBox.Show($"リポジトリ削除エラー: {ex.Message}", "エラー");
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GitRepositoryViewModel : INotifyPropertyChanged
    {
        private readonly IGitService _gitService;
        private string _currentBranch = string.Empty;
        private ObservableCollection<string> _branches = new();
        private ObservableCollection<GitCommit> _commits = new();
        private List<GitCommit> _selectedCommits = new();
        private bool _isLoading;

        public GitRepositoryViewModel(string path, IGitService gitService)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
            _gitService = gitService;
            
            CheckoutBranchCommand = new RelayCommand(async param => await CheckoutBranchAsync(param?.ToString() ?? string.Empty));
            ClearSelectionCommand = new RelayCommand(_ => ClearCommitSelection());
        }

        public string Name { get; }
        public string Path { get; }

        public string CurrentBranch
        {
            get => _currentBranch;
            set { _currentBranch = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Branches
        {
            get => _branches;
            set { _branches = value; OnPropertyChanged(); }
        }

        public ObservableCollection<GitCommit> Commits
        {
            get => _commits;
            set { _commits = value; OnPropertyChanged(); }
        }

        public string SelectedCommitsInfo
        {
            get
            {
                if (_selectedCommits.Count == 0)
                    return "選択なし";
                if (_selectedCommits.Count == 1)
                    return $"1個選択: {_selectedCommits[0].ShortHash}";
                if (_selectedCommits.Count == 2)
                    return $"2個選択: {_selectedCommits[0].ShortHash} ～ {_selectedCommits[1].ShortHash}";
                return $"{_selectedCommits.Count}個選択 (2個まで選択してください)";
            }
        }

        public bool CanExportDiff => _selectedCommits.Count == 2;

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ICommand CheckoutBranchCommand { get; }
        public ICommand ClearSelectionCommand { get; }

        public async Task LoadDetailsAsync()
        {
            await Task.WhenAll(
                LoadCurrentBranchAsync(),
                LoadBranchesAsync(),
                LoadCommitsAsync()
            );
        }

        private async Task LoadCurrentBranchAsync()
        {
            try
            {
                var branch = await _gitService.GetCurrentBranchAsync(Path);
                CurrentBranch = branch ?? "不明";
            }
            catch
            {
                CurrentBranch = "エラー";
            }
        }

        private async Task LoadBranchesAsync()
        {
            try
            {
                var branches = await _gitService.GetBranchesAsync(Path);
                Branches.Clear();
                foreach (var branch in branches)
                {
                    Branches.Add(branch);
                }
            }
            catch
            {
                Branches.Clear();
            }
        }

        private async Task LoadCommitsAsync()
        {
            try
            {
                var commits = await _gitService.GetCommitHistoryAsync(Path, 50);
                Commits.Clear();
                foreach (var commit in commits)
                {
                    Commits.Add(commit);
                }
            }
            catch
            {
                Commits.Clear();
            }
        }

        public async Task CheckoutBranchAsync(string branchName)
        {
            if (string.IsNullOrEmpty(branchName)) return;

            try
            {
                var success = await _gitService.CheckoutBranchAsync(Path, branchName);
                if (success)
                {
                    await LoadCurrentBranchAsync();
                    await LoadCommitsAsync();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"ブランチ切り替えエラー: {ex.Message}", "エラー");
            }
        }

        private void ClearCommitSelection()
        {
            _selectedCommits.Clear();
            OnPropertyChanged(nameof(SelectedCommitsInfo));
            OnPropertyChanged(nameof(CanExportDiff));
        }

        public void UpdateSelectedCommits(List<GitCommit> selectedCommits)
        {
            _selectedCommits = selectedCommits?.Take(2).OrderBy(c => c.Date).ToList() ?? new List<GitCommit>();
            OnPropertyChanged(nameof(SelectedCommitsInfo));
            OnPropertyChanged(nameof(CanExportDiff));
        }

        public (GitCommit fromCommit, GitCommit toCommit)? GetSelectedCommitPair()
        {
            if (_selectedCommits.Count == 2)
            {
                // Return in chronological order (older first, newer second)
                var ordered = _selectedCommits.OrderBy(c => c.Date).ToList();
                return (ordered[0], ordered[1]);
            }
            return null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}