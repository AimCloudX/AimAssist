﻿using AimAssist.Core.Units;
using AimAssist.Units.Implementation;

namespace AimAssist.Plugins
{
    public interface IPluginUnitsFactory
    {
        IEnumerable<IUnit> GetPluginUnits();
        void RegisterPlugin(IUnitPlugin plugin);
        void UnregisterPlugin(string pluginName);
        IEnumerable<string> GetRegisteredPluginNames();
    }

    public class PluginUnitsFactory : AbstractUnitsFactory, IPluginUnitsFactory
    {
        private readonly Dictionary<string, IUnitPlugin> plugins = new();
        private readonly object @lock = new();

        public PluginUnitsFactory() : base("Plugin", priority: 50)
        {
        }

        public override IEnumerable<IUnit> CreateUnits()
        {
            return GetPluginUnits();
        }

        public IEnumerable<IUnit> GetPluginUnits()
        {
            var allUnits = new List<IUnit>();

            lock (@lock)
            {
                foreach (var plugin in plugins.Values)
                {
                    try
                    {
                        if (plugin != null)
                        {
                            var units = plugin.GetUnitsFactory().SelectMany(x=>x.GetUnits());
                            if (units != null)
                            {
                                allUnits.AddRange(units);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error getting units from plugin: {ex.Message}");
                    }
                }
            }

            return allUnits;
        }

        public void RegisterPlugin(IUnitPlugin plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));

            var pluginName = plugin.GetType().Name;

            lock (@lock)
            {
                if (plugins.ContainsKey(pluginName))
                {
                    throw new InvalidOperationException($"Plugin '{pluginName}' is already registered");
                }

                plugins[pluginName] = plugin;
            }
        }

        public void UnregisterPlugin(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName)) throw new ArgumentNullException(nameof(pluginName));

            lock (@lock)
            {
                if (plugins.ContainsKey(pluginName))
                {
                    plugins.Remove(pluginName);
                }
            }
        }

        public IEnumerable<string> GetRegisteredPluginNames()
        {
            lock (@lock)
            {
                return plugins.Keys.ToList();
            }
        }

        public override void Initialize()
        {
            lock (@lock)
            {
                foreach (var plugin in plugins.Values)
                {
                    try
                    {
                        // Initialize plugin if it has an initialization method
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error initializing plugin: {ex.Message}");
                    }
                }
            }
        }

        public override void Dispose()
        {
            lock (@lock)
            {
                foreach (var plugin in plugins.Values)
                {
                    try
                    {
                        if (plugin is IDisposable disposablePlugin)
                        {
                            disposablePlugin.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error disposing plugin: {ex.Message}");
                    }
                }
                plugins.Clear();
            }
        }
    }
}
