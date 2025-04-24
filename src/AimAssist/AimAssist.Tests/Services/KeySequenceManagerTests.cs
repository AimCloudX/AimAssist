using AimAssist.Core.Commands;
using AimAssist.Core.Interfaces;
using AimAssist.Service;
using Common.Commands;
using Common.Commands.Shortcus;
using Moq;
using System.Windows;
using System.Windows.Input;
using Xunit;

namespace AimAssist.Tests.Services
{
    public class KeySequenceManagerTests
    {
        private readonly Mock<ICommandService> _commandServiceMock;
        private readonly IKeySequenceManager _keySequenceManager;
        private readonly Mock<Window> _windowMock;

        public KeySequenceManagerTests()
        {
            _commandServiceMock = new Mock<ICommandService>();
            _keySequenceManager = new KeySequenceManager(_commandServiceMock.Object);
            _windowMock = new Mock<Window>();
        }

        [Fact]
        public void HandleKeyPress_WithSingleKeyCommand_ShouldExecuteCommand()
        {
            // Arrange
            bool commandExecuted = false;
            var command = new RelayCommand("TestCommand", _ => { commandExecuted = true; });
            var keyGesture = new KeyGesture(Key.A, ModifierKeys.Control);
            
            _commandServiceMock.Setup(x => x.TryGetFirstOnlyKeyCommand(It.IsAny<KeyGesture>(), out command))
                .Returns(true);

            // Act
            var result = _keySequenceManager.HandleKeyPress(Key.A, ModifierKeys.Control, _windowMock.Object);

            // Assert
            Assert.True(result);
            Assert.True(commandExecuted);
        }

        [Fact]
        public void HandleKeyPress_WithKeySequence_ShouldExecuteCommand()
        {
            // Arrange
            bool commandExecuted = false;
            var command = new RelayCommand("TestCommand", _ => { commandExecuted = true; });
            
            // 最初のキーでは、コマンドが見つからないように設定
            RelayCommand outCommand = null;
            _commandServiceMock.Setup(x => x.TryGetFirstOnlyKeyCommand(It.IsAny<KeyGesture>(), out outCommand))
                .Returns(false);
            
            // HandleKeyPressを2回呼び出すので、時間を制御するためにテスト内でプロパティを使用
            var firstPress = _keySequenceManager.HandleKeyPress(Key.A, ModifierKeys.Control, _windowMock.Object);
            
            // 2つ目のキー入力でコマンドが見つかるように設定
            _commandServiceMock.Setup(x => x.TryGetFirstSecontKeyCommand(It.IsAny<KeySequence>(), out command))
                .Returns(true);

            // Act - 2つ目のキーを押す（500ミリ秒以内に）
            var secondPress = _keySequenceManager.HandleKeyPress(Key.B, ModifierKeys.None, _windowMock.Object);

            // Assert
            Assert.False(firstPress); // 最初のキーではコマンドを実行しない
            Assert.True(secondPress); // 2つ目のキーでコマンドを実行する
            Assert.True(commandExecuted); // コマンドが実行された
        }

        [Fact]
        public void HandleKeyPress_WithModifierKeyOnly_ShouldReturnFalse()
        {
            // Act
            var result = _keySequenceManager.HandleKeyPress(Key.LeftCtrl, ModifierKeys.Control, _windowMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HandleKeyPress_WithNoModifiers_ShouldReturnFalse()
        {
            // Act
            var result = _keySequenceManager.HandleKeyPress(Key.A, ModifierKeys.None, _windowMock.Object);

            // Assert
            Assert.False(result);
        }
    }
}
