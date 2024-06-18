using System.Windows;
using System.Windows.Controls;

namespace AimAssist.Unit.Implementation.Knoledges
{
    /// <summary>
    /// KnowledModeDscriptionControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ModeDscriptionControl : UserControl
    {
        public ModeDscriptionControl(string description, UIElement content)
        {
            InitializeComponent();
            this.Description.Text = description;
            this.Contents.Children.Add(content);
        }
    }
}
