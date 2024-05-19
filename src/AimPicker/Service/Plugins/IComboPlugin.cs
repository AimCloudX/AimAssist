using AimPicker.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimPicker.Service.Plugins
{
    public interface IComboPlugin
    {
        IEnumerable<ICombo> GetCombo();
    }
}
