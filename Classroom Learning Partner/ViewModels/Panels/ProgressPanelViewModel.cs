using System;
using System.Collections.ObjectModel;
using System.IO;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ProgressPanelViewModel : APanelBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressPanelViewModel" /> class.
        /// </summary>
        public ProgressPanelViewModel(Notebook notebook)
        {
            Notebook = notebook;
            CurrentPages = notebook.Pages;
            Initialized += ProgressPanelViewModel_Initialized;

            if(App.MainWindowViewModel.CurrentClassPeriod != null)
            {
                StudentList = App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.StudentList;
            }
            else
            {
                StudentList = new ObservableCollection<Person>();
                StudentList.Add(Person.TestSubmitter);
            }

            PageSets = new ObservableCollection<string>();
            PageSets.Add("Current Class Period");
            PageSets.Add("Full Notebook");

            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
        }

        void ProgressPanelViewModel_Initialized(object sender, EventArgs e)
        {
            var calculatedWidth = CurrentPages.Count * 40 + 70;
            if(App.Current.MainWindow.ActualWidth < calculatedWidth * 2)
            {
                Length = App.Current.MainWindow.ActualWidth / 2;
            }
            else
            {
                Length = calculatedWidth;
            }
        }

        public override string Title
        {
            get { return "ProgressPanelVM"; }
        }

        #endregion //Constructor

        #region Model
        /// <summary>
        /// Notebook associated with the panel.
        /// </summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof(Notebook));

        /// <summary>
        /// Pages of the Notebook.
        /// </summary>
        public ObservableCollection<CLPPage> CurrentPages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(CurrentPagesProperty); }
            set { SetValue(CurrentPagesProperty, value); }
        }

        public static readonly PropertyData CurrentPagesProperty = RegisterProperty("CurrentPages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        #endregion //Model

        #region Bindings

        /// <summary>
        /// Current, selected page in the notebook.
        /// </summary>
        [ViewModelToModel("Notebook")]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        public ObservableCollection<Person> StudentList
        {
            get { return GetValue<ObservableCollection<Person>>(StudentListProperty); }
            set { SetValue(StudentListProperty, value); }
        }

        public static readonly PropertyData StudentListProperty = RegisterProperty("StudentList", typeof(ObservableCollection<Person>), () => new ObservableCollection<Person>());

        public ObservableCollection<String> PageSets
        {
            get { return GetValue<ObservableCollection<String>>(PageSetsProperty); }
            set { SetValue(PageSetsProperty, value); }
        }

        public static readonly PropertyData PageSetsProperty = RegisterProperty("PageSets", typeof(ObservableCollection<String>), () => new ObservableCollection<String>());


        #endregion //Bindings

        #region IPanel Override

        #region Overrides of APanelBaseViewModel

        /// <summary>
        /// Initial Length of the Panel, before any resizing.
        /// </summary>
        public override double InitialLength
        {
            get { return 400; }
        }

        #endregion

        #endregion //IPanel Override

        #region Commands

        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command<CLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(CLPPage page)
        {
            Notebook.CurrentPage = page;
        }

        #endregion

    }
}