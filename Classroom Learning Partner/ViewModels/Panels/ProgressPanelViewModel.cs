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
        private readonly IDataService _dataService;

        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="ProgressPanelViewModel" /> class.</summary>
        public ProgressPanelViewModel(Notebook notebook, StagingPanelViewModel stagingPanel)
        {
            _dataService = DependencyResolver.Resolve<IDataService>();
            StagingPanel = stagingPanel;
            Notebook = notebook;

            RefreshProgressPanelData();
            InitializedAsync += ProgressPanelViewModel_InitializedAsync;

            SetCurrentPageCommand = new Command<CLPPage>(OnSetCurrentPageCommandExecute);
            OpenNotebookCommand = new Command<Notebook>(OnOpenNotebookCommandExecute);
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

        /// <summary>All loaded Teacher Notebooks.</summary>
        public ObservableCollection<Notebook> TeacherNotebooks
        {
            get { return GetValue<ObservableCollection<Notebook>>(TeacherNotebooksProperty); }
            set { SetValue(TeacherNotebooksProperty, value); }
        }

        public static readonly PropertyData TeacherNotebooksProperty = RegisterProperty("TeacherNotebooks", typeof(ObservableCollection<Notebook>), () => new ObservableCollection<Notebook>());

        /// <summary>All loaded Student Notebooks.</summary>
        public ObservableCollection<Notebook> StudentNotebooks
        {
            get { return GetValue<ObservableCollection<Notebook>>(StudentNotebooksProperty); }
            set { SetValue(StudentNotebooksProperty, value); }
        }

        public static readonly PropertyData StudentNotebooksProperty = RegisterProperty("StudentNotebooks", typeof(ObservableCollection<Notebook>), () => new ObservableCollection<Notebook>());

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
        public override double InitialLength => 200;

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
            if (_dataService == null ||
                !_dataService.LoadedNotebooks.Any())
            {
                Length = InitialLength;
                return;
            }

            var referenceNotebook = _dataService.LoadedNotebooks.FirstOrDefault(n => !n.Owner.IsStudent);
            if (referenceNotebook == null)
            {
                referenceNotebook = _dataService.LoadedNotebooks.FirstOrDefault();
            }

            var pageCount = referenceNotebook.Pages.Count;

            var calculatedWidth = pageCount * 40 + 110;
            if (Application.Current.MainWindow.ActualWidth < calculatedWidth * 2)
            {
                Length = Application.Current.MainWindow.ActualWidth / 2;
            }
            else
            {
                if (calculatedWidth < 200)
                {
                    calculatedWidth = 200;
                }
                Length = calculatedWidth;
            }
        }

        private void RefreshProgressPanelData()
        {
            if (_dataService == null ||
                !_dataService.LoadedNotebooks.Any())
            {
                return;
            }

            TeacherNotebooks = new ObservableCollection<Notebook>(_dataService.LoadedNotebooks.Where(n => !n.Owner.IsStudent).OrderBy(n => n.Owner.DisplayName));
            StudentNotebooks = new ObservableCollection<Notebook>(_dataService.LoadedNotebooks.Where(n => n.Owner.IsStudent).OrderBy(n => n.Owner.DisplayName));
        }

        #endregion // Methods

        #region Commands

        /// <summary>Sets the current selected page for the notebook.</summary>
        public Command<CLPPage> SetCurrentPageCommand { get; private set; }

        private void OnSetCurrentPageCommandExecute(CLPPage page)
        {
            if (page == null ||
                _dataService == null)
            {
                return;
            }

            var pageToSwitchTo = page;
            if (page.Owner.IsStudent &&
                page.Submissions.Any())
            {
                pageToSwitchTo = page.Submissions.Last();
            }

            _dataService.SetCurrentPage(pageToSwitchTo);
        }

        /// <summary>Switches current notebook to selected notebook.</summary>
        public Command<Notebook> OpenNotebookCommand { get; private set; }

        private void OnOpenNotebookCommandExecute(Notebook notebook)
        {
            if (_dataService == null)
            {
                return;
            }

            if (StagingPanel != null)
            {
                StagingPanel.IsVisible = false;
            }

            // TODO: DataService.LoadNotebook (either because it's already open or open it from zip).

            App.MainWindowViewModel.MajorRibbon.CurrentLeftPanel = Panels.NotebookPages;
        }

        #endregion
    }
}