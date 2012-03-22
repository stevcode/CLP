using System;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using Classroom_Learning_Partner.Model;
using System.Windows.Media;
using Classroom_Learning_Partner.Views.PageObjects;
using System.Windows.Ink;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Windows.Controls.Ribbon;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Timers;
using Classroom_Learning_Partner.ViewModels.Displays;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.IO;
using System.Windows.Documents;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;

namespace Classroom_Learning_Partner.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        public const string clpText = "Classroom Learning Partner - ";

        /// <summary>
        /// Initializes a new instance of the MainWindowViewModel class.
        /// </summary>
        public MainWindowViewModel()
            : base()
        {
            //Console.WriteLine(Title + " created");
            //MainWindow Content
            SetTitleBarText("Starting Up");
            IsAuthoring = false;
            IsMinimized = false;
            IsPlaybackEnabled = false;
            PageObjectAddMode = PageObjectAddMode.None;
            OpenNotebooks = new ObservableCollection<CLPNotebook>();

            //MainWindow Commands
            SetInstructorCommand = new Command(OnSetInstructorCommandExecute);
            SetStudentCommand = new Command(OnSetStudentCommandExecute);
            SetProjectorCommand = new Command(OnSetProjectorCommandExecute);
            SetServerCommand = new Command(OnSetServerCommandExecute);

            //Ribbon Content
            SideBarVisibility = true;
            CanSendToTeacher = true;
            IsSending = false;
            DrawingAttributes = new DrawingAttributes();
            DrawingAttributes.Height = PEN_RADIUS;
            DrawingAttributes.Width = PEN_RADIUS;
            DrawingAttributes.Color = Colors.Black;
            DrawingAttributes.FitToCurve = true;
            EditingMode = InkCanvasEditingMode.Ink;

            CurrentColorButton = new RibbonButton();
            CurrentColorButton.Background = new SolidColorBrush(Colors.Black);

            foreach (var color in _colors)
            {
                _fontColors.Add(new SolidColorBrush(color));
            }

            CurrentFontColor = new SolidColorBrush(Colors.Black);
            //Steve - Set to New Times Roman
            CurrentFontFamily = Fonts[0];
            CurrentFontSize = 26;
            

            AuthoringTabVisibility = Visibility.Collapsed;
            InstructorVisibility = Visibility.Collapsed;
            StudentVisibility = Visibility.Collapsed;
            switch (App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    break;
                case App.UserMode.Instructor:
                    InstructorVisibility = Visibility.Visible;
                    break;
                case App.UserMode.Projector:
                    break;
                case App.UserMode.Student:   
                    StudentVisibility = Visibility.Visible;
                    break;
                default:
                    break;
            }

            //Ribbon Commands
            //App Menu
            NewNotebookCommand = new Command(OnNewNotebookCommandExecute);
            OpenNotebookCommand = new Command(OnOpenNotebookCommandExecute);
            EditNotebookCommand = new Command(OnEditNotebookCommandExecute);
            DoneEditingNotebookCommand = new Command(OnDoneEditingNotebookCommandExecute);
            SaveNotebookCommand = new Command(OnSaveNotebookCommandExecute);
            SaveAllNotebooksCommand = new Command(OnSaveAllNotebooksCommandExecute);
            ConvertToXPSCommand = new Command(OnConvertToXPSCommandExecute);
            ExitCommand = new Command(OnExitCommandExecute);

            //Tools
            SetPenCommand = new Command(OnSetPenCommandExecute);
            SetMarkerCommand = new Command(OnSetMarkerCommandExecute);
            SetEraserCommand = new Command(OnSetEraserCommandExecute);
            SetStrokeEraserCommand = new Command(OnSetStrokeEraserCommandExecute);
            SetSnapTileCommand = new Command(OnSetSnapTileCommandExecute);
            SetPenColorCommand = new Command<RibbonButton>(OnSetPenColorCommandExecute);

            //History
            EnablePlaybackCommand = new Command(OnEnablePlaybackCommandExecute);
            UndoCommand = new Command(OnUndoCommandExecute);
            RedoCommand = new Command(OnRedoCommandExecute);

            //Submit
            SubmitPageCommand = new Command(OnSubmitPageCommandExecute);

            //Displays
            SendDisplayToProjectorcommand = new Command(OnSendDisplayToProjectorcommandExecute);

            //Page
            AddNewPageCommand = new Command(OnAddNewPageCommandExecute);
            DeletePageCommand = new Command(OnDeletePageCommandExecute);
            CopyPageCommand = new Command(OnCopyPageCommandExecute);

            //Insert
            InsertTextBoxCommand = new Command(OnInsertTextBoxCommandExecute);
            InsertImageCommand = new Command(OnInsertImageCommandExecute);
            InsertImageStampCommand = new Command(OnInsertImageStampCommandExecute);
            InsertBlankStampCommand = new Command(OnInsertBlankStampCommandExecute);
            InsertSquareShapeCommand = new Command(OnInsertSquareShapeCommandExecute);
            InsertCircleShapeCommand = new Command(OnInsertCircleShapeCommandExecute);
            InsertInkRegionCommand = new Command(OnInsertInkRegionCommandExecute);

            //Student Record and Playback 
            RecordVisualCommand = new Command(OnRecordVisualCommandExecute);
            PlayPauseVisualCommand = new Command(OnPlayPauseVisualCommandExecute);
            StopVisualCommand = new Command(OnStopVisualCommandExecute);
            RecordAudioCommand = new Command(OnRecordAudioCommandExecute);
            PlayAudioCommand = new Command(OnPlayAudioCommandExecute);
            RecordBothCommand = new Command(OnRecordBothCommandExecute);
            PlayStopBothCommand = new Command(OnPlayStopBothCommandExecute);

            //Steve - Can this be done in XAML? And switched to toggle button?
            VisualRecordImage = new Uri("..\\Images\\record.png", UriKind.Relative);
            AudioRecordImage = new Uri("..\\Images\\mic_start.png", UriKind.Relative);
            PlayPauseVisualImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
            PlayPauseBothImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
            RecordBothImage = new Uri("..\\Images\\video.png", UriKind.Relative);

            currentlyPlayingVisual = false;
        }

        public override string Title { get { return "MainWindowVM"; } }

        #region NonRibbon Items

        #region Bindings

        /// <summary>
        /// Gets or sets the Title Bar text of the window.
        /// </summary>
        public string TitleBarText
        {
            get { return GetValue<string>(TitleBarTextProperty); }
            private set { SetValue(TitleBarTextProperty, value); }
        }

        /// <summary>
        /// Register the TitleBarText property so it is known in the class.
        /// </summary>
        public static readonly PropertyData TitleBarTextProperty = RegisterProperty("TitleBarText", typeof(string));

        /// <summary>
        /// Gets or sets the current Workspace.
        /// </summary>
        public IWorkspaceViewModel SelectedWorkspace
        {
            get { return GetValue<IWorkspaceViewModel>(SelectedWorkspaceProperty); }
            set { SetValue(SelectedWorkspaceProperty, value); }
        }

        /// <summary>
        /// Register the SelectedWorkspace property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SelectedWorkspaceProperty = RegisterProperty("SelectedWorkspace", typeof(IWorkspaceViewModel));

        #endregion //Bindings

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<CLPNotebook> OpenNotebooks
        {
            get { return GetValue<ObservableCollection<CLPNotebook>>(OpenNotebooksProperty); }
            private set { SetValue(OpenNotebooksProperty, value); }
        }

        /// <summary>
        /// Register the OpenNotebooks property so it is known in the class.
        /// </summary>
        public static readonly PropertyData OpenNotebooksProperty = RegisterProperty("OpenNotebooks", typeof(ObservableCollection<CLPNotebook>));

        /// <summary>
        /// Gets or sets the Authoring flag.
        /// </summary>
        public bool IsAuthoring
        {
            get { return GetValue<bool>(IsAuthoringProperty); }
            set { SetValue(IsAuthoringProperty, value); }
        }

        /// <summary>
        /// Register the IsAuthoring property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsAuthoringProperty = RegisterProperty("IsAuthoring", typeof(bool));

        #endregion //Properties

        #region Methods

        //Sets the text in the title bar of the window, endText can add optional information
        public void SetTitleBarText(string endText)
        {
            string isOnline = "Disconnected";
            string userName = "";
            switch (App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    userName = "ServerMode";
                    break;
                case App.UserMode.Instructor:
                    userName = "InstructorMode";
                    break;
                case App.UserMode.Projector:
                    userName = "ProjectorMode";
                    break;
                case App.UserMode.Student:
                    userName = "No One";
                    break;
                default:
                    break;
            }
            
            if (App.Peer != null)
            {
                if (App.Peer.OnlineStatusHandler != null)
                {
                    if (App.Peer.OnlineStatusHandler.IsOnline)
                    {
                        isOnline = "Connected";
                    }
                }

                userName = App.Peer.UserName;
            }

            TitleBarText = clpText + "Logged In As: " + userName + ", Connection Status: " + isOnline + " | " + endText;
        }

        public void SetWorkspace()
        {
            IsAuthoring = false;
            IsMinimized = false;
            switch (App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    SelectedWorkspace = new ServerWorkspaceViewModel();
                    break;
                case App.UserMode.Instructor:
                    SelectedWorkspace = new NotebookChooserWorkspaceViewModel();
                    break;
                case App.UserMode.Projector:
                    SelectedWorkspace = new NotebookChooserWorkspaceViewModel();
                    IsMinimized = true;
                    break;
                case App.UserMode.Student:
                    SelectedWorkspace = new UserLoginWorkspaceViewModel();
                    break;
            }
        }

        #endregion //Methods

        #region Commands

        /// <summary>
        /// Gets the SetInstructorCommand command.
        /// </summary>
        public Command SetInstructorCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetInstructorCommand command is executed.
        /// </summary>
        private void OnSetInstructorCommandExecute()
        {
            App.CurrentUserMode = App.UserMode.Instructor;
            SetWorkspace();
        }

        /// <summary>
        /// Gets the SetStudentCommand command.
        /// </summary>
        public Command SetStudentCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetStudentCommand command is executed.
        /// </summary>
        private void OnSetStudentCommandExecute()
        {
            App.CurrentUserMode = App.UserMode.Student;
            SetWorkspace();
        }

        /// <summary>
        /// Gets the SetProjectorCommand command.
        /// </summary>
        public Command SetProjectorCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetProjectorCommand command is executed.
        /// </summary>
        private void OnSetProjectorCommandExecute()
        {
            App.CurrentUserMode = App.UserMode.Projector;
            SetWorkspace();
        }

        /// <summary>
        /// Gets the SetServerCommand command.
        /// </summary>
        public Command SetServerCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetServerCommand command is executed.
        /// </summary>
        private void OnSetServerCommandExecute()
        {
            App.CurrentUserMode = App.UserMode.Server;
            SetWorkspace();
        }

        #endregion //Commands

        #endregion //NonRibbon Items

        #region Ribbon

        public const double PEN_RADIUS = 2;
        public const double MARKER_RADIUS = 5;
        public const double ERASER_RADIUS = 5;

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsMinimized
        {
            get { return GetValue<bool>(IsMinimizedProperty); }
            set { SetValue(IsMinimizedProperty, value); }
        }

        /// <summary>
        /// Register the IsMinimized property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsMinimizedProperty = RegisterProperty("IsMinimized", typeof(bool));

        //Steve - Dont' want Views in ViewModels, can this be fixed?
        public CLPTextBoxView LastFocusedTextBox = null;

        /// <summary>
        /// Gets the DrawingAttributes of the Ribbon.
        /// </summary>
        public DrawingAttributes DrawingAttributes
        {
            get { return GetValue<DrawingAttributes>(DrawingAttributesProperty); }
            private set { SetValue(DrawingAttributesProperty, value); }
        }

        /// <summary>
        /// Register the DrawingAttributes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DrawingAttributesProperty = RegisterProperty("DrawingAttributes", typeof(DrawingAttributes));

        /// <summary>
        /// Gets or sets the EditingMode for the InkCanvas.
        /// </summary>
        public InkCanvasEditingMode EditingMode
        {
            get { return GetValue<InkCanvasEditingMode>(EditingModeProperty); }
            set { SetValue(EditingModeProperty, value); }
        }

        private bool _currentlyPlayingVisual;
        public bool currentlyPlayingVisual
        {
            get { return _currentlyPlayingVisual; }
            set { _currentlyPlayingVisual = value; }
        }
        /// <summary>
        /// Register the EditingMode property so it is known in the class.
        /// </summary>
        public static readonly PropertyData EditingModeProperty = RegisterProperty("EditingMode", typeof(InkCanvasEditingMode));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public PageObjectAddMode PageObjectAddMode
        {
            get { return GetValue<PageObjectAddMode>(PageObjectAddModeProperty); }
            set { SetValue(PageObjectAddModeProperty, value); }
        }

        /// <summary>
        /// Register the PageObjectAddMode property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageObjectAddModeProperty = RegisterProperty("PageObjectAddMode", typeof(PageObjectAddMode));

        #endregion //Properties

        #region Bindings

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool SideBarVisibility
        {
            get { return GetValue<bool>(SideBarVisibilityProperty); }
            set { SetValue(SideBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Register the SideBarVisibility property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SideBarVisibilityProperty = RegisterProperty("SideBarVisibility", typeof(bool));


        #region Convert to XAMLS?

        //Steve - Switch these visibility tags into XAML getters/setters
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility AuthoringTabVisibility
        {
            get { return GetValue<Visibility>(AuthoringTabVisibilityProperty); }
            set { SetValue(AuthoringTabVisibilityProperty, value); }
        }

        /// <summary>
        /// Register the AuthoringTabVisibility property so it is known in the class.
        /// </summary>
        public static readonly PropertyData AuthoringTabVisibilityProperty = RegisterProperty("AuthoringTabVisibility", typeof(Visibility));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility InstructorVisibility
        {
            get { return GetValue<Visibility>(InstructorVisibilityProperty); }
            set { SetValue(InstructorVisibilityProperty, value); }
        }

        /// <summary>
        /// Register the InstructorVisibility property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InstructorVisibilityProperty = RegisterProperty("InstructorVisibility", typeof(Visibility));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility StudentVisibility
        {
            get { return GetValue<Visibility>(StudentVisibilityProperty); }
            set { SetValue(StudentVisibilityProperty, value); }
        }

        /// <summary>
        /// Register the StudentVisibility property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StudentVisibilityProperty = RegisterProperty("StudentVisibility", typeof(Visibility));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Uri AudioRecordImage
        {
            get { return GetValue<Uri>(AudioRecordImageProperty); }
            set { SetValue(AudioRecordImageProperty, value); }
        }
        /// <summary>
        /// Register the RecordImage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData AudioRecordImageProperty = RegisterProperty("AudioRecordImage", typeof(Uri));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Uri VisualRecordImage
        {
            get { return GetValue<Uri>(VisualRecordImageProperty); }
            set { SetValue(VisualRecordImageProperty, value); }
        }
        
        // <summary>
        /// Register the VisualRecordImage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData VisualRecordImageProperty = RegisterProperty("VisualRecordImage", typeof(Uri));
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Uri PlayPauseVisualImage
        {
            get { return GetValue<Uri>(PlayPauseVisualImageProperty); }
            set { SetValue(PlayPauseVisualImageProperty, value); }
        }
        /// <summary>
        /// Register the PlayPauseVisualImage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PlayPauseVisualImageProperty = RegisterProperty("PlayPauseVisualImage", typeof(Uri));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Uri PlayPauseBothImage
        {
            get { return GetValue<Uri>(PlayPauseBothImageProperty); }
            set { SetValue(PlayPauseBothImageProperty, value); }
        }
        /// <summary>
        /// Register the PlayPauseVisualImage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PlayPauseBothImageProperty = RegisterProperty("PlayPauseBothImage", typeof(Uri));
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Uri RecordBothImage
        {
            get { return GetValue<Uri>(RecordBothImageProperty); }
            set { SetValue(RecordBothImageProperty, value); }
        }
        /// <summary>
        /// Register the RecordBothImage property so it is known in the class.
        /// </summary>
        public static readonly PropertyData RecordBothImageProperty = RegisterProperty("RecordBothImage", typeof(Uri));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public RibbonButton CurrentColorButton
        {
            get { return GetValue<RibbonButton>(CurrentColorButtonProperty); }
            set { SetValue(CurrentColorButtonProperty, value); }
        }

        /// <summary>
        /// Register the CurrentColorButton property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentColorButtonProperty = RegisterProperty("CurrentColorButton", typeof(RibbonButton));

        #endregion //Convert to XAMLS?

        #region TextBox

        private ObservableCollection<FontFamily> _fonts = new ObservableCollection<FontFamily>(System.Windows.Media.Fonts.SystemFontFamilies);
        public ObservableCollection<FontFamily> Fonts
        {
            get
            {
                return _fonts;
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public FontFamily CurrentFontFamily
        {
            get { return GetValue<FontFamily>(CurrentFontFamilyProperty); }
            set
            {
                SetValue(CurrentFontFamilyProperty, value);
                Console.WriteLine("fontfamily changed");
                AppMessages.UpdateFont.Send(-1, CurrentFontFamily, null);
            }
        }

        /// <summary>
        /// Register the CurrentFontFamily property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentFontFamilyProperty = RegisterProperty("CurrentFontFamily", typeof(FontFamily));

        private ObservableCollection<double> _fontSizes = new ObservableCollection<double>(){3.0, 4.0, 5.0, 6.0, 6.5, 7.0, 7.5, 8.0, 8.5, 9.0, 9.5, 
		                                                                                    10.0, 10.5, 11.0, 11.5, 12.0, 12.5, 13.0, 13.5, 14.0, 15.0,
		                                                                                    16.0, 17.0, 18.0, 19.0, 20.0, 22.0, 24.0, 26.0, 28.0, 30.0,
		                                                                                    32.0, 34.0, 36.0, 38.0, 40.0, 44.0, 48.0, 52.0, 56.0, 60.0, 64.0, 68.0, 72.0, 76.0,
		                                                                                    80.0, 88.0, 96.0, 104.0, 112.0, 120.0, 128.0, 136.0, 144.0};

        public ObservableCollection<double> FontSizes
        {
            get
            {
                return _fontSizes;
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public double CurrentFontSize
        {
            get { return GetValue<double>(CurrentFontSizeProperty); }
            set
            {
                SetValue(CurrentFontSizeProperty, value);
                Console.WriteLine("fontsize changed");
                AppMessages.UpdateFont.Send(CurrentFontSize, null, null);
            }
        }

        /// <summary>
        /// Register the CurrentFontSize property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentFontSizeProperty = RegisterProperty("CurrentFontSize", typeof(double));

        private List<Color> _colors = new List<Color>() { Colors.Black, Colors.Red, Colors.Blue, Colors.Purple, Colors.Brown, Colors.Green };
        private ObservableCollection<Brush> _fontColors = new ObservableCollection<Brush>();

        public ObservableCollection<Brush> FontColors
        {
            get
            {
                return _fontColors;
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Brush CurrentFontColor
        {
            get { return GetValue<Brush>(CurrentFontColorProperty); }
            set
            {
                SetValue(CurrentFontColorProperty, value);
                Console.WriteLine("fontcolor changed");
                AppMessages.UpdateFont.Send(-1, null, CurrentFontColor);
            }
        }

        /// <summary>
        /// Register the CurrentFontColor property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentFontColorProperty = RegisterProperty("CurrentFontColor", typeof(Brush));

        #endregion //TextBox

        #endregion //Bindings

        #region Commands

        #region Notebook Commands

        /// <summary>
        /// Gets the NewNotebookCommand command.
        /// </summary>
        public Command NewNotebookCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the NewNotebookCommand command is executed.
        /// </summary>
        private void OnNewNotebookCommandExecute()
        {
            CLPServiceAgent.Instance.OpenNewNotebook();
            (SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage = new CLPPageViewModel((SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages[0]);
        }

        /// <summary>
        /// Gets the OpenNotebookCommand command.
        /// </summary>
        public Command OpenNotebookCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the OpenNotebookCommand command is executed.
        /// </summary>
        private void OnOpenNotebookCommandExecute()
        {
            SelectedWorkspace = new NotebookChooserWorkspaceViewModel();
        }

        /// <summary>
        /// Gets the EditNotebookCommand command.
        /// </summary>
        public Command EditNotebookCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the EditNotebookCommand command is executed.
        /// </summary>
        private void OnEditNotebookCommandExecute()
        {
            IsAuthoring = true;
        }

        /// <summary>
        /// Gets the DoneEditingNotebookCommand command.
        /// </summary>
        public Command DoneEditingNotebookCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the DoneEditingNotebookCommand command is executed.
        /// </summary>
        private void OnDoneEditingNotebookCommandExecute()
        {
            IsAuthoring = false;
            CLPNotebook notebook = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook;
            foreach (CLPPage page in notebook.Pages)
            {
                page.PageHistory.ClearHistory();
            }
            //CLPService.DistributeNotebook(App.CurrentNotebookViewModel, App.Peer.UserName);
        }

        /// <summary>
        /// Gets the SaveNotebookCommand command.
        /// </summary>
        public Command SaveNotebookCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SaveNotebookCommand command is executed.
        /// </summary>
        private void OnSaveNotebookCommandExecute()
        {
            CLPServiceAgent.Instance.SaveNotebook((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook);
        }

        /// <summary>
        /// Gets the SaveAllNotebooksCommand command.
        /// </summary>
        public Command SaveAllNotebooksCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SaveAllNotebooksCommand command is executed.
        /// </summary>
        private void OnSaveAllNotebooksCommandExecute()
        {
            foreach (var notebook in App.MainWindowViewModel.OpenNotebooks)
            {
                CLPServiceAgent.Instance.SaveNotebook(notebook);
            }
        }

        /// <summary>
        /// Gets the ConvertToXPSCommand command.
        /// </summary>
        public Command ConvertToXPSCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the ConvertToXPS command is executed.
        /// </summary>
        private void OnConvertToXPSCommandExecute()
        {
            foreach (var notebook in OpenNotebooks)
            {
                if (App.CurrentUserMode == App.UserMode.Instructor)
                {
                    FixedDocument docSubmissions = new FixedDocument();
                    docSubmissions.DocumentPaginator.PageSize = new Size(96 * 11, 96 * 8.5);


                    foreach (var pageID in notebook.Submissions.Keys)
                    {
                        foreach (CLPPage page in notebook.Submissions[pageID])
                        {
                            PageContent pageContent = new PageContent();
                            FixedPage fixedPage = new FixedPage();

                            CLPPagePreviewView currentPage = new CLPPagePreviewView();
                            CLPPageViewModel pageVM = new CLPPageViewModel(page);
                            currentPage.DataContext = pageVM;
                            currentPage.UpdateLayout();

                            Grid grid = new Grid();
                            grid.Children.Add(currentPage);
                            Label label = new Label();
                            label.FontSize = 20;
                            label.FontWeight = FontWeights.Bold;
                            label.FontStyle = FontStyles.Oblique;
                            label.HorizontalAlignment = HorizontalAlignment.Left;
                            label.VerticalAlignment = VerticalAlignment.Top;
                            label.Content = pageVM.SubmitterName;
                            grid.Children.Add(label);

                            //Create first page of document
                            RotateTransform rotate = new RotateTransform(90.0);
                            TranslateTransform translate = new TranslateTransform(816 + 2, -2);
                            TransformGroup transform = new TransformGroup();
                            transform.Children.Add(rotate);
                            transform.Children.Add(translate);
                            grid.RenderTransform = transform;

                            fixedPage.Children.Add(grid);
                            ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
                            docSubmissions.Pages.Add(pageContent);
                        }
                    }

                    //Save the submissions
                    string filenameSubs = notebook.NotebookName + " - Submissions" + ".xps";
                    string pathSubs = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\" + filenameSubs;
                    if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\"))
                    {
                        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\");
                    }
                    if (File.Exists(pathSubs))
                    {
                        File.Delete(pathSubs);
                    }


                    XpsDocument xpsdSubs = new XpsDocument(pathSubs, FileAccess.ReadWrite);
                    XpsDocumentWriter xwSubs = XpsDocument.CreateXpsDocumentWriter(xpsdSubs);
                    if (docSubmissions.Pages.Count > 0)
                    {
                        xwSubs.Write(docSubmissions);
                    }
                    xpsdSubs.Close();
                }


                FixedDocument doc = new FixedDocument();
                doc.DocumentPaginator.PageSize = new Size(96 * 11, 96 * 8.5);

                foreach (CLPPage page in notebook.Pages)
                {
                    PageContent pageContent = new PageContent();
                    FixedPage fixedPage = new FixedPage();

                    CLPPagePreviewView currentPage = new CLPPagePreviewView();
                    CLPPageViewModel pageVM = new CLPPageViewModel(page);
                    currentPage.DataContext = pageVM;
                    currentPage.UpdateLayout();

                    //Create first page of document
                    RotateTransform rotate = new RotateTransform(90.0);
                    TranslateTransform translate = new TranslateTransform(816 + 2, -2);
                    TransformGroup transform = new TransformGroup();
                    transform.Children.Add(rotate);
                    transform.Children.Add(translate);
                    currentPage.RenderTransform = transform;

                    fixedPage.Children.Add(currentPage);
                    ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
                    doc.Pages.Add(pageContent);
                }

                //Save the document
                string filename = notebook.NotebookName + ".xps";
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\" + filename;
                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\"))
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\");
                }
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                XpsDocument xpsd = new XpsDocument(path, FileAccess.ReadWrite);
                XpsDocumentWriter xw = XpsDocument.CreateXpsDocumentWriter(xpsd);
                xw.Write(doc);
                xpsd.Close();
            }

        }

        /// <summary>
        /// Gets the ExitCommand command.
        /// </summary>
        public Command ExitCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the ExitCommand command is executed.
        /// </summary>
        private void OnExitCommandExecute()
        {
            if (MessageBox.Show("Are you sure you want to exit?",
                                        "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                CLPServiceAgent.Instance.Exit();
            }
        }

        #endregion //Notebook Commands

        #region Pen Commands

        /// <summary>
        /// Gets the SetPenCommand command.
        /// </summary>
        public Command SetPenCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetPenCommand command is executed.
        /// </summary>
        private void OnSetPenCommandExecute()
        {
            DrawingAttributes.Height = PEN_RADIUS;
            DrawingAttributes.Width = PEN_RADIUS;
            EditingMode = InkCanvasEditingMode.Ink;
            PageObjectAddMode = PageObjectAddMode.None;
        }

        /// <summary>
        /// Gets the SetMarkerCommand command.
        /// </summary>
        public Command SetMarkerCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetMarkerCommand command is executed.
        /// </summary>
        private void OnSetMarkerCommandExecute()
        {
            DrawingAttributes.Height = MARKER_RADIUS;
            DrawingAttributes.Width = MARKER_RADIUS;
            EditingMode = InkCanvasEditingMode.Ink;
            PageObjectAddMode = PageObjectAddMode.None;
        }

        /// <summary>
        /// Gets the SetEraserCommand command.
        /// </summary>
        public Command SetEraserCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetEraserCommand command is executed.
        /// </summary>
        private void OnSetEraserCommandExecute()
        {
            DrawingAttributes.Height = ERASER_RADIUS;
            DrawingAttributes.Width = ERASER_RADIUS;
            EditingMode = InkCanvasEditingMode.EraseByPoint;
            PageObjectAddMode = PageObjectAddMode.None;
        }

                /// <summary>
        /// Gets the SetStrokeEraserCommand command.
        /// </summary>
        public Command SetStrokeEraserCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetStrokeEraserCommand command is executed.
        /// </summary>
        private void OnSetStrokeEraserCommandExecute()
        {
            EditingMode = InkCanvasEditingMode.EraseByStroke;
            PageObjectAddMode = PageObjectAddMode.None;
        }

        /// <summary>
        /// Gets the SetPenColorCommand command.
        /// </summary>
        public Command<RibbonButton> SetPenColorCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetPenColorCommand command is executed.
        /// </summary>
        private void OnSetPenColorCommandExecute(RibbonButton button)
        {
            CurrentColorButton = button as RibbonButton;
            DrawingAttributes.Color = (CurrentColorButton.Background as SolidColorBrush).Color;
        }

        /// <summary>
        /// Gets the SetSnapTileCommand command.
        /// </summary>
        public Command SetSnapTileCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SetSnapTileCommand command is executed.
        /// </summary>
        private void OnSetSnapTileCommandExecute()
        {
            EditingMode = InkCanvasEditingMode.None;
            PageObjectAddMode = PageObjectAddMode.SnapTile;
        }

        #endregion //Pen Commands

        #region HistoryCommands

        /// <summary>
        /// Gets the EnablePlaybackCommand command.
        /// </summary>
        public Command EnablePlaybackCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the EnablePlaybackCommand command is executed.
        /// </summary>
        private void OnEnablePlaybackCommandExecute()
        {
            IsPlaybackEnabled = !IsPlaybackEnabled;
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsPlaybackEnabled
        {
            get { return GetValue<bool>(IsPlaybackEnabledProperty); }
            set { SetValue(IsPlaybackEnabledProperty, value); }
        }

        /// <summary>
        /// Register the IsPlaybackEnabled property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsPlaybackEnabledProperty = RegisterProperty("IsPlaybackEnabled", typeof(bool));

        /// <summary>
        /// Gets the UndoCommand command.
        /// </summary>
        public Command UndoCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the UndoCommand command is executed.
        /// </summary>
        private void OnUndoCommandExecute()
        {
            if (SelectedWorkspace.WorkspaceName == "NotebookWorkspace")
            {
                (SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage.Undo();
            }
        }

        /// <summary>
        /// Gets the RedoCommand command.
        /// </summary>
        public Command RedoCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the RedoCommand command is executed.
        /// </summary>
        private void OnRedoCommandExecute()
        {
            if (SelectedWorkspace.WorkspaceName == "NotebookWorkspace")
            {
                (SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage.Redo();
            }
        }

        //private RelayCommand _audioCommand;
        //private bool recording = false;
        //public RelayCommand AudioCommand
        //{
        //    get
        //    {
        //        return _audioCommand
        //            ?? (_audioCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      AppMessages.Audio.Send("start");
        //                                      recording = !recording;
        //                                      if (recording)
        //                                      {
        //                                          RecordImage = new Uri("..\\Images\\mic_stop.png", UriKind.Relative);
        //                                      }
        //                                      else
        //                                      {
        //                                          RecordImage = new Uri("..\\Images\\mic_start.png", UriKind.Relative);
        //                                      }
        //                                  }));
        //    }
        //}
        //private RelayCommand _playAudioCommand;
        //public RelayCommand PlayAudioCommand
        //{
        //    get
        //    {
        //        return _playAudioCommand
        //            ?? (_playAudioCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      AppMessages.Audio.Send("play");


        //                                  }));
        //    }
        //}
        //private RelayCommand _enablePlaybackCommand;
        //public RelayCommand EnablePlaybackCommand
        //{
        //    get
        //    {
        //        return _enablePlaybackCommand
        //            ?? (_enablePlaybackCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      AppMessages.ChangePlayback.Send(true);

        //                                  }));
        //    }
        //}
        #endregion //History Commands

        #region Submission Command

        /// <summary>
        /// Gets the SubmitPageCommand command.
        /// </summary>
        public Command SubmitPageCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SubmitPageCommand command is executed.
        /// </summary>
        private void OnSubmitPageCommandExecute()
        {
            //Steve - change to different thread and do callback to make sure sent page has arrived
            IsSending = true;
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;

            if (CanSendToTeacher)
            {
                CLPPage page = (SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage.Page;
                CLPServiceAgent.Instance.SubmitPage(page);
            }
            CanSendToTeacher = false;
            
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool CanSendToTeacher
        {
            get { return GetValue<bool>(CanSendToTeacherProperty); }
            set { SetValue(CanSendToTeacherProperty, value); }
        }

        /// <summary>
        /// Register the CanSendToTeacher property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CanSendToTeacherProperty = RegisterProperty("CanSendToTeacher", typeof(bool));


        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Timer timer = sender as Timer;
            timer.Stop();
            timer.Elapsed -= timer_Elapsed;
            IsSending = false;
            timer.Dispose();
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsSending
        {
            get { return GetValue<bool>(IsSendingProperty); }
            set
            {
                SetValue(IsSendingProperty, value);
                if (IsSending)
                {
                    SendButtonVisibility = Visibility.Collapsed;
                    IsSentInfoVisibility = Visibility.Visible;
                }
                else
                {
                    SendButtonVisibility = Visibility.Visible;
                    IsSentInfoVisibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Register the IsSending property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsSendingProperty = RegisterProperty("IsSending", typeof(bool));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility SendButtonVisibility
        {
            get { return GetValue<Visibility>(SendButtonVisibilityProperty); }
            set { SetValue(SendButtonVisibilityProperty, value); }
        }

        /// <summary>
        /// Register the SendButtonVisibility property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SendButtonVisibilityProperty = RegisterProperty("SendButtonVisibility", typeof(Visibility));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility IsSentInfoVisibility
        {
            get { return GetValue<Visibility>(IsSentInfoVisibilityProperty); }
            set { SetValue(IsSentInfoVisibilityProperty, value); }
        }

        /// <summary>
        /// Register the IsSentInfoVisibility property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsSentInfoVisibilityProperty = RegisterProperty("IsSentInfoVisibility", typeof(Visibility));

        #endregion //Submission Command

        #region Display Commands

        /// <summary>
        /// Gets the SendDisplayToProjectorcommand command.
        /// </summary>
        public Command SendDisplayToProjectorcommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SendDisplayToProjectorcommand command is executed.
        /// </summary>
        private void OnSendDisplayToProjectorcommandExecute()
        {
            if (App.Peer.Channel != null)
            {
                if ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay is LinkedDisplayViewModel)
                {
                    (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay.IsOnProjector = true;
                    foreach (var gridDisplay in (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).GridDisplays)
                    {

                    }
                }
            }
        }

        //private RelayCommand _sendDisplayToProjectorCommand;

        ///// <summary>
        ///// Gets the SendDisplayToProjectorCommand.
        ///// </summary>
        //public RelayCommand SendDisplayToProjectorCommand
        //{
        //    get
        //    {
        //        return _sendDisplayToProjectorCommand
        //            ?? (_sendDisplayToProjectorCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      if (App.Peer.Channel != null)
        //                                      {
        //                                          if ((App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).Display is LinkedDisplayViewModel)
        //                                          {
        //                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay.IsOnProjector = true;
        //                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.IsOnProjector = false;
        //                                              App.Peer.Channel.SwitchProjectorDisplay("LinkedDisplay", new List<string>());
        //                                          }
        //                                          else
        //                                          {
        //                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay.IsOnProjector = false;
        //                                              (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.IsOnProjector = true;
        //                                              List<string> pageList = new List<string>();
        //                                              foreach (var page in (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.DisplayPages)
        //                                              {
        //                                                  pageList.Add(ObjectSerializer.ToString(page.Page));
        //                                              }

        //                                              App.Peer.Channel.SwitchProjectorDisplay("GridDisplay", pageList);
        //                                          }
        //                                      }
        //                                  }));
        //    }
        //}

        //private RelayCommand _switchToLinkedDisplayCommand;

        ///// <summary>
        ///// Gets the SwitchToLinkedDisplayCommand.
        ///// </summary>
        //public RelayCommand SwitchToLinkedDisplayCommand
        //{
        //    get
        //    {
        //        return _switchToLinkedDisplayCommand
        //            ?? (_switchToLinkedDisplayCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).Display = (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay;
        //                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay.IsActive = true;
        //                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.IsActive = false;
        //                                  }));
        //    }
        //}

        //private RelayCommand _createNewGridDisplayCommand;

        ///// <summary>
        ///// Gets the CreateNewGridDisplayCommand.
        ///// </summary>
        //public RelayCommand CreateNewGridDisplayCommand
        //{
        //    get
        //    {
        //        return _createNewGridDisplayCommand
        //            ?? (_createNewGridDisplayCommand = new RelayCommand(
        //                                  () =>
        //                                  {
        //                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).Display = (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay;
        //                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).LinkedDisplay.IsActive = false;
        //                                      (App.MainWindowViewModel.Workspace as InstructorWorkspaceViewModel).GridDisplay.IsActive = true;
        //                                  }));
        //    }
        //}

        #endregion //Display Commands

        #region Page Commands

        /// <summary>
        /// Gets the AddPageCommand command.
        /// </summary>
        public Command AddNewPageCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the AddPageCommand command is executed.
        /// </summary>
        private void OnAddNewPageCommandExecute()
        {
            //Steve - clpserviceagent
            int index = (SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.IndexOf(((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page);
            index++;
            CLPPage page = new CLPPage();
            page.ParentNotebookID = (SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.UniqueID;
            (SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.InsertPageAt(index, page);
            //(SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.Insert(index, new CLPPageViewModel(page));
        }

        /// <summary>
        /// Gets the DeletePageCommand command.
        /// </summary>
        public Command DeletePageCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the DeletePageCommand command is executed.
        /// </summary>
        private void OnDeletePageCommandExecute()
        {
            //Steve - clpserviceagent
            int index = (SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.IndexOf(((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page);
            if (index != -1)
            {
                
                (SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.RemovePageAt(index);
                //(SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.RemoveAt(index);
            }
        }

        /// <summary>
        /// Gets the CopyPageCommand command.
        /// </summary>
        public Command CopyPageCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the CopyPageCommand command is executed.
        /// </summary>
        private void OnCopyPageCommandExecute()
        {
            // TODO: Handle command logic here
        }

        #endregion //Page Commands

        #region Insert Commands

        /// <summary>
        /// Gets the InsertTextBoxCommand command.
        /// </summary>
        public Command InsertTextBoxCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertTextBoxCommand command is executed.
        /// </summary>
        private void OnInsertTextBoxCommandExecute()
        {
            CLPTextBox textBox = new CLPTextBox();
            CLPServiceAgent.Instance.AddPageObjectToPage(((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page, textBox);
        }

        /// <summary>
        /// Gets the InsertImageCommand command.
        /// </summary>
        public Command InsertImageCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertImageCommand command is executed.
        /// </summary>
        private void OnInsertImageCommandExecute()
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Images|*.png;*.jpg;*.jpeg;*.gif"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                CLPImage image = new CLPImage(filename);
                CLPServiceAgent.Instance.AddPageObjectToPage(((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page, image);
            }
        }

        /// <summary>
        /// Gets the InsertImageStampCommand command.
        /// </summary>
        public Command InsertImageStampCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertImageStampCommand command is executed.
        /// </summary>
        private void OnInsertImageStampCommandExecute()
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Images|*.png;*.jpg;*.jpeg;*.gif"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                CLPImage image = new CLPImage(filename);
                CLPStamp stamp = new CLPStamp(image);
                CLPServiceAgent.Instance.AddPageObjectToPage(((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page, stamp);
            }
        }

        /// <summary>
        /// Gets the InsertBlankStampCommand command.
        /// </summary>
        public Command InsertBlankStampCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertBlankStampCommand command is executed.
        /// </summary>
        private void OnInsertBlankStampCommandExecute()
        {
            CLPStamp stamp = new CLPStamp(null);
            CLPServiceAgent.Instance.AddPageObjectToPage(((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page, stamp);
        }

        /// <summary>
        /// Gets the InsertSquareShapeCommand command.
        /// </summary>
        public Command InsertSquareShapeCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertSquareShapeCommand command is executed.
        /// </summary>
        private void OnInsertSquareShapeCommandExecute()
        {
            CLPShape square = new CLPShape(CLPShape.CLPShapeType.Rectangle);
            CLPServiceAgent.Instance.AddPageObjectToPage(((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page, square);
        }

        /// <summary>
        /// Gets the InsertCircleShapeCommand command.
        /// </summary>
        public Command InsertCircleShapeCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertCircleShapeCommand command is executed.
        /// </summary>
        private void OnInsertCircleShapeCommandExecute()
        {
            CLPShape circle = new CLPShape(CLPShape.CLPShapeType.Ellipse);
            CLPServiceAgent.Instance.AddPageObjectToPage(((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page, circle);
        }

        /// <summary>
        /// Gets the InsertInkRegionCommand command.
        /// </summary>
        public Command InsertInkRegionCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertInkRegionCommand command is executed.
        /// </summary>
        private void OnInsertInkRegionCommandExecute()
        {
            CustomizeInkRegionView optionChooser = new CustomizeInkRegionView();
            optionChooser.Owner = Application.Current.MainWindow;
            optionChooser.ShowDialog();
            if (optionChooser.DialogResult == true)
            {
                string correct_answer = optionChooser.CorrectAnswer.Text;
                int selected_type = optionChooser.ExpectedType.SelectedIndex;

                CLPInkRegion region = new CLPInkRegion(correct_answer, selected_type);
                CLPServiceAgent.Instance.AddPageObjectToPage(((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page, region);
            }
        }

        /// <summary>
        /// Gets the RecordVisualCommand command.
        /// </summary>
        public Command RecordVisualCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the RecordVisualCommand command is executed.
        /// </summary>
        bool currentlyRecording = false;
        private void OnRecordVisualCommandExecute()
        {
            if (!currentlyRecording)
            {
                CLPPage page = ((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page;
                CLPServiceAgent.Instance.StartRecordingVisual(page);
                VisualRecordImage = new Uri("..\\Images\\recording.png", UriKind.Relative);
                currentlyRecording = true;
            }
            else
            {
                CLPPage page = ((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page;
                CLPServiceAgent.Instance.StopRecordingVisual(page);
                VisualRecordImage = new Uri("..\\Images\\record.png", UriKind.Relative);
                currentlyRecording = false;
            }
        }

        /// <summary>
        /// Gets the PlayPauseVisualCommand command.
        /// </summary>
        public Command PlayPauseVisualCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the PlayPauseVisualCommand command is executed.
        /// </summary>
        
        private void OnPlayPauseVisualCommandExecute()
        {
            CLPPage page = ((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page;   
            if (!currentlyPlayingVisual)
            {
                CLPServiceAgent.Instance.PlaybackRecording(page);
                PlayPauseVisualImage = new Uri("..\\Images\\pause_blue.png", UriKind.Relative);
                currentlyPlayingVisual = true;
            }
            else
            {
                CLPServiceAgent.Instance.PlaybackRecording(page);
                PlayPauseVisualImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
                currentlyPlayingVisual = false;
            }
        }

        /// <summary>
        /// Gets the StopVisualCommand command.
        /// </summary>
        public Command StopVisualCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the StopVisualCommand command is executed.
        /// </summary>
        private void OnStopVisualCommandExecute()
        {
            
                CLPPage page = ((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page;
                CLPServiceAgent.Instance.StopPlayback(page);
            
        }

        /// <summary>
        /// Gets the RecordAudioCommand command.
        /// </summary>
        public Command RecordAudioCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the RecordAudioCommand command is executed.
        /// </summary>
        bool isRecordingAudio = false;
        private void OnRecordAudioCommandExecute()
        {
            CLPPage page = ((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page;
            if (!isRecordingAudio)
            {
                AudioRecordImage = new Uri("..\\Images\\mic_stop.png", UriKind.Relative);
                CLPServiceAgent.Instance.RecordAudio(page);
                isRecordingAudio = true;
            }
            else
            {
                AudioRecordImage = new Uri("..\\Images\\mic_start.png", UriKind.Relative);
                CLPServiceAgent.Instance.StopAudio(page);
                isRecordingAudio = false;
            }
        }

        /// <summary>
        /// Gets the PlayAudioCommand command.
        /// </summary>
        public Command PlayAudioCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the PlayAudioCommand command is executed.
        /// </summary>
        private void OnPlayAudioCommandExecute()
        {
            CLPPage page = ((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page;
            CLPServiceAgent.Instance.PlayAudio(page);
        }

        /// <summary>
        /// Gets the RecordBothCommand command.
        /// </summary>
        public Command RecordBothCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the RecordBothCommand command is executed.
        /// </summary>
        private void OnRecordBothCommandExecute()
        {
            //audio
            CLPPage page = ((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page;
            if (!isRecordingAudio && !currentlyRecording)
            {
                
                CLPServiceAgent.Instance.RecordAudio(page);
                isRecordingAudio = true;
                CLPServiceAgent.Instance.StartRecordingVisual(page);
                RecordBothImage = new Uri("..\\Images\\recording.png", UriKind.Relative);
                currentlyRecording = true;
            }
            else if(currentlyRecording && isRecordingAudio)
            {
                
                CLPServiceAgent.Instance.StopAudio(page);
                isRecordingAudio = false;
                CLPServiceAgent.Instance.StopRecordingVisual(page);
                RecordBothImage = new Uri("..\\Images\\video.png", UriKind.Relative);
                currentlyRecording = false;
            }

        }

        /// <summary>
        /// Gets the PlayStopBothCommand command.
        /// </summary>
        public Command PlayStopBothCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the PlayStopBothCommand command is executed.
        /// </summary>
        private void OnPlayStopBothCommandExecute()
        {
            CLPPage page = ((SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.Page;
            

            if (!currentlyPlayingVisual && !currentlyRecording)
            {
                CLPServiceAgent.Instance.PlayAudio(page);
                CLPServiceAgent.Instance.PlaybackRecording(page);
                PlayPauseBothImage = new Uri("..\\Images\\stop.png", UriKind.Relative);
                currentlyPlayingVisual = true;
            }
            else if(!currentlyRecording)
            {
                CLPServiceAgent.Instance.StopAudioPlayback(page);
                CLPServiceAgent.Instance.StopPlayback(page);
                PlayPauseBothImage = new Uri("..\\Images\\play_green.png", UriKind.Relative);
                currentlyPlayingVisual = false;
            }
        }

        #endregion //Insert Commands

        #endregion //Commands

        #endregion //Ribbon
    }
}