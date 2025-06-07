using AimAssist.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AimAssist.Core.Model;

namespace AimAssist.Units.Implementation.WorkTools
{
    public class WorkItemOptionService : IWorkItemOptionService
    {
        private ConfigModel option;
        private FileSystemWatcher? watcher;

        public ConfigModel Option => option;

        public string OptionPath => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AimAssist", "workitem.option.json");

        public WorkItemOptionService()
        {
            option = ConfigModel.Default();
            LoadOption();
        }

        public void LoadOption()
        {
            if (File.Exists(OptionPath))
            {
                try 
                {
                    var text = File.ReadAllText(OptionPath);
                    var loadedOption = JsonConvert.DeserializeObject<ConfigModel>(text);
                    if (loadedOption == null)
                    {
                        loadedOption = ConfigModel.Default();
                    }

                    option = loadedOption;
                }
                catch(Exception)
                {
                    option = ConfigModel.Default();
                }
            }
            else
            {
                var newOption = ConfigModel.Default();
                option = newOption;
                SaveOption();
            }

            SetupFileWatcher();
        }

        private void SetupFileWatcher()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= OnChanged;
                watcher.Dispose();
            }

            var directoryPath = Path.GetDirectoryName(OptionPath);
            if (directoryPath != null)
            {
                watcher = new FileSystemWatcher(directoryPath);
                watcher.NotifyFilter = NotifyFilters.FileName
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.LastAccess
                                     | NotifyFilters.CreationTime;

                watcher.Filter = Path.GetFileName(OptionPath);
                watcher.Changed += OnChanged;
                watcher.EnableRaisingEvents = true;
            }
        }

        private void OnChanged(object? source, FileSystemEventArgs e)
        {
            LoadOption();
        }

        public void SaveOption()
        {
            var text = JsonConvert.SerializeObject(option, Formatting.Indented);
            File.WriteAllText(OptionPath, text);
        }
    }
}
