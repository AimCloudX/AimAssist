using AimAssist.Commands;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Services;
using AimAssist.UI.UnitContentsView;
using AimAssist.Units.Core.Modes;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.Snippets;
using AimAssist.Units.Implementation.Web;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AimAssist.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IUnitManagementService _unitManagementService;
        private readonly INavigationService _navigationService;
        private readonly IApplicationLogService _logService;
        private readonly IUnitsService _unitsService;
        private readonly UnitViewFactory _unitViewFactory;
        
        private string _filterText = string.Empty;
        private UnitViewModel _selectedUnit;
        private IMode _selectedMode;
        private bool _isLoading;
        private bool _isItemListVisible = true;
        private UIElement _currentContent;

        public MainWindowViewModel(
            IUnitManagementService unitManagementService,
            INavigationService navigationService,
            IApplicationLogService logService,
            IUnitsService unitsService,
            UnitViewFactory unitViewFactory)
        {
            _unitManagementService = unitManagementService;
            _navigationService = navigationService;
            _logService = logService;
            _unitsService = unitsService;
            _unitViewFactory = unitViewFactory;
            
            Units = new ObservableCollection<UnitViewModel>();
            Modes = new ObservableCollection<IMode>();
            
            RefreshUnitsCommand = new RelayCommand(async () => await RefreshUnitsAsync());
            ExecuteUnitCommand = new RelayCommand<UnitViewModel>(ExecuteUnit, unit => unit != null);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            CloseWindowCommand = new RelayCommand(() => 
            {
                try
                {
                    _navigationService.HideMainWindow();
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex, "ウィンドウクローズ中にエラーが発生しました");
                }
            });
            
            LoadModesAndUnits();
        }

        public ObservableCollection<UnitViewModel> Units { get; }
        public ObservableCollection<IMode> Modes { get; }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged(nameof(FilterText));
                }
            }
        }

        public UnitViewModel SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (_selectedUnit != value)
                {
                    _selectedUnit = value;
                    OnPropertyChanged(nameof(SelectedUnit));
                    UpdateCurrentContent();
                }
            }
        }

        public IMode SelectedMode
        {
            get => _selectedMode;
            set
            {
                if (_selectedMode != value)
                {
                    _selectedMode = value;
                    OnPropertyChanged(nameof(SelectedMode));
                    LoadUnitsForMode(value);
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public bool IsItemListVisible
        {
            get => _isItemListVisible;
            set
            {
                if (_isItemListVisible != value)
                {
                    _isItemListVisible = value;
                    OnPropertyChanged(nameof(IsItemListVisible));
                }
            }
        }

        public UIElement CurrentContent
        {
            get => _currentContent;
            set
            {
                if (_currentContent != value)
                {
                    _currentContent = value;
                    OnPropertyChanged(nameof(CurrentContent));
                }
            }
        }

        public Predicate<object> FilterPredicate => Filter;

        public ICommand RefreshUnitsCommand { get; }
        public ICommand ExecuteUnitCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand CloseWindowCommand { get; }

        private void LoadModesAndUnits()
        {
            try
            {
                Modes.Clear();
                foreach (var mode in _unitsService.GetAllModes())
                {
                    Modes.Add(mode);
                }

                if (Modes.Any())
                {
                    SelectedMode = AllInclusiveMode.Instance;
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "モード読み込み中にエラーが発生しました");
            }
        }

        private void LoadUnitsForMode(IMode mode)
        {
            if (mode == null) return;

            try
            {
                Units.Clear();
                var units = _unitsService.CreateUnits(mode);

                foreach (var unit in units)
                {
                    if (unit is UrlUnit urlUnit)
                    {
                        Units.Add(new UnitViewModel(urlUnit));
                    }
                    else if (unit is SnippetModelUnit && mode != SnippetMode.Instance)
                    {
                        continue;
                    }
                    else
                    {
                        Units.Add(new UnitViewModel(unit));
                    }
                }

                if (Units.Any())
                {
                    SelectedUnit = Units.First();
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"モード '{mode.Name}' のユニット読み込み中にエラーが発生しました");
            }
        }

        private void UpdateCurrentContent()
        {
            if (SelectedUnit != null)
            {
                try
                {
                    CurrentContent = _unitViewFactory.Create(SelectedUnit);
                }
                catch (Exception ex)
                {
                    _logService.LogException(ex, $"ユニット '{SelectedUnit.Name}' のコンテンツ作成中にエラーが発生しました");
                }
            }
        }

        private async Task RefreshUnitsAsync()
        {
            try
            {
                IsLoading = true;
                _unitManagementService.RefreshUnits();
                LoadUnitsForMode(SelectedMode);
                _logService.Info("ユニットが更新されました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "ユニット更新中にエラーが発生しました");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool Filter(object obj)
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                return true;
            }

            if (obj is UnitViewModel unitViewModel)
            {
                return unitViewModel.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        private void ExecuteUnit(UnitViewModel unitViewModel)
        {
            try
            {
                if (unitViewModel?.Content != null)
                {
                    _logService.Info($"ユニットを実行しました: {unitViewModel.Name}");
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"ユニット実行中にエラーが発生しました: {unitViewModel?.Name}");
            }
        }

        private void ClearFilter()
        {
            FilterText = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
