using AimAssist.Core.Interfaces;
using AimAssist.Service;
using Common.Commands.Shortcus;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace AimAssist.Tests.Services
{
    public class SettingManagerTests
    {
        private readonly string _testSettingsFilePath;
        private readonly ISettingManager _settingManager;

        public SettingManagerTests()
        {
            // テスト用の一時ファイルパスを設定
            _testSettingsFilePath = Path.Combine(Path.GetTempPath(), "AimAssistTest", "KeyboardSettings.json");
            
            // テスト用の設定マネージャーを作成
            _settingManager = new SettingManager();
            
            // テスト用ディレクトリが存在しない場合は作成
            Directory.CreateDirectory(Path.GetDirectoryName(_testSettingsFilePath));
            
            // テスト用ファイルが存在する場合は削除
            if (File.Exists(_testSettingsFilePath))
            {
                File.Delete(_testSettingsFilePath);
            }
        }

        [Fact]
        public void SaveAndLoadSettings_ShouldPreserveData()
        {
            // Arrange
            var settings = new Dictionary<string, KeySequence>
            {
                { "Test1", new KeySequence(System.Windows.Input.Key.A, System.Windows.Input.ModifierKeys.Control) },
                { "Test2", new KeySequence(System.Windows.Input.Key.B, System.Windows.Input.ModifierKeys.Alt) }
            };

            // Act
            _settingManager.SaveSettings(settings);
            var loadedSettings = _settingManager.LoadSettings();

            // Assert
            Assert.NotNull(loadedSettings);
            Assert.Equal(2, loadedSettings.Count);
            Assert.True(loadedSettings.ContainsKey("Test1"));
            Assert.True(loadedSettings.ContainsKey("Test2"));
            Assert.Equal(System.Windows.Input.Key.A, loadedSettings["Test1"].FirstKey);
            Assert.Equal(System.Windows.Input.ModifierKeys.Control, loadedSettings["Test1"].FirstModifiers);
            Assert.Equal(System.Windows.Input.Key.B, loadedSettings["Test2"].FirstKey);
            Assert.Equal(System.Windows.Input.ModifierKeys.Alt, loadedSettings["Test2"].FirstModifiers);
        }

        [Fact]
        public void LoadSettings_WhenFileDoesNotExist_ShouldReturnEmptyDictionary()
        {
            // Arrange
            if (File.Exists(_testSettingsFilePath))
            {
                File.Delete(_testSettingsFilePath);
            }

            // Act
            var loadedSettings = _settingManager.LoadSettings();

            // Assert
            Assert.NotNull(loadedSettings);
            Assert.Empty(loadedSettings);
        }

        [Fact]
        public void SaveSettings_ShouldCreateFile()
        {
            // Arrange
            var settings = new Dictionary<string, KeySequence>
            {
                { "Test", new KeySequence(System.Windows.Input.Key.C, System.Windows.Input.ModifierKeys.Shift) }
            };

            // 事前にファイルが存在する場合は削除
            if (File.Exists(_testSettingsFilePath))
            {
                File.Delete(_testSettingsFilePath);
            }

            // Act
            _settingManager.SaveSettings(settings);

            // Assert
            Assert.True(File.Exists(_testSettingsFilePath));
        }
    }
}
