﻿using AimAssist.Core.Units;

namespace AimAssist.Units.Implementation.Web.Rss
{
    public class RssItemUnit : IUnit
    {
        public RssItemUnit(string titile,string category, string searchUrl)
        {
            Category = category;
            SearchUrl = searchUrl;
            Name = titile;
        }

        public bool IsEnabled { get; set; }
        public string SearchUrl { get; set; }
        public string Category { get; set; }

        public CategoryUrl GetCategoryUrl => new CategoryUrl(Category, SearchUrl);

        public IMode Mode => RssMode.Instance;

        public string Name { get; }

        public string Description =>string.Empty;
    }
}
