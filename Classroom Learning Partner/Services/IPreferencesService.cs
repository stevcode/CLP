using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLP.CustomControls;

namespace Classroom_Learning_Partner.Services
{
    //serialize this to save preferences to xml
    public interface IPreferencesService
    {

        //does order of buttons indicate their layout order? Or do we need to store that? 
        void addPreference(string ID, PreferencesService.prefType type);
        void removePreference(string ID, PreferencesService.prefType type);

    }
}
