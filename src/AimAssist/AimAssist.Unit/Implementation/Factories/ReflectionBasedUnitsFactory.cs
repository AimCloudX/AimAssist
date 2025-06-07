using AimAssist.Core.Attributes;
using AimAssist.Core.Units;
using System.Reflection;

namespace AimAssist.Units.Implementation.Factories
{
    public class ReflectionBasedUnitsFactory : AbstractUnitsFactory
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Assembly[] assemblies;

        public ReflectionBasedUnitsFactory(IServiceProvider serviceProvider)
            : base("ReflectionBased", priority: 2000)
        {
            this.serviceProvider = serviceProvider;
            this.assemblies = new[]
            {
                Assembly.GetExecutingAssembly(),
                Assembly.GetAssembly(typeof(IUnit)),
                AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "AimAssist.Unit")
            }.Where(a => a != null).ToArray();
        }

        public override IEnumerable<IUnit> CreateUnits()
        {
            System.Diagnostics.Debug.WriteLine("ReflectionBasedUnitsFactory.CreateUnits() started");
            var discoveredUnits = new List<(Type type, AutoRegisterUnitAttribute attr)>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var unitTypes = assembly.GetTypes()
                        .Where(type => typeof(IUnit).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        .Where(type => type.GetCustomAttribute<AutoRegisterUnitAttribute>() != null);

                    foreach (var unitType in unitTypes)
                    {
                        var attr = unitType.GetCustomAttribute<AutoRegisterUnitAttribute>();
                        if (attr.IsEnabled)
                        {
                            discoveredUnits.Add((unitType, attr));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error scanning assembly {assembly.FullName}: {ex.Message}");
                }
            }

            var sortedUnits = discoveredUnits
                .OrderByDescending(x => x.attr.Priority)
                .ThenBy(x => x.attr.Category)
                .ThenBy(x => x.type.Name);

            foreach (var (unitType, attr) in sortedUnits)
            {
                    var unit = CreateUnitInstance(unitType);
                    if (unit != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Auto-registered: {unitType.Name} (Category: {attr.Category}, Priority: {attr.Priority})");
                        yield return unit;
                    }
            }

            System.Diagnostics.Debug.WriteLine($"ReflectionBasedUnitsFactory completed. Discovered: {discoveredUnits.Count} units");
        }

        private IUnit CreateUnitInstance(Type unitType)
        {
            var constructors = unitType.GetConstructors()
                .OrderBy(c => c.GetParameters().Length);

            foreach (var constructor in constructors)
            {
                try
                {
                    var parameters = constructor.GetParameters();
                    if (parameters.Length == 0)
                    {
                        return (IUnit)Activator.CreateInstance(unitType);
                    }

                    var args = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var service = serviceProvider.GetService(parameters[i].ParameterType);
                        if (service == null)
                        {
                            throw new InvalidOperationException($"Required service {parameters[i].ParameterType.Name} not found");
                        }
                        args[i] = service;
                    }

                    return (IUnit)Activator.CreateInstance(unitType, args);
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }
    }
}
