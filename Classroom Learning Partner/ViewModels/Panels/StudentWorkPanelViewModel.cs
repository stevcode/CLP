using System;
using System.Collections.ObjectModel;
using System.IO;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
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
                StudentList.Add(Person.TestSubmitter);
            }
            
            foreach(CLPPage page in Notebook.Pages)
            {
                CurrentPages.Add(page);
            }
            FirstPage = CurrentPages[0];
            SecondPage = CurrentPages[1];
        }

        void StudentWorkPanelViewModel_Initialized(object sender, EventArgs e)
        {
            Length = InitialLength;
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

        /// <summary>
        /// Current, selected page in the notebook.
        /// </summary>
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
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel != null)
            {
                notebookWorkspaceViewModel.CurrentDisplay.AddPageToDisplay(page);

                // TODO: Entities, StagingPanel
                //var historyPanel = notebookWorkspaceViewModel.SubmissionHistoryPanel;
                //if(historyPanel != null)
                //{
                //    historyPanel.CurrentPage = null;
                //    historyPanel.IsSubmissionHistoryVisible = false;
                //}
            }
        }

        #endregion

    }
}