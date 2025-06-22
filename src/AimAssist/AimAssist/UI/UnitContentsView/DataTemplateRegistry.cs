using System.Reflection;
using System.Windows;
using AimAssist.Core.Attributes;
using AimAssist.Core.Units;
using UserControl = System.Windows.Controls.UserControl;

namespace AimAssist.UI.UnitContentsView
{
    public static class DataTemplateRegistry
    {
        private static readonly Dictionary<Type, TemplateInfo> registeredTemplates = new();
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
                    foreach (var unitType in attribute.UnitTypes)
                    {
                        registeredTemplates[unitType] = new TemplateInfo(viewType);
                        System.Diagnostics.Debug.WriteLine($"Registered DataTemplate: {unitType.Name} -> {viewType.Name}");
                    }
                }
            }
        }

        public static UIElement? CreateView(IUnit unit, IServiceProvider? serviceProvider = null)
        {
            var unitType = unit.GetType();
            if (registeredTemplates.TryGetValue(unitType, out var templateInfo))
            {
                try
                {
                    UIElement? view;
                    var useDependencyInjection =
                        templateInfo.ViewType.GetConstructors().Any(c => c.GetParameters().Length != 0);
                    
                    if (useDependencyInjection && serviceProvider != null)
                    {
                        view = CreateInstanceWithDI(unit,templateInfo.ViewType, serviceProvider);
                    }
                    else
                    {
                        view = Activator.CreateInstance(templateInfo.ViewType) as UIElement;
                    }

                    return view ?? throw new InvalidOperationException($"Failed to create view for {unitType.Name}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create view for {unitType.Name}: {ex.Message}");
                }
            }

            return null;
        }

        private static UIElement? CreateInstanceWithDI(IUnit unit, Type viewType, IServiceProvider serviceProvider)
        {
            var constructors = viewType.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length);

            foreach (var constructor in constructors)
            {
                try
                {
                    var parameters = constructor.GetParameters();
                    var args = new object[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameterType = parameters[i].ParameterType;
                        object? service;

                        if (parameterType == typeof(IServiceProvider))
                        {
                            service = serviceProvider;
                        }
                        else if (parameterType == unit.GetType())
                        {
                            args[i] = unit;
                            continue;
                        }
                        else
                        {
                            service = serviceProvider.GetService(parameterType);
                        }

                        if (service == null && !parameters[i].HasDefaultValue)
                        {
                            break;
                        }

                        args[i] = service ?? parameters[i].DefaultValue!;
                    }

                    return (UIElement?)Activator.CreateInstance(viewType, args);
                }
                catch
                {
                    continue;
                }
            }

            return (UIElement?)Activator.CreateInstance(viewType);
        }

        public static bool HasTemplate(Type unitType)
        {
            return registeredTemplates.ContainsKey(unitType);
        }

        public static IReadOnlyDictionary<Type, Type> GetRegisteredTemplates()
        {
            return registeredTemplates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ViewType).AsReadOnly();
        }

        private class TemplateInfo
        {
            public Type ViewType { get; }

            public TemplateInfo(Type viewType)
            {
                ViewType = viewType;
            }
        }
    }
}
