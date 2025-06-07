using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using AimAssist.Core.Attributes;
using UserControl = System.Windows.Controls.UserControl;

namespace AimAssist.UI.UnitContentsView
{
    public static class DataTemplateRegistry
    {
        private static readonly Dictionary<Type, Type> registeredTemplates = new();
        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (isInitialized) return;

            RegisterDataTemplatesFromAssemblies();
            isInitialized = true;
        }

        private static void RegisterDataTemplatesFromAssemblies()
        {
            var assemblies = new[]
            {
                Assembly.GetExecutingAssembly(),
                Assembly.LoadFrom("AimAssist.Units.dll")
            };

            foreach (var assembly in assemblies)
            {
                try
                {
                    RegisterDataTemplatesFromAssembly(assembly);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to register DataTemplates from {assembly.FullName}: {ex.Message}");
                }
            }
        }

        private static void RegisterDataTemplatesFromAssembly(Assembly assembly)
        {
            var typesWithAttribute = assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract)
                .Where(type => typeof(UserControl).IsAssignableFrom(type))
                .Where(type => type.GetCustomAttribute<AutoDataTemplateAttribute>() != null);

            foreach (var viewType in typesWithAttribute)
            {
                var attribute = viewType.GetCustomAttribute<AutoDataTemplateAttribute>();
                if (attribute != null)
                {
                    registeredTemplates[attribute.UnitType] = viewType;
                    System.Diagnostics.Debug.WriteLine($"Registered DataTemplate: {attribute.UnitType.Name} -> {viewType.Name}");
                }
            }
        }

        public static UIElement CreateView(Type unitType)
        {
            if (registeredTemplates.TryGetValue(unitType, out var viewType))
            {
                try
                {
                    return Activator.CreateInstance(viewType) as UIElement;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create view for {unitType.Name}: {ex.Message}");
                }
            }

            return null;
        }

        public static bool HasTemplate(Type unitType)
        {
            return registeredTemplates.ContainsKey(unitType);
        }

        public static IReadOnlyDictionary<Type, Type> GetRegisteredTemplates()
        {
            return registeredTemplates.AsReadOnly();
        }
    }
}
