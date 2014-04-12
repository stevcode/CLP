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
            Initialized += ProgressPanelViewModel_Initialized;

            //CurrentPages.Add(Notebook.Pages[2]);
            //CurrentPages.Add(Notebook.Pages[3]);
            //CurrentPages.Add(Notebook.Pages[4]);
            //CurrentPages.Add(Notebook.Pages[5]);
            for(int page = 0; page < Notebook.Pages.Count && page < 10; page++)
            {
                CurrentPages.Add(Notebook.Pages[page]);
            }

            PanelWidth = CurrentPages.Count * 40 + 65;
            StudentList = GetStudentNames();
            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
        }

        void ProgressPanelViewModel_Initialized(object sender, EventArgs e)
        {
            Length = InitialLength;
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

        public ObservableCollection<string> StudentList
        {
            get { return GetValue<ObservableCollection<string>>(StudentListProperty); }
            set { SetValue(StudentListProperty, value); }
        }

        public static readonly PropertyData StudentListProperty = RegisterProperty("StudentList", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

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
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            CurrentPage = page;
            if(notebookWorkspaceViewModel != null)
            {
                notebookWorkspaceViewModel.CurrentDisplay.AddPageToDisplay(page);
            }
        }

        #endregion

        //This is copied over from SubmissionsPanelViewModel, it wants to be 
        //database-agnostic when stuff's finalized
        public ObservableCollection<string> GetStudentNames()
        {
            var userNames = new ObservableCollection<string>();
            var filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\StudentNames.txt";
            userNames.Add("Original");
            if(!File.Exists(filePath))
            {
                return userNames;
            }
            var reader = new StreamReader(filePath);
            string name;
            while((name = reader.ReadLine()) != null)
            {
                var user = name.Split(new[] {','})[0];
                userNames.Add(user);
            }
            reader.Dispose();
            return userNames;
        }
    }
}