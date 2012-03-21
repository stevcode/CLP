using System.Windows;
using System;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Collections.ObjectModel;
using Classroom_Learning_Partner.Model;
using System.IO;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using MongoDB.Driver;
using System.Threading;
using Classroom_Learning_Partner.Views;
using Catel.Logging;
using ProtoBuf;
using ProtoBuf.Meta;


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

            CurrentUserMode = UserMode.Student;
            _databaseUse = DatabaseMode.NotUsing;

            Logger.Instance.InitializeLog();
            CLPServiceAgent.Instance.Initialize();
            
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
            ProtoBufferSetup();
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
            string ConnectionString = "mongodb://localhost/?conenct=direct;slaveok=true";
            _databaseServer = MongoServer.Create(ConnectionString);
            Console.WriteLine("Connected to DB");
        }

        protected void ProtoBufferSetup()
        {
            var model = TypeModel.Create();
            model[typeof(CLPPageObjectBase)]
                .Add(1, "PageID")
                .Add(2, "ParentID")
                .Add(3, "CreationDate")
                .Add(4, "UniqueID")
                .Add(5, "PageObjectStrokes")
                .Add(6, "CanAcceptStrokes")
                .Add(7, "Height")
                .Add(8, "Width")
                .Add(9, "xPos")
                .Add(10, "yPos")
                .Add(11, "PageObjectType")
                .AddSubType(15, typeof(CLPStamp));
            model[typeof(CLPPage)]
                .Add(1, "ParentNotebookID")
                .Add(2, "Strokes")
                .Add(3, "PageObjects")
                .Add(4, "PageHistory")
                .Add(5, "IsSubmission")
                .Add(6, "UniqueID")
                .Add(7, "PageIndex")
                .Add(8, "PageTopics")
                .Add(9, "CreationDate")
                .Add(10, "SubmissionID")
                .Add(11, "SubmitterName")
                .Add(12, "SubmissionTime");
               // .AddSubType(2, typeof(SomeDerived))
               // .AddSubType(3, typeof(AnotherDerived));
            model[typeof(CLPStamp)].Add(1, "InternalPageObject");
           // model[typeof(AnotherDerived)].Add(1, "C");
            //model[typeof(AlsoNotInvolved)].Add(1, "E");
            _pageTypeModel = model;
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

        private static RuntimeTypeModel _pageTypeModel;
        public static RuntimeTypeModel PageTypeModel
        {
            get
            {
                return _pageTypeModel;
            }
        }

        #endregion //Properties
    }
}
