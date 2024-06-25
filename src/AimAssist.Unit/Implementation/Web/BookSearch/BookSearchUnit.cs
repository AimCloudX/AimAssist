using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Implementation.Web.BookSearch
{
    public class BookSearchUnit : IUnit
    {
        public BitmapImage Icon => new BitmapImage();

        public IMode Mode => BookSearchMode.Instance;

        public string Category => string.Empty;

        public string Name => "BookSearchSettings";

        public string Text => "キーワード検索して、Amazonのページを表示";

        public UIElement GetUiElement()
        {
            return new BookSearchControl();
        }
    }
}
