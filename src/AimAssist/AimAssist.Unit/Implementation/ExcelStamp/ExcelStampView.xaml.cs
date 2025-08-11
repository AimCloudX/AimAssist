using System.Windows.Controls;
using AimAssist.Core.Attributes;

namespace AimAssist.Units.Implementation.ExcelStamp
{
    
    [AutoDataTemplate(typeof(ExcelStampUnit))]
    public partial class ExcelStampView : UserControl
    {
        public ExcelStampView()
        {
            InitializeComponent();
        }
    }
}