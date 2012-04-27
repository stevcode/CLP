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
            //Comment out to speed up program, all the consoles write are very taxing.
            //LogManager.RegisterDebugListener();

            CurrentUserMode = UserMode.Instructor;
            _databaseUse = DatabaseMode.Using;

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
            string ConnectionString = "mongodb://jessmilmbp.local/?connect=direct;slaveok=true";
            _databaseServer = MongoServer.Create(ConnectionString);
            Console.WriteLine("Connected to DB");
        }

        protected void ProtoBufferSetup()
        {
            var model = RuntimeTypeModel.Default;
            //var model = TypeModel.Create();
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
                .Add(12, "SubmissionTime")
                .Add(17, "PageObjectsSer");


            model[typeof(CLPPage)][17].AsReference = true;
            model[typeof(CLPPage)][17].OverwriteList = true;
           

            
            //Page Object hierarchy 
            model[typeof(ICLPPageObject)]
                .Add(1, "PageID")
                .Add(2, "ParentID")
                .Add(3, "CreationDate")
                .Add(4, "UniqueID")
                .Add(5, "PageObjectStrokes")
                .Add(6, "CanAcceptStrokes")
                .Add(7, "Height")
                .Add(8, "Width")
                .Add(9, "XPosition")
                .Add(10, "YPosition")
                .AddSubType(15, typeof(CLPPageObjectBase))
                .AddSubType(16, typeof(CLPStamp));
            model[typeof(CLPPageObjectBase)]
                .AddSubType(7, typeof(CLPImage))
                .AddSubType(8, typeof(CLPInkRegion))
                .AddSubType(9, typeof(CLPShape))
                .AddSubType(10, typeof(CLPSnapTileContainer))
                .AddSubType(11, typeof(CLPStrokePathContainer))
                .AddSubType(12, typeof(CLPTextBox))
                .AddSubType(13, typeof(CLPAudio));
            model[typeof(CLPStamp)]
                .Add(1, "StrokePathContainer");
            model[typeof(CLPImage)]
                .Add(1, "ByteSource");

            model[typeof(CLPInkRegion)]
                .AddSubType(1, typeof(CLPInkShapeRegion))
                .AddSubType(2, typeof(CLPHandwritingRegion))
                .AddSubType(3, typeof(CLPDataTable))
                .AddSubType(4, typeof(CLPShadingRegion));
            model[typeof(CLPHandwritingRegion)]
                .Add(1, "AnalysisType")
                .Add(2, "StoredAnswer");    
            model[typeof(CLPDataTable)]
                .Add(1, "DataValues")
                .Add(2, "Rows")
                .Add(3, "Cols")
                .Add(4, "AnalysisType");
            model[typeof(CLPShadingRegion)]
                .Add(1, "PercentFilled")
                .Add(2, "Rows")
                .Add(3, "Cols");
            model[typeof(CLPInkShapeRegion)]
                .Add(1, "InkShapesString")
                .Add(2, "InkShapes");
            model[typeof(CLPNamedInkSet)]
                .Add(1, "InkShapeStrokes")
                .Add(2, "InkShapeType");
            model[typeof(CLPShape)].Add(1, "ShapeType");
            model[typeof(CLPSnapTileContainer)].Add(1, "NumberOfTiles");
            model[typeof(CLPStrokePathContainer)].Add(1, "InternalPageObject");
            model[typeof(CLPTextBox)].Add(1, "Text");
            model[typeof(CLPAudio)].Add(1, "ID")
                .Add(2, "File");
            //Page History
            model[typeof(CLPHistory)]
                .Add(1, "IgnoreHistory")
                .Add(2, "HistoryItems")
                .Add(3, "UndoneHistoryItems")
                .Add(4, "TrashedPageObjects")
                .Add(5, "TrashedInkStrokes");
            model[typeof(CLPHistoryItem)]
                .Add(1, "CreationDate")
                .Add(2, "ObjectID")
                .Add(3, "UniqueID")
                .Add(4, "ItemType")
                .Add(5, "OldValue")
                .Add(6, "NewValue");
           
            model.CompileInPlace();
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

