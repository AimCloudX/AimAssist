using AimAssist.Core.Interfaces;
using Common.UI.Commands.Shortcus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq;

namespace AimAssist.Services
{
    public interface IConfigurationManagerService
    {
        T GetConfiguration<T>(string section, string key, T defaultValue = default);
        void SetConfiguration<T>(string section, string key, T value);
        void SaveSection(string section);
        void LoadSection(string section);
        void SaveAllSections();
        void LoadAllSections();
        bool ValidateConfiguration<T>(string section, string key, T value);
        IEnumerable<string> GetSectionNames();
    }

    public class ConfigurationManagerService : IConfigurationManagerService
    {
        private readonly IEditorOptionService _editorOptionService;
        private readonly ISnippetOptionService _snippetOptionService;
        private readonly IWorkItemOptionService _workItemOptionService;
        private readonly ISettingManager _settingManager;
        private readonly IApplicationLogService _logService;

        private readonly Dictionary<string, IConfigurationSection> _sections = new();
        private readonly Dictionary<string, object> _configurationCache = new();

        public ConfigurationManagerService(
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
            
            InitializeSections();
        }

        private void InitializeSections()
        {
            _sections["Editor"] = new EditorConfigurationSection(_editorOptionService, _logService);
            _sections["Snippet"] = new SnippetConfigurationSection(_snippetOptionService, _logService);
            _sections["WorkItem"] = new WorkItemConfigurationSection(_workItemOptionService, _logService);
            _sections["Keymap"] = new KeymapConfigurationSection(_settingManager, _logService);
        }

        public T GetConfiguration<T>(string section, string key, T defaultValue = default)
        {
            try
            {
                var cacheKey = $"{section}.{key}";
                
                if (_configurationCache.TryGetValue(cacheKey, out var cachedValue) && cachedValue is T)
                {
                    return (T)cachedValue;
                }

                if (_sections.TryGetValue(section, out var configSection))
                {
                    var value = configSection.GetValue<T>(key, defaultValue);
                    _configurationCache[cacheKey] = value;
                    return value;
                }

                _logService.Warning($"不明なセクション: {section}");
                return defaultValue;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"設定取得中にエラーが発生しました。セクション: {section}, キー: {key}");
                return defaultValue;
            }
        }

        public void SetConfiguration<T>(string section, string key, T value)
        {
            try
            {
                if (!ValidateConfiguration(section, key, value))
                {
                    _logService.Warning($"無効な設定値です。セクション: {section}, キー: {key}, 値: {value}");
                    return;
                }

                if (_sections.TryGetValue(section, out var configSection))
                {
                    configSection.SetValue(key, value);
                    _configurationCache[$"{section}.{key}"] = value;
                    _logService.Info($"設定が更新されました。セクション: {section}, キー: {key}");
                }
                else
                {
                    _logService.Warning($"不明なセクション: {section}");
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"設定更新中にエラーが発生しました。セクション: {section}, キー: {key}");
            }
        }

        public void SaveSection(string section)
        {
            try
            {
                if (_sections.TryGetValue(section, out var configSection))
                {
                    configSection.Save();
                    _logService.Info($"セクション {section} の設定を保存しました");
                }
                else
                {
                    _logService.Warning($"不明なセクション: {section}");
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"セクション {section} の設定保存中にエラーが発生しました");
            }
        }

        public void LoadSection(string section)
        {
            try
            {
                if (_sections.TryGetValue(section, out var configSection))
                {
                    configSection.Load();
                    var keysToRemove = _configurationCache.Keys
                        .Where(k => k.StartsWith($"{section}."))
                        .ToList();
                    
                    foreach (var key in keysToRemove)
                    {
                        _configurationCache.Remove(key);
                    }
                    
                    _logService.Info($"セクション {section} の設定を読み込みました");
                }
                else
                {
                    _logService.Warning($"不明なセクション: {section}");
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, $"セクション {section} の設定読み込み中にエラーが発生しました");
            }
        }

        public void SaveAllSections()
        {
            try
            {
                _logService.Info("全設定の保存を開始します");
                
                foreach (var section in _sections.Values)
                {
                    section.Save();
                }
                
                _logService.Info("全設定の保存が完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "全設定保存中にエラーが発生しました");
            }
        }

        public void LoadAllSections()
        {
            try
            {
                _logService.Info("全設定の読み込みを開始します");
                
                _configurationCache.Clear();
                
                foreach (var section in _sections.Values)
                {
                    section.Load();
                }
                
                _logService.Info("全設定の読み込みが完了しました");
            }
            catch (Exception ex)
            {
                _logService.LogException(ex, "全設定読み込み中にエラーが発生しました");
            }
        }

        public bool ValidateConfiguration<T>(string section, string key, T value)
        {
            if (value == null) return false;

            if (_sections.TryGetValue(section, out var configSection))
            {
                return configSection.ValidateValue(key, value);
            }

            return true;
        }

        public IEnumerable<string> GetSectionNames()
        {
            return _sections.Keys;
        }
    }

    public interface IConfigurationSection
    {
        T GetValue<T>(string key, T defaultValue = default);
        void SetValue<T>(string key, T value);
        void Save();
        void Load();
        bool ValidateValue<T>(string key, T value);
    }

    public class EditorConfigurationSection : IConfigurationSection
    {
        private readonly IEditorOptionService _editorService;
        private readonly IApplicationLogService _logService;

        public EditorConfigurationSection(IEditorOptionService editorService, IApplicationLogService logService)
        {
            _editorService = editorService;
            _logService = logService;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            return defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
        }

        public void Save()
        {
            _editorService.SaveOption();
        }

        public void Load()
        {
        }

        public bool ValidateValue<T>(string key, T value)
        {
            return key switch
            {
                "Theme" => value is string theme && !string.IsNullOrWhiteSpace(theme),
                "FontSize" => value is int fontSize && fontSize > 0 && fontSize <= 72,
                "TabSize" => value is int tabSize && tabSize > 0 && tabSize <= 16,
                _ => true
            };
        }
    }

    public class SnippetConfigurationSection : IConfigurationSection
    {
        private readonly ISnippetOptionService _snippetService;
        private readonly IApplicationLogService _logService;

        public SnippetConfigurationSection(ISnippetOptionService snippetService, IApplicationLogService logService)
        {
            _snippetService = snippetService;
            _logService = logService;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            return defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
        }

        public void Save()
        {
            _snippetService.SaveOption();
        }

        public void Load()
        {
        }

        public bool ValidateValue<T>(string key, T value)
        {
            return key switch
            {
                "Timeout" => value is int timeout && timeout > 0,
                "MaxHistoryCount" => value is int count && count > 0 && count <= 1000,
                _ => true
            };
        }
    }

    public class WorkItemConfigurationSection : IConfigurationSection
    {
        private readonly IWorkItemOptionService _workItemService;
        private readonly IApplicationLogService _logService;

        public WorkItemConfigurationSection(IWorkItemOptionService workItemService, IApplicationLogService logService)
        {
            _workItemService = workItemService;
            _logService = logService;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            return defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
        }

        public void Save()
        {
            _workItemService.SaveOption();
        }

        public void Load()
        {
        }

        public bool ValidateValue<T>(string key, T value)
        {
            return key switch
            {
                "RefreshInterval" => value is int interval && interval > 0,
                "MaxItems" => value is int maxItems && maxItems > 0 && maxItems <= 10000,
                _ => true
            };
        }
    }

    public class KeymapConfigurationSection : IConfigurationSection
    {
        private readonly ISettingManager _settingManager;
        private readonly IApplicationLogService _logService;

        public KeymapConfigurationSection(ISettingManager settingManager, IApplicationLogService logService)
        {
            _settingManager = settingManager;
            _logService = logService;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            var settings = _settingManager.LoadSettings();
            if (settings.TryGetValue(key, out var value))
            {
                if (typeof(T) == typeof(Dictionary<string, KeySequence>))
                {
                    return (T)(object)settings;
                }
                if (value is T typedValue)
                {
                    return typedValue;
                }
            }
            return defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
            if (typeof(T) == typeof(Dictionary<string, KeySequence>) && value is Dictionary<string, KeySequence> keymap)
            {
                _settingManager.SaveSettings(keymap);
            }
        }

        public void Save()
        {
            var settings = _settingManager.LoadSettings();
            var validSettings = settings.Where(x => x.Value.FirstModifiers != 0).ToDictionary(x => x.Key, x => x.Value);
            _settingManager.SaveSettings(validSettings);
        }

        public void Load()
        {
            _settingManager.LoadSettings();
        }

        public bool ValidateValue<T>(string key, T value)
        {
            return value != null;
        }
    }
}
