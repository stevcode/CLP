using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System;
using Classroom_Learning_Partner.ViewModels;
using System.Collections.ObjectModel;
using Classroom_Learning_Partner.Model;
using System.IO;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using MongoDB.Driver;
using System.Threading;

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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CurrentUserMode = UserMode.Student;

            MainWindow window = new MainWindow();
            _mainWindowViewModel = new MainViewModel();
            window.DataContext = MainWindowViewModel;
            window.Show();

            _notebookDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks";
            
            MainWindowViewModel.Workspace = new NotebookChooserWorkspaceViewModel();


            DispatcherHelper.Initialize();

            JoinMeshNetwork();
        }

        protected void ConnectToDB()
        {
            string ConnectionString = "mongodb://18.28.5.202";
            _databaseServer = MongoServer.Create(ConnectionString);
        }

        #region Methods

        public void JoinMeshNetwork()
        {
            _peer = new PeerNode();
            _peerThread = new Thread(_peer.Run) { IsBackground = true };
            PeerThread.Start();
        }

        public void LeaveMeshNetwork()
        {
            Peer.Stop();
            PeerThread.Join();
        }

        #endregion //Methods

        #region Properties

        private static MainViewModel _mainWindowViewModel;
        public static MainViewModel MainWindowViewModel
        {
            get
            {
                return _mainWindowViewModel;
            }
        }

        private static string _notebookDirectory;
        public static string NotebookDirectory
        {
            get
            {
                return _notebookDirectory;
            }
        }

        private static ObservableCollection<CLPNotebookViewModel> _notebookViewModels = new ObservableCollection<CLPNotebookViewModel>();
        public static ObservableCollection<CLPNotebookViewModel> NotebookViewModels
        {
            get
            {
                return _notebookViewModels;
            }
        }

        //make this send message?
        private static CLPNotebookViewModel _currentNotebookViewModel;
        public static CLPNotebookViewModel CurrentNotebookViewModel
        {
            get
            {
                return _currentNotebookViewModel;
            }
            set
            {
                _currentNotebookViewModel = value;
            }
        }

        private static bool _isAuthoring = false;
        public static bool IsAuthoring
        {
            get
            {
                return _isAuthoring;
            }
            set
            {
                _isAuthoring = value;
            }
        }

        private static UserMode _currentUserMode;
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

        private static PeerNode _peer;
        public static PeerNode Peer
        {
            get
            {
                return _peer;
            }
        }

        private static Thread _peerThread;
        public static Thread PeerThread
        {
            get
            {
                return _peerThread;
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



        #endregion //Properties
    }
}
