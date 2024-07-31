using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Units.Implementation.WorkTools
{
    public class ConfigModel
    {
        [JsonProperty("workItemPaths")]
        public List<WorkItemPath> ItemPaths { get; set; }
    }

    public class WorkItemPath
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        public string GetActualPath()
        {
            var actualPath = Path.Replace("{{appdata}}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), StringComparison.OrdinalIgnoreCase);
            return actualPath;
        }
    }
}
