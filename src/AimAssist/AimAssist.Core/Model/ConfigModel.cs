using Newtonsoft.Json;

namespace AimAssist.Core.Model
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
