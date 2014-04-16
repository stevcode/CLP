using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Catel.Logging;
using Catel.Reflection;
using Catel.Runtime.Serialization;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Views;
using CLP.Entities;
using Path = Catel.IO.Path;

namespace Classroom_Learning_Partner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public enum UserMode
        {
            Server,
            Instructor,
            Projector,
            Student
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            base.OnStartup(e);

            InitializeCatelSettings();
            InitializeLocalCache();

            Logger.Instance.InitializeLog();
            CLPServiceAgent.Instance.Initialize();

            _currentUserMode = UserMode.Projector;

            MainWindowViewModel = new MainWindowViewModel();
            var window = new MainWindowView {DataContext = MainWindowViewModel};
            MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
            window.Show();

            CLPServiceAgent.Instance.NetworkSetup();
            MainWindowViewModel.SetWorkspace();
        }

        private static void InitializeCatelSettings()
        { 
            //Preload all assemblies during startup
            var directory = typeof(MainWindowView).Assembly.GetDirectory();
            AppDomain.CurrentDomain.PreloadAssemblies(directory);

            //Uncomment this to enable Catel Logging
            //Comment out to speed up program, all the consoles write are very taxing.
            //LogManager.RegisterDebugListener();

            //Stops Catel UserControls from searching for InfoBar (not being used for this project, massive time consumer)
            Catel.Windows.Controls.UserControl.DefaultSkipSearchingForInfoBarMessageControlValue = true;
            Catel.Windows.Controls.UserControl.DefaultCreateWarningAndErrorValidatorForViewModelValue = false;

            //Warm up Serializer to make loading of notebook faster.
            var typesToWarmup = new[] {  typeof(Notebook) };
            var xmlSerializer = SerializationFactory.GetXmlSerializer();
            xmlSerializer.Warmup(typesToWarmup);
        }

        private static void InitializeLocalCache()
        {
            LocalCacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "LocalCache");
            if(!Directory.Exists(LocalCacheDirectory))
            {
                Directory.CreateDirectory(LocalCacheDirectory);
            }

            NotebookCacheDirectory = Path.Combine(LocalCacheDirectory, "Notebooks");
            if(!Directory.Exists(NotebookCacheDirectory))
            {
                Directory.CreateDirectory(NotebookCacheDirectory);
            }

            ClassCacheDirectory = Path.Combine(LocalCacheDirectory, "Classes");
            if(!Directory.Exists(ClassCacheDirectory))
            {
                Directory.CreateDirectory(ClassCacheDirectory);
            }
        }

        #region Methods

        static void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG   // In debug mode do not custom-handle the exception, let Visual Studio handle it
            e.Handled = false;
#else
            ShowUnhandeledException(e);
#endif
        }

        static void ShowUnhandeledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            var errorMessage =
                string.Format(
                    "An application error occurred.\nPlease check whether your data is correct and repeat the action. " +
                    "If this error occurs again there seems to be a more serious malfunction in the application, and you better " +
                    "close it.\n\nError:{0}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you " +
                    "click No the application will close)",

                    e.Exception.Message + (e.Exception.InnerException != null ? "\n" + e.Exception.InnerException.Message : null));

            Logger.Instance.WriteToLog("[UNHANDLED ERROR] - " + e.Exception.Message + " " + (e.Exception.InnerException != null ? "\n" + e.Exception.InnerException.Message : null));
            Logger.Instance.WriteToLog("[HResult]: " + e.Exception.HResult);
            Logger.Instance.WriteToLog("[Source]: " + e.Exception.Source);
            Logger.Instance.WriteToLog("[Method]: " + e.Exception.TargetSite);
            Logger.Instance.WriteToLog("[StackTrace]: " + e.Exception.StackTrace);

            if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
            {
                if (MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Current.Shutdown();
                }
            }
        }

        #endregion //Methods

        #region Properties

        private static CLPNetwork _network = new CLPNetwork();
        public static CLPNetwork Network
        {
            get
            {
                return _network;
            }
            set
            {
                _network = value;
            }
        }

        public static MainWindowViewModel MainWindowViewModel { get; private set; }

        public static string LocalCacheDirectory { get; private set; }
        public static string NotebookCacheDirectory { get; private set; }

        public static string ClassCacheDirectory { get; private set; }

        private static UserMode _currentUserMode = UserMode.Projector;
        public static UserMode CurrentUserMode
        {
            get { return _currentUserMode; }
            set { _currentUserMode = value; }
        }

        #endregion //Properties
    }
}

