using AimAssist.Core.Interfaces;
using Common.UI.Commands.Shortcus;

namespace AimAssist.Services
{
    public interface IConfigurationManagerService
    {
        T? GetConfiguration<T>(string section, string key, T? defaultValue = default);
        void SetConfiguration<T>(string section, string key, T? value);
        void SaveSection(string section);
        void LoadSection(string section);
        void SaveAllSections();
        void LoadAllSections();
        bool ValidateConfiguration<T>(string section, string key, T value);
        IEnumerable<string> GetSectionNames();
    }

    public class ConfigurationManagerService : IConfigurationManagerService
    {
        private readonly IEditorOptionService editorOptionService;
        private readonly ISnippetOptionService snippetOptionService;
        private readonly IWorkItemOptionService workItemOptionService;
        private readonly ISettingManager settingManager;
        private readonly IApplicationLogService logService;

        private readonly Dictionary<string, IConfigurationSection> sections = new();
        private readonly Dictionary<string, object?> configurationCache = new();

        public ConfigurationManagerService(
            IEditorOptionService editorOptionService,
            ISnippetOptionService snippetOptionService,
            IWorkItemOptionService workItemOptionService,
            ISettingManager settingManager,
            IApplicationLogService logService)
        {
            this.editorOptionService = editorOptionService;
            this.snippetOptionService = snippetOptionService;
            this.workItemOptionService = workItemOptionService;
            this.settingManager = settingManager;
            this.logService = logService;
            
            InitializeSections();
        }

        private void InitializeSections()
        {
            sections["Editor"] = new EditorConfigurationSection(editorOptionService);
            sections["Snippet"] = new SnippetConfigurationSection(snippetOptionService);
            sections["WorkItem"] = new WorkItemConfigurationSection(workItemOptionService);
            sections["Keymap"] = new KeymapConfigurationSection(settingManager);
        }

        public T? GetConfiguration<T>(string section, string key, T? defaultValue = default)
        {
            try
            {
                var cacheKey = $"{section}.{key}";
                
                if (configurationCache.TryGetValue(cacheKey, out var cachedValue) && cachedValue is T)
                {
                    return (T)cachedValue;
                }

                if (sections.TryGetValue(section, out var configSection))
                {
                    if (defaultValue != null)
                    {
                        var value = configSection.GetValue(key, defaultValue);
                        configurationCache[cacheKey] = value;
                        return value;
                    }
                }

                logService.Warning($"不明なセクション: {section}");
                return defaultValue;
            }
            catch (Exception ex)
            {
                logService.LogException(ex, $"設定取得中にエラーが発生しました。セクション: {section}, キー: {key}");
                return defaultValue;
            }
        }

        public void SetConfiguration<T>(string section, string key, T? value)
        {
            try
            {
                if (!ValidateConfiguration(section, key, value))
                {
                    logService.Warning($"無効な設定値です。セクション: {section}, キー: {key}, 値: {value}");
                    return;
                }

                if (sections.TryGetValue(section, out var configSection))
                {
                    configSection.SetValue(key, value);
                    configurationCache[$"{section}.{key}"] = value;
                    logService.Info($"設定が更新されました。セクション: {section}, キー: {key}");
                }
                else
                {
                    logService.Warning($"不明なセクション: {section}");
                }
            }
            catch (Exception ex)
            {
                logService.LogException(ex, $"設定更新中にエラーが発生しました。セクション: {section}, キー: {key}");
            }
        }

        public void SaveSection(string section)
        {
            try
            {
                if (sections.TryGetValue(section, out var configSection))
                {
                    configSection.Save();
                    logService.Info($"セクション {section} の設定を保存しました");
                }
                else
                {
                    logService.Warning($"不明なセクション: {section}");
                }
            }
            catch (Exception ex)
            {
                logService.LogException(ex, $"セクション {section} の設定保存中にエラーが発生しました");
            }
        }

        public void LoadSection(string section)
        {
            try
            {
                if (sections.TryGetValue(section, out var configSection))
                {
                    configSection.Load();
                    var keysToRemove = configurationCache.Keys
                        .Where(k => k.StartsWith($"{section}."))
                        .ToList();
                    
                    foreach (var key in keysToRemove)
                    {
                        configurationCache.Remove(key);
                    }
                    
                    logService.Info($"セクション {section} の設定を読み込みました");
                }
                else
                {
                    logService.Warning($"不明なセクション: {section}");
                }
            }
            catch (Exception ex)
            {
                logService.LogException(ex, $"セクション {section} の設定読み込み中にエラーが発生しました");
            }
        }

        public void SaveAllSections()
        {
            try
            {
                logService.Info("全設定の保存を開始します");
                
                foreach (var section in sections.Values)
                {
                    section.Save();
                }
                
                logService.Info("全設定の保存が完了しました");
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "全設定保存中にエラーが発生しました");
            }
        }

        public void LoadAllSections()
        {
            try
            {
                logService.Info("全設定の読み込みを開始します");
                
                configurationCache.Clear();
                
                foreach (var section in sections.Values)
                {
                    section.Load();
                }
                
                logService.Info("全設定の読み込みが完了しました");
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "全設定読み込み中にエラーが発生しました");
            }
        }

        public bool ValidateConfiguration<T>(string section, string key, T value)
        {
            if (value == null) return false;

            if (sections.TryGetValue(section, out var configSection))
            {
                return configSection.ValidateValue(key, value);
            }

            return true;
        }

        public IEnumerable<string> GetSectionNames()
        {
            return sections.Keys;
        }
    }

    public interface IConfigurationSection
    {
        T? GetValue<T>(string key, T? defaultValue = default);
        void SetValue<T>(string key, T value);
        void Save();
        void Load();
        bool ValidateValue<T>(string key, T value);
    }

    public class EditorConfigurationSection(IEditorOptionService editorService)
        : IConfigurationSection
    {
        public T? GetValue<T>(string key, T? defaultValue = default)
        {
            return defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
        }

        public void Save()
        {
            editorService.SaveOption();
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
        private readonly ISnippetOptionService snippetService;

        public SnippetConfigurationSection(ISnippetOptionService snippetService)
        {
            this.snippetService = snippetService;
        }

        public T? GetValue<T>(string key, T? defaultValue = default)
        {
            return defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
        }

        public void Save()
        {
            snippetService.SaveOption();
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

    public class WorkItemConfigurationSection(IWorkItemOptionService workItemService) : IConfigurationSection
    {
        public T? GetValue<T>(string key, T? defaultValue = default)
        {
            return defaultValue;
        }

        public void SetValue<T>(string key, T value)
        {
        }

        public void Save()
        {
            workItemService.SaveOption();
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
        private readonly ISettingManager settingManager;

        public KeymapConfigurationSection(ISettingManager settingManager)
        {
            this.settingManager = settingManager;
        }

        public T? GetValue<T>(string key, T? defaultValue = default)
        {
            var settings = settingManager.LoadSettings();
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
                settingManager.SaveSettings(keymap);
            }
        }

        public void Save()
        {
            var settings = settingManager.LoadSettings();
            var validSettings = settings.Where(x => x.Value.FirstModifiers != 0).ToDictionary(x => x.Key, x => x.Value);
            settingManager.SaveSettings(validSettings);
        }

        public void Load()
        {
            settingManager.LoadSettings();
        }

        public bool ValidateValue<T>(string key, T value)
        {
            return value != null;
        }
    }
}
