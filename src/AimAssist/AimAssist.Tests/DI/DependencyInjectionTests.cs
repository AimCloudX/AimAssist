using AimAssist.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace AimAssist.Tests.DI
{
    public class DependencyInjectionTests
    {
        /// <summary>
        /// DIコンテナの設定をテストするためのユーティリティメソッドです
        /// </summary>
        /// <returns>設定されたサービスプロバイダーを返します</returns>
        private ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            
            // サービスを登録
            services.AddSingleton<IUnitsService, Service.UnitsService>();
            services.AddSingleton<ICommandService, Core.Commands.CommandService>();
            services.AddSingleton<IApplicationLogService, Service.ApplicationLogService>();
            services.AddSingleton<ISettingManager, Service.SettingManager>();
            services.AddSingleton<IKeySequenceManager, Service.KeySequenceManager>();
            services.AddSingleton<IEditorOptionService, Library.Options.EditorOptionService>();
            services.AddSingleton<IAppCommands, Core.Commands.AppCommands>();
            services.AddSingleton<Service.PickerService>();
            services.AddSingleton<Service.WindowHandleService>();
            
            return services.BuildServiceProvider();
        }

        [Fact]
        public void ResolveIUnitsService_ShouldReturnInstance()
        {
            // Arrange
            var serviceProvider = ConfigureServices();

            // Act
            var service = serviceProvider.GetService<IUnitsService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<Service.UnitsService>(service);
        }

        [Fact]
        public void ResolveICommandService_ShouldReturnInstance()
        {
            // Arrange
            var serviceProvider = ConfigureServices();

            // Act
            var service = serviceProvider.GetService<ICommandService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<Core.Commands.CommandService>(service);
        }

        [Fact]
        public void ResolveIApplicationLogService_ShouldReturnInstance()
        {
            // Arrange
            var serviceProvider = ConfigureServices();

            // Act
            var service = serviceProvider.GetService<IApplicationLogService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<Service.ApplicationLogService>(service);
        }

        [Fact]
        public void ResolveISettingManager_ShouldReturnInstance()
        {
            // Arrange
            var serviceProvider = ConfigureServices();

            // Act
            var service = serviceProvider.GetService<ISettingManager>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<Service.SettingManager>(service);
        }

        [Fact]
        public void ResolveIKeySequenceManager_ShouldReturnInstance()
        {
            // Arrange
            var serviceProvider = ConfigureServices();

            // Act
            var service = serviceProvider.GetService<IKeySequenceManager>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<Service.KeySequenceManager>(service);
        }

        [Fact]
        public void ResolveIEditorOptionService_ShouldReturnInstance()
        {
            // Arrange
            var serviceProvider = ConfigureServices();

            // Act
            var service = serviceProvider.GetService<IEditorOptionService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<Library.Options.EditorOptionService>(service);
        }

        [Fact]
        public void ResolveIAppCommands_ShouldReturnInstance()
        {
            // Arrange
            var serviceProvider = ConfigureServices();

            // Act
            var service = serviceProvider.GetService<IAppCommands>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<Core.Commands.AppCommands>(service);
        }
    }
}
