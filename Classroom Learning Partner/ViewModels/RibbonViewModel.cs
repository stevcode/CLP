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
            CurrentColorButton = new RibbonButton {Background = new SolidColorBrush(Colors.Black)};

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
            SetPenColorCommand = new Command<RibbonButton>(OnSetPenColorCommandExecute);

            //Submission
            SubmitPageCommand = new Command(OnSubmitPageCommandExecute);
            GroupSubmitPageCommand = new Command(OnGroupSubmitPageCommandExecute);

            //History
            ReplayCommand = new Command(OnReplayCommandExecute);
            UndoCommand = new Command(OnUndoCommandExecute, OnUndoCanExecute);
            RedoCommand = new Command(OnRedoCommandExecute, OnRedoCanExecute);

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
            AddNewProofPageCommand = new Command<string>(OnAddNewProofPageCommandExecute);
            SwitchPageLayoutCommand = new Command(OnSwitchPageLayoutCommandExecute);
            SwitchPageTypeCommand = new Command(OnSwitchPageTypeCommandExecute);

            DeletePageCommand = new Command(OnDeletePageCommandExecute);
            CopyPageCommand = new Command(OnCopyPageCommandExecute);
            AddPageTopicCommand = new Command(OnAddPageTopicCommandExecute);
            MakePageLongerCommand = new Command(OnMakePageLongerCommandExecute);
            TrimPageCommand = new Command(OnTrimPageCommandExecute);
            ClearPageCommand = new Command(OnClearPageCommandExecute);

            //Debug
            InterpretPageCommand = new Command(OnInterpretPageCommandExecute);
            UpdateObjectPropertiesCommand = new Command(OnUpdateObjectPropertiesCommandExecute);

            //Authoring
            EditPageDefinitionCommand = new Command(OnEditPageDefinitionCommandExecute);

            //Analysis
            AnalyzeArrayCommand = new Command(OnAnalyzeArrayCommandExecute);
            AnalyzeStampsCommand = new Command(OnAnalyzeStampsCommandExecute);
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
        public bool DisplayPanelVisibility
        {
            get { return GetValue<bool>(DisplayPanelVisibilityProperty); }
            set { SetValue(DisplayPanelVisibilityProperty, value); }
        }

        public static readonly PropertyData DisplayPanelVisibilityProperty = RegisterProperty("DisplayPanelVisibility", typeof(bool), false);

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

        private ObservableCollection<double> _fontSizes = new ObservableCollection<double> {3.0, 4.0, 5.0, 6.0, 6.5, 7.0, 7.5, 8.0, 8.5, 9.0, 9.5, 
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

        private List<Color> _colors = new List<Color> { Colors.Black, Colors.Red, Colors.DarkOrange, Colors.Tan, Colors.Gold, Colors.SaddleBrown, Colors.DarkGreen, Colors.MediumSeaGreen, Colors.Blue, Colors.HotPink, Colors.BlueViolet, Colors.Aquamarine, Colors.SlateGray, Colors.SkyBlue, Colors.Turquoise };
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
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }
            var currentPage = notebookWorkspaceViewModel.CurrentPage;
            ACLPPageBaseViewModel.ClearAdorners(currentPage);
        }

        /// <summary>
        /// Leaves Authoring Mode.
        /// </summary>
        public Command DoneEditingNotebookCommand { get; private set; }

        private void OnDoneEditingNotebookCommandExecute()
        {
            MainWindow.IsAuthoring = false;

            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }
            var currentPage = notebookWorkspaceViewModel.CurrentPage;
            ACLPPageBaseViewModel.ClearAdorners(currentPage);

            foreach(var page in notebookWorkspaceViewModel.NotebookPages)
            {
                page.PageHistory.ClearNonAnimationHistory();
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

                foreach(var page in notebook.Pages)
                {
                    foreach(var pageObject in page.PageObjects)
                    {
                        pageObject.ParentPage = page;
                    }

                    page.TrimPage();
                    double printHeight = page.PageWidth / page.InitialPageAspectRatio;

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

                foreach(var page in notebook.Submissions[notebookWorkspaceViewModel.CurrentPage.UniqueID])
                {
                    foreach(var pageObject in page.PageObjects)
                    {
                        pageObject.ParentPage = page;
                    }

                    page.TrimPage();
                    double printHeight = page.PageWidth / page.InitialPageAspectRatio;

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
                            Content = page.Submitter.FullName,
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

                foreach(var page in notebook.Submissions.Keys.SelectMany(pageID => notebook.Submissions[pageID]))
                {
                    foreach(var pageObject in page.PageObjects)
                    {
                        pageObject.ParentPage = page;
                    }

                    page.TrimPage();
                    double printHeight = page.PageWidth / page.InitialPageAspectRatio;

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
                            Content = page.Submitter.FullName,
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
            }, null, "Converting All Submissions to XPS", 0.0 / 0.0);
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
                foreach(var page in notebook.Pages)
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
                foreach(var page in notebook.Pages)
                {
                    page.PageHistory.UseHistory = false;
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
            var currentPage = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
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
            var currentPage = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
            int index = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.IndexOf(currentPage);

            if(index < (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.Count - 1)
            {
                (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages[index + 1];
            }
        }

        #endregion //Notebook Commands

        #region Tool Commands

        /// <summary>
        /// Sets the Pen Color.
        /// </summary>
        public Command<RibbonButton> SetPenColorCommand { get; private set; }

        private void OnSetPenColorCommandExecute(RibbonButton button)
        {
            CurrentColorButton = button;
            var solidColorBrush = CurrentColorButton.Background as SolidColorBrush;
            if(solidColorBrush != null)
            {
                DrawingAttributes.Color = solidColorBrush.Color;
            }

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
                var page = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
                page.SerializedStrokes = StrokeDTO.SaveInkStrokes(page.InkStrokes);
                CLPNotebook notebook = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook;

                CLPServiceAgent.Instance.SubmitPage(page, notebook.UniqueID, false);

                ICLPPage submission = null;
                if(page is CLPPage)
                {
                    submission = (page as CLPPage).Clone() as CLPPage;
                }
                if(page is CLPAnimationPage)
                {
                    submission = (page as CLPAnimationPage).Clone() as CLPAnimationPage;
                }
                submission.InkStrokes = StrokeDTO.LoadInkStrokes(submission.SerializedStrokes);

                if(notebook != null && submission != null)
                {
                    submission.SubmissionType = SubmissionType.Single;
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
                var page = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
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

        #region HistoryCommands

        /// <summary>
        /// Replays the entire history of the current page.
        /// </summary>
        public Command ReplayCommand { get; private set; }

        private void OnReplayCommandExecute()
        {
            //Thread t = new Thread(() =>
            //{
            //    try
            //    {
            //        CLPPage page = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
            //        ICLPHistory pageHistory = page.PageHistory;

            //        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //        (DispatcherOperationCallback)delegate(object arg)
            //        {
            //            pageHistory.Freeze();
            //            return null;
            //        }, null);

            //        Stack<CLPHistoryItem> metaFuture = new Stack<CLPHistoryItem>();
            //        Stack<CLPHistoryItem> metaPast = new Stack<CLPHistoryItem>(new Stack<CLPHistoryItem>(pageHistory.MetaPast));

            //        while(metaPast.Count > 0)
            //        {
            //            CLPHistoryItem item = metaPast.Pop();
            //            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //            (DispatcherOperationCallback)delegate(object arg)
            //            {
            //                if(item != null) // TODO (caseymc): find out why one of these would ever be null and fix
            //                {
            //                    item.Undo(page);
            //                }
            //                return null;
            //            }, null);
            //            metaFuture.Push(item);
            //        }

                
            //        Thread.Sleep(400);
            //        while(metaFuture.Count > 0)
            //        {
            //            CLPHistoryItem item = metaFuture.Pop();
            //            if(item.ItemType == HistoryItemType.MoveObject || item.ItemType == HistoryItemType.ResizeObject)
            //            {
            //                Thread.Sleep(50); // make intervals between move-steps less painfully slow
            //            }
            //            else
            //            {
            //                Thread.Sleep(400);
            //            }
            //            Console.WriteLine("This is the action being REDONE: " + item.ItemType);
            //            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //            (DispatcherOperationCallback)delegate(object arg)
            //            {
            //                if(item != null)
            //                {
            //                    item.Redo(page);
            //                }
            //                return null;
            //            }, null);
            //        }
            //        Thread.Sleep(400);
            //        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //        (DispatcherOperationCallback)delegate(object arg)
            //        {
            //            pageHistory.Unfreeze();
            //            return null;
            //        }, null);
            //    }
            //    catch(Exception e)
            //    {
            //        Console.WriteLine(e.Message);
            //    }
            //});
            //t.Start();
        }

        /// <summary>
        /// Undoes the last action.
        /// </summary>
        public Command UndoCommand { get; private set; }

        private void OnUndoCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }
            var page = notebookWorkspaceViewModel.CurrentPage;
            page.PageHistory.Undo();
        }

        private bool OnUndoCanExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return false;
            }
            var page = notebookWorkspaceViewModel.CurrentPage;

            var recordIndicator = page.PageHistory.RedoItems.FirstOrDefault() as CLPAnimationIndicator;
            if(recordIndicator != null && recordIndicator.AnimationIndicatorType == AnimationIndicatorType.Record)
            {
                return false;
            }

            return page.PageHistory.CanUndo;
        }

        /// <summary>
        /// Redoes the last undone action.
        /// </summary>
        public Command RedoCommand { get; private set; }

        private void OnRedoCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }
            var page = notebookWorkspaceViewModel.CurrentPage;
            page.PageHistory.Redo();
        }

        private bool OnRedoCanExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return false;
            }
            var page = notebookWorkspaceViewModel.CurrentPage;

            return page.PageHistory.CanRedo;
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
            var page = ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage;
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
            var page = ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage;
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
            foreach(var pages in notebook.Submissions.Values)
            {
                pages.Clear();
            }
            foreach(var page in notebook.Pages)
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
            int index = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.IndexOf(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
            index++;
            CLPPage page = new CLPPage();
            if(pageOrientation == "Portrait")
            {
                page.PageHeight = CLPPage.PORTRAIT_HEIGHT;
                page.PageWidth = CLPPage.PORTRAIT_WIDTH;
                page.InitialPageAspectRatio = page.PageWidth / page.PageHeight;
            }
            page.ParentNotebookID = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.UniqueID;
            (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.InsertPageAt(index, page);
        }


        public Command<string> AddNewProofPageCommand { get; private set; }

        private void OnAddNewProofPageCommandExecute(string pageOrientation)
        {
            //Steve - clpserviceagent
            int index = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.IndexOf(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
            index++;
            var page = new CLPAnimationPage();
            if(pageOrientation == "Portrait")
            {
                page.PageHeight = CLPPage.PORTRAIT_HEIGHT;
                page.PageWidth = CLPPage.PORTRAIT_WIDTH;
                page.InitialPageAspectRatio = page.PageWidth / page.PageHeight;
            }
            else if(pageOrientation == "Landscape") 
            {
                page.PageHeight = CLPPage.LANDSCAPE_HEIGHT;
                page.PageWidth = CLPPage.LANDSCAPE_WIDTH;
                page.InitialPageAspectRatio = page.PageWidth / page.PageHeight;
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
            var page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage;

            if(page.InitialPageAspectRatio == CLPPage.LANDSCAPE_WIDTH / CLPPage.LANDSCAPE_HEIGHT)
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
                page.InitialPageAspectRatio = page.PageWidth / page.PageHeight;
            }
            else if(page.InitialPageAspectRatio == CLPPage.PORTRAIT_WIDTH / CLPPage.PORTRAIT_HEIGHT)
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
                page.InitialPageAspectRatio = page.PageWidth / page.PageHeight;
            }
        }

        public Command SwitchPageTypeCommand { get; private set; }

        public void OnSwitchPageTypeCommandExecute(){
            var page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage;
            int index = page.PageIndex;
            double pageheight = page.PageHeight;
            double pagewidth = page.PageWidth;
            double pageaspectratio = page.InitialPageAspectRatio;

            //TODO: reimplement
            //if(page.PageType == PageTypeEnum.CLPProofPage) {
            //    CLPAnimationPage proofpage = (CLPAnimationPage)page;
            //    CLPProofHistory proofhistory = (CLPProofHistory)page.PageHistory;
            //    if(proofhistory.Future.Count > 0 || proofhistory.MetaPast.Count > 0)
            //    {
            //        if(MessageBox.Show("Are you sure you want to switch to a different page type? Your animation will be lost!",
            //                       "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            //        {
            //            return;
            //        }
            //    }
            //    CLPPage newpage = new CLPPage();
            //    CLPHistory newpagehistory = (CLPHistory)newpage.PageHistory;

            //    newpage.PageHeight = pageheight;
            //    newpage.PageWidth = pagewidth;
            //    newpage.InitialPageAspectRatio = pageaspectratio;

            //    newpagehistory.Past = proofhistory.Past;
            //    newpagehistory.MetaPast = proofhistory.MetaPast;
            //    newpagehistory.Future = proofhistory.Future;

            //    newpage.PageObjects = proofpage.PageObjects;
            //    newpage.InkStrokes = proofpage.InkStrokes;
            //    foreach(ICLPPageObject po in newpage.PageObjects)
            //    {
            //        po.ParentPage = newpage;
            //    }

            //    page.ParentNotebookID = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.UniqueID;
            //    CLPNotebook nb = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook;
            //                nb.InsertPageAt(index, newpage);
            //                nb.Submissions.Remove(nb.Pages[index - 1].UniqueID);
            //                nb.Pages.RemoveAt(index - 1);
            //                nb.GeneratePageIndexes();
            //                (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage = newpage;      
            //}else{
            //    CLPHistory pagehistory = (CLPHistory)page.PageHistory;

            //    CLPAnimationPage newproofpage = new CLPAnimationPage();
            //    CLPProofHistory newproofpagehistory = (CLPProofHistory)newproofpage.PageHistory;

            //    newproofpage.PageHeight = pageheight;
            //    newproofpage.PageWidth = pagewidth;
            //    newproofpage.InitialPageAspectRatio = pageaspectratio;

            //    newproofpage.PageObjects = page.PageObjects;
            //    newproofpage.InkStrokes =  page.InkStrokes;
            //    foreach(ICLPPageObject po in newproofpage.PageObjects)
            //    {
            //        po.ParentPage = newproofpage;
            //    }
            //    page.ParentNotebookID = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.UniqueID;
            //    CLPNotebook nb = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook;
            //                nb.InsertPageAt(index, newproofpage);
            //                nb.Submissions.Remove(nb.Pages[index - 1].UniqueID);
            //                nb.Pages.RemoveAt(index - 1);
            //                nb.GeneratePageIndexes();
            //                (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage = newproofpage; 
            //}
        }

        /// <summary>
        /// Deletes current page from the notebook.
        /// </summary>
        public Command DeletePageCommand { get; private set; }

        private void OnDeletePageCommandExecute()
        {
            //TODO: Steve - clpserviceagent method for this?
            int index = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.IndexOf(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
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
            int index = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).NotebookPages.IndexOf(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
            index++;

            var newPage = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage.DuplicatePage();
            (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.InsertPageAt(index, newPage);
        }

        /// <summary>
        /// Bring up window to tag a page with Page Topics.
        /// </summary>
        public Command AddPageTopicCommand { get; private set; }

        private void OnAddPageTopicCommandExecute()
        {
            //TODO: PageTopics gone, convert to Tags
            //var page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;

            //PageTopicWindowView pageTopicWindow = new PageTopicWindowView();
            //pageTopicWindow.Owner = Application.Current.MainWindow;

            //string originalPageTopics = String.Join(",", page.PageTopics);
            //pageTopicWindow.PageTopicName.Text = originalPageTopics;
            //pageTopicWindow.ShowDialog();
            //if(pageTopicWindow.DialogResult == true)
            //{
            //    string pageTopics = pageTopicWindow.PageTopicName.Text;
            //    string[] stringArray = pageTopics.Split(',');
            //    page.PageTopics = new ObservableCollection<string>(new List<string>(stringArray));
            //}
        }

        /// <summary>
        /// Add 200 pixels to the height of the current page.
        /// </summary>
        public Command MakePageLongerCommand { get; private set; }
        
        private void OnMakePageLongerCommandExecute()
        {
            if((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay is CLPMirrorDisplay)
            {
                var page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage;
                page.PageHeight += 200;
     //PageHeight change should force ResizePage() call           ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).ResizePage();
            
            }
        }

        /// <summary>
        /// Trims the current page's excess height if free of ink strokes and pageObjects.
        /// </summary>
        public Command TrimPageCommand { get; private set; }

        private void OnTrimPageCommandExecute()
        {
            if((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay is CLPMirrorDisplay)
            {
                var page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage;
                page.TrimPage();
            }
        }

        /// <summary>
        /// Completely clears a page of ink strokes and pageObjects.
        /// </summary>
        public Command ClearPageCommand { get; private set; }

        private void OnClearPageCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null ||
               !(notebookWorkspaceViewModel.SelectedDisplay is CLPMirrorDisplay))
            {
                return;
            }
            var page = (notebookWorkspaceViewModel.SelectedDisplay as CLPMirrorDisplay).CurrentPage;
            page.PageHistory.ClearHistory();
            page.PageObjects.Clear();
            page.InkStrokes.Clear();
            page.SerializedStrokes.Clear();
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
            CLPTextBox textBox = new CLPTextBox(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
            CLPServiceAgent.Instance.AddPageObjectToPage(textBox);
        }

        /// <summary>
        /// Gets the InsertAggregationDataTableCommand command.
        /// </summary>
        public Command InsertAggregationDataTableCommand { get; private set; }

        private void OnInsertAggregationDataTableCommandExecute()
        {
            CLPAggregationDataTable dataTable = new CLPAggregationDataTable(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
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

            var page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage;


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

                    var page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage;


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

                    var page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage;


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
            CLPStamp stamp = new CLPStamp(null, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
            CLPServiceAgent.Instance.AddPageObjectToPage(stamp);
        }

        /// <summary>
        /// Gets the InsertArrayCommand command.
        /// </summary>
        public Command<string> InsertArrayCommand { get; private set; }

        private void OnInsertArrayCommandExecute(string useDivisions)
        {
            var arrayCreationView = new ArrayCreationView {Owner = Application.Current.MainWindow};
            arrayCreationView.ShowDialog();

            if(arrayCreationView.DialogResult == true)
            {
                var notebookWorkspaceViewModel = MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel;
                if(notebookWorkspaceViewModel == null)
                {
                    return;
                }
                var clpMirrorDisplay = notebookWorkspaceViewModel.SelectedDisplay as CLPMirrorDisplay;
                if(clpMirrorDisplay == null)
                {
                    return;
                }
                var currentPage = clpMirrorDisplay.CurrentPage;

                int rows;
                try
                {
                    rows = Convert.ToInt32(arrayCreationView.Rows.Text);
                }
                catch(FormatException)
                {
                    rows = 1;
                }

                int columns;
                try
                {
                    columns = Convert.ToInt32(arrayCreationView.Columns.Text);
                }
                catch(FormatException)
                {
                    columns = 1;
                }

                int numberOfArrays;
                try
                {
                    numberOfArrays = Convert.ToInt32(arrayCreationView.NumberOfArrays.Text);
                }
                catch(FormatException)
                {
                    numberOfArrays = 1;
                }

                var isHorizontallyAligned = arrayCreationView.HorizontalToggle.IsChecked != null &&
                                            (bool)arrayCreationView.HorizontalToggle.IsChecked;
                var isVerticallyAligned = arrayCreationView.VerticalToggle.IsChecked != null &&
                                          (bool)arrayCreationView.VerticalToggle.IsChecked;
                var initializedSquareSize = 45.0;
                var xPosition = 0.0;
                var yPosition = 150.0;
                var useTwoColumns = false;
                const double LARGE_LABEL_LENGTH = 70.0;
                const double MIN_SIDE_LENGTH = 15.0;
                while(xPosition + LARGE_LABEL_LENGTH + columns * initializedSquareSize >= currentPage.PageWidth || yPosition + LARGE_LABEL_LENGTH + rows * initializedSquareSize >= currentPage.PageHeight)
                {
                    initializedSquareSize = initializedSquareSize / 2;
                }
                if(numberOfArrays > 1)
                {
                    if(isHorizontallyAligned)
                    {
                        while(xPosition + LARGE_LABEL_LENGTH + (columns * numberOfArrays + numberOfArrays - 1) * initializedSquareSize >= currentPage.PageWidth)
                        {
                            if(initializedSquareSize / 2 * columns < MIN_SIDE_LENGTH || initializedSquareSize / 2 * rows < MIN_SIDE_LENGTH)
                            {
                                useTwoColumns = true;
                                break;
                            }
                            initializedSquareSize = initializedSquareSize / 2;
                        }
                    }
                    else if(isVerticallyAligned)
                    {
                        yPosition = 80;
                        while(yPosition + LARGE_LABEL_LENGTH + (rows * numberOfArrays + numberOfArrays - 1) * initializedSquareSize >= currentPage.PageHeight)
                        {
                            if(initializedSquareSize / 2 * columns < MIN_SIDE_LENGTH || initializedSquareSize / 2 * rows < MIN_SIDE_LENGTH)
                            {
                                useTwoColumns = true;
                                break;
                            }
                            initializedSquareSize = initializedSquareSize / 2;
                        }
                    }
                }

                if(useTwoColumns && isHorizontallyAligned)
                {
                    yPosition = 40.0;
                }
                foreach(var index in Enumerable.Range(1, numberOfArrays))
                {
                    var array = new CLPArray(rows, columns, currentPage);

                    switch(useDivisions)
                    {
                        case "TRUE":
                            array.IsDivisionBehaviorOn = true;
                            break;
                        case "FALSE":
                            array.IsDivisionBehaviorOn = false;
                            break;
                    }

                    if(isHorizontallyAligned)
                    {
                        if(useTwoColumns && index == numberOfArrays / 2 + 1)
                        {
                            xPosition = 0.0;
                            yPosition += LARGE_LABEL_LENGTH + rows * initializedSquareSize;
                        }
                        array.XPosition = xPosition;
                        array.YPosition = yPosition;
                        xPosition += ((columns + 1) * initializedSquareSize);
                        array.SizeArrayToGridLevel(initializedSquareSize);
                    }
                    else if(isVerticallyAligned)
                    {
                        if(useTwoColumns && index == numberOfArrays / 2 + 1)
                        {
                            xPosition += LARGE_LABEL_LENGTH + columns * initializedSquareSize;
                            yPosition = 80.0;
                        }
                        array.XPosition = xPosition;
                        array.YPosition = yPosition;
                        yPosition += ((rows + 1) * initializedSquareSize);
                        array.SizeArrayToGridLevel(initializedSquareSize);
                    }

                    CLPServiceAgent.Instance.AddPageObjectToPage(array);
                }
              
            }
        }

        /// <summary>
        /// Gets the EditPageDefinitionCommand command.
        /// </summary>
        public Command EditPageDefinitionCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the EditPageDefinitionCommand command is executed.
        /// </summary>
        private void OnEditPageDefinitionCommandExecute()
        {
            // Get the tags on this page
            Logger.Instance.WriteToLog("Page Definition");
            var page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage;
            ObservableCollection<Tag> tags = page.PageTags;

            // If the page already has a Page Definition tag, use that one
            Tag oldTag = null;
            ProductRelation oldRelation = null;
            foreach(Tag tag in tags)
            {
                if(tag.TagType.Name == PageDefinitionTagType.Instance.Name)
                {
                    oldTag = tag;
                    oldRelation = (ProductRelation) oldTag.Value[0].Value;
                    break;
                }
            }

            // Otherwise, create a new page definition tag
            if(oldTag == null)
            {
                oldRelation = new ProductRelation();
            }

            ProductRelationViewModel viewModel = new ProductRelationViewModel(oldRelation);
            PageDefinitionView definitionView = new PageDefinitionView(viewModel);
            definitionView.Owner = Application.Current.MainWindow;
            definitionView.ShowDialog();

            if(definitionView.DialogResult == true)
            {
                // Update this page's definition tag

                if(oldTag != null)
                {
                    tags.Remove(oldTag);
                }

                Tag newTag = new Tag(Tag.Origins.Author, PageDefinitionTagType.Instance);
                newTag.Value.Add(new TagOptionValue(viewModel.Model));

                tags.Add(newTag);
            }

            // Logs the currently tagged relation. TODO: Remove after testing
            foreach(Tag tag in tags)
            {
                if(tag.TagType.Name == PageDefinitionTagType.Instance.Name)
                {
                    Logger.Instance.WriteToLog(((ProductRelation) tag.Value[0].Value).GetExampleNumberSentence());
                }
            }
            Logger.Instance.WriteToLog("End of OnEditPageDefinitionCommandExecute()");
        }

        /// <summary>
        /// Gets the AnalyzeArrayCommand command.
        /// </summary>
        public Command AnalyzeArrayCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the AnalyzeArrayCommand command is executed.
        /// </summary>
        private void OnAnalyzeArrayCommandExecute()
        {
            Logger.Instance.WriteToLog("Start of OnAnalyzeArrayCommandExecute()");

            // Get the page's math definition, or be sad if it doesn't have one
            var page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage;
            ObservableCollection<Tag> tags = page.PageTags;
            ProductRelation relation = null;
            foreach(Tag tag in tags)
            {
                if(tag.TagType.Name == PageDefinitionTagType.Instance.Name)
                {
                    relation = (ProductRelation)tag.Value[0].Value;
                    break;
                }
            }

            if(relation == null)
            {
                // No definition for the page!
                Logger.Instance.WriteToLog("No page definition found! :(");
                return;
            }


            // Find an array object on the page (just use the first one we find), or be sad if we don't find one
            ObservableCollection<ICLPPageObject> objects = page.PageObjects;
            CLPArray array = null;

            foreach(ICLPPageObject pageObject in objects)
            {
                if(pageObject.GetType() == typeof(CLPArray))
                {
                    array = (CLPArray)pageObject;
                    break;
                }
            }

            if(array == null)
            {
                // No array on the page!
                Logger.Instance.WriteToLog("No array found! :(");
                return;
            }

            // We have a page definition and an array, so we're good to go!
            Logger.Instance.WriteToLog("Array found! Dimensions: " + array.Columns + " by " + array.Rows);

            int arrayWidth = array.Columns;
            int arrayHeight = array.Rows;
            int arrayArea = arrayWidth * arrayHeight;

            // TODO: Add handling for variables in math relation
            int factor1 = Convert.ToInt32(relation.Factor1);
            int factor2 = Convert.ToInt32(relation.Factor2);
            int product = Convert.ToInt32(relation.Product);

            // Make a tag based on the array's orientation
            // First, clear out any old ArrayOrientationTagTypes
            foreach(Tag tag in tags.ToList())
            {
                if(tag.TagType == null ||
                    tag.TagType.Name == RepresentationCorrectnessTagType.Instance.Name ||
                    tag.TagType.Name == ArrayOrientationTagType.Instance.Name ||
                    tag.TagType.Name == ArrayXAxisStrategyTagType.Instance.Name ||
                    tag.TagType.Name == ArrayYAxisStrategyTagType.Instance.Name ||
                    tag.TagType.Name == ArrayDivisionCorrectnessTagType.Instance.Name ||
                    tag.TagType.Name == ArrayVerticalDivisionsTagType.Instance.Name ||
                    tag.TagType.Name == ArrayHorizontalDivisionsTagType.Instance.Name)
                {
                    tags.Remove(tag);
                }
            }

            // Apply a representation correctness tag
            Tag correctnessTag = new Tag(Tag.Origins.Generated, RepresentationCorrectnessTagType.Instance);
            correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Other"));

            if(factor1 == arrayWidth && factor2 == arrayHeight)
            {
                correctnessTag.AddTagOptionValue(new TagOptionValue("Correct"));
            }
            else
            {
                if(factor2 == arrayWidth && factor1 == arrayHeight)
                {
                    // Are the factors swapped? Depends on whether we care about width vs. height
                    if(relation.RelationType == ProductRelation.ProductRelationTypes.Area)
                    {
                        correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Swapped Factors"));
                    }
                    else
                    {
                        correctnessTag.AddTagOptionValue(new TagOptionValue("Correct"));
                    }
                }
                else
                {
                    // One more possibility: The representation uses the givens in the problem, but in the wrong way
                    ObservableCollection<int> givens = new ObservableCollection<int>();
                    if(relation.Factor1Given) { givens.Add(factor1); }
                    if(relation.Factor2Given) { givens.Add(factor2); }
                    if(relation.ProductGiven) { givens.Add(product); }

                    ObservableCollection<int> numbersUsed = new ObservableCollection<int>();
                    numbersUsed.Add(arrayWidth);
                    numbersUsed.Add(arrayHeight);
                    numbersUsed.Add(arrayWidth * arrayHeight);

                    if(givens.Count == 2 && numbersUsed.Contains(givens[0]) && numbersUsed.Contains(givens[1]))
                    {
                        correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Misused Givens"));
                    }
                }
            }

            tags.Add(correctnessTag);

            // Apply an orientation tag
            Tag orientationTag = new Tag(Tag.Origins.Generated, ArrayOrientationTagType.Instance);
            if(arrayWidth == factor1 && arrayHeight == factor2)
            {
                orientationTag.AddTagOptionValue(new TagOptionValue("First factor is width"));
            }
            else if(arrayWidth == factor2 && arrayHeight == factor1)
            {
                orientationTag.AddTagOptionValue(new TagOptionValue("First factor is height"));
            }
            else
            {
                orientationTag.AddTagOptionValue(new TagOptionValue("unknown"));
            }
            tags.Add(orientationTag);
            Logger.Instance.WriteToLog("Tag added: " + orientationTag.TagType.Name + " -> " + orientationTag.Value[0].Value);

            // Apply a strategy tag
            Tag strategyTagX = new Tag(Tag.Origins.Generated, ArrayXAxisStrategyTagType.Instance);
            Tag strategyTagY = new Tag(Tag.Origins.Generated, ArrayYAxisStrategyTagType.Instance);

            // First check the horizontal divisions
            // Create a sorted list of the divisions' labels (as entered by the student)
            List<int> horizDivs = new List<int>();
            foreach(CLPArrayDivision div in array.HorizontalDivisions)
            {
                horizDivs.Add(div.Value);
            }
            horizDivs.Sort();
            /*String horizDivsString = "";
            foreach(int x in horizDivs)
            {
                horizDivsString += (x.ToString() + " ");
            }
            Logger.Instance.WriteToLog("Number of horizontal regions: " + horizDivs.Count);
            Logger.Instance.WriteToLog("Student's horizontal divisions (sorted): " + horizDivsString);*/

            // Now check the student's divisions against known strategies
            if(array.HorizontalDivisions.Count == 0)
            {
                strategyTagX.AddTagOptionValue(new TagOptionValue("none")); 
            }
            else if(horizDivs.SequenceEqual(PlaceValueStrategyDivisions(arrayHeight)))
            {
                strategyTagX.AddTagOptionValue(new TagOptionValue("place value"));
            }
            else if (arrayHeight > 20 && arrayHeight < 100 && horizDivs.SequenceEqual(TensStrategyDivisions(arrayHeight)))
            {
                strategyTagX.AddTagOptionValue(new TagOptionValue("10's"));
            }
            else if ((arrayHeight % 2 == 0) && horizDivs.SequenceEqual(HalvingStrategyDivisions(arrayHeight)))
            {
                strategyTagX.AddTagOptionValue(new TagOptionValue("half"));
            }
            else
            {
                strategyTagX.AddTagOptionValue(new TagOptionValue("other"));
            }

            // Then the vertical divisions
            List<int> vertDivs = new List<int>();
            foreach(CLPArrayDivision div in array.VerticalDivisions)
            {
                vertDivs.Add(div.Value);
            }
            vertDivs.Sort();

            // Now check the student's divisions against known strategies
            if(array.VerticalDivisions.Count == 0)
            {
                strategyTagY.AddTagOptionValue(new TagOptionValue("none"));
            }
            else if(vertDivs.SequenceEqual(PlaceValueStrategyDivisions(arrayWidth)))
            {
                strategyTagY.AddTagOptionValue(new TagOptionValue("place value"));
            }
            else if(arrayWidth > 20 && arrayWidth < 100 && vertDivs.SequenceEqual(TensStrategyDivisions(arrayWidth)))
            {
                strategyTagY.AddTagOptionValue(new TagOptionValue("10's"));
            }
            else if((arrayWidth % 2 == 0) && vertDivs.SequenceEqual(HalvingStrategyDivisions(arrayWidth)))
            {
                strategyTagY.AddTagOptionValue(new TagOptionValue("half"));
            }
            else
            {
                strategyTagY.AddTagOptionValue(new TagOptionValue("other"));
            }

            tags.Add(strategyTagX);
            tags.Add(strategyTagY);

            Logger.Instance.WriteToLog("Tag added: " + strategyTagX.TagType.Name + " -> " + strategyTagX.Value[0].Value);
            Logger.Instance.WriteToLog("Tag added: " + strategyTagY.TagType.Name + " -> " + strategyTagY.Value[0].Value);

            // Add an array divider correctness tag
            Tag divisionCorrectnessTag = CheckArrayDivisionCorrectness(array);
            tags.Add(divisionCorrectnessTag);

            Logger.Instance.WriteToLog("Tag added: " + divisionCorrectnessTag.TagType.Name + " -> " + divisionCorrectnessTag.Value[0].Value);

            // Add tags for the number of horizontal and vertical divisions
            Tag horizDivsTag = new Tag(Tag.Origins.Generated, ArrayHorizontalDivisionsTagType.Instance);
            int horizRegions = array.HorizontalDivisions.Count == 0 ? 1 : array.HorizontalDivisions.Count;
            horizDivsTag.Value.Add(new TagOptionValue(horizRegions.ToString() + " region" + (horizRegions == 1 ? "" : "s")));
            tags.Add(horizDivsTag);

            Tag vertDivsTag = new Tag(Tag.Origins.Generated, ArrayVerticalDivisionsTagType.Instance);
            int vertRegions = array.VerticalDivisions.Count == 0 ? 1 : array.VerticalDivisions.Count;
            vertDivsTag.Value.Add(new TagOptionValue(vertRegions.ToString() + " region" + (horizRegions == 1 ? "" : "s")));
            tags.Add(vertDivsTag);

            Logger.Instance.WriteToLog("Tag added: " + horizDivsTag.TagType.Name + " -> " + horizDivsTag.Value[0].Value);
            Logger.Instance.WriteToLog("Tag added: " + vertDivsTag.TagType.Name + " -> " + vertDivsTag.Value[0].Value);
        }

        // Get the (sorted) list of subdivisions of startingValue, using the place value strategy
        private List<int> PlaceValueStrategyDivisions(int startingValue)
        {
            int currentValue = startingValue;
            int currentPlace = 1;
            List<int> output = new List<int>();
            while(currentValue > 0)
            {
                if(currentValue % 10 > 0)
                {
                    output.Add((currentValue % 10) * currentPlace);
                }
                currentValue /= 10;
                currentPlace *= 10;
            }
            output.Sort();

            /*String outputString = "";
            foreach(int x in output)
            {
                outputString += (x.ToString() + " ");
            }
            Logger.Instance.WriteToLog("Place value divisions of " + startingValue + ": " + outputString);*/

            return output;
        }

        // Get the (sorted) list of subdivisions of startingValue, using the tens strategy
        // Assumes 20 < startingValue < 100
        private List<int> TensStrategyDivisions(int startingValue)
        {
            int currentValue = startingValue;
            List<int> output = new List<int>();
            if (currentValue % 10 > 0) {
                output.Add(currentValue % 10);
                currentValue -= (currentValue % 10);
            }
            while (currentValue > 0)
            {
                output.Add(10);
                currentValue -= 10;
            }
            output.Sort();

            return output;
        }

        // Get the (sorted) list of subdivisions of startingValue, using the halving strategy
        // Assumes startingValue is even
        private List<int> HalvingStrategyDivisions(int startingValue)
        {
            List<int> output = new List<int>();
            output.Add(startingValue / 2);
            output.Add(startingValue / 2);

            return output;
        }

        private Tag CheckArrayDivisionCorrectness(CLPArray array)
        {
            Tag tag = new Tag(Tag.Origins.Generated, ArrayDivisionCorrectnessTagType.Instance);

            bool horizUnfinished = false;
            bool vertUnfinished = false;

            // First check horizontal divisions
            if(array.HorizontalDivisions.Count > 0)
            {
                int sum = 0;
                foreach(CLPArrayDivision div in array.HorizontalDivisions)
                {
                    if(div.Value == 0) 
                    {
                        horizUnfinished = true;
                    }
                    sum += div.Value;
                }
                if(!horizUnfinished && sum != array.Rows)
                {
                    tag.AddTagOptionValue(new TagOptionValue("Incorrect"));
                    return tag;
                }
            }

            // Then check vertical divisions
            if(array.VerticalDivisions.Count > 0)
            {
                int sum = 0;
                foreach(CLPArrayDivision div in array.VerticalDivisions)
                {
                    if(div.Value == 0)
                    {
                        vertUnfinished = true;
                    }
                    sum += div.Value;
                }
                if(!vertUnfinished && sum != array.Columns)
                {
                    tag.AddTagOptionValue(new TagOptionValue("Incorrect"));
                    return tag;
                }
            }

            if(horizUnfinished || vertUnfinished)
            {
                tag.AddTagOptionValue(new TagOptionValue("Unfinished"));
                return tag;
            }
            else
            {
                tag.AddTagOptionValue(new TagOptionValue("Correct"));
                return tag;
            }
        }

        /// <summary>
        /// Gets the AnalyzeStampsCommand command.
        /// </summary>
        public Command AnalyzeStampsCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the AnalyzeStampsCommand command is executed.
        /// </summary>
        private void OnAnalyzeStampsCommandExecute()
        {
            Logger.Instance.WriteToLog("Analyzing stamp grouping region...");

            // Get the page's math definition, or be sad if it doesn't have one
            var page = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage;
            ObservableCollection<Tag> tags = page.PageTags;
            ProductRelation relation = null;
            foreach(Tag tag in tags)
            {
                if(tag.TagType.Name == PageDefinitionTagType.Instance.Name)
                {
                    relation = (ProductRelation)tag.Value[0].Value;
                    break;
                }
            }

            if(relation == null)
            {
                // No definition for the page!
                Logger.Instance.WriteToLog("No page definition found! :(");
                return;
            }


            // Find an array object on the page (just use the first one we find), or be sad if we don't find one
            ObservableCollection<ICLPPageObject> objects = page.PageObjects;
            CLPGroupingRegion region = null;

            foreach(ICLPPageObject pageObject in objects)
            {
                if(pageObject.GetType() == typeof(CLPGroupingRegion))
                {
                    region = (CLPGroupingRegion)pageObject;
                    break;
                }
            }

            if(region == null)
            {
                // No CLPGroupingRegion on this page!
                Logger.Instance.WriteToLog("No grouping region found! :(");
                return;
            }

            region.DoInterpretation();
            Logger.Instance.WriteToLog("Done with stamps interpretation");

            // Now we have a list of the possible interpretations of the student's stamps
            ObservableCollection<CLPGrouping> groupings = region.Groupings;

            // Clear out any old stamp-related Tags
            foreach(Tag tag in tags.ToList())
            {
                if(tag.TagType == null ||                                           // Clear out any tags that somehow never got a TagType!
                    tag.TagType.Name == RepresentationCorrectnessTagType.Instance.Name ||
                    tag.TagType.Name == StampPartsPerStampTagType.Instance.Name ||
                    tag.TagType.Name == StampGroupingTypeTagType.Instance.Name)
                {
                    tags.Remove(tag);
                }
            }

            ObservableCollection<Tag> newTags = GetStampTags(groupings, relation);

            foreach(Tag tag in newTags)
            {
                tags.Add(tag);
            }
        }

        /// <summary>
        /// Returns an appropriate StampCorrectnessTag for the given interpretation and product relation
        /// </summary>
        public ObservableCollection<Tag> GetStampTags(ObservableCollection<CLPGrouping> groupings, ProductRelation relation)
        {
            ObservableCollection<Tag> tags = new ObservableCollection<Tag>();
            Tag correctnessTag = new Tag(Tag.Origins.Generated, RepresentationCorrectnessTagType.Instance);
            correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Other")); // The student's work is assumed incorrect until proven correct

            Tag partsPerStampTag = new Tag(Tag.Origins.Generated, StampPartsPerStampTagType.Instance);
            Tag groupingTypeTag = new Tag(Tag.Origins.Generated, StampGroupingTypeTagType.Instance);

            foreach(CLPGrouping grouping in groupings)
            {
                if(HasEqualGroups(grouping) && grouping.Groups[0].Values.Count > 0) // If we can assume that this grouping has a homogeneous structure...
                {
                    int numGroups = grouping.Groups.Count;
                    List<ICLPPageObject> objList = grouping.Groups[0].Values.ToList()[0];
                    int objectsPerGroup = objList.Count;
                    int partsPerObject = objList[0].Parts;
                    int partsPerGroup = objectsPerGroup * partsPerObject;

                    partsPerStampTag.Value.Add(new TagOptionValue(partsPerObject.ToString() + (partsPerObject == 1 ? " part" : " parts")));
                    groupingTypeTag.AddTagOptionValue(new TagOptionValue(grouping.GroupingType));

                    // We're a little stricter about correctness if it's specifically an equal-grouping problem
                    if(relation.RelationType == ProductRelation.ProductRelationTypes.EqualGroups)
                    {
                        if(relation.Factor1.Equals(numGroups.ToString()) && relation.Factor2.Equals(partsPerGroup.ToString()))
                        {
                            correctnessTag.AddTagOptionValue(new TagOptionValue("Correct"));
                            tags.Add(partsPerStampTag);
                            tags.Add(groupingTypeTag);
                            break;
                        }
                        else
                        {
                            if(relation.Factor2.Equals(numGroups.ToString()) && relation.Factor1.Equals(partsPerGroup.ToString()))
                            {
                                correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Swapped Factors"));
                                tags.Add(partsPerStampTag);
                                tags.Add(groupingTypeTag);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if((relation.Factor1.Equals(numGroups.ToString()) && relation.Factor2.Equals(partsPerGroup.ToString())) ||
                            (relation.Factor2.Equals(numGroups.ToString()) && relation.Factor1.Equals(partsPerGroup.ToString())))
                        {
                            correctnessTag.AddTagOptionValue(new TagOptionValue("Correct"));
                            tags.Add(partsPerStampTag);
                            tags.Add(groupingTypeTag);
                            break;
                        }
                    }

                    // If we haven't hit a break yet, then this isn't looking good. Check for a student using the wrong operator
                    ObservableCollection<int> givens = new ObservableCollection<int>();
                    if(relation.Factor1Given) { givens.Add(Convert.ToInt32(relation.Factor1)); }
                    if(relation.Factor2Given) { givens.Add(Convert.ToInt32(relation.Factor2)); }
                    if(relation.ProductGiven) { givens.Add(Convert.ToInt32(relation.Product)); }

                    ObservableCollection<int> numbersUsed = new ObservableCollection<int>();
                    numbersUsed.Add(numGroups);
                    numbersUsed.Add(partsPerGroup);
                    numbersUsed.Add(numGroups * partsPerGroup);

                    if(givens.Count == 2 && numbersUsed.Contains(givens[0]) && numbersUsed.Contains(givens[1]))
                    {
                        correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Misused Givens"));
                        tags.Add(partsPerStampTag);
                        tags.Add(groupingTypeTag);
                        break;
                    }
                }
            }
            tags.Add(correctnessTag); // A correctness tag always gets added
            return tags;
        }

        /// <summary>
        /// Checks a grouping for overall homogeneity.
        /// Returns true iff each subgroup in this grouping contains the same number of page objects, each of which has the same number of parts
        /// </summary>
        public Boolean HasEqualGroups(CLPGrouping grouping)
        {
            int expectedPartsPerObject = -1;
            int expectedObjectsPerGroup = -1;

            foreach(Dictionary<string,List<ICLPPageObject>> groupEntry in grouping.Groups)
            {
                foreach(KeyValuePair<string,List<ICLPPageObject>> kvp in groupEntry)
                {
                    List<ICLPPageObject> objList = kvp.Value;
                    int objectsInGroup = objList.Count;
                    if(expectedObjectsPerGroup == -1) { expectedObjectsPerGroup = objectsInGroup; }
                    else if(expectedObjectsPerGroup != objectsInGroup) { return false; }

                    foreach(ICLPPageObject pageObj in objList)
                    {
                        int parts = pageObj.Parts;
                        if(expectedPartsPerObject == -1) { expectedPartsPerObject = parts; }
                        else if(expectedPartsPerObject != parts) { return false; }
                    }
                }
            }

            return true;
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
            CLPAudio audio = new CLPAudio(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
            CLPServiceAgent.Instance.AddPageObjectToPage(audio);
        }

        /// <summary>
        /// Gets the InsertProtractorCommand command.
        /// </summary>
        public Command InsertProtractorCommand { get; private set; }

        private void OnInsertProtractorCommandExecute()
        {
            CLPShape square = new CLPShape(CLPShape.CLPShapeType.Protractor, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
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
            CLPShape square = new CLPShape(CLPShape.CLPShapeType.Rectangle, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
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
            CLPShape circle = new CLPShape(CLPShape.CLPShapeType.Ellipse, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
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
            CLPShape line = new CLPShape(CLPShape.CLPShapeType.HorizontalLine, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
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
            CLPShape line = new CLPShape(CLPShape.CLPShapeType.VerticalLine, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
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
                CLPHandwritingRegion region = new CLPHandwritingRegion(selected_type, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
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
            CLPInkShapeRegion region = new CLPInkShapeRegion(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
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
            CLPGroupingRegion region = new CLPGroupingRegion(((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
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

                CLPDataTable region = new CLPDataTable(rows, cols, selected_type, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
               
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

                CLPShadingRegion region = new CLPShadingRegion(rows, cols, ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage);
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
            var currentPage = ((MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as CLPMirrorDisplay).CurrentPage;
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

        #endregion //Debug Commands

        #endregion //Commands

    }
}
