using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AimAssist.Commands;
using AimAssist.Core.Commands;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Core.Attributes;
using AimAssist.Services;
using AimAssist.UI.UnitContentsView;
using AimAssist.Units.Core.Modes;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.Web;
using UnitViewModel = AimAssist.UI.UnitContentsView.UnitViewModel;

namespace AimAssist.UI.MainWindows
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IUnitManagementService unitManagementService;
        private readonly IApplicationLogService logService;
        private readonly IUnitsService unitsService;
        private readonly UnitViewFactory unitViewFactory;
        
        private string filterText = string.Empty;
        private UnitViewModel selectedUnit = null!;
        private IMode selectedMode = null!;
        private bool isLoading;
        private bool isItemListVisible = true;
        private UIElement currentContent = null!;

        public MainWindowViewModel(
            IUnitManagementService unitManagementService,
            INavigationService navigationService,
            IApplicationLogService logService,
            IUnitsService unitsService,
            UnitViewFactory unitViewFactory)
        {
            this.unitManagementService = unitManagementService;
            var service = navigationService;
            this.logService = logService;
            this.unitsService = unitsService;
            this.unitViewFactory = unitViewFactory;
            
            Units = new ObservableCollection<UnitViewModel>();
            Modes = new ObservableCollection<IMode>();
            
            var binding = new CommandBinding(AimAssistCommands.SendUnitCommand, ExecuteReceiveData);
            CommandManager.RegisterClassCommandBinding(typeof(Window), binding);
            
            RefreshUnitsCommand = new RelayCommand(async () => await RefreshUnitsAsync());
            ExecuteUnitCommand = new RelayCommand<UnitViewModel>(ExecuteUnit, unit => unit != null);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            CloseWindowCommand = new RelayCommand(() => 
            {
                try
                {
                    service.HideMainWindow();
                }
                catch (Exception ex)
                {
                    this.logService.LogException(ex, "ウィンドウクローズ中にエラーが発生しました");
                }
            });
            
            LoadModesAndUnits();
        }

        public ObservableCollection<UnitViewModel> Units { get; }
        public ObservableCollection<IMode> Modes { get; }

        public string FilterText
        {
            get => filterText;
            set
            {
                if (filterText != value)
                {
                    filterText = value;
                    OnPropertyChanged(nameof(FilterText));
                }
            }
        }

        public UnitViewModel SelectedUnit
        {
            get => selectedUnit;
            set
            {
                if (selectedUnit != value)
                {
                    selectedUnit = value;
                    OnPropertyChanged(nameof(SelectedUnit));
                    UpdateCurrentContent();
                }
            }
        }

        public IMode SelectedMode
        {
            get => selectedMode;
            set
            {
                if (selectedMode != value)
                {
                    selectedMode = value;
                    OnPropertyChanged(nameof(SelectedMode));
                    LoadUnitsForMode(value);
                }
            }
        }

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public bool IsItemListVisible
        {
            get => isItemListVisible;
            set
            {
                if (isItemListVisible != value)
                {
                    isItemListVisible = value;
                    OnPropertyChanged(nameof(IsItemListVisible));
                }
            }
        }

        public UIElement CurrentContent
        {
            get => currentContent;
            set
            {
                if (currentContent != value)
                {
                    currentContent = value;
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
                foreach (var mode in unitsService.GetAllModes())
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
                logService.LogException(ex, "モード読み込み中にエラーが発生しました");
            }
        }

        private void LoadUnitsForMode(IMode mode)
        {
            if (mode == null) return;

            try
            {
                Units.Clear();
                var units = unitsService.CreateUnits(mode);

                if (units?.Any() == true)
                {
                    var sortedUnits = units.OrderBy(u => u is ISupportUnit ? 1 : 0)
                                         .ThenBy(u => unitsService.GetModeDisplayOrder(u.Mode))
                                         .ThenBy(u => u.Category)
                                         .ThenBy(u => u.Name);

                    foreach (var unit in sortedUnits)
                    {
                        Units.Add(UnitViewModel.Instance(unit));
                    }

                    if (Units.Any())
                    {
                        SelectedUnit = Units.First();
                    }
                }
                else
                {
                    logService.Info($"モード '{mode.Name}' にユニットが見つかりません。Factory初期化が未完了の可能性があります。");
                }
            }
            catch (Exception ex)
            {
                logService.LogException(ex, $"モード '{mode.Name}' のユニット読み込み中にエラーが発生しました");
            }
        }

        private void UpdateCurrentContent()
        {
            if (SelectedUnit != null)
            {
                try
                {
                    CurrentContent = unitViewFactory.Create(SelectedUnit);
                }
                catch (Exception ex)
                {
                    logService.LogException(ex, $"ユニット '{SelectedUnit.Name}' のコンテンツ作成中にエラーが発生しました");
                }
            }
        }

        private Task RefreshUnitsAsync()
        {
            try
            {
                IsLoading = true;
                unitManagementService.RefreshUnits();
                LoadUnitsForMode(SelectedMode);
                logService.Info("ユニットが更新されました");
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "ユニット更新中にエラーが発生しました");
            }
            finally
            {
                IsLoading = false;
            }
            
            return Task.CompletedTask;
        }
        
        private void ExecuteReceiveData(object? sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is UnitsArgs unitsArgs)
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                if (unitsArgs.NeedSetMode)
                {
                    if (SelectedMode != unitsArgs.Mode)
                    {
                        SelectedMode = unitsArgs.Mode;
                    }
                }

                foreach (var unit in unitsArgs.Units)
                {
                        Units.Add(UnitViewModel.Instance(unit));
                }

                Mouse.OverrideCursor = null;
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
                    logService.Info($"ユニットを実行しました: {unitViewModel.Name}");
                }
            }
            catch (Exception ex)
            {
                logService.LogException(ex, $"ユニット実行中にエラーが発生しました: {unitViewModel?.Name}");
            }
        }

        private void ClearFilter()
        {
            FilterText = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
