using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ProgressPanelViewModel : APanelBaseViewModel
    {
        protected readonly IDataService DataService;

        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="ProgressPanelViewModel" /> class.</summary>
        public ProgressPanelViewModel(Notebook notebook, StagingPanelViewModel stagingPanel)
        {
            DataService = DependencyResolver.Resolve<IDataService>();
            StagingPanel = stagingPanel;
            Notebook = notebook;

            RefreshProgressPanelData();
            InitializedAsync += ProgressPanelViewModel_InitializedAsync;

            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
            //OpenNotebookCommand = new Command<NotebookInfo>(OnOpenNotebookCommandExecute);
        }

        public override string Title
        {
            get { return "ProgressPanelVM"; }
        }

        #endregion //Constructor

        #region Model

        /// <summary>Notebook associated with the panel.</summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof (Notebook));

        /// <summary>Current, selected page in the notebook.</summary>
        [ViewModelToModel("Notebook")]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        #endregion //Model

        #region Bindings

        ///// <summary>NotebookInfos for all loaded Teacher Notebooks.</summary>
        //public ObservableCollection<NotebookInfo> TeacherNotebooks
        //{
        //    get { return GetValue<ObservableCollection<NotebookInfo>>(TeacherNotebooksProperty); }
        //    set { SetValue(TeacherNotebooksProperty, value); }
        //}

        //public static readonly PropertyData TeacherNotebooksProperty = RegisterProperty("TeacherNotebooks", typeof (ObservableCollection<NotebookInfo>), () => new ObservableCollection<NotebookInfo>());

        ///// <summary>NotebookInfos for all loaded Student Notebooks.</summary>
        //public ObservableCollection<NotebookInfo> StudentNotebooks
        //{
        //    get { return GetValue<ObservableCollection<NotebookInfo>>(StudentNotebooksProperty); }
        //    set { SetValue(StudentNotebooksProperty, value); }
        //}

        //public static readonly PropertyData StudentNotebooksProperty = RegisterProperty("StudentNotebooks", typeof (ObservableCollection<NotebookInfo>), () => new ObservableCollection<NotebookInfo>());
        
        /// <summary>Staging Panel for submissions</summary>
        public StagingPanelViewModel StagingPanel
        {
            get { return GetValue<StagingPanelViewModel>(StagingPanelProperty); }
            set { SetValue(StagingPanelProperty, value); }
        }

        public static readonly PropertyData StagingPanelProperty = RegisterProperty("StagingPanel", typeof (StagingPanelViewModel));

        #endregion //Bindings

        #region IPanel Override

        /// <summary>Initial Length of the Panel, before any resizing.</summary>
        public override double InitialLength
        {
            get { return 200; }
        }

        #endregion //IPanel Override

        #region Events

        private async Task ProgressPanelViewModel_InitializedAsync(object sender, EventArgs e)
        {
            RefreshProgressPanelData();
            SetPanelWidth();
        }

        #endregion // Events

        #region Methods

        private void SetPanelWidth()
        {
            //if (DataService == null ||
            //    !DataService.LoadedNotebooksInfo.Any())
            //{
            //    Length = InitialLength;
            //    return;
            //}

            //var referenceNotebook = DataService.LoadedNotebooksInfo.FirstOrDefault(ni => !ni.Notebook.Owner.IsStudent);
            //if (referenceNotebook == null)
            //{
            //    referenceNotebook = DataService.LoadedNotebooksInfo.FirstOrDefault();
            //}

            //var pageCount = referenceNotebook.Notebook.Pages.Count;

            //var calculatedWidth = pageCount * 40 + 110;
            //if (Application.Current.MainWindow.ActualWidth < calculatedWidth * 2)
            //{
            //    Length = Application.Current.MainWindow.ActualWidth / 2;
            //}
            //else
            //{
            //    if (calculatedWidth < 200)
            //    {
            //        calculatedWidth = 200;
            //    }
            //    Length = calculatedWidth;
            //}
        }

        private void RefreshProgressPanelData()
        {
            //if (DataService == null ||
            //    !DataService.LoadedNotebooksInfo.Any())
            //{
            //    return;
            //}

            //TeacherNotebooks = new ObservableCollection<NotebookInfo>(DataService.LoadedNotebooksInfo.Where(ni => !ni.Notebook.Owner.IsStudent).OrderBy(ni => ni.Notebook.Owner.DisplayName));
            //StudentNotebooks = new ObservableCollection<NotebookInfo>(DataService.LoadedNotebooksInfo.Where(ni => ni.Notebook.Owner.IsStudent).OrderBy(ni => ni.Notebook.Owner.DisplayName));
        }

        #endregion // Methods

        #region Commands

        /// <summary>Sets the current selected page for the notebook.</summary>
        public Command<CLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(CLPPage page)
        {
            if (page == null ||
                DataService == null)
            {
                return;
            }

            var pageToSwitchTo = page;
            if (page.Owner.IsStudent &&
                page.Submissions.Any())
            {
                pageToSwitchTo = page.Submissions.Last();
            }

            DataService.SetCurrentPage(pageToSwitchTo);
        }

        /// <summary>Switches current notebook to selected notebook.</summary>
        //public Command<NotebookInfo> OpenNotebookCommand { get; private set; }

        //private void OnOpenNotebookCommandExecute(NotebookInfo notebookInfo)
        //{
        //    if (DataService == null)
        //    {
        //        return;
        //    }

        //    if (StagingPanel != null)
        //    {
        //        StagingPanel.IsVisible = false;
        //    }

        //    // TODO: DataService.LoadNotebook (either because it's already open or open it from zip).

        //    App.MainWindowViewModel.MajorRibbon.CurrentLeftPanel = Panels.NotebookPages;
        //}

        #endregion
    }
}