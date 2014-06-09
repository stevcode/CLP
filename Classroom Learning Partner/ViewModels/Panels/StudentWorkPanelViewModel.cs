using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        public StudentWorkPanelViewModel(Notebook notebook, StagingPanelViewModel stagingPanel)
        {
            Notebook = notebook;
            Initialized += StudentWorkPanelViewModel_Initialized;
            StagingPanel = stagingPanel;

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
            if(CurrentPages.Count > 0)
            {
                FirstPage = CurrentPages[0];
            }
            if(CurrentPages.Count > 1)
            {
                SecondPage = CurrentPages[1];
            }
            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
            ShowSubmissionsCommand = new Command<CLPPage>(OnShowSubmissionsCommandExecute);
            AppendStarredCommand = new Command<CLPPage>(OnAppendStarredCommandExecute);
            AppendCorrectCommand = new Command<CLPPage>(OnAppendCorrectCommandExecute);
            AppendAlmostCorrectCommand = new Command<CLPPage>(OnAppendAlmostCorrectCommandExecute);
            AppendIncorrectCommand = new Command<CLPPage>(OnAppendIncorrectCommandExecute);
            AddPageToStageCommand = new Command<StudentProgressInfo>(OnAddPageToStageCommandExecute);
            StageStudentNotebookCommand = new Command<Person>(OnStageStudentNotebookCommandExecute);
            PageHeightUpdateCommand = new Command(OnPageHeightUpdateCommandExecute);
            BackCommand = new Command(OnBackCommandExecute);
            ForwardCommand = new Command(OnForwardCommandExecute);
        }

        void StudentWorkPanelViewModel_Initialized(object sender, EventArgs e)
        {
            Length = 600; // I want it wider than InitialLength, which is read-only.  Maybe I should be overriding.
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

        /// <summary>
        /// Staging Panel for submissions
        /// </summary>
        public StagingPanelViewModel StagingPanel
        {
            get { return GetValue<StagingPanelViewModel>(StagingPanelProperty); }
            set { SetValue(StagingPanelProperty, value); }
        }

        public static readonly PropertyData StagingPanelProperty = RegisterProperty("StagingPanel", typeof(StagingPanelViewModel)); 

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Sets the current selected page in the listbox.
        /// </summary>
        public Command<CLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(CLPPage page)
        {
            if(page != null)
            {
                //Take thumbnail of page before navigating away from it.
                ACLPPageBaseViewModel.TakePageThumbnail(CurrentPage);
                Notebook.CurrentPage = page;
            }
        }

        /// <summary>
        /// Shows the submissions for the selected page.
        /// </summary>
        public Command<CLPPage> ShowSubmissionsCommand { get; private set; }

        private void OnShowSubmissionsCommandExecute(CLPPage page)
        {
            var stagingPanel = StagingPanel as StagingPanelViewModel;
            if(stagingPanel == null)
            {
                return;
            }

            stagingPanel.IsVisible = true;

            stagingPanel.SetSubmissionsForPage(page);
        }

        /// <summary>
        /// Stages starred submissions for the page.
        /// </summary>
        public Command<CLPPage> AppendStarredCommand { get; private set; }

        private void OnAppendStarredCommandExecute(CLPPage page)
        {
            var stagingPanel = StagingPanel as StagingPanelViewModel;
            if(stagingPanel == null)
            {
                return;
            }

            stagingPanel.IsVisible = true;

            stagingPanel.AppendStarredSubmissionsForPage(page);
        }

        /// <summary>
        /// Stages correct submissions for the page.
        /// </summary>
        public Command<CLPPage> AppendCorrectCommand { get; private set; }

        private void OnAppendCorrectCommandExecute(CLPPage page)
        {
            var stagingPanel = StagingPanel as StagingPanelViewModel;
            if(stagingPanel == null)
            {
                return;
            }

            stagingPanel.IsVisible = true;

            stagingPanel.AppendCollectionOfPagesToStage(page.Submissions, 
                x => x.Tags.FirstOrDefault(t => t is CorrectnessTag && t.Value == CorrectnessTag.AcceptedValues.Correct.ToString()) != null);

            //TODO: keep CurrentSort and skip this if already sorted that way.
            stagingPanel.ApplySortAndGroupByName();
        }

        /// <summary>
        /// Stages almost-correct submissions for the page.
        /// </summary>
        public Command<CLPPage> AppendAlmostCorrectCommand { get; private set; }

        private void OnAppendAlmostCorrectCommandExecute(CLPPage page)
        {
            var stagingPanel = StagingPanel as StagingPanelViewModel;
            if(stagingPanel == null)
            {
                return;
            }

            stagingPanel.IsVisible = true;

            stagingPanel.AppendCollectionOfPagesToStage(page.Submissions, 
                x => x.Tags.FirstOrDefault(t => t is CorrectnessTag && t.Value == CorrectnessTag.AcceptedValues.AlmostCorrect.ToString()) != null);

            //TODO: keep CurrentSort and skip this if already sorted that way.
            stagingPanel.ApplySortAndGroupByName();
        }

        /// <summary>
        /// Stages incorrect submissions for the page.
        /// </summary>
        public Command<CLPPage> AppendIncorrectCommand { get; private set; }

        private void OnAppendIncorrectCommandExecute(CLPPage page)
        {
            var stagingPanel = StagingPanel as StagingPanelViewModel;
            if(stagingPanel == null)
            {
                return;
            }

            stagingPanel.IsVisible = true;

            stagingPanel.AppendCollectionOfPagesToStage(page.Submissions, 
                x => x.Tags.FirstOrDefault(t => t is CorrectnessTag && t.Value == CorrectnessTag.AcceptedValues.Incorrect.ToString()) != null);

            //TODO: keep CurrentSort and skip this if already sorted that way.
            stagingPanel.ApplySortAndGroupByName();
        }

        /// <summary>
        /// Appends submissions for the given student/page combo to the staging panel
        /// </summary>
        public Command<Person> StageStudentNotebookCommand { get; private set; }

        private void OnStageStudentNotebookCommandExecute(Person student)
        {
            var stagingPanel = StagingPanel as StagingPanelViewModel;
            if(stagingPanel == null)
            {
                return;
            }

            stagingPanel.IsVisible = true;

            stagingPanel.SetStudentNotebook(student);
        }

       /// <summary>
        /// Adds individual page to the Staging Panel
        /// </summary>
        public Command<StudentProgressInfo> AddPageToStageCommand { get; private set; }

        private void OnAddPageToStageCommandExecute(StudentProgressInfo info)
        {
            foreach(CLPPage page in info.AllPages) {
                StagingPanel.AddPageToStage(page);
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
            if(SecondPage != null) // don't go forward if there's only one page
            {
                var nextIndex = CurrentPages.IndexOf(SecondPage) + 1;
                if(nextIndex < CurrentPages.Count)
                {
                    FirstPage = SecondPage;
                    SecondPage = CurrentPages[nextIndex];
                }
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