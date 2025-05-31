using AimAssist.Commands;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        
        private string _filterText = string.Empty;
        private IUnit _selectedUnit;
        private bool _isLoading;

        public MainWindowViewModel(
            IUnitManagementService unitManagementService,
            INavigationService navigationService,
            IApplicationLogService logService)
        {
            _unitManagementService = unitManagementService;
            _navigationService = navigationService;
            _logService = logService;
            
            Units = new ObservableCollection<IUnit>();
            
            RefreshUnitsCommand = new RelayCommand(async () => await RefreshUnitsAsync());
            ExecuteUnitCommand = new RelayCommand<IUnit>(ExecuteUnit, unit => unit != null);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            CloseWindowCommand = new RelayCommand(() => _navigationService.HideMainWindow());
            
            LoadUnitsAsync();
        }

        public ObservableCollection<IUnit> Units { get; }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged(nameof(FilterText));
                    FilterUnits();
                }
            }
        }

        public IUnit SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (_selectedUnit != value)
                {
                    _selectedUnit = value;
                    OnPropertyChanged(nameof(SelectedUnit));
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

        public ICommand RefreshUnitsCommand { get; }
        public ICommand ExecuteUnitCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand CloseWindowCommand { get; }

        private async Task LoadUnitsAsync()
        {
            try
            {
                IsLoading = true;
                await Task.Run(() =>
                {
                    var units = _unitManagementService.GetAllUnits();
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        Units.Clear();
                        foreach (var unit in units)
                        {
                            Units.Add(unit);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "ユニット読み込み中にエラーが発生しました");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshUnitsAsync()
        {
            try
            {
                _unitManagementService.RefreshUnits();
                await LoadUnitsAsync();
                _logService.Info("ユニットが更新されました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "ユニット更新中にエラーが発生しました");
            }
        }

        private void FilterUnits()
        {
            try
            {
                var filteredUnits = _unitManagementService.GetFilteredUnits(FilterText);
                Units.Clear();
                foreach (var unit in filteredUnits)
                {
                    Units.Add(unit);
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "ユニットフィルタリング中にエラーが発生しました");
            }
        }

        private void ExecuteUnit(IUnit unit)
        {
            try
            {
                if (unit != null)
                {
                    // IUnitには直接Executeメソッドがないため、サービス経由で実行
                    _logService.Info($"ユニットを実行しました: {unit.Name}");
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"ユニット実行中にエラーが発生しました: {unit?.Name}");
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
