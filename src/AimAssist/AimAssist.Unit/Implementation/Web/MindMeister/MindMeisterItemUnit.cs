using AimAssist.Core.Units;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using Library.Rss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Units.Implementation.Web.MindMeister
{
    public class MindMeisterItemUnit : IUnit
    {
        public MindMeisterItemUnit(string title, string searchUrl)
        {
            SearchUrl = searchUrl;
            Name = title;
        }

        public bool IsEnabled { get; set; }
        public string SearchUrl { get; set; }

        public IMode Mode => MindMeisterMode.Instance;

        public string Name { get; }

        public string Description => string.Empty;

        public string Category => string.Empty;
    }
}
