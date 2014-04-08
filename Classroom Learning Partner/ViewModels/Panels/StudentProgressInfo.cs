using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class StudentProgressInfo : ViewModelBase
    {
        public StudentProgressInfo(string name, ObservableCollection<CLPPage> pages)
        {
            Name = name;
            Pages = pages;
        }

        /// <summary>
        /// The student's name.
        /// </summary>
        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string), "Nobody");

        /// <summary>
        /// The number of pages in the notebook.
        /// </summary>
        public ObservableCollection<CLPPage> Pages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPage>));
    }
}