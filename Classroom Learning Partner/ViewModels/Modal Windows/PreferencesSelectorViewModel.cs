using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using CLP.CustomControls.Button_Controls;
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

        public bool IsVisibleOnStudent
        {
            get { return GetValue<bool>(IsVisibleOnStudentProperty); }
            set { SetValue(IsVisibleOnStudentProperty, value); }
        }

        public static readonly PropertyData IsVisibleOnStudentProperty = RegisterProperty("IsVisibleOnStudent", typeof(bool), true);


        public bool IsVisibleOnProjector
        {
            get { return GetValue<bool>(IsVisibleOnProjectorProperty); }
            set { SetValue(IsVisibleOnProjectorProperty, value); }
        }

        public static readonly PropertyData IsVisibleOnProjectorProperty = RegisterProperty("IsVisibleOnProjector", typeof(bool), true);

    }

    public class PreferencesSelectorViewModel : ViewModelBase
    {

        public PreferencesSelectorViewModel(ObservableCollection<IPreferenceButton> buttonsIn)
        {
            Buttons = buttonsIn;
            
            foreach (IPreferenceButton pb in Buttons)
            {
                PreferenceCheckboxes.Add(new ButtonPreferenceSelector(pb.buttonID));
            }
        }

        private ObservableCollection<IPreferenceButton> buttons;

        public ObservableCollection<IPreferenceButton> Buttons
        {
            get { return buttons; }
            set { buttons = value; }
        }

        public ObservableCollection<ButtonPreferenceSelector> PreferenceCheckboxes
        {
            get { return GetValue<ObservableCollection<ButtonPreferenceSelector>>(PreferenceCheckboxesProperty); }
            set { SetValue(PreferenceCheckboxesProperty, value); }
        }

        public static readonly PropertyData PreferenceCheckboxesProperty = RegisterProperty("PreferenceCheckboxes", typeof(ObservableCollection<ButtonPreferenceSelector>), () => new ObservableCollection<ButtonPreferenceSelector>());

    }
}
