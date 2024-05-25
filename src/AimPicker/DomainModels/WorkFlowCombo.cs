using AimPicker.UI.Combos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimPicker.DomainModels
{
    public class WorkFlowCombo : ICombo
    {
        public string Name { get; }

        public WorkFlowCombo(string name, string description, IPreviewFactory previewFactory)
        {
            Name = name;
            this.Code = description;
            PreviewFactory = previewFactory;
        }

        public string Code { get; }
        public IPreviewFactory PreviewFactory { get; }
    }
}
