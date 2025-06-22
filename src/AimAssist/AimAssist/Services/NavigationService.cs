using AimAssist.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using AimAssist.UI.HotKeys;

namespace AimAssist.Services
{
    public interface INavigationService
    {
        void ShowMainWindow();
        void HideMainWindow();
        void ToggleMainWindow();
        void ShowHotKeysWindow();
        void ShowPickerWindow();
        bool IsMainWindowVisible { get; }
    }

    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationLogService _logService;
        private AimAssist.UI.MainWindows.MainWindow? _mainWindow;

        public NavigationService(
            IServiceProvider serviceProvider,
            IApplicationLogService logService)
        {
            _serviceProvider = serviceProvider;
            _logService = logService;
        }

        public bool IsMainWindowVisible => _mainWindow?.IsVisible ?? false;

        public void ShowMainWindow()
        {
            try
            {
                if (_mainWindow == null)
                {
                    _mainWindow = _serviceProvider.GetRequiredService<AimAssist.UI.MainWindows.MainWindow>();
                }

                if (!_mainWindow.IsVisible)
                {
                    _mainWindow.Show();
                    _mainWindow.Activate();
                    _logService.Info("メインウィンドウを表示しました");
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "メインウィンドウ表示中にエラーが発生しました");
            }
        }

        public void HideMainWindow()
        {
            try
            {
                if (_mainWindow?.IsVisible == true)
                {
                    _mainWindow.Hide();
                    _logService.Info("メインウィンドウを非表示にしました");
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "メインウィンドウ非表示中にエラーが発生しました");
            }
        }

        public void ToggleMainWindow()
        {
            try
            {
                if (IsMainWindowVisible)
                {
                    HideMainWindow();
                }
                else
                {
                    ShowMainWindow();
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "メインウィンドウ切り替え中にエラーが発生しました");
            }
        }

        public void ShowHotKeysWindow()
        {
            try
            {
                var hotKeysWindow = _serviceProvider.GetRequiredService<WaitHotKeysWindow>();
                if (!hotKeysWindow.IsVisible)
                {
                    hotKeysWindow.Show();
                    hotKeysWindow.Activate();
                    _logService.Info("ホットキーウィンドウを表示しました");
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "ホットキーウィンドウ表示中にエラーが発生しました");
            }
        }

        public void ShowPickerWindow()
        {
            try
            {
                var pickerService = _serviceProvider.GetRequiredService<IPickerService>();
                pickerService.ShowPicker();
                _logService.Info("ピッカーウィンドウを表示しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "ピッカーウィンドウ表示中にエラーが発生しました");
            }
        }
    }
}
