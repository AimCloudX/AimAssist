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
        public MindMeisterMap(string id, string title)
        {
            Id = id;
            Title = title;
        }

        public string Id { get; set; }
        public string Title { get; set; }

        [JsonIgnore]
        public string Url => $"https://www.mindmeister.com/app/map/{Id}";
    }
}
