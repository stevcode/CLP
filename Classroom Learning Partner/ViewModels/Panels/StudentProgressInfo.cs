using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.ViewModels
{
    public class StudentProgressInfo : ViewModelBase
    {
        public StudentProgressInfo(String name, ObservableCollection<ICLPPage> pages)
        {
            Name = name;
            Pages = pages;
        }

        /// <summary>
        /// The student's name.
        /// </summary>
        public String Name
        {
            get { return GetValue<String>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(String), "Nobody");

        /// <summary>
        /// The number of pages in the notebook.
        /// </summary>
        public ObservableCollection<ICLPPage> Pages
        {
            get { return GetValue<ObservableCollection<ICLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<ICLPPage>));


    }
}
