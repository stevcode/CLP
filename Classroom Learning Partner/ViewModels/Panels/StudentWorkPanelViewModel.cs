using System;
using System.Collections.ObjectModel;
using System.IO;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    [InterestedIn(typeof(HoverBoxViewModel))]
    public class StudentWorkPanelViewModel : APanelBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressPanelViewModel" /> class.
        /// </summary>
        public StudentWorkPanelViewModel(Notebook notebook)
        {
            Notebook = notebook;
            Initialized += StudentWorkPanelViewModel_Initialized;
            //    LinkedPanel = new SubmissionsPanelViewModel(notebook); // TODO: Entities, staging panel
            
            // TODO: DATABASE - inject IPersonService to grab student names
            if(App.MainWindowViewModel.CurrentClassPeriod != null)
            {
                StudentList = App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.StudentList;
            }
            else
            {
                StudentList = new ObservableCollection<Person>();
                for(int i = 1; i <= 10; i++)
                {
                    StudentList.Add(Person.TestSubmitter);
                }
            }
            
            foreach(CLPPage page in Notebook.Pages)
            {
                CurrentPages.Add(page);
            }
            FirstPage = CurrentPages[0];
            SecondPage = CurrentPages[1];

            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
            PageHeightUpdateCommand = new Command(OnPageHeightUpdateCommandExecute);
            BackCommand = new Command(OnBackCommandExecute);
            ForwardCommand = new Command(OnForwardCommandExecute);
        }

        void StudentWorkPanelViewModel_Initialized(object sender, EventArgs e)
        {
            Length = InitialLength;
            OnPageHeightUpdateCommandExecute();
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title
        {
            get { return "StudentWorkPanelVM"; }
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
            private set { SetValue(NotebookProperty, value); }
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

        public double PageHeight
        {
            get { return GetValue<double>(PageHeightProperty); }
            set { SetValue(PageHeightProperty, value); }
        }

        public static readonly PropertyData PageHeightProperty = RegisterProperty("PageHeight", typeof(double));

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

        /// <summary>
        /// First of the two pages displayed.
        /// </summary>
        public CLPPage FirstPage
        {
            get { return GetValue<CLPPage>(FirstPageProperty); }
            set { SetValue(FirstPageProperty, value); }
        }

        public static readonly PropertyData FirstPageProperty = RegisterProperty("FirstPage", typeof(CLPPage));
        
        /// <summary>
        /// Second of the two pages displayed.
        /// </summary>
        public CLPPage SecondPage
        {
            get { return GetValue<CLPPage>(SecondPageProperty); }
            set { SetValue(SecondPageProperty, value); }
        }

        public static readonly PropertyData SecondPageProperty = RegisterProperty("SecondPage", typeof(CLPPage));

        public ObservableCollection<Person> StudentList
        {
            get { return GetValue<ObservableCollection<Person>>(StudentListProperty); }
            set { SetValue(StudentListProperty, value); }
        }

        public static readonly PropertyData StudentListProperty = RegisterProperty("StudentList",
                                                                                   typeof(ObservableCollection<Person>),
                                                                                   () => new ObservableCollection<Person>());

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command<CLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(CLPPage page)
        {
            Logger.Instance.WriteToLog("Set current page");
            //TODO staging panel
            if(page != null)
            {
                Notebook.CurrentPage = page;
            }
        }

        /// <summary>
        /// Updates page height
        /// </summary>
        public Command PageHeightUpdateCommand
        {
            get;
            private set;
        }

        private void OnPageHeightUpdateCommandExecute()
        {
            PageHeight = ((Length - 100) / 2 - 6) * CLPPage.LANDSCAPE_HEIGHT / CLPPage.LANDSCAPE_WIDTH;
        }

        /// <summary>
        /// Navigates to the next page.
        /// </summary>
        public Command ForwardCommand { get; private set; }

        private void OnForwardCommandExecute()
        {
            var nextIndex = CurrentPages.IndexOf(SecondPage) + 1;
            if(nextIndex < CurrentPages.Count)
            {
                FirstPage = SecondPage;
                SecondPage = CurrentPages[nextIndex];
            }
        }

        /// <summary>
        /// Navigates to the previous page.
        /// </summary>
        public Command BackCommand { get; private set; }

        private void OnBackCommandExecute()
        {
            var prevIndex = CurrentPages.IndexOf(FirstPage) - 1;
            if(prevIndex >= 0)
            {
                SecondPage = FirstPage;
                FirstPage = CurrentPages[prevIndex];
            }
        }

        #endregion

    }
}