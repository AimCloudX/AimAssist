﻿namespace AimAssist.Core.Rss
{
    public class CategoryUrl
    {
        public CategoryUrl(string category, string url)
        {
            Category = category;
            Url = url;
        }

        public string Category { get; }
        public string Url { get; }
    }
}