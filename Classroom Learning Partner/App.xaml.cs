using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Catel;
using Catel.IO;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Windows.Controls;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Views;

namespace Classroom_Learning_Partner
{
    /// <summary>Interaction logic for App.xaml</summary>
    public partial class App
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            // TODO: Is there a difference between this and AppDomain.CurrentDomain.UnhandledException += blah?
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            base.OnStartup(e);

            var currentProgramMode = ProgramModes.Teacher;

#if TEACHER
            currentProgramMode = ProgramModes.Teacher;
#endif
#if STUDENT
            currentProgramMode = ProgramModes.Student;
#endif
#if PROJECTOR
            currentProgramMode = ProgramModes.Projector;
#endif

            InitializeCatelSettings();
            InitializeServices();
            
            MainWindowViewModel = new MainWindowViewModel(currentProgramMode);
            var window = new MainWindowView
                         {
                             DataContext = MainWindowViewModel
                         };
            MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
            window.Show();

            StartNetwork();
            MainWindowViewModel.SetWorkspace();
        }

#region Static Properties

        public static MainWindowViewModel MainWindowViewModel { get; private set; }

#endregion // Static Properties

#region Static Methods

        private static void InitializeCatelSettings()
        {
            //Preload all assemblies during startup
            var directory = typeof(MainWindowView).Assembly.GetDirectory();
            AppDomain.CurrentDomain.PreloadAssemblies(directory);

            //var fileLogListener = new FileLogListener();
            //fileLogListener.FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "catellog.log");
            //LogManager.AddListener(fileLogListener);
            //LogManager.IsDebugEnabled = true;

            // Uncomment this to enable Catel Logging
            // Comment out to speed up program, all the consoles write are very taxing.
            //LogManager.AddDebugListener();

            // Stops Catel UserControls from searching for InfoBar (not being used for this project, massive time consumer)
            UserControl.DefaultSkipSearchingForInfoBarMessageControlValue = true;
            UserControl.DefaultCreateWarningAndErrorValidatorForViewModelValue = false;

            // Disable Catel Validation
            Catel.Data.ModelBase.SuspendValidationForAllModels = true;

            // Manual Register views to viewModels that don't adhere to standard naming conventions.
            var viewModelLocator = ServiceLocator.Default.ResolveType<IViewModelLocator>();
            viewModelLocator.Register(typeof(ColumnDisplayPreviewView), typeof(ColumnDisplayViewModel));
            viewModelLocator.Register(typeof(GridDisplayPreviewView), typeof(GridDisplayViewModel));
            viewModelLocator.Register(typeof(CLPPagePreviewView), typeof(CLPPageViewModel));
            viewModelLocator.Register(typeof(CLPPageThumbnailView), typeof(CLPPageViewModel));
            viewModelLocator.Register(typeof(CLPPageThumbnailView), typeof(CLPPageViewModel));
            viewModelLocator.Register(typeof(NonAsyncPagePreviewView), typeof(CLPPageViewModel));
            viewModelLocator.Register(typeof(GroupCreationView), typeof(GroupCreationViewModel));
        }

        private static void InitializeServices()
        {
            var dataService = new DataService();
            ServiceLocator.Default.RegisterInstance<IDataService>(dataService);

            var networkService = new NetworkService();
            ServiceLocator.Default.RegisterInstance<INetworkService>(networkService);

            var pageInteractionService = new PageInteractionService();
            ServiceLocator.Default.RegisterInstance<IPageInteractionService>(pageInteractionService);
        }

        private static void StartNetwork()
        {
            var networkService = ServiceLocator.Default.ResolveType<INetworkService>();
            networkService.Connect();
        }

#region Error Handling

        private static void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG // In debug mode do not custom-handle the exception, let Visual Studio handle it
            e.Handled = false;
#else
            ShowUnhandeledException(e);
#endif
        }

        private static void ShowUnhandeledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            var errorMessage =
                string.Format(
                              "An application error occurred.\nPlease check whether your data is correct and repeat the action. " +
                              "If this error occurs again there seems to be a more serious malfunction in the application, and you better " +
                              "close it.\n\nError:{0}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you " +
                              "click No the application will close)",
                              e.Exception.Message + (e.Exception.InnerException != null ? "\n" + e.Exception.InnerException.Message : null));

            Logger.Instance.WriteToLog("[UNHANDLED ERROR] - " + e.Exception.Message + " " +
                                       (e.Exception.InnerException != null ? "\n" + e.Exception.InnerException.Message : null));
            Logger.Instance.WriteToLog("[HResult]: " + e.Exception.HResult);
            Logger.Instance.WriteToLog("[Source]: " + e.Exception.Source);
            Logger.Instance.WriteToLog("[Method]: " + e.Exception.TargetSite);
            Logger.Instance.WriteToLog("[StackTrace]: " + e.Exception.StackTrace);

            if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
            {
                if (
                    MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?",
                                    "Close the application!",
                                    MessageBoxButton.YesNoCancel,
                                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Current.Shutdown();
                }
            }
        }

#endregion // Error Handling

#endregion // Static Methods

#region Old Network Methods

        private static CLPNetwork _network = new CLPNetwork();

        public static CLPNetwork Network
        {
            get { return _network; }
            set { _network = value; }
        }



#endregion // Network Methods
    }
}