using Newtonsoft.Json;

namespace AimAssist.Core.Model
{
    public class ConfigModel(List<WorkItemPath> itemPaths)
    {
        [JsonProperty("workItemPaths")]
        public List<WorkItemPath> ItemPaths { get; set; } = itemPaths;

        public static ConfigModel Default()
        {
            return new ConfigModel([]);
        }
    }

    public class WorkItemPath(string path)
    {
        [JsonProperty("path")]
        public string Path { get; set; } = path;

        public string GetActualPath()
        {
            var actualPath = Path.Replace("{{appdata}}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), StringComparison.OrdinalIgnoreCase);
            return actualPath;
        }
    }
}
