using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using AimAssist.Core.Attributes;
using AimAssist.Core.Interfaces;

namespace AimAssist.Units.Implementation.Git
{
    [AutoDataTemplate(typeof(GitUnit))]
    public partial class GitView : UserControl
    {
        private GitViewModel? _viewModel;

        public GitView()
        {
            InitializeComponent();
            Loaded += GitView_Loaded;
        }

        private void GitView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null)
            {
                try
                {
                    var serviceProvider = Application.Current.Properties["ServiceProvider"] as IServiceProvider;
                    var gitService = serviceProvider?.GetService<IGitService>();
                    
                    if (gitService != null)
                    {
                        _viewModel = new GitViewModel(gitService);
                        DataContext = _viewModel;
                        
                        // Hook up event handlers
                        CommitsDataGrid.SelectionChanged += CommitsDataGrid_SelectionChanged;
                        BranchComboBox.SelectionChanged += BranchComboBox_SelectionChanged;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"GitViewの初期化エラー: {ex.Message}", "エラー");
                }
            }
        }

        private void CommitsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_viewModel?.SelectedRepository != null && sender is System.Windows.Controls.DataGrid dataGrid)
            {
                var selectedCommits = dataGrid.SelectedItems.Cast<GitCommit>().ToList();
                _viewModel.SelectedRepository.UpdateSelectedCommits(selectedCommits);
            }
        }

        private async void BranchComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_viewModel?.SelectedRepository != null && sender is System.Windows.Controls.ComboBox comboBox)
            {
                var selectedBranch = comboBox.SelectedItem as string;
                if (!string.IsNullOrEmpty(selectedBranch) && selectedBranch != _viewModel.SelectedRepository.CurrentBranch)
                {
                    await _viewModel.SelectedRepository.CheckoutBranchAsync(selectedBranch);
                }
            }
        }
    }
}