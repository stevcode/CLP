using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using Catel.Windows;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class OpenNotebookPaneViewModel : APaneBaseViewModel
    {
        #region Constructor

        public OpenNotebookPaneViewModel()
        {
            InitializeCommands();
            AvailableCaches.AddRange(DataService.AvailableCaches);
            SelectedCache = AvailableCaches.FirstOrDefault();
        }

        private void InitializeCommands()
        {
            OpenNotebookCommand = new Command(OnOpenNotebookCommandExecute, OnOpenNotebookCanExecute);
            OpenPageRangeCommand = new Command(OnOpenPageRangeCommandExecute, OnOpenNotebookCanExecute);
            StartClassPeriodCommand = new Command(OnStartClassPeriodCommandExecute);
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public override string PaneTitleText
        {
            get { return "Open Notebook"; }
        }

        #region Cache Bindings

        /// <summary>List of available Caches.</summary>
        public ObservableCollection<CacheInfo> AvailableCaches
        {
            get { return GetValue<ObservableCollection<CacheInfo>>(AvailableCachesProperty); }
            set { SetValue(AvailableCachesProperty, value); }
        }

        public static readonly PropertyData AvailableCachesProperty = RegisterProperty("AvailableCaches", typeof (ObservableCollection<CacheInfo>), () => new ObservableCollection<CacheInfo>());

        /// <summary>Selected Cache.</summary>
        public CacheInfo SelectedCache
        {
            get { return GetValue<CacheInfo>(SelectedCacheProperty); }
            set { SetValue(SelectedCacheProperty, value); }
        }

        public static readonly PropertyData SelectedCacheProperty = RegisterProperty("SelectedCache", typeof (CacheInfo), null, OnSelectedCacheChanged);

        private static void OnSelectedCacheChanged(object sender, AdvancedPropertyChangedEventArgs args)
        {
            var openNotebookPaneViewModel = sender as OpenNotebookPaneViewModel;
            if (openNotebookPaneViewModel == null ||
                openNotebookPaneViewModel.SelectedCache == null)
            {
                return;
            }

            openNotebookPaneViewModel.DataService.CurrentCacheInfo = openNotebookPaneViewModel.SelectedCache;
            openNotebookPaneViewModel.AvailableNotebooks.Clear();
            openNotebookPaneViewModel.AvailableNotebooks.AddRange(Services.DataService.GetNotebooksInCache(openNotebookPaneViewModel.SelectedCache));
            openNotebookPaneViewModel.SelectedNotebook = openNotebookPaneViewModel.AvailableNotebooks.FirstOrDefault();
        }

        #endregion //Cache Bindings

        #region Notebook Bindings

        /// <summary>Available notebooks in the currently selected Cache.</summary>
        public ObservableCollection<NotebookInfo> AvailableNotebooks
        {
            get { return GetValue<ObservableCollection<NotebookInfo>>(AvailableNotebooksProperty); }
            set { SetValue(AvailableNotebooksProperty, value); }
        }

        public static readonly PropertyData AvailableNotebooksProperty = RegisterProperty("AvailableNotebooks",
                                                                                          typeof (ObservableCollection<NotebookInfo>),
                                                                                          () => new ObservableCollection<NotebookInfo>());

        /// <summary>Currently selected Notebook.</summary>
        public NotebookInfo SelectedNotebook
        {
            get { return GetValue<NotebookInfo>(SelectedNotebookProperty); }
            set { SetValue(SelectedNotebookProperty, value); }
        }

        public static readonly PropertyData SelectedNotebookProperty = RegisterProperty("SelectedNotebook", typeof (NotebookInfo));

        /// <summary>Toggles the loading of submissions when opening a notebook.</summary>
        public bool IsIncludeSubmissionsChecked
        {
            get { return GetValue<bool>(IsIncludeSubmissionsCheckedProperty); }
            set { SetValue(IsIncludeSubmissionsCheckedProperty, value); }
        }

        public static readonly PropertyData IsIncludeSubmissionsCheckedProperty = RegisterProperty("IsIncludeSubmissionsChecked", typeof (bool), true);

        #endregion //Notebook Bindings

        #endregion //Bindings

        #region Commands

        /// <summary>Opens selected notebook.</summary>
        public Command OpenNotebookCommand { get; private set; }

        private void OnOpenNotebookCommandExecute()
        {
            PleaseWaitHelper.Show(() => DataService.OpenNotebook(SelectedNotebook), null, "Loading Notebook");
            var pageIDs = Services.DataService.GetAllPageIDsInNotebook(SelectedNotebook);
            PleaseWaitHelper.Show(() =>
                                  {
                                      DataService.LoadPages(SelectedNotebook, pageIDs, true);
                                      DataService.LoadLocalSubmissions(SelectedNotebook, pageIDs, true);
                                      if (App.MainWindowViewModel.CurrentProgramMode == ProgramModes.Teacher && IsIncludeSubmissionsChecked)
                                      {
                                          Parallel.ForEach(AvailableNotebooks,
                                                           notebookInfo =>
                                                           {
                                                               if (notebookInfo.NameComposite.OwnerTypeTag == "A" ||
                                                                   notebookInfo.NameComposite.OwnerTypeTag == "T")
                                                               {
                                                                   return;
                                                               }

                                                               DataService.OpenNotebook(notebookInfo, false, false);
                                                               DataService.LoadPages(notebookInfo, pageIDs, true);
                                                               DataService.LoadLocalSubmissions(notebookInfo, pageIDs, true);
                                                           });
                                      }
                                  },
                                  null,
                                  "Loading Pages");

            //if (App.Network.InstructorProxy == null)
            //{
            //    return;
            //}

            //var connectionString = App.Network.InstructorProxy.StudentLogin(App.MainWindowViewModel.CurrentUser.FullName,
            //                                         App.MainWindowViewModel.CurrentUser.ID,
            //                                         App.Network.CurrentMachineName,
            //                                         App.Network.CurrentMachineAddress);

            //if (connectionString == "connected")
            //{
            //    App.MainWindowViewModel.MajorRibbon.ConnectionStatus = ConnectionStatuses.LoggedIn;
            //}
        }

        /// <summary>Opens a range of pages in a notebook.</summary>
        public Command OpenPageRangeCommand { get; private set; }

        private void OnOpenPageRangeCommandExecute()
        {
            var textInputViewModel = new TextInputViewModel();
            var textInputView = new TextInputView(textInputViewModel);
            textInputView.ShowDialog();

            if (textInputView.DialogResult == null ||
                textInputView.DialogResult != true ||
                string.IsNullOrEmpty(textInputViewModel.InputText))
            {
                return;
            }

            var pageNumbersToOpen = RangeHelper.ParseStringToIntNumbers(textInputViewModel.InputText);
            if (!pageNumbersToOpen.Any())
            {
                return;
            }

            PleaseWaitHelper.Show(() => DataService.OpenNotebook(SelectedNotebook), null, "Loading Notebook");
            var pageIDs = Services.DataService.GetPageIDsFromPageNumbers(SelectedNotebook, pageNumbersToOpen);
            PleaseWaitHelper.Show(() =>
                                  {
                                      DataService.LoadPages(SelectedNotebook, pageIDs, false);
                                      DataService.LoadLocalSubmissions(SelectedNotebook, pageIDs, false);
                                  },
                                  null,
                                  "Loading Pages");
        }

        private bool OnOpenNotebookCanExecute() { return SelectedNotebook != null; }

        /// <summary>Starts the closest <see cref="ClassPeriod" />.</summary>
        public Command StartClassPeriodCommand { get; private set; }

        private void OnStartClassPeriodCommandExecute()
        {
            //LoadedNotebookService.StartSoonestClassPeriod(SelectedCacheDirectory);
            //    LoadedNotebookService.StartLocalClassPeriod(, SelectedCacheDirectory);
        }

        #endregion //Commands
    }
}