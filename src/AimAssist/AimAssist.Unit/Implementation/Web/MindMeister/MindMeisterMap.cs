using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AimAssist.Units.Implementation.Web.MindMeister
{
    public class MindMeisterMap
    {
        public MindMeisterMap(string id, string title, string updatedTime)
        {
            Id = id;
            Title = title;
            UpdatedTime = updatedTime;
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string UpdatedTime { get; }

        [JsonIgnore]
        public string Url => $"https://www.mindmeister.com/app/map/{Id}";
    }
}
