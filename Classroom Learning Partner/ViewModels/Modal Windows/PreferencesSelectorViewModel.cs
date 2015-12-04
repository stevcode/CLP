using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{

    public class ButtonPreferenceSelector : AEntityBase
    {
        public ButtonPreferenceSelector(string id)
        {
            ID = id;
        }
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        public bool IsVisibleOnTeacher
        {
            get { return GetValue<bool>(IsVisibleOnTeacherProperty); }
            set { SetValue(IsVisibleOnTeacherProperty, value); }
        }

        public static readonly PropertyData IsVisibleOnTeacherProperty = RegisterProperty("IsVisibleOnTeacher", typeof(bool), true);

    }

    public class PreferencesSelectorViewModel : ViewModelBase
    {

        public PreferencesSelectorViewModel()
        {
            PreferenceCheckboxes.Add(new ButtonPreferenceSelector("insertArrayButton"));
            PreferenceCheckboxes.Add(new ButtonPreferenceSelector("insertPileButton"));
            PreferenceCheckboxes.First().IsVisibleOnTeacher = false;
        }

        public ObservableCollection<ButtonPreferenceSelector> PreferenceCheckboxes
        {
            get { return GetValue<ObservableCollection<ButtonPreferenceSelector>>(PreferenceCheckboxesProperty); }
            set { SetValue(PreferenceCheckboxesProperty, value); }
        }

        public static readonly PropertyData PreferenceCheckboxesProperty = RegisterProperty("PreferenceCheckboxes", typeof(ObservableCollection<ButtonPreferenceSelector>), () => new ObservableCollection<ButtonPreferenceSelector>());

    }
}
