namespace Library.Rss
{
    public class RssSetting
    {
        public RssSetting(string category, string searchUrl)
        {
            Category = category;
            SearchUrl = searchUrl;
        }

        public bool IsEnabled { get; set; }
        public string SearchUrl { get; set; }
        public string Category { get; set; }

        public CategoryUrl GetCategoryUrl => new CategoryUrl(Category, SearchUrl);
    }
}
