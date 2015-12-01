using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classroom_Learning_Partner.ViewModels.Modal_Windows
{
    public class PreferencesSelectorViewModel
    {
        private ObservableCollection<string> preferencesCheckboxes;
        public ObservableCollection<string> PreferencesCheckboxes
        { get { return preferencesCheckboxes; } }
    }
}
