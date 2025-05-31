using AimAssist.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace AimAssist.Services
{
    public interface IConfigurationService
    {
        T GetConfiguration<T>(string key, T defaultValue = default);
        void SetConfiguration<T>(string key, T value);
        void SaveAllConfigurations();
        void LoadAllConfigurations();
        bool ValidateConfiguration<T>(string key, T value);
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly IEditorOptionService _editorOptionService;
        private readonly ISnippetOptionService _snippetOptionService;
        private readonly IWorkItemOptionService _workItemOptionService;
        private readonly ISettingManager _settingManager;
        private readonly IApplicationLogService _logService;

        private readonly Dictionary<string, object> _configurationCache = new();

        public ConfigurationService(
            IEditorOptionService editorOptionService,
            ISnippetOptionService snippetOptionService,
            IWorkItemOptionService workItemOptionService,
            ISettingManager settingManager,
            IApplicationLogService logService)
        {
            _editorOptionService = editorOptionService;
            _snippetOptionService = snippetOptionService;
            _workItemOptionService = workItemOptionService;
            _settingManager = settingManager;
            _logService = logService;
        }

        public T GetConfiguration<T>(string key, T defaultValue = default)
        {
            try
            {
                if (_configurationCache.TryGetValue(key, out var cachedValue) && cachedValue is T)
                {
                    return (T)cachedValue;
                }

                var value = GetConfigurationFromSource<T>(key) ?? defaultValue;
                _configurationCache[key] = value;
                return value;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"設定取得中にエラーが発生しました。キー: {key}");
                return defaultValue;
            }
        }

        public void SetConfiguration<T>(string key, T value)
        {
            try
            {
                if (!ValidateConfiguration(key, value))
                {
                    _logService.Warning($"無効な設定値が設定されようとしました。キー: {key}, 値: {value}");
                    return;
                }

                _configurationCache[key] = value;
                SetConfigurationToSource(key, value);
                _logService.Info($"設定が更新されました。キー: {key}");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"設定更新中にエラーが発生しました。キー: {key}");
            }
        }

        public void SaveAllConfigurations()
        {
            try
            {
                _logService.Info("全設定の保存を開始します");
                _editorOptionService.SaveOption();
                _snippetOptionService.SaveOption();
                _workItemOptionService.SaveOption();
                _logService.Info("全設定の保存が完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "設定保存中にエラーが発生しました");
            }
        }

        public void LoadAllConfigurations()
        {
            try
            {
                _logService.Info("全設定の読み込みを開始します");
                _configurationCache.Clear();
                _logService.Info("全設定の読み込みが完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "設定読み込み中にエラーが発生しました");
            }
        }

        public bool ValidateConfiguration<T>(string key, T value)
        {
            if (value == null) return false;
            
            return key switch
            {
                "EditorTheme" => value is string theme && !string.IsNullOrWhiteSpace(theme),
                "SnippetTimeout" => value is int timeout && timeout > 0,
                "WorkItemRefreshInterval" => value is int interval && interval > 0,
                _ => true
            };
        }

        private T GetConfigurationFromSource<T>(string key)
        {
            return key switch
            {
                var k when k.StartsWith("Editor") => (T)GetEditorConfiguration(k),
                var k when k.StartsWith("Snippet") => (T)GetSnippetConfiguration(k),
                var k when k.StartsWith("WorkItem") => (T)GetWorkItemConfiguration(k),
                _ => default
            };
        }

        private void SetConfigurationToSource<T>(string key, T value)
        {
            switch (key)
            {
                case var k when k.StartsWith("Editor"):
                    SetEditorConfiguration(k, value);
                    break;
                case var k when k.StartsWith("Snippet"):
                    SetSnippetConfiguration(k, value);
                    break;
                case var k when k.StartsWith("WorkItem"):
                    SetWorkItemConfiguration(k, value);
                    break;
            }
        }

        private object GetEditorConfiguration(string key)
        {
            return null;
        }

        private object GetSnippetConfiguration(string key)
        {
            return null;
        }

        private object GetWorkItemConfiguration(string key)
        {
            return null;
        }

        private void SetEditorConfiguration<T>(string key, T value)
        {
        }

        private void SetSnippetConfiguration<T>(string key, T value)
        {
        }

        private void SetWorkItemConfiguration<T>(string key, T value)
        {
        }
    }
}
