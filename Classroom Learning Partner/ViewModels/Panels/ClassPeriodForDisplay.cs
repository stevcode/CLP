using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities.Old;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ClassPeriodForDisplay : ViewModelBase
    {
        public ClassPeriodForDisplay(ClassPeriod data, bool showing)
        {
            Showing = showing;
            Data = data;
        }

        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>
        /// Whether the class period is showing
        /// </summary>
        public ClassPeriod Data
        {
            get { return GetValue<ClassPeriod>(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly PropertyData DataProperty = RegisterProperty("Data", typeof(ClassPeriod));

        /// <summary>
        /// Whether the class period is showing
        /// </summary>
        public bool Showing
        {
            get { return GetValue<bool>(ShowingProperty); }
            set { SetValue(ShowingProperty, value); }
        }

        public static readonly PropertyData ShowingProperty = RegisterProperty("Showing", typeof(bool));

        /// <summary>
        /// The date of the class period
        /// </summary>
        public string DateString
        {
            get { return Data.StartTime.ToShortDateString(); }
            set { }
        }

        public static readonly PropertyData DateStringProperty = RegisterProperty("DateString", typeof(string));
    }
}
