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
            //    LinkedPanel = new SubmissionsPanelViewModel(notebook); // TODO: Entities, staging panel
            Length = InitialLength;
            StudentList = GetStudentNames();
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
        [Model]
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

        public static readonly PropertyData CurrentPagesProperty = RegisterProperty("CurrentPages", typeof(ObservableCollection<CLPPage>));

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

        public ObservableCollection<StudentProgressInfo> StudentList
        {
            get { return GetValue<ObservableCollection<StudentProgressInfo>>(StudentListProperty); }
            set { SetValue(StudentListProperty, value); }
        }

        public static readonly PropertyData StudentListProperty = RegisterProperty("StudentList",
                                                                                   typeof(ObservableCollection<StudentProgressInfo>),
                                                                                   () => new ObservableCollection<StudentProgressInfo>());

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

        //This is copied over from SubmissionsPanelViewModel, it wants to be 
        //database-agnostic when stuff's finalized
        public ObservableCollection<StudentProgressInfo> GetStudentNames()
        {
            var userNames = new ObservableCollection<StudentProgressInfo>();
            //userNames.Add(new StudentProgressInfo("Original", Pages));

            var filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\StudentNames.txt";

            if(File.Exists(filePath))
            {
                var reader = new StreamReader(filePath);
                string name;
                while((name = reader.ReadLine()) != null)
                {
                    var user = name.Split(new[] {','})[0];
                    userNames.Add(new StudentProgressInfo(user, CurrentPages));
                }
                reader.Dispose();
            }
            return userNames;
        }
    }
}