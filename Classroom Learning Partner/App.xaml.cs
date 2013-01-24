using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Views;
using MongoDB.Driver;
using ProtoBuf.Meta;
using Catel.Logging;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public enum UserMode
        {
            Server,
            Instructor,
            Projector,
            Student
        }

        public enum DatabaseMode
        {
            Using,
            NotUsing
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            base.OnStartup(e);

            //Uncomment this to enable Catel Logging
            //Comment out to speed up program, all the consoles write are very taxing.
            //LogManager.RegisterDebugListener();

            //Stops Catel UserControls from searching for InfoBar (not being used for this project, massive time consumer)
            Catel.Windows.Controls.UserControl.DefaultSkipSearchingForInfoBarMessageControlValue = true;

            _currentUserMode = UserMode.Instructor;
            _databaseUse = DatabaseMode.Using;

            Classroom_Learning_Partner.Logger.Instance.InitializeLog();
            Classroom_Learning_Partner.Model.CLPServiceAgent.Instance.Initialize();
            
            if (_databaseUse == DatabaseMode.Using && App.CurrentUserMode == UserMode.Server) 
            {
                ConnectToDB();
            }

            MainWindowView window = new MainWindowView();
            _mainWindowViewModel = new MainWindowViewModel();
            window.DataContext = MainWindowViewModel;
            window.Show();
            MainWindowViewModel.SelectedWorkspace = new BlankWorkspaceViewModel();

            _notebookDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks";

            _peer = new Classroom_Learning_Partner.Model.PeerNode();
            CLPServiceAgent.Instance.NetworkSetup();
            //JoinMeshNetwork();
            //ProtoBufferSetup();
            MainWindowViewModel.SetWorkspace();
        }

        #region Methods

        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG   // In debug mode do not custom-handle the exception, let Visual Studio handle it
            e.Handled = false;
#else
            ShowUnhandeledException(e);
#endif
        }

        void ShowUnhandeledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            string errorMessage = string.Format("An application error occurred.\nPlease check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it.\n\nError:{0}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you click No the application will close)",

            e.Exception.Message + (e.Exception.InnerException != null ? "\n" +
            e.Exception.InnerException.Message : null));

            Classroom_Learning_Partner.Logger.Instance.WriteToLog("[UNHANDLED ERROR] - " + e.Exception.Message + " " + (e.Exception.InnerException != null ? "\n" + e.Exception.InnerException.Message : null));

            if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
            {
                if (MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        public void JoinMeshNetwork()
        {
            _peer = new Classroom_Learning_Partner.Model.PeerNode();
            _peerThread = new Thread(_peer.Run) { IsBackground = true };
            PeerThread.Start();
        }

        public void LeaveMeshNetwork()
        {
            Peer.Stop();
            PeerThread.Join();
        }

        protected void ConnectToDB()
        {
            string ConnectionString = "mongodb://localhost/?connect=direct;slaveok=true";
            _databaseServer = MongoServer.Create(ConnectionString);
            Console.WriteLine("Connected to DB");
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


        private static MainWindowViewModel _mainWindowViewModel;
        public static MainWindowViewModel MainWindowViewModel
        {
            get
            {
                return _mainWindowViewModel;
            }
        }

        //TODO: Steve - Make this a String Resource
        private static string _notebookDirectory;
        public static string NotebookDirectory
        {
            get
            {
                return _notebookDirectory;
            }
        }

        private static UserMode _currentUserMode = UserMode.Projector;
        public static UserMode CurrentUserMode
        {
            get
            {
                return _currentUserMode;
            }
            set
            {
                _currentUserMode = value;
            }
        }

        #region Stuff To Delete

        //delete
        private static Classroom_Learning_Partner.Model.PeerNode _peer;
        public static Classroom_Learning_Partner.Model.PeerNode Peer
        {
            get
            {
                return _peer;
            }
        }


        //delete
        private static Thread _peerThread;
        public static Thread PeerThread
        {
            get
            {
                return _peerThread;
            }
        }

        private static DatabaseMode _databaseUse;
        public static DatabaseMode DatabaseUse
        {
            get
            {
                return _databaseUse;
            }
        }

        private static MongoServer _databaseServer;
        public static MongoServer DatabaseServer
        {
            get
            {
                return _databaseServer;
            }
        }

        private static RuntimeTypeModel _pageTypeModel;
        public static RuntimeTypeModel PageTypeModel
        {
            get
            {
                return _pageTypeModel;
            }
        }

        #endregion //Stuff To Delete

        #endregion //Properties
    }
}

