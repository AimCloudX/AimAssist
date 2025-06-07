using System.Collections.Generic;

namespace AimAssist.Services.Markdown
{
    public static class CategoryOrderManager
    {
        private static readonly Dictionary<string, int> CategoryOrder = new Dictionary<string, int>();
        
        public static void SetCategoryOrder(string category, int order)
        {
            if (!string.IsNullOrEmpty(category))
            {
                CategoryOrder[category] = order;
            }
        }
        
        public static int GetCategoryOrder(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return int.MaxValue;
            }
            
            return CategoryOrder.ContainsKey(category) ? CategoryOrder[category] : int.MaxValue - 1;
        }
        
        public static void ClearCategoryOrder()
        {
            CategoryOrder.Clear();
        }
    }
}
