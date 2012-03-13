using System.Windows;
using System;
using Classroom_Learning_Partner.ViewModels;
using System.Collections.ObjectModel;
using Classroom_Learning_Partner.Model;
using System.IO;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using MongoDB.Driver;
using System.Threading;
using Classroom_Learning_Partner.Views;
using Catel.Logging;


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
            base.OnStartup(e);

            //Uncomment this to enable Catel Logging
            //LogManager.RegisterDebugListener();
            
            CLPServiceAgent.Instance.Initialize();

            CurrentUserMode = UserMode.Instructor;
            _databaseUse = DatabaseMode.NotUsing;
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
           
            JoinMeshNetwork();
            MainWindowViewModel.SetWorkspace();
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

        protected void ConnectToDB()
        {
            string ConnectionString = "mongodb://18.28.6.168";
            _databaseServer = MongoServer.Create(ConnectionString);
            Console.WriteLine("Connected to DB");
        }

        #endregion //Methods

        #region Properties

        private static MainWindowViewModel _mainWindowViewModel;
        public static MainWindowViewModel MainWindowViewModel
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

        #endregion //Properties
    }
}
