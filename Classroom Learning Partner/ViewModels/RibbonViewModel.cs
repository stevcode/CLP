using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Xps.Packaging;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Models;
using System.Windows.Threading;
using System.ServiceModel;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class RibbonViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindow
        {
            get { return App.MainWindowViewModel; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonViewModel"/> class.
        /// </summary>
        public RibbonViewModel()
        {
            InitializeCommands();

            switch(App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    ServerVisibility = Visibility.Visible;
                    break;
                case App.UserMode.Instructor:
                    InstructorVisibility = Visibility.Visible;
                    break;
                case App.UserMode.Projector:
                    IsMinimized = true;
                    break;
                case App.UserMode.Student:
                    StudentVisibility = Visibility.Visible;
                    break;
            }

            PenSize = 3;
            DrawingAttributes = new DrawingAttributes
                                {
                                    Height = PenSize,
                                    Width = PenSize,
                                    Color = Colors.Black,
                                    FitToCurve = true
                                };
            EditingMode = InkCanvasEditingMode.Ink;
            CurrentColorButton = new RibbonButton();
            CurrentColorButton.Background = new SolidColorBrush(Colors.Black);

            foreach(var color in _colors)
            {
                _fontColors.Add(new SolidColorBrush(color));

            }

            CurrentFontSize = 34;
            CurrentFontColor = _fontColors[0];
            CurrentFontFamily = Fonts[0];

            PageInteractionMode = PageInteractionMode.Pen;
        }

        private void InitializeCommands()
        {
            //File Menu
            NewNotebookCommand = new Command(OnNewNotebookCommandExecute);
            OpenNotebookCommand = new Command(OnOpenNotebookCommandExecute);
            EditNotebookCommand = new Command(OnEditNotebookCommandExecute);
            DoneEditingNotebookCommand = new Command(OnDoneEditingNotebookCommandExecute);
            SaveNotebookCommand = new Command(OnSaveNotebookCommandExecute);
            SaveAllNotebooksCommand = new Command(OnSaveAllNotebooksCommandExecute);
            SubmitNotebookToTeacherCommand = new Command(OnSubmitNotebookToTeacherCommandExecute);
            ConvertToXPSCommand = new Command(OnConvertToXPSCommandExecute);
            ConvertPageSubmissionToXPSCommand = new Command(OnConvertPageSubmissionToXPSCommandExecute);
            ConvertAllSubmissionsToXPSCommand = new Command(OnConvertAllSubmissionsToXPSCommandExecute);
            RefreshNetworkCommand = new Command(OnRefreshNetworkCommandExecute);
            ToggleThumbnailsCommand = new Command(OnToggleThumbnailsCommandExecute);
            ClearHistoryCommand = new Command(OnClearHistoryCommandExecute);
            DisableHistoryCommand = new Command(OnDisableHistoryCommandExecute);
            ExitCommand = new Command(OnExitCommandExecute);

            //Notebook
            PreviousPageCommand = new Command(OnPreviousPageCommandExecute);
            NextPageCommand = new Command(OnNextPageCommandExecute);

            //Tools
            SetSelectCommand = new Command(OnSetSelectCommandExecute);
            SetPenCommand = new Command(OnSetPenCommandExecute);
            SetHighlighterCommand = new Command(OnSetHighlighterCommandExecute);
            SetEraserCommand = new Command<string>(OnSetEraserCommandExecute);
            SetSnapTileCommand = new Command<RibbonToggleButton>(OnSetSnapTileCommandExecute);
            SetPenColorCommand = new Command<RibbonButton>(OnSetPenColorCommandExecute);

            //Submission
            SubmitPageCommand = new Command(OnSubmitPageCommandExecute);
            GroupSubmitPageCommand = new Command(OnGroupSubmitPageCommandExecute);

            //Displays
            SendDisplayToProjectorcommand = new Command(OnSendDisplayToProjectorcommandExecute);
            SwitchToLinkedDisplayCommand = new Command(OnSwitchToLinkedDisplayCommandExecute);
            CreateNewGridDisplayCommand = new Command(OnCreateNewGridDisplayCommandExecute);

            //History
            ReplayCommand = new Command(OnReplayCommandExecute);
            UndoCommand = new Command(OnUndoCommandExecute);
            RedoCommand = new Command(OnRedoCommandExecute);

            //Insert
            ToggleWebcamPanelCommand = new Command<bool>(OnToggleWebcamPanelCommandExecute);
            InsertImageCommand = new Command(OnInsertImageCommandExecute);
            InsertStaticImageCommand = new Command<string>(OnInsertStaticImageCommandExecute);
            InsertBlankStampCommand = new Command(OnInsertBlankStampCommandExecute);
            InsertImageStampCommand = new Command(OnInsertImageStampCommandExecute);
            InsertArrayCommand = new Command<string>(OnInsertArrayCommandExecute);
            InsertProtractorCommand = new Command(OnInsertProtractorCommandExecute);
            InsertSquareShapeCommand = new Command(OnInsertSquareShapeCommandExecute);
            InsertCircleShapeCommand = new Command(OnInsertCircleShapeCommandExecute);
            InsertHorizontalLineShapeCommand = new Command(OnInsertHorizontalLineShapeCommandExecute);
            InsertVerticalLineShapeCommand = new Command(OnInsertVerticalLineShapeCommandExecute);
            InsertTextBoxCommand = new Command(OnInsertTextBoxCommandExecute);
            InsertAggregationDataTableCommand = new Command(OnInsertAggregationDataTableCommandExecute);
            InsertAudioCommand = new Command(OnInsertAudioCommandExecute);
            InsertHandwritingRegionCommand = new Command(OnInsertHandwritingRegionCommandExecute);
            InsertInkShapeRegionCommand = new Command(OnInsertInkShapeRegionCommandExecute);
            InsertDataTableCommand = new Command(OnInsertDataTableCommandExecute);
            InsertShadingRegionCommand = new Command(OnInsertShadingRegionCommandExecute);
            InsertGroupingRegionCommand = new Command(OnInsertGroupingRegionCommandExecute);  
            
            //Testing
            TurnOffWebcamSharing = new Command(OnTurnOffWebcamSharingExecute);
            BroadcastPageCommand = new Command(OnBroadcastPageCommandExecute);
            ReplacePageCommand = new Command(OnReplacePageCommandExecute);
            RemoveAllSubmissionsCommand = new Command(OnRemoveAllSubmissionsCommandExecute);
            
            //Page
            AddNewPageCommand = new Command<string>(OnAddNewPageCommandExecute);
            SwitchPageLayoutCommand = new Command(OnSwitchPageLayoutCommandExecute);
            DeletePageCommand = new Command(OnDeletePageCommandExecute);
            CopyPageCommand = new Command(OnCopyPageCommandExecute);
            AddPageTopicCommand = new Command(OnAddPageTopicCommandExecute);
            MakePageLongerCommand = new Command(OnMakePageLongerCommandExecute);
            TrimPageCommand = new Command(OnTrimPageCommandExecute);

            //Debug
            InterpretPageCommand = new Command(OnInterpretPageCommandExecute);
            UpdateObjectPropertiesCommand = new Command(OnUpdateObjectPropertiesCommandExecute);
            ZoomToPageWidthCommand = new Command(OnZoomToPageWidthCommandExecute);
            ZoomToWholePageCommand = new Command(OnZoomToWholePageCommandExecute);   
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "RibbonVM"; } }

        #region Properties

        /// <summary>
        /// Minimizes Ribbon.
        /// </summary>
        public bool IsMinimized
        {
            get { return GetValue<bool>(IsMinimizedProperty); }
            set { SetValue(IsMinimizedProperty, value); }
        }

        public static readonly PropertyData IsMinimizedProperty = RegisterProperty("IsMinimized", typeof(bool), false);

        //Steve - Dont' want Views in ViewModels, can this be fixed?
        public CLPTextBoxView LastFocusedTextBox = null;

        /// <summary>
        /// Size of the Pen.
        /// </summary>
        public double PenSize
        {
            get { return GetValue<double>(PenSizeProperty); }
            set { SetValue(PenSizeProperty, value); }
        }

        public static readonly PropertyData PenSizeProperty = RegisterProperty("PenSize", typeof(double), 3);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string EraserType
        {
            get { return GetValue<string>(EraserTypeProperty); }
            set { SetValue(EraserTypeProperty, value); }
        }

        public static readonly PropertyData EraserTypeProperty = RegisterProperty("EraserType", typeof(string), "Stroke Eraser", (sender, e) => ((RibbonViewModel)sender).OnEraserTypeChanged());

        /// <summary>
        /// Called when the EraserType property has changed.
        /// </summary>
        private void OnEraserTypeChanged()
        {
            if(EraserType == "Point Eraser")
            {
                EraserMode = InkCanvasEditingMode.EraseByPoint;
            }
            else if(EraserType == "Stroke Eraser")
            {
                EraserMode = InkCanvasEditingMode.EraseByStroke;
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public InkCanvasEditingMode EraserMode
        {
            get { return GetValue<InkCanvasEditingMode>(EraserModeProperty); }
            set { SetValue(EraserModeProperty, value); }
        }

        public static readonly PropertyData EraserModeProperty = RegisterProperty("EraserMode", typeof(InkCanvasEditingMode), InkCanvasEditingMode.EraseByStroke);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool ThumbnailsTop
        {
            get { return GetValue<bool>(ThumbnailsTopProperty); }
            set { SetValue(ThumbnailsTopProperty, value); }
        }

        public static readonly PropertyData ThumbnailsTopProperty = RegisterProperty("ThumbnailsTop", typeof(bool), false);

        /// <summary>
        /// Gets the DrawingAttributes of the Ribbon.
        /// </summary>
        public DrawingAttributes DrawingAttributes
        {
            get { return GetValue<DrawingAttributes>(DrawingAttributesProperty); }
            private set { SetValue(DrawingAttributesProperty, value); }
        }

        public static readonly PropertyData DrawingAttributesProperty = RegisterProperty("DrawingAttributes", typeof(DrawingAttributes));

        /// <summary>
        /// Gets or sets the EditingMode for the InkCanvas.
        /// </summary>
        public InkCanvasEditingMode EditingMode
        {
            get { return GetValue<InkCanvasEditingMode>(EditingModeProperty); }
            set { SetValue(EditingModeProperty, value); }
        }

        public static readonly PropertyData EditingModeProperty = RegisterProperty("EditingMode", typeof(InkCanvasEditingMode));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public PageInteractionMode PageInteractionMode
        {
            get { return GetValue<PageInteractionMode>(PageInteractionModeProperty); }
            set { SetValue(PageInteractionModeProperty, value); }
        }

        public static readonly PropertyData PageInteractionModeProperty = RegisterProperty("PageInteractionMode", typeof(PageInteractionMode));

        /// <summary>
        /// Enables pictures taken with Webcam to be shared with Group Members.
        /// </summary>
        public bool AllowWebcamShare
        {
            get { return GetValue<bool>(AllowWebcamShareProperty); }
            set { SetValue(AllowWebcamShareProperty, value); }
        }

        public static readonly PropertyData AllowWebcamShareProperty = RegisterProperty("AllowWebcamShare", typeof(bool), true);

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

        public static readonly PropertyData SideBarVisibilityProperty = RegisterProperty("SideBarVisibility", typeof(bool), true);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool GridDisplaysVisibility
        {
            get { return GetValue<bool>(GridDisplaysVisibilityProperty); }
            set
            {
                SetValue(GridDisplaysVisibilityProperty, value);
                if(App.MainWindowViewModel != null && App.MainWindowViewModel.SelectedWorkspace != null && App.MainWindowViewModel.SelectedWorkspace is NotebookWorkspaceViewModel)
                {
                    (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).RightPanel = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).DisplayListPanel;
                    (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).RightPanel.IsVisible = value;
                }
            }
        }

        public static readonly PropertyData GridDisplaysVisibilityProperty = RegisterProperty("GridDisplaysVisibility", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool BroadcastInkToStudents
        {
            get { return GetValue<bool>(BroadcastInkToStudentsProperty); }
            set { SetValue(BroadcastInkToStudentsProperty, value); }
        }

        public static readonly PropertyData BroadcastInkToStudentsProperty = RegisterProperty("BroadcastInkToStudents", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool BlockStudentPenInput
        {
            get { return GetValue<bool>(BlockStudentPenInputProperty); }
            set 
            { 
                SetValue(BlockStudentPenInputProperty, value); 
            
                if(App.Network.ClassList.Count > 0)
                {
                    foreach(Person student in App.Network.ClassList)
                    {
                        try
                        {
                            NetTcpBinding binding = new NetTcpBinding();
                            binding.Security.Mode = SecurityMode.None;
                            IStudentContract StudentProxy = ChannelFactory<IStudentContract>.CreateChannel(binding, new EndpointAddress(student.CurrentMachineAddress));
                            StudentProxy.TogglePenDownMode(value);
                            (StudentProxy as ICommunicationObject).Close();
                        }
                        catch(System.Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                else
                {
                    Logger.Instance.WriteToLog("No Students Found");
                }
            }
        }

        public static readonly PropertyData BlockStudentPenInputProperty = RegisterProperty("BlockStudentPenInput", typeof(bool), false);

        #region Convert to XAMLS?

        /// <summary>
        /// Visibility of Authoring Tab.
        /// </summary>
        public Visibility AuthoringTabVisibility
        {
            get { return GetValue<Visibility>(AuthoringTabVisibilityProperty); }
            set { SetValue(AuthoringTabVisibilityProperty, value); }
        }

        public static readonly PropertyData AuthoringTabVisibilityProperty = RegisterProperty("AuthoringTabVisibility", typeof(Visibility), Visibility.Collapsed);

        /// <summary>
        /// Visibility of Debug Tab.
        /// </summary>
        public Visibility DebugTabVisibility
        {
            get { return GetValue<Visibility>(DebugTabVisibilityProperty); }
            set { SetValue(DebugTabVisibilityProperty, value); }
        }

        public static readonly PropertyData DebugTabVisibilityProperty = RegisterProperty("DebugTabVisibility", typeof(Visibility), Visibility.Collapsed);

        /// <summary>
        /// Visibility of Extras Tab
        /// </summary>
        public Visibility ExtrasTabVisibility
        {
            get { return GetValue<Visibility>(ExtrasTabVisibilityProperty); }
            set { SetValue(ExtrasTabVisibilityProperty, value); }
        }

        public static readonly PropertyData ExtrasTabVisibilityProperty = RegisterProperty("ExtrasTabVisibility", typeof(Visibility), Visibility.Collapsed);

        /// <summary>
        /// Instructor Only Items Visibility.
        /// </summary>
        public Visibility InstructorVisibility
        {
            get { return GetValue<Visibility>(InstructorVisibilityProperty); }
            set { SetValue(InstructorVisibilityProperty, value); }
        }

        public static readonly PropertyData InstructorVisibilityProperty = RegisterProperty("InstructorVisibility", typeof(Visibility), Visibility.Collapsed);

        /// <summary>
        /// Server Only Items Visibility.
        /// </summary>
        public Visibility ServerVisibility
        {
            get { return GetValue<Visibility>(ServerVisibilityProperty); }
            set { SetValue(ServerVisibilityProperty, value); }
        }

        public static readonly PropertyData ServerVisibilityProperty = RegisterProperty("ServerVisibility", typeof(Visibility), Visibility.Collapsed);

        /// <summary>
        /// Student Only Items Visibility.
        /// </summary>
        public Visibility StudentVisibility
        {
            get { return GetValue<Visibility>(StudentVisibilityProperty); }
            set { SetValue(StudentVisibilityProperty, value); }
        }

        public static readonly PropertyData StudentVisibilityProperty = RegisterProperty("StudentVisibility", typeof(Visibility), Visibility.Collapsed);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public RibbonButton CurrentColorButton
        {
            get { return GetValue<RibbonButton>(CurrentColorButtonProperty); }
            set { SetValue(CurrentColorButtonProperty, value); }
        }

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
                if(LastFocusedTextBox != null)
                {
                    if(!LastFocusedTextBox.isUpdatingVisualState)
                    {
                        LastFocusedTextBox.SetFont(-1.0, value, null);
                    }
                }
            }
        }

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
                if(LastFocusedTextBox != null)
                {
                    if(!LastFocusedTextBox.isUpdatingVisualState)
                    {
                        LastFocusedTextBox.SetFont(CurrentFontSize, null, null);
                    }
                }
            }
        }

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
                if(LastFocusedTextBox != null)
                {
                    if(!LastFocusedTextBox.isUpdatingVisualState)
                    {
                        LastFocusedTextBox.SetFont(-1.0, null, CurrentFontColor);
                    }
                }
            }
        }

        public static readonly PropertyData CurrentFontColorProperty = RegisterProperty("CurrentFontColor", typeof(Brush));

        #endregion //TextBox

        #endregion //Bindings

        #region Commands

        #region File Menu

        /// <summary>
        /// Creates a new notebook.
        /// </summary>
        public Command NewNotebookCommand { get; private set; }

        private void OnNewNotebookCommandExecute()
        {
            CLPServiceAgent.Instance.OpenNewNotebook();
            (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages[0];
        }

        /// <summary>
        /// Opens a notebook from the Notebooks folder.
        /// </summary>
        public Command OpenNotebookCommand { get; private set; }

        private void OnOpenNotebookCommandExecute()
        {
            MainWindow.SelectedWorkspace = new NotebookChooserWorkspaceViewModel();
        }

        //TODO: Steve - Combine with DoneEditing to make ToggleEditingMode
        /// <summary>
        /// Puts current notebook in Authoring Mode.
        /// </summary>
        public Command EditNotebookCommand { get; private set; }

        private void OnEditNotebookCommandExecute()
        {
            MainWindow.IsAuthoring = true;

            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel != null)
            {
                var currentPage = notebookWorkspaceViewModel.CurrentPage;
                CLPPageViewModel.ClearAdorners(currentPage);
            }
        }

        /// <summary>
        /// Leaves Authoring Mode.
        /// </summary>
        public Command DoneEditingNotebookCommand { get; private set; }

        private void OnDoneEditingNotebookCommandExecute()
        {
            MainWindow.IsAuthoring = false;

            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel != null) 
            {
                var currentPage = notebookWorkspaceViewModel.CurrentPage;
                CLPPageViewModel.ClearAdorners(currentPage);
            }
        }

        /// <summary>
        /// Saves the current notebook to disk.
        /// </summary>
        public Command SaveNotebookCommand { get; private set; }

        private void OnSaveNotebookCommandExecute()
        {
            if(App.MainWindowViewModel.SelectedWorkspace is NotebookWorkspaceViewModel)
            {
                Catel.Windows.PleaseWaitHelper.Show(() =>
                    CLPServiceAgent.Instance.SaveNotebook((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook), null, "Saving Notebook", 0.0 / 0.0);
            }
        }

        /// <summary>
        /// Saves all open notebooks to disk.
        /// </summary>
        public Command SaveAllNotebooksCommand { get; private set; }

        private void OnSaveAllNotebooksCommandExecute()
        {
            foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
            {
                CLPServiceAgent.Instance.SaveNotebook(notebook);
            }
        }

        /// <summary>
        /// Submits the entirety of a student's current notebook to the teacher to save on her desktop.
        /// </summary>
        public Command SubmitNotebookToTeacherCommand { get; private set; }

        private void OnSubmitNotebookToTeacherCommandExecute()
        {
            if(App.MainWindowViewModel.SelectedWorkspace is NotebookWorkspaceViewModel)
            {
                CLPNotebook notebook = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook;

                if(App.Network.InstructorProxy != null)
                {
                    try
                    {
                        string sNotebook = ObjectSerializer.ToString(notebook);

                        App.Network.InstructorProxy.CollectStudentNotebook(sNotebook, App.Network.CurrentUser.FullName);
                    }
                    catch(Exception)
                    {

                    }
                }
                else
                {
                    Console.WriteLine("Instructor NOT Available");
                }
            }
        }

        /// <summary>
        /// Converts Notebook Pages to XPS.
        /// </summary>
        public Command ConvertToXPSCommand { get; private set; }

        private void OnConvertToXPSCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            Catel.Windows.PleaseWaitHelper.Show(() =>
            {
                var notebook = notebookWorkspaceViewModel.Notebook;
                string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\";
                if(!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string fileName = notebook.NotebookName + ".xps";
                string filePath = directoryPath + fileName;
                if(File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                var document = new FixedDocument();
                document.DocumentPaginator.PageSize = new Size(96 * 11, 96 * 8.5);

                foreach(CLPPage page in notebook.Pages)
                {
                    page.TrimPage();
                    double printHeight = page.PageWidth / page.PageAspectRatio;

                    double transformAmount = 0;
                    do
                    {
                        var currentPageView = new CLPPagePreviewView { DataContext = page };
                        currentPageView.UpdateLayout();
                        var transform = new TransformGroup();
                        var translate = new TranslateTransform(0, -transformAmount);
                        transform.Children.Add(translate);
                        if(page.PageWidth == CLPPage.LANDSCAPE_WIDTH)
                        {
                            var rotate = new RotateTransform(90.0);
                            var translate2 = new TranslateTransform(816, 0);
                            transform.Children.Add(rotate);
                            transform.Children.Add(translate2);
                        }
                        currentPageView.RenderTransform = transform;
                        transformAmount += printHeight;

                        var pageContent = new PageContent();
                        var fixedPage = new FixedPage();
                        fixedPage.Children.Add(currentPageView);

                        ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
                        document.Pages.Add(pageContent);
                    } while(page.PageHeight > transformAmount);
                }

                //Save the document
                var xpsDocument = new XpsDocument(filePath, FileAccess.ReadWrite);
                var documentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                documentWriter.Write(document);
                xpsDocument.Close();

            }, null, "Converting Notebook to XPS", 0.0 / 0.0);
        }

        /// <summary>
        /// Converts the Submissions of the currently selected page to XPS.
        /// </summary>
        public Command ConvertPageSubmissionToXPSCommand { get; private set; }

        private void OnConvertPageSubmissionToXPSCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            Catel.Windows.PleaseWaitHelper.Show(() =>
            {
                var notebook = notebookWorkspaceViewModel.Notebook;
                string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\";
                if(!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string fileName = notebook.NotebookName + " - Page " + notebookWorkspaceViewModel.CurrentPage.PageIndex + " Submissions.xps";
                string filePath = directoryPath + fileName;
                if(File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                if(!notebook.Submissions[notebookWorkspaceViewModel.CurrentPage.UniqueID].Any())
                {
                    return;
                }

                var document = new FixedDocument();
                document.DocumentPaginator.PageSize = new Size(96 * 11, 96 * 8.5);

                foreach(CLPPage page in notebook.Submissions[notebookWorkspaceViewModel.CurrentPage.UniqueID])
                {
                    page.TrimPage();
                    double printHeight = page.PageWidth / page.PageAspectRatio;

                    double transformAmount = 0;
                    do
                    {
                        var currentPageView = new CLPPagePreviewView { DataContext = page };
                        currentPageView.UpdateLayout();

                        var grid = new Grid();
                        grid.Children.Add(currentPageView);
                        var label = new Label
                        {
                            FontSize = 20,
                            FontWeight = FontWeights.Bold,
                            FontStyle = FontStyles.Oblique,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Top,
                            Content = page.SubmitterName,
                            Margin = new Thickness(0, transformAmount + 5, 5, 0)
                        };
                        grid.Children.Add(label);
                        grid.UpdateLayout();

                        var transform = new TransformGroup();
                        var translate = new TranslateTransform(0, -transformAmount);
                        transform.Children.Add(translate);
                        if(page.PageWidth == CLPPage.LANDSCAPE_WIDTH)
                        {
                            var rotate = new RotateTransform(90.0);
                            var translate2 = new TranslateTransform(816, 0);
                            transform.Children.Add(rotate);
                            transform.Children.Add(translate2);
                        }
                        grid.RenderTransform = transform;
                        transformAmount += printHeight;

                        var pageContent = new PageContent();
                        var fixedPage = new FixedPage();
                        fixedPage.Children.Add(grid);

                        ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
                        document.Pages.Add(pageContent);
                    } while(page.PageHeight > transformAmount);
                }

                //Save the document
                var xpsDocument = new XpsDocument(filePath, FileAccess.ReadWrite);
                var documentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                documentWriter.Write(document);
                xpsDocument.Close();
            }, null, "Converting Submissions for this page to XPS", 0.0 / 0.0);
        }

        /// <summary>
        /// Converts all Submissions in a notebook to XPS.
        /// </summary>
        public Command ConvertAllSubmissionsToXPSCommand { get; private set; }

        private void OnConvertAllSubmissionsToXPSCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            Catel.Windows.PleaseWaitHelper.Show(() =>
            {
                var notebook = notebookWorkspaceViewModel.Notebook;
                string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\";
                if(!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string fileName = notebook.NotebookName + " - All Submissions.xps";
                string filePath = directoryPath + fileName;
                if(File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                var document = new FixedDocument();
                document.DocumentPaginator.PageSize = new Size(96 * 11, 96 * 8.5);

                foreach(CLPPage page in notebook.Submissions.Keys.SelectMany(pageID => notebook.Submissions[pageID]))
                {
                    page.TrimPage();
                    double printHeight = page.PageWidth / page.PageAspectRatio;

                    double transformAmount = 0;
                    do
                    {
                        var currentPageView = new CLPPagePreviewView { DataContext = page };
                        currentPageView.UpdateLayout();

                        var grid = new Grid();
                        grid.Children.Add(currentPageView);
                        var label = new Label
                        {
                            FontSize = 20,
                            FontWeight = FontWeights.Bold,
                            FontStyle = FontStyles.Oblique,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Top,
                            Content = page.SubmitterName,
                            Margin = new Thickness(0, transformAmount + 5, 5, 0)
                        };
                        grid.Children.Add(label);
                        grid.UpdateLayout();

                        var transform = new TransformGroup();
                        var translate = new TranslateTransform(0, -transformAmount);
                        transform.Children.Add(translate);
                        if(page.PageWidth == CLPPage.LANDSCAPE_WIDTH)
                        {
                            var rotate = new RotateTransform(90.0);
                            var translate2 = new TranslateTransform(816, 0);
                            transform.Children.Add(rotate);
                            transform.Children.Add(translate2);
                        }
                        grid.RenderTransform = transform;
                        transformAmount += printHeight;

                        var pageContent = new PageContent();
                        var fixedPage = new FixedPage();
                        fixedPage.Children.Add(grid);

                        ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
                        document.Pages.Add(pageContent);
                    } while(page.PageHeight > transformAmount);
                }

                //Save the document
                var xpsDocument = new XpsDocument(filePath, FileAccess.ReadWrite);
                var documentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                documentWriter.Write(document);
                xpsDocument.Close();
            }, null, "Converting Submissions for this page to XPS", 0.0 / 0.0);
        }

        /// <summary>
        /// Reconnects to the network.
        /// </summary>
        public Command RefreshNetworkCommand { get; private set; }

        private void OnRefreshNetworkCommandExecute()
        {
            CanSendToTeacher = true;
            CLPServiceAgent.Instance.NetworkReconnect();
        }

        /// <summary>
        /// Switchs NotebookPage filmstrip to portrait mode and back.
        /// </summary>
        public Command ToggleThumbnailsCommand { get; private set; }

        private void OnToggleThumbnailsCommandExecute()
        {
            ThumbnailsTop = (ThumbnailsTop == false);
        }

        /// <summary>
        /// Completely clears all histories in a notebook.
        /// </summary>
        public Command ClearHistoryCommand { get; private set; }

        private void OnClearHistoryCommandExecute()
        {
            if(App.MainWindowViewModel.SelectedWorkspace is NotebookWorkspaceViewModel)
            {
                CLPNotebook notebook = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook;
                foreach(CLPPage page in notebook.Pages)
                {
                    page.PageHistory.ClearHistory();
                }
            }
        }

        /// <summary>
        /// Prevents history from storing actions.
        /// </summary>
        public Command DisableHistoryCommand { get; private set; }

        private void OnDisableHistoryCommandExecute()
        {
            if(App.MainWindowViewModel.SelectedWorkspace is NotebookWorkspaceViewModel)
            {
                CLPNotebook notebook = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook;
                foreach(CLPPage page in notebook.Pages)
                {
                    page.PageHistory._useHistory = false;
                }
            }
        }

        /// <summary>
        /// Exits the program.
        /// </summary>
        public Command ExitCommand { get; private set; }

        private void OnExitCommandExecute()
        {
            if(MessageBox.Show("Are you sure you want to exit?",
                                        "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                CLPServiceAgent.Instance.Exit();
            }
        }

        #endregion //File Menu

        #region Notebook Commands

        /// <summary>
        /// Navigates to previous page in the notebook.
        /// </summary>
        public Command PreviousPageCommand { get; private set; }

        private void OnPreviousPageCommandExecute()
        {
            CLPPage currentPage = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
            int index = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.IndexOf(currentPage);

            if(index > 0)
            {
                (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages[index - 1];
            }
        }

        /// <summary>
        /// Navigates to the next page in the notebook.
        /// </summary>
        public Command NextPageCommand { get; private set; }

        private void OnNextPageCommandExecute()
        {
            CLPPage currentPage = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
            int index = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.IndexOf(currentPage);

            if(index < (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.Count - 1)
            {
                (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages[index + 1];
            }
        }

        #endregion //Notebook Commands

        #region Tool Commands

        /// <summary>
        /// Set Select Mode.
        /// </summary>
        public Command SetSelectCommand { get; private set; }

        private void OnSetSelectCommandExecute()
        {
            PageInteractionMode = PageInteractionMode.Select;
        }

        /// <summary>
        /// Set Pen/Inking mode.
        /// </summary>
        public Command SetPenCommand { get; private set; }

        private void OnSetPenCommandExecute()
        {
            EditingMode = InkCanvasEditingMode.Ink;
            PageInteractionMode = PageInteractionMode.Pen;
            DrawingAttributes.IsHighlighter = false;
            DrawingAttributes.Height = PenSize;
            DrawingAttributes.Width = PenSize;
            DrawingAttributes.StylusTip = StylusTip.Ellipse;
        }

        /// <summary>
        /// Sets Highlighter Mode.
        /// </summary>
        public Command SetHighlighterCommand { get; private set; }

        private void OnSetHighlighterCommandExecute()
        {
            EditingMode = InkCanvasEditingMode.Ink;
            PageInteractionMode = PageInteractionMode.Highlighter;
            DrawingAttributes.IsHighlighter = true;
            DrawingAttributes.Height = 12;
            DrawingAttributes.Width = 12;
            DrawingAttributes.StylusTip = StylusTip.Rectangle;
        }

        /// <summary>
        /// Sets the Eraser mode for the back of the Pen.
        /// </summary>
        public Command<string> SetEraserCommand { get; private set; }

        private void OnSetEraserCommandExecute(string eraserStyle)
        {
            if(eraserStyle == "EraseByPoint")
            {
                EditingMode = InkCanvasEditingMode.EraseByPoint;
            }
            else if(eraserStyle == "EraseByStroke")
            {
                EditingMode = InkCanvasEditingMode.EraseByStroke;
            }
        }

        /// <summary>
        /// Sets Tile Mode.
        /// </summary>
        public Command<RibbonToggleButton> SetSnapTileCommand { get; private set; }

        private void OnSetSnapTileCommandExecute(RibbonToggleButton button)
        {
            EditingMode = InkCanvasEditingMode.None;
            PageInteractionMode = PageInteractionMode.Tile;
        }

        /// <summary>
        /// Sets the Pen Color.
        /// </summary>
        public Command<RibbonButton> SetPenColorCommand { get; private set; }

        private void OnSetPenColorCommandExecute(RibbonButton button)
        {
            CurrentColorButton = button as RibbonButton;
            DrawingAttributes.Color = (CurrentColorButton.Background as SolidColorBrush).Color;

            EditingMode = InkCanvasEditingMode.Ink;
            if(!(PageInteractionMode == PageInteractionMode.Pen || PageInteractionMode == PageInteractionMode.Highlighter))
            {
                PageInteractionMode = PageInteractionMode.Pen;
            }
        }

        #endregion //Tool Commands

        #region Submission Commands

        /// <summary>
        /// Submits the current page as an individual.
        /// </summary>
        public Command SubmitPageCommand { get; private set; }

        private void OnSubmitPageCommandExecute()
        {
            //TODO: Steve - change to different thread and do callback to make sure sent page has arrived
            IsSending = true;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;

            if(CanSendToTeacher)
            {
                CLPPage page = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
                page.SerializedStrokes = CLPPage.SaveInkStrokes(page.InkStrokes);
                CLPNotebook notebook = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook;

                CLPServiceAgent.Instance.SubmitPage(page, notebook.UniqueID, false);

                CLPPage submission = page.Clone() as CLPPage;
                submission.InkStrokes = CLPPage.LoadInkStrokes(submission.SerializedStrokes);

                if(notebook != null && submission != null)
                {
                    submission.IsSubmission = true;
                    foreach (var pageObject in submission.PageObjects)
                    {
                        pageObject.ParentPage = submission;
                    }
                    notebook.AddStudentSubmission(submission.UniqueID, submission);
                }
            }
            CanSendToTeacher = false;
        }

        /// <summary>
        /// Submits the current page as a group.
        /// </summary>
        public Command GroupSubmitPageCommand { get; private set; }

        private void OnGroupSubmitPageCommandExecute()
        {
            IsSending = true;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;

            if(CanGroupSendToTeacher)
            {
                CLPPage page = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
                CLPServiceAgent.Instance.SubmitPage(page, (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.UniqueID, true);
            }
            CanGroupSendToTeacher = false;
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool CanSendToTeacher
        {
            get { return GetValue<bool>(CanSendToTeacherProperty); }
            set { SetValue(CanSendToTeacherProperty, value); }
        }

        public static readonly PropertyData CanSendToTeacherProperty = RegisterProperty("CanSendToTeacher", typeof(bool), true);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool CanGroupSendToTeacher
        {
            get { return GetValue<bool>(CanGroupSendToTeacherProperty); }
            set { SetValue(CanGroupSendToTeacherProperty, value); }
        }

        public static readonly PropertyData CanGroupSendToTeacherProperty = RegisterProperty("CanGroupSendToTeacher", typeof(bool), true);

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var timer = sender as System.Timers.Timer;
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
                if(IsSending)
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

        public static readonly PropertyData IsSendingProperty = RegisterProperty("IsSending", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility SendButtonVisibility
        {
            get { return GetValue<Visibility>(SendButtonVisibilityProperty); }
            set { SetValue(SendButtonVisibilityProperty, value); }
        }

        public static readonly PropertyData SendButtonVisibilityProperty = RegisterProperty("SendButtonVisibility", typeof(Visibility), Visibility.Visible);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Visibility IsSentInfoVisibility
        {
            get { return GetValue<Visibility>(IsSentInfoVisibilityProperty); }
            set { SetValue(IsSentInfoVisibilityProperty, value); }
        }

        public static readonly PropertyData IsSentInfoVisibilityProperty = RegisterProperty("IsSentInfoVisibility", typeof(Visibility), Visibility.Collapsed);

        #endregion //Submission Command

        #region Display Commands

        /// <summary>
        /// Sends the current Display to the projector.
        /// </summary>
        public Command SendDisplayToProjectorcommand { get; private set; }

        private void OnSendDisplayToProjectorcommandExecute()
        {
            if(App.Network.ProjectorProxy != null)
            {
                (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).LinkedDisplay.IsOnProjector = false;
                foreach(var gridDisplay in (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).GridDisplays)
                {
                    gridDisplay.IsOnProjector = false;
                }

                (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay.IsOnProjector = true;
                (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).WorkspaceBackgroundColor = new SolidColorBrush(Colors.PaleGreen);

                List<string> pageIDs = new List<string>();
                if((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay is LinkedDisplayViewModel)
                {
                    CLPPage page = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
                    string pageID;
                    if(page.IsSubmission)
                    {
                        pageID = page.SubmissionID;
                    }
                    else
                    {
                        pageID = page.UniqueID;
                    }
                    pageIDs.Add(pageID);
                    try
                    {
                        App.Network.ProjectorProxy.SwitchProjectorDisplay((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay.DisplayName, pageIDs);
                    }
                    catch(System.Exception)
                    {

                    }
                }
                else
                {
                    foreach(var page in ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as GridDisplayViewModel).DisplayedPages)
                    {
                        if(page.IsSubmission)
                        {
                            pageIDs.Add(page.SubmissionID);
                        }
                        else
                        {
                            pageIDs.Add(page.UniqueID);
                        }
                    }
                    try
                    {
                        App.Network.ProjectorProxy.SwitchProjectorDisplay((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay.DisplayID, pageIDs);
                    }
                    catch(System.Exception)
                    {

                    }
                }
            }
            else
            {
                Console.WriteLine("Projector NOT Available");
            }
        }

        /// <summary>
        /// Switches to the Mirror Display.
        /// </summary>
        public Command SwitchToLinkedDisplayCommand { get; private set; }

        private void OnSwitchToLinkedDisplayCommandExecute()
        {
            (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).LinkedDisplay;
            if((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay.IsOnProjector)
            {
                (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).WorkspaceBackgroundColor = new SolidColorBrush(Colors.PaleGreen);
            }
            else
            {
                (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
            }
        }

        /// <summary>
        /// Creates a new Grid Display.
        /// </summary>
        public Command CreateNewGridDisplayCommand { get; private set; }

        private void OnCreateNewGridDisplayCommandExecute()
        {
            (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).GridDisplays.Add(new GridDisplayViewModel());
            //(App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay = null;
            (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).GridDisplays[(App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).GridDisplays.Count - 1];
            (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).WorkspaceBackgroundColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F3F3F3"));
        }

        #endregion //Display Commands

        #region HistoryCommands

        /// <summary>
        /// Replays the entire history of the current page.
        /// </summary>
        public Command ReplayCommand { get; private set; }

        private void OnReplayCommandExecute()
        {
            Thread t = new Thread(() =>
            {
                Console.WriteLine("Replay");
                try
                {
                    CLPPage page = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
                    CLPHistory pageHistory = page.PageHistory;

                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (DispatcherOperationCallback)delegate(object arg)
                    {
                        pageHistory.Freeze();
                        return null;
                    }, null);

                    Stack<CLPHistoryItem> metaFuture = new Stack<CLPHistoryItem>();
                    Stack<CLPHistoryItem> metaPast = new Stack<CLPHistoryItem>(new Stack<CLPHistoryItem>(pageHistory.MetaPast));

                    while(metaPast.Count > 0)
                    {
                        CLPHistoryItem item = metaPast.Pop();
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (DispatcherOperationCallback)delegate(object arg)
                        {
                            if(item != null) // TODO (caseymc): find out why one of these would ever be null and fix
                            {
                                item.Undo(page);
                            }
                            return null;
                        }, null);
                        metaFuture.Push(item);
                    }

                    Console.WriteLine("done undoing");
                    Thread.Sleep(400);
                    while(metaFuture.Count > 0)
                    {
                        CLPHistoryItem item = metaFuture.Pop();
                        if(item.ItemType == HistoryItemType.MoveObject || item.ItemType == HistoryItemType.ResizeObject)
                        {
                            Thread.Sleep(50); // make intervals between move-steps less painfully slow
                        }
                        else
                        {
                            Thread.Sleep(400);
                        }
                        Console.WriteLine("This is the action being REDONE: " + item.ItemType);
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (DispatcherOperationCallback)delegate(object arg)
                        {
                            if(item != null)
                            {
                                item.Redo(page);
                            }
                            return null;
                        }, null);
                    }
                    Thread.Sleep(400);
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (DispatcherOperationCallback)delegate(object arg)
                    {
                        pageHistory.Unfreeze();
                        return null;
                    }, null);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            t.Start();
        }

        /// <summary>
        /// Undoes the last action.
        /// </summary>
        public Command UndoCommand { get; private set; }

        private void OnUndoCommandExecute()
        {
            CLPPage page = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
            Thread tx = new Thread(() =>
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (DispatcherOperationCallback)delegate(object arg)
                    {
                        page.PageHistory.Undo(page);
                        return null;
                    }, null);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            tx.Start();
        }

        /// <summary>
        /// Redoes the last undone action.
        /// </summary>
        public Command RedoCommand { get; private set; }

        private void OnRedoCommandExecute()
        {
            CLPPage page = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
            Thread ty = new Thread(() =>
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (DispatcherOperationCallback)delegate(object arg)
                    {
                        page.PageHistory.Redo(page);
                        return null;
                    }, null);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            ty.Start();
        }

        #endregion //History Commands

        #region Testing

        /// <summary>
        /// Enables pictures taken with Webcam to be shared with Group Members.
        /// </summary>
        public Command TurnOffWebcamSharing { get; private set; }

        private void OnTurnOffWebcamSharingExecute()
        {
            AllowWebcamShare = false;
        }

        /// <summary>
        /// Broadcast the current page of a MirrorDisplay to all connected Students.
        /// </summary>
        public Command BroadcastPageCommand { get; private set; }

        private void OnBroadcastPageCommandExecute()
        {
            //TODO: Steve - also broadcast to Projector
            CLPPage page = ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;
            string s_page = ObjectSerializer.ToString(page);
            int index = page.PageIndex - 1;

            if(App.Network.ClassList.Any())
            {
                foreach(Person student in App.Network.ClassList)
                {
                    try
                    {
                        IStudentContract StudentProxy = ChannelFactory<IStudentContract>.CreateChannel(App.Network.DefaultBinding, new EndpointAddress(student.CurrentMachineAddress));
                        StudentProxy.AddNewPage(s_page, index);
                        (StudentProxy as ICommunicationObject).Close();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                Logger.Instance.WriteToLog("No Students Found");
            }
        }

        /// <summary>
        /// Replaces the current page on all Student Machines.
        /// </summary>
        public Command ReplacePageCommand { get; private set; }

        private void OnReplacePageCommandExecute()
        {
            //TODO: Steve - also broadcast to Projector
            CLPPage page = ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;
            string s_page = ObjectSerializer.ToString(page);
            int index = page.PageIndex - 1;

            if(App.Network.ClassList.Count > 0)
            {
                foreach(Person student in App.Network.ClassList)
                {
                    try
                    {
                        IStudentContract StudentProxy = ChannelFactory<IStudentContract>.CreateChannel(App.Network.DefaultBinding, new EndpointAddress(student.CurrentMachineAddress));
                        StudentProxy.ReplacePage(s_page, index);
                        (StudentProxy as ICommunicationObject).Close();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                Logger.Instance.WriteToLog("No Students Found");
            }
        }

        /// <summary>
        /// Removes all the submissions on a notebook, making it essentially a Student Notebook.
        /// </summary>
        public Command RemoveAllSubmissionsCommand { get; private set; }

        private void OnRemoveAllSubmissionsCommandExecute()
        {
            CLPNotebook notebook = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook;
            foreach(ObservableCollection<CLPPage> pages in notebook.Submissions.Values)
            {
                pages.Clear();
            }
            foreach(CLPPage page in notebook.Pages)
            {
                page.NumberOfSubmissions = 0;
                page.NumberOfGroupSubmissions = 0;
            }
        }

        #endregion //Testing

        #region Page Commands

        /// <summary>
        /// Adds a new page to the notebook.
        /// </summary>
        public Command<string> AddNewPageCommand { get; private set; }

        private void OnAddNewPageCommandExecute(string pageOrientation)
        {
            //Steve - clpserviceagent
            int index = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.IndexOf(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            index++;
            CLPPage page = new CLPPage();
            if(pageOrientation == "Portrait")
            {
                page.PageHeight = CLPPage.PORTRAIT_HEIGHT;
                page.PageWidth = CLPPage.PORTRAIT_WIDTH;
                page.PageAspectRatio = page.PageWidth / page.PageHeight;
            }
            page.ParentNotebookID = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.UniqueID;
            (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.InsertPageAt(index, page);
        }

        /// <summary>
        /// Converts current page between landscape and portrait.
        /// </summary>
        public Command SwitchPageLayoutCommand { get; private set; }

        private void OnSwitchPageLayoutCommandExecute()
        {
            CLPPage page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;

            if(page.PageAspectRatio == CLPPage.LANDSCAPE_WIDTH / CLPPage.LANDSCAPE_HEIGHT)
            {
                foreach(ICLPPageObject pageObject in page.PageObjects)
                {
                    if (pageObject.XPosition + pageObject.Width > CLPPage.PORTRAIT_WIDTH)
                    {
                        pageObject.XPosition = CLPPage.PORTRAIT_WIDTH - pageObject.Width;
                    }
                    if(pageObject.YPosition + pageObject.Height > CLPPage.PORTRAIT_HEIGHT)
                    {
                        pageObject.YPosition = CLPPage.PORTRAIT_HEIGHT - pageObject.Height;
                    }
                }

                page.PageWidth = CLPPage.PORTRAIT_WIDTH;
                page.PageHeight = CLPPage.PORTRAIT_HEIGHT;
                page.PageAspectRatio = page.PageWidth / page.PageHeight;
            }
            else if(page.PageAspectRatio == CLPPage.PORTRAIT_WIDTH / CLPPage.PORTRAIT_HEIGHT)
            {
                foreach(ICLPPageObject pageObject in page.PageObjects)
                {
                    if(pageObject.XPosition + pageObject.Width > CLPPage.LANDSCAPE_WIDTH)
                    {
                        pageObject.XPosition = CLPPage.LANDSCAPE_WIDTH - pageObject.Width;
                    }
                    if(pageObject.YPosition + pageObject.Height > CLPPage.LANDSCAPE_HEIGHT)
                    {
                        pageObject.YPosition = CLPPage.LANDSCAPE_HEIGHT - pageObject.Height;
                    }
                }

                page.PageWidth = CLPPage.LANDSCAPE_WIDTH;
                page.PageHeight = CLPPage.LANDSCAPE_HEIGHT;
                page.PageAspectRatio = page.PageWidth / page.PageHeight;
            }
        }

        /// <summary>
        /// Deletes current page from the notebook.
        /// </summary>
        public Command DeletePageCommand { get; private set; }

        private void OnDeletePageCommandExecute()
        {
            //TODO: Steve - clpserviceagent method for this?
            int index = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.IndexOf(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            if(index != -1)
            {
                (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.RemovePageAt(index);
                int count = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.Pages.Count;
                (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.Pages[index == count ? index - 1 : index];
            }
        }

        /// <summary>
        /// Makes a duplicate of the current page.
        /// </summary>
        public Command CopyPageCommand { get; private set; }

        private void OnCopyPageCommandExecute()
        {
            int index = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.IndexOf(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            index++;

            CLPPage newPage = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.DuplicatePage();
            (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.InsertPageAt(index, newPage);
        }

        /// <summary>
        /// Bring up window to tag a page with Page Topics.
        /// </summary>
        public Command AddPageTopicCommand { get; private set; }

        private void OnAddPageTopicCommandExecute()
        {
            CLPPage page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;

            PageTopicWindowView pageTopicWindow = new PageTopicWindowView();
            pageTopicWindow.Owner = Application.Current.MainWindow;

            string originalPageTopics = String.Join(",", page.PageTopics);
            pageTopicWindow.PageTopicName.Text = originalPageTopics;
            pageTopicWindow.ShowDialog();
            if(pageTopicWindow.DialogResult == true)
            {
                string pageTopics = pageTopicWindow.PageTopicName.Text;
                string[] stringArray = pageTopics.Split(',');
                page.PageTopics = new ObservableCollection<string>(new List<string>(stringArray));
            }
        }

        /// <summary>
        /// Add 200 pixels to the height of the current page.
        /// </summary>
        public Command MakePageLongerCommand { get; private set; }
        
        private void OnMakePageLongerCommandExecute()
        {
            if((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay is LinkedDisplayViewModel)
            {
                CLPPage page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;
                page.PageHeight += 200;
                ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).ResizePage();
            
                double yDifference = page.PageHeight - CLPPage.LANDSCAPE_HEIGHT;

                double times = yDifference / 200;

                Logger.Instance.WriteToLog("[METRICS]: PageLength Increased " + times + " times on page " + page.PageIndex);
            }
        }

        /// <summary>
        /// Trims the current page's excess height if free of ink strokes and pageObjects.
        /// </summary>
        public Command TrimPageCommand { get; private set; }

        private void OnTrimPageCommandExecute()
        {
            if((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay is LinkedDisplayViewModel)
            {
                CLPPage page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;
                page.TrimPage();
            }
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
            CLPTextBox textBox = new CLPTextBox(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            CLPServiceAgent.Instance.AddPageObjectToPage(textBox);
        }

        /// <summary>
        /// Gets the InsertAggregationDataTableCommand command.
        /// </summary>
        public Command InsertAggregationDataTableCommand { get; private set; }

        private void OnInsertAggregationDataTableCommandExecute()
        {
            CLPAggregationDataTable dataTable = new CLPAggregationDataTable(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            CLPServiceAgent.Instance.AddPageObjectToPage(dataTable);
        }

        /// <summary>
        /// Gets the InsertStaticImageCommand command.
        /// </summary>
        public Command<string> InsertStaticImageCommand { get; private set; }

        private void OnInsertStaticImageCommandExecute(string fileName)
        {
            var uri = new Uri("pack://application:,,,/Classroom Learning Partner;component/Images/Money/" + fileName);
            var info = Application.GetResourceStream(uri);
            var memoryStream = new MemoryStream();
            info.Stream.CopyTo(memoryStream);

            byte[] byteSource = memoryStream.ToArray();

            var ByteSource = new List<byte>(byteSource);

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(byteSource);
            string imageID = Convert.ToBase64String(hash);

            CLPPage page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;


            if(!page.ImagePool.ContainsKey(imageID))
            {
                page.ImagePool.Add(imageID, ByteSource);
            }
            CLPImage image = new CLPImage(imageID, page);
           // CLPStamp stamp = new CLPStamp(image, page);

            switch(fileName)
            {
                case "penny.png":
                    image.Height = 90;
                    image.Width = 90;
                    break;
                case "dime.png":
                    image.Height = 80;
                    image.Width = 80;
                    break;
                case "nickel.png":
                    image.Height = 100;
                    image.Width = 100;
                    break;
                case "quarter.png":
                    image.Height = 120;
                    image.Width = 120;
                    break;
                default:
                    image.Height = 128;
                    image.Width = 300;
                    break;
            }

            CLPServiceAgent.Instance.AddPageObjectToPage(image);
        }

        /// <summary>
        /// Gets the InsertImageCommand command.
        /// </summary>
        public Command InsertImageCommand { get; private set; }

        private void OnInsertImageCommandExecute()
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Images|*.png;*.jpg;*.jpeg;*.gif"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if(result == true)
            {
                // Open document
                string filename = dlg.FileName;
                if(File.Exists(filename))
                {
                    byte[] byteSource = File.ReadAllBytes(filename);
                    List<byte> ByteSource = new List<byte>(byteSource);

                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    byte[] hash = md5.ComputeHash(byteSource);
                    string imageID = Convert.ToBase64String(hash);

                    CLPPage page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;


                    if(!page.ImagePool.ContainsKey(imageID))
                    {
                        page.ImagePool.Add(imageID, ByteSource);
                    }
                    CLPImage image = new CLPImage(imageID, page);

                    CLPServiceAgent.Instance.AddPageObjectToPage(image);
                }
                else
                {
                    MessageBox.Show("Error opening image file. Please try again.");
                }
            }
        }

        /// <summary>
        /// Gets the ToggleWebcamPanelCommand command.
        /// </summary>
        public Command<bool> ToggleWebcamPanelCommand { get; private set; }

        DispatcherTimer panelCloserTimer = new DispatcherTimer();

        private void OnToggleWebcamPanelCommandExecute(bool isButtonChecked)
        {
            if(!isButtonChecked) //ClosePanel
            {
                ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).RightPanel as IPanel).IsVisible = false;
                ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).RightPanel as ViewModelBase).SaveAndCloseViewModel();
                (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).RightPanel = null;
            }
            else //OpenPanel
            {
                if((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).RightPanel == null)
                {
                    (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).RightPanel = new WebcamPanelViewModel();
                }

                ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).RightPanel as IPanel).IsVisible = true;
            }
        }

        void panelCloserTimer_Tick(object sender, EventArgs e)
        {
            if((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).RightPanel != null)
            {
                ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).RightPanel as ViewModelBase).SaveAndCloseViewModel();
                (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).RightPanel = null;
                panelCloserTimer.Stop();
            }
        }

        /// <summary>
        /// Gets the InsertImageStampCommand command.
        /// </summary>
        public Command InsertImageStampCommand { get; private set; }

        private void OnInsertImageStampCommandExecute()
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Images|*.png;*.jpg;*.jpeg;*.gif"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if(result == true)
            {
                // Open document
                string filename = dlg.FileName;
                if(File.Exists(filename))
                {
                    byte[] byteSource = File.ReadAllBytes(filename);
                    List<byte> ByteSource = new List<byte>(byteSource);

                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    byte[] hash = md5.ComputeHash(byteSource);
                    string imageID = Convert.ToBase64String(hash);

                    CLPPage page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;


                    if(!page.ImagePool.ContainsKey(imageID))
                    {
                        page.ImagePool.Add(imageID, ByteSource);
                    }
                    CLPImage image = new CLPImage(imageID, page);
                    CLPStamp stamp = new CLPStamp(image, page);

                    CLPServiceAgent.Instance.AddPageObjectToPage(stamp);
                }
                else
                {
                    MessageBox.Show("Error opening image file. Please try again.");
                }
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
            CLPStamp stamp = new CLPStamp(null, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            CLPServiceAgent.Instance.AddPageObjectToPage(stamp);
            if(EditingMode != InkCanvasEditingMode.Ink)
            {
                SetPenCommand.Execute();
            }
        }

        /// <summary>
        /// Gets the InsertArrayCommand command.
        /// </summary>
        public Command<string> InsertArrayCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertArrayCommand command is executed.
        /// </summary>
        private void OnInsertArrayCommandExecute(string useDivisions)
        {
            //pop up number pads to get dimensions
            CustomizeArrayView dimensionChooser = new CustomizeArrayView();
            //CustomizeDataTableView dimensionChooser = new CustomizeDataTableView();
            dimensionChooser.Owner = Application.Current.MainWindow;
            dimensionChooser.ShowDialog();
            if(dimensionChooser.DialogResult == true)
            {
                //CLPHandwritingAnalysisType selected_type = (CLPHandwritingAnalysisType)optionChooser.ExpectedType.SelectedIndex;

                int rows = 1;
                try { rows = Convert.ToInt32(dimensionChooser.Rows.Text); }
                catch(FormatException) { rows = 1; }

                int cols = 1;
                try { cols = Convert.ToInt32(dimensionChooser.Columns.Text); }
                catch(FormatException) { cols = 1; }

                CLPArray array = new CLPArray(rows, cols, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);

                if (useDivisions == "TRUE")
                {
                    array.IsDivisionBehaviorOn = true;
                }
                else if (useDivisions == "FALSE")
                {
                    array.IsDivisionBehaviorOn = false;
                }

                CLPServiceAgent.Instance.AddPageObjectToPage(array);
            }

            if(EditingMode != InkCanvasEditingMode.Ink)
            {
                SetPenCommand.Execute();
            }
        }

        /// <summary>
        /// Gets the InsertAudioCommand command.
        /// </summary>
        public Command InsertAudioCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertAudioCommand command is executed.
        /// </summary>
        private void OnInsertAudioCommandExecute()
        {
            CLPAudio audio = new CLPAudio(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            CLPServiceAgent.Instance.AddPageObjectToPage(audio);
        }

        /// <summary>
        /// Gets the InsertProtractorCommand command.
        /// </summary>
        public Command InsertProtractorCommand { get; private set; }

        private void OnInsertProtractorCommandExecute()
        {
            CLPShape square = new CLPShape(CLPShape.CLPShapeType.Protractor, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            square.Height = 200;
            square.Width = 2.0*square.Height;
            
            CLPServiceAgent.Instance.AddPageObjectToPage(square);
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
            CLPShape square = new CLPShape(CLPShape.CLPShapeType.Rectangle, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            CLPServiceAgent.Instance.AddPageObjectToPage(square);
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
            CLPShape circle = new CLPShape(CLPShape.CLPShapeType.Ellipse, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            CLPServiceAgent.Instance.AddPageObjectToPage(circle);
        }

        /// <summary>
        /// Gets the InsertHorizontalLineShapCommand command.
        /// </summary>
        public Command InsertHorizontalLineShapeCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertHorizontalLineShapeCommand command is executed.
        /// </summary>
        private void OnInsertHorizontalLineShapeCommandExecute()
        {
            CLPShape line = new CLPShape(CLPShape.CLPShapeType.HorizontalLine, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            CLPServiceAgent.Instance.AddPageObjectToPage(line);
        }

        /// <summary>
        /// Gets the InsertHorizontalLineShapCommand command.
        /// </summary>
        public Command InsertVerticalLineShapeCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertHorizontalLineShapeCommand command is executed.
        /// </summary>
        private void OnInsertVerticalLineShapeCommandExecute()
        {
            CLPShape line = new CLPShape(CLPShape.CLPShapeType.VerticalLine, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            CLPServiceAgent.Instance.AddPageObjectToPage(line);
        }

        /// <summary>
        /// Gets the InsertHandwritingRegionCommand command.
        /// </summary>
        public Command InsertHandwritingRegionCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertHandwritingRegionCommand command is executed.
        /// </summary>
        private void OnInsertHandwritingRegionCommandExecute()
        {
            CustomizeInkRegionView optionChooser = new CustomizeInkRegionView();
            optionChooser.Owner = Application.Current.MainWindow;
            optionChooser.ShowDialog();
            if(optionChooser.DialogResult == true)
            {
                CLPHandwritingAnalysisType selected_type = (CLPHandwritingAnalysisType)optionChooser.ExpectedType.SelectedIndex;
                CLPHandwritingRegion region = new CLPHandwritingRegion(selected_type, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
                CLPServiceAgent.Instance.AddPageObjectToPage(region);
            }
        }

        /// <summary>
        /// Gets the InsertInkShapeRegionCommand command.
        /// </summary>
        public Command InsertInkShapeRegionCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertInkShapeRegionCommand command is executed.
        /// </summary>
        private void OnInsertInkShapeRegionCommandExecute()
        {
            CLPInkShapeRegion region = new CLPInkShapeRegion(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            CLPServiceAgent.Instance.AddPageObjectToPage(region);
        }

        /// <summary>
        /// Gets the InsertGroupingRegionCommand command.
        /// </summary>
        public Command InsertGroupingRegionCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertGroupingRegionCommand command is executed.
        /// </summary>
        private void OnInsertGroupingRegionCommandExecute()
        {
            CLPGroupingRegion region = new CLPGroupingRegion(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
            CLPServiceAgent.Instance.AddPageObjectToPage(region);
        }

        /// <summary>
        /// Gets the InsertDataTableCommand command.
        /// </summary>
        public Command InsertDataTableCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertInkShapeRegionCommand command is executed.
        /// </summary>
        private void OnInsertDataTableCommandExecute()
        {
            CustomizeDataTableView optionChooser = new CustomizeDataTableView();
            optionChooser.Owner = Application.Current.MainWindow;
            optionChooser.ShowDialog();
            if(optionChooser.DialogResult == true)
            {
                CLPHandwritingAnalysisType selected_type = (CLPHandwritingAnalysisType)optionChooser.ExpectedType.SelectedIndex;

                int rows = 1;
                try { rows = Convert.ToInt32(optionChooser.Rows.Text); }
                catch(FormatException) { rows = 1; }

                int cols = 1;
                try { cols = Convert.ToInt32(optionChooser.Cols.Text); }
                catch(FormatException) { cols = 1; }

                CLPDataTable region = new CLPDataTable(rows, cols, selected_type, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
               
                CLPServiceAgent.Instance.AddPageObjectToPage(region);
            }
        }

        /// <summary>
        /// Gets the InsertShadingRegionCommand command.
        /// </summary>
        public Command InsertShadingRegionCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the InsertInkShapeRegionCommand command is executed.
        /// </summary>
        private void OnInsertShadingRegionCommandExecute()
        {
            CustomizeShadingRegionView optionChooser = new CustomizeShadingRegionView();
            optionChooser.Owner = Application.Current.MainWindow;
            optionChooser.ShowDialog();
            if(optionChooser.DialogResult == true)
            {
                //CLPHandwritingAnalysisType selected_type = (CLPHandwritingAnalysisType)optionChooser.ExpectedType.SelectedIndex;

                int rows = 0;
                try { rows = Convert.ToInt32(optionChooser.Rows.Text); }
                catch(FormatException) { rows = 0; }

                int cols = 0;
                try { cols = Convert.ToInt32(optionChooser.Cols.Text); }
                catch(FormatException) { cols = 0; }

                CLPShadingRegion region = new CLPShadingRegion(rows, cols, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage);
                CLPServiceAgent.Instance.AddPageObjectToPage(region);
            }
        }

        #endregion //Insert Commands

        #region Debug Commands

        /// <summary>
        /// Runs interpretation methods of all pageObjects on current page.
        /// </summary>
        public Command InterpretPageCommand { get; private set; }

        private void OnInterpretPageCommandExecute()
        {
            CLPPage currentPage = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;
            foreach (ICLPPageObject pageObject in currentPage.PageObjects)
            {
                if (pageObject.GetType().IsSubclassOf(typeof(ACLPInkRegion)))
                {
                    CLPServiceAgent.Instance.InterpretRegion(pageObject as ACLPInkRegion);
                }
            }
        }

        /// <summary>
        /// Gets the UpdateObjectPropertiesCommand command.
        /// </summary>
        public Command UpdateObjectPropertiesCommand { get; private set; }

        private void OnUpdateObjectPropertiesCommandExecute()
        {
            PageInteractionMode = PageInteractionMode.EditObjectProperties;
        }

        /// <summary>
        /// Gets the ZoomToPageWidthCommand command.
        /// </summary>
        public Command ZoomToPageWidthCommand { get; private set; }

        private void OnZoomToPageWidthCommandExecute()
        {
            ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).ZoomToWholePage = false;
            ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).ResizePage();
        }

        /// <summary>
        /// Gets the ZoomToWholePageCommand command.
        /// </summary>
        public Command ZoomToWholePageCommand { get; private set; }

        private void OnZoomToWholePageCommandExecute()
        {
            ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).ZoomToWholePage = true;
            ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).ResizePage();
        }

        #endregion //Debug Commands

        #endregion //Commands

    }
}
