﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using Catel.Collections;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Services;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities;
using System.Windows.Threading;
using System.ServiceModel;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum Panels
    {
        NotebookPages,
        StudentWork,
        Progress,
        Displays,
        PageInformation,
        Webcam
    }

    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class RibbonViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindow
        {
            get { return App.MainWindowViewModel; }
        }

        public static CLPPage CurrentPage 
        {
            get { return NotebookPagesPanelViewModel.GetCurrentPage(); }
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

            PenSize = 2;
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
            CurrentLeftPanel = Panels.NotebookPages;
            IsProjectorFrozen = App.CurrentUserMode != App.UserMode.Projector;
        }

        private void InitializeCommands()
        {
            //File Menu
            NewNotebookCommand = new Command(OnNewNotebookCommandExecute);
            OpenNotebookCommand = new Command(OnOpenNotebookCommandExecute);
            CopyNotebookForNewOwnerCommand = new Command(OnCopyNotebookForNewOwnerCommandExecute);
            SaveNotebookCommand = new Command(OnSaveNotebookCommandExecute);
            ForceSaveNotebookCommand = new Command(OnForceSaveNotebookCommandExecute);
            SaveAllNotebooksCommand = new Command(OnSaveAllNotebooksCommandExecute);
            ConvertDisplaysToXPSCommand = new Command(OnConvertDisplaysToXPSCommandExecute);
            ConvertToXPSCommand = new Command(OnConvertToXPSCommandExecute);
            ConvertPageSubmissionToXPSCommand = new Command(OnConvertPageSubmissionToXPSCommandExecute);
            ConvertAllSubmissionsToXPSCommand = new Command(OnConvertAllSubmissionsToXPSCommandExecute);
            StartClassPeriodCommand = new Command(OnStartClassPeriodCommandExecute);
            RefreshNetworkCommand = new Command(OnRefreshNetworkCommandExecute);
            ToggleThumbnailsCommand = new Command(OnToggleThumbnailsCommandExecute);
            ExitCommand = new Command(OnExitCommandExecute);

            //Notebook
            PreviousPageCommand = new Command(OnPreviousPageCommandExecute, OnPreviousPageCanExecute);
            NextPageCommand = new Command(OnNextPageCommandExecute, OnNextPageCanExecute);

            //Tools
            SetPenColorCommand = new Command<RibbonButton>(OnSetPenColorCommandExecute);

            //Submission
            SubmitPageCommand = new Command(OnSubmitPageCommandExecute, OnInsertPageObjectCanExecute);
            GroupSubmitPageCommand = new Command(OnGroupSubmitPageCommandExecute, OnInsertPageObjectCanExecute);

            //History
            DisableHistoryCommand = new Command(OnDisableHistoryCommandExecute, OnClearHistoryCommandCanExecute);
            ReplayCommand = new Command(OnReplayCommandExecute);
            UndoCommand = new Command(OnUndoCommandExecute, OnUndoCanExecute);
            RedoCommand = new Command(OnRedoCommandExecute, OnRedoCanExecute);
            ClearPageHistoryCommand = new Command(OnClearPageHistoryCommandExecute, OnClearHistoryCommandCanExecute);
            ClearPageNonAnimationHistoryCommand = new Command(OnClearPageNonAnimationHistoryCommandExecute, OnClearHistoryCommandCanExecute);
            ClearPagesHistoryCommand = new Command(OnClearPagesHistoryCommandExecute, OnClearHistoryCommandCanExecute);
            ClearPagesNonAnimationHistoryCommand = new Command(OnClearPagesNonAnimationHistoryCommandExecute, OnClearHistoryCommandCanExecute);
            ClearSubmissionsHistoryCommand = new Command(OnClearSubmissionsHistoryCommandExecute, OnClearHistoryCommandCanExecute);
            ClearSubmissionsNonAnimationHistoryCommand = new Command(OnClearSubmissionsNonAnimationHistoryCommandExecute, OnClearHistoryCommandCanExecute);

            //Insert
            ToggleWebcamPanelCommand = new Command<bool>(OnToggleWebcamPanelCommandExecute);
            InsertImageCommand = new Command(OnInsertImageCommandExecute, OnInsertPageObjectCanExecute);
            InsertStaticImageCommand = new Command<string>(OnInsertStaticImageCommandExecute, OnInsertPageObjectCanExecute);
            InsertBlankStampCommand = new Command(OnInsertBlankStampCommandExecute, OnInsertPageObjectCanExecute);
            InsertImageStampCommand = new Command(OnInsertImageStampCommandExecute, OnInsertPageObjectCanExecute);
            InsertBlankContainerStampCommand = new Command(OnInsertBlankContainerStampCommandExecute, OnInsertPageObjectCanExecute);
            InsertImageContainerStampCommand = new Command(OnInsertImageContainerStampCommandExecute, OnInsertPageObjectCanExecute);
            InsertArrayCommand = new Command<string>(OnInsertArrayCommandExecute, OnInsertPageObjectCanExecute);
            InsertProtractorCommand = new Command(OnInsertProtractorCommandExecute, OnInsertPageObjectCanExecute);
            InsertSquareShapeCommand = new Command(OnInsertSquareShapeCommandExecute, OnInsertPageObjectCanExecute);
            InsertCircleShapeCommand = new Command(OnInsertCircleShapeCommandExecute, OnInsertPageObjectCanExecute);
            InsertHorizontalLineShapeCommand = new Command(OnInsertHorizontalLineShapeCommandExecute, OnInsertPageObjectCanExecute);
            InsertVerticalLineShapeCommand = new Command(OnInsertVerticalLineShapeCommandExecute, OnInsertPageObjectCanExecute);
            InsertTextBoxCommand = new Command(OnInsertTextBoxCommandExecute, OnInsertPageObjectCanExecute);
            InsertAggregationDataTableCommand = new Command(OnInsertAggregationDataTableCommandExecute, OnInsertPageObjectCanExecute);
            InsertAudioCommand = new Command(OnInsertAudioCommandExecute, OnInsertPageObjectCanExecute);
            InsertHandwritingRegionCommand = new Command(OnInsertHandwritingRegionCommandExecute, OnInsertPageObjectCanExecute);
            InsertInkShapeRegionCommand = new Command(OnInsertInkShapeRegionCommandExecute, OnInsertPageObjectCanExecute);
            InsertDataTableCommand = new Command(OnInsertDataTableCommandExecute, OnInsertPageObjectCanExecute);
            InsertShadingRegionCommand = new Command(OnInsertShadingRegionCommandExecute, OnInsertPageObjectCanExecute);
            InsertGroupingRegionCommand = new Command(OnInsertGroupingRegionCommandExecute, OnInsertPageObjectCanExecute);  
            
            //Testing
            TurnOffWebcamSharing = new Command(OnTurnOffWebcamSharingExecute);
            BroadcastPageCommand = new Command(OnBroadcastPageCommandExecute);
            ReplacePageCommand = new Command(OnReplacePageCommandExecute);
            CreatePageSubmissionCommand = new Command(OnCreatePageSubmissionCommandExecute);
            ShowTagsCommand = new Command(OnShowTagsCommandExecute);

            //Page
            AddNewPageCommand = new Command<string>(OnAddNewPageCommandExecute);
            AddNewProofPageCommand = new Command<string>(OnAddNewProofPageCommandExecute);
            MovePageUpCommand = new Command(OnMovePageUpCommandExecute, OnMovePageUpCanExecute);
            MovePageDownCommand = new Command(OnMovePageDownCommandExecute, OnMovePageDownCanExecute);
            SwitchPageLayoutCommand = new Command(OnSwitchPageLayoutCommandExecute);
            SwitchPageTypeCommand = new Command(OnSwitchPageTypeCommandExecute);

            DeletePageCommand = new Command(OnDeletePageCommandExecute, OnInsertPageObjectCanExecute);
            CopyPageCommand = new Command(OnCopyPageCommandExecute, OnInsertPageObjectCanExecute);
            MakePageLongerCommand = new Command(OnMakePageLongerCommandExecute, OnInsertPageObjectCanExecute);
            TrimPageCommand = new Command(OnTrimPageCommandExecute, OnInsertPageObjectCanExecute);
            ClearPageCommand = new Command(OnClearPageCommandExecute, OnInsertPageObjectCanExecute);

            //Metadata
            AddPageTopicCommand = new Command(OnAddPageTopicCommandExecute, OnInsertPageObjectCanExecute);
            EditPageMetadataCommand = new Command(OnEditPageMetadataCommandExecute, OnInsertPageObjectCanExecute);
            SetNotebookCurriculumCommand = new Command(OnSetNotebookCurriculumCommandExecute);

            //Debug
            InterpretPageCommand = new Command(OnInterpretPageCommandExecute);
            UpdateObjectPropertiesCommand = new Command(OnUpdateObjectPropertiesCommandExecute);

            //Authoring
            EditPageDefinitionCommand = new Command(OnEditPageDefinitionCommandExecute);
            SetEntireNotebookAsAuthoredCommand = new Command(OnSetEntireNotebookAsAuthoredCommandExecute);

            //Analysis
            AnalyzeArrayCommand = new Command(OnAnalyzeArrayCommandExecute);
            AnalyzeFuzzyFactorCardCommand = new Command(OnAnalyzeFuzzyFactorCardCommandExecute);
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
            switch(EraserType)
            {
                case "Point Eraser":
                    EraserMode = InkCanvasEditingMode.EraseByPoint;
                    break;
                case "Stroke Eraser":
                    EraserMode = InkCanvasEditingMode.EraseByStroke;
                    break;
            }
            if(PageInteractionMode != PageInteractionMode.Pen &&
               PageInteractionMode != PageInteractionMode.Highlighter)
            {
                PageInteractionMode = PageInteractionMode.Pen;
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
        public PageInteractionMode PageInteractionMode
        {
            get { return GetValue<PageInteractionMode>(PageInteractionModeProperty); }
            set { SetValue(PageInteractionModeProperty, value); }
        }

        public static readonly PropertyData PageInteractionModeProperty = RegisterProperty("PageInteractionMode", typeof(PageInteractionMode));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Panels? CurrentLeftPanel
        {
            get { return GetValue<Panels?>(CurrentLeftPanelProperty); }
            set { SetValue(CurrentLeftPanelProperty, value); }
        }

        public static readonly PropertyData CurrentLeftPanelProperty = RegisterProperty("CurrentLeftPanel", typeof(Panels?));

        /// <summary>
        /// Right Panel.
        /// </summary>
        public Panels? CurrentRightPanel
        {
            get { return GetValue<Panels?>(CurrentRightPanelProperty); }
            set { SetValue(CurrentRightPanelProperty, value); }
        }

        public static readonly PropertyData CurrentRightPanelProperty = RegisterProperty("CurrentRightPanel", typeof(Panels?));

        /// <summary>
        /// Whether or not to mirror the displays to the projector.
        /// </summary>
        public bool IsProjectorFrozen
        {
            get { return GetValue<bool>(IsProjectorFrozenProperty); }
            set { SetValue(IsProjectorFrozenProperty, value); }
        }

        public static readonly PropertyData IsProjectorFrozenProperty = RegisterProperty("IsProjectorFrozen", typeof(bool), true);

        /// <summary>
        /// Disables the use of history to broadcast changes to a page to the projector.
        /// </summary>
        public bool IsBroadcastHistoryDisabled
        {
            get { return GetValue<bool>(IsBroadcastHistoryDisabledProperty); }
            set { SetValue(IsBroadcastHistoryDisabledProperty, value); }
        }

        public static readonly PropertyData IsBroadcastHistoryDisabledProperty = RegisterProperty("IsBroadcastHistoryDisabled", typeof(bool), false);

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
            
                if(App.MainWindowViewModel.AvailableUsers.Any())
                {
                    Parallel.ForEach(App.MainWindowViewModel.AvailableUsers,
                                     student =>
                                     {
                                         try
                                            {
                                                var binding = new NetTcpBinding
                                                              {
                                                                  Security = {
                                                                                 Mode = SecurityMode.None
                                                                             }
                                                              };
                                                var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(binding, new EndpointAddress(student.CurrentMachineAddress));
                                                studentProxy.TogglePenDownMode(value);
                                                (studentProxy as ICommunicationObject).Close();
                                            }
                                            catch(Exception ex)
                                            {
                                                Console.WriteLine(ex.Message);
                                            }
                                     });
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
            MainWindowViewModel.CreateNewNotebook();
        }

        /// <summary>
        /// Opens a notebook from the Notebooks folder.
        /// </summary>
        public Command OpenNotebookCommand { get; private set; }

        private void OnOpenNotebookCommandExecute()
        {
            MainWindow.Workspace = new NotebookChooserWorkspaceViewModel();
        }

        /// <summary>
        /// Copies the current authored notebook for a new owner.
        /// </summary>
        public Command CopyNotebookForNewOwnerCommand { get; private set; }

        private void OnCopyNotebookForNewOwnerCommandExecute()
        {
            MainWindowViewModel.CopyNotebookForNewOwner();
        }

        /// <summary>
        /// Saves the current notebook to disk.
        /// </summary>
        public Command SaveNotebookCommand { get; private set; }

        private void OnSaveNotebookCommandExecute()
        {
            if(App.MainWindowViewModel.Workspace is NotebookWorkspaceViewModel)
            {
                Catel.Windows.PleaseWaitHelper.Show(() =>
                    MainWindowViewModel.SaveNotebook((App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).Notebook), null, "Saving Notebook");
            }
        }

        /// <summary>
        /// Bypasses IsCached during save to fully save everything.
        /// </summary>
        public Command ForceSaveNotebookCommand { get; private set; }

        private void OnForceSaveNotebookCommandExecute()
        {
            if(App.MainWindowViewModel.Workspace is NotebookWorkspaceViewModel)
            {
                Catel.Windows.PleaseWaitHelper.Show(() =>
                    MainWindowViewModel.SaveNotebook((App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).Notebook, true), null, "Saving Notebook");
            }
        }

        /// <summary>
        /// Saves all open notebooks to disk.
        /// </summary>
        public Command SaveAllNotebooksCommand { get; private set; }

        private void OnSaveAllNotebooksCommandExecute()
        {
            // TODO: Entities
            //foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
            //{
            //    CLPServiceAgent.Instance.SaveNotebook(notebook);
            //}
        }

        /// <summary>
        /// Converts Notebook Pages to XPS.
        /// </summary>
        public Command ConvertDisplaysToXPSCommand { get; private set; }

        private void OnConvertDisplaysToXPSCommandExecute()
        {
            // TODO: Entities
            //var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            //if(notebookWorkspaceViewModel == null)
            //{
            //    return;
            //}

            //Catel.Windows.PleaseWaitHelper.Show(() =>
            //{
            //    var notebook = notebookWorkspaceViewModel.Notebook;
            //    var directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Displays - XPS\";
            //    if(!Directory.Exists(directoryPath))
            //    {
            //        Directory.CreateDirectory(directoryPath);
            //    }

            //    var fileName = notebook.Name + " - Displays.xps";
            //    var filePath = directoryPath + fileName;
            //    if(File.Exists(filePath))
            //    {
            //        File.Delete(filePath);
            //    }

            //    var document = new FixedDocument();
            //    document.DocumentPaginator.PageSize = new Size(96 * 11, 96 * 8.5);

            //    foreach(var display in notebook.Displays)
            //    {
            //        var currentDisplayView = new GridDisplayPreviewView
            //                                 {
            //                                     DataContext = display,
            //                                     Height = 96 * 8.5,
            //                                     Width = 96 * 11
            //                                 };
            //        currentDisplayView.UpdateLayout();
            //        var gridDisplay = display as CLPGridDisplay;
            //        var displayIndex = gridDisplay != null ? gridDisplay.DisplayNumber : 0;

            //        var grid = new Grid();
            //        grid.Children.Add(currentDisplayView);
            //        var pageIndexlabel = new Label
            //        {
            //            FontSize = 20,
            //            FontWeight = FontWeights.Bold,
            //            FontStyle = FontStyles.Oblique,
            //            HorizontalAlignment = HorizontalAlignment.Right,
            //            VerticalAlignment = VerticalAlignment.Top,
            //            Content = "Display " + displayIndex,
            //            Margin = new Thickness(0, 5, 5, 0)
            //        };
            //        grid.Children.Add(pageIndexlabel);
            //        grid.UpdateLayout();

            //        var transform = new TransformGroup();
            //        var rotate = new RotateTransform(90.0);
            //        var translate = new TranslateTransform(816, 0);
            //        transform.Children.Add(rotate);
            //        transform.Children.Add(translate);
            //        grid.RenderTransform = transform;

            //        var pageContent = new PageContent();
            //        var fixedPage = new FixedPage();
            //        fixedPage.Children.Add(grid);

            //        ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
            //        document.Pages.Add(pageContent);
            //    }

            //    //Save the document
            //    var xpsDocument = new XpsDocument(filePath, FileAccess.ReadWrite);
            //    var documentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            //    documentWriter.Write(document);
            //    xpsDocument.Close();

            //}, null, "Converting Notebook Displays to XPS", 0.0 / 0.0);
        }

        private void AddCLPPageToXPSDocument(CLPPage page, ref FixedDocument document)
        {
            page.TrimPage();
            var printHeight = page.Width / page.InitialAspectRatio;

            double transformAmount = 0;
            do
            {
                var currentPageView = new CLPPagePreviewView { DataContext = page };
                currentPageView.UpdateLayout();

                var grid = new Grid();
                grid.Children.Add(currentPageView);

                var pageNumberOffset = 5.0;
                if(page.SubmissionType != SubmissionTypes.Unsubmitted)
                {
                    var submitterLabel = new Label
                    {
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        FontStyle = FontStyles.Oblique,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Content = page.Owner.FullName,
                        Margin = new Thickness(0, transformAmount + 5, 5, 0)
                    };
                    grid.Children.Add(submitterLabel);
                    pageNumberOffset = 30.0;
                }

                var pageIndexlabel = new Label
                {
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    FontStyle = FontStyles.Oblique,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Content = "Page " + page.PageNumber,
                    Margin = new Thickness(0, transformAmount + pageNumberOffset, 5, 0)
                };
                grid.Children.Add(pageIndexlabel);
                grid.UpdateLayout();

                var transform = new TransformGroup();
                var translate = new TranslateTransform(0, -transformAmount);
                transform.Children.Add(translate);
                if(Math.Abs(page.Width - CLPPage.LANDSCAPE_WIDTH) < 0.001)
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
            } while(page.Height > transformAmount);
        }

        /// <summary>
        /// Converts Notebook Pages to XPS.
        /// </summary>
        public Command ConvertToXPSCommand { get; private set; }

        private void OnConvertToXPSCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            Catel.Windows.PleaseWaitHelper.Show(() =>
            {
                var notebook = notebookWorkspaceViewModel.Notebook;
                var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"Notebooks - XPS");
                if(!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileName = notebook.Name + ".xps";
                var filePath = Path.Combine(directoryPath, fileName);
                if(File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                var document = new FixedDocument();
                document.DocumentPaginator.PageSize = new Size(96 * 11, 96 * 8.5);

                foreach(var page in notebook.Pages)
                {
                    page.TrimPage();
                    var printHeight = page.Width / page.InitialAspectRatio;

                    double transformAmount = 0;
                    do
                    {
                        var currentPageView = new CLPPagePreviewView { DataContext = page };
                        currentPageView.UpdateLayout();

                        var grid = new Grid();
                        grid.Children.Add(currentPageView);

                        var pageNumberOffset = 5.0;
                        if(page.SubmissionType != SubmissionTypes.Unsubmitted)
                        {
                            var submitterLabel = new Label
                            {
                                FontSize = 20,
                                FontWeight = FontWeights.Bold,
                                FontStyle = FontStyles.Oblique,
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = VerticalAlignment.Top,
                                Content = page.Owner.FullName,
                                Margin = new Thickness(0, transformAmount + 5, 5, 0)
                            };
                            grid.Children.Add(submitterLabel);
                            pageNumberOffset = 30.0;
                        }

                        var pageIndexlabel = new Label
                        {
                            FontSize = 20,
                            FontWeight = FontWeights.Bold,
                            FontStyle = FontStyles.Oblique,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Top,
                            Content = "Page " + page.PageNumber,
                            Margin = new Thickness(0, transformAmount + pageNumberOffset, 5, 0)
                        };
                        grid.Children.Add(pageIndexlabel);
                        grid.UpdateLayout();

                        var transform = new TransformGroup();
                        var translate = new TranslateTransform(0, -transformAmount);
                        transform.Children.Add(translate);
                        if(Math.Abs(page.Width - CLPPage.LANDSCAPE_WIDTH) < 0.001)
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
                    } while(page.Height > transformAmount);
                }

                //Save the document
                var xpsDocument = new XpsDocument(filePath, FileAccess.ReadWrite);
                var documentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                documentWriter.Write(document);
                xpsDocument.Close();

            }, null, "Converting Notebook Pages to XPS");
        }

        /// <summary>
        /// Converts the Submissions of the currently selected page to XPS.
        /// </summary>
        public Command ConvertPageSubmissionToXPSCommand { get; private set; }

        private void OnConvertPageSubmissionToXPSCommandExecute()
        {
            // TODO: Entities
            //var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            //if(notebookWorkspaceViewModel == null)
            //{
            //    return;
            //}

            //Catel.Windows.PleaseWaitHelper.Show(() =>
            //{
            //    var notebook = notebookWorkspaceViewModel.Notebook;
            //    string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\";
            //    if(!Directory.Exists(directoryPath))
            //    {
            //        Directory.CreateDirectory(directoryPath);
            //    }

            //    var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            //    if(currentPage == null)
            //    {
            //        return;
            //    }

            //    string fileName = notebook.NotebookName + " - Page " + currentPage.PageIndex + " Submissions.xps";
            //    string filePath = directoryPath + fileName;
            //    if(File.Exists(filePath))
            //    {
            //        File.Delete(filePath);
            //    }

            //    if(!notebook.Submissions[currentPage.UniqueID].Any())
            //    {
            //        return;
            //    }

            //    var document = new FixedDocument();
            //    document.DocumentPaginator.PageSize = new Size(96 * 11, 96 * 8.5);

            //    foreach(var page in notebook.Submissions[currentPage.UniqueID])
            //    {
    
            //    }

            //    //Save the document
            //    var xpsDocument = new XpsDocument(filePath, FileAccess.ReadWrite);
            //    var documentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            //    documentWriter.Write(document);
            //    xpsDocument.Close();
            //}, null, "Converting Submissions for this page to XPS", 0.0 / 0.0);
        }

        /// <summary>
        /// Converts all Submissions in a notebook to XPS.
        /// </summary>
        public Command ConvertAllSubmissionsToXPSCommand { get; private set; }

        private void OnConvertAllSubmissionsToXPSCommandExecute()
        {
            // TODO: Entities
            //var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            //if(notebookWorkspaceViewModel == null)
            //{
            //    return;
            //}

            //Catel.Windows.PleaseWaitHelper.Show(() =>
            //{
            //    var notebook = notebookWorkspaceViewModel.Notebook;
            //    string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks - XPS\";
            //    if(!Directory.Exists(directoryPath))
            //    {
            //        Directory.CreateDirectory(directoryPath);
            //    }

            //    string fileName = notebook.NotebookName + " - All Submissions.xps";
            //    string filePath = directoryPath + fileName;
            //    if(File.Exists(filePath))
            //    {
            //        File.Delete(filePath);
            //    }

            //    var document = new FixedDocument();
            //    document.DocumentPaginator.PageSize = new Size(96 * 11, 96 * 8.5);

                
            //    foreach(var clpPage in notebook.Pages)
            //    {
            //        foreach(var page in notebook.Submissions[clpPage.UniqueID])
                   
               
            //  //  foreach(var page in notebook.Submissions.Keys.SelectMany(pageID => notebook.Submissions[pageID]))
            //    {
         
            //    }
            //         }

            //    //Save the document
            //    var xpsDocument = new XpsDocument(filePath, FileAccess.ReadWrite);
            //    var documentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            //    documentWriter.Write(document);
            //    xpsDocument.Close();
            //}, null, "Converting All Submissions to XPS", 0.0 / 0.0);
        }

        /// <summary>
        /// Starts the closest <see cref="ClassPeriod" />.
        /// </summary>
        public Command StartClassPeriodCommand { get; private set; }

        private void OnStartClassPeriodCommandExecute()
        {
            MainWindowViewModel.OpenClassPeriod();
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
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            var panel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(panel == null || currentPage == null)
            {
                return;
            }

            var index = panel.Notebook.Pages.IndexOf(currentPage);

            if(index > 0)
            {
                var page = panel.Notebook.Pages[index - 1];
                panel.CurrentPage = page;
            }
        }

        private bool OnPreviousPageCanExecute()
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            var panel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(panel == null || currentPage == null)
            {
                return false;
            }

            var index = panel.Notebook.Pages.IndexOf(currentPage);
            return index > 0;
        }

        /// <summary>
        /// Navigates to the next page in the notebook.
        /// </summary>
        public Command NextPageCommand { get; private set; }

        private void OnNextPageCommandExecute()
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            var panel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(panel == null || currentPage == null)
            {
                return;
            }

            var index = panel.Notebook.Pages.IndexOf(currentPage);
            if(index < panel.Notebook.Pages.Count - 1)
            {
                var page = panel.Notebook.Pages[index + 1];
                panel.CurrentPage = page;
            }
        }

        private bool OnNextPageCanExecute()
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            var panel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(panel == null || currentPage == null)
            {
                return false;
            }

            var index = panel.Notebook.Pages.IndexOf(currentPage);
            return index < panel.Notebook.Pages.Count - 1;
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
            var timer = new System.Timers.Timer
                        {
                            Interval = 2000
                        };
            timer.Elapsed += timer_Elapsed;
            timer.Enabled = true;

            var page = NotebookPagesPanelViewModel.GetCurrentPage();
            var notebookPagesPanel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(!CanSendToTeacher || page == null || notebookPagesPanel == null)
            {
                return;
            }

            CLPServiceAgent.Instance.SubmitPage(page, notebookPagesPanel.Notebook.ID, false);
            
            CanSendToTeacher = false;
        }

        /// <summary>
        /// Submits the current page as a group.
        /// </summary>
        public Command GroupSubmitPageCommand { get; private set; }

        private void OnGroupSubmitPageCommandExecute()
        {
            IsSending = true;
            var timer = new System.Timers.Timer
                        {
                            Interval = 2000
                        };
            timer.Elapsed += timer_Elapsed;
            timer.Enabled = true;

            var page = NotebookPagesPanelViewModel.GetCurrentPage();
            var notebookPagesPanel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(!CanGroupSendToTeacher || page == null || notebookPagesPanel == null)
            {
                return;
            }

            CLPServiceAgent.Instance.SubmitPage(page, notebookPagesPanel.Notebook.ID, true);
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
            if(timer == null)
            {
                return;
            }
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

        private bool OnClearHistoryCommandCanExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return false;
            }

            return notebookWorkspaceViewModel.CurrentDisplay == null;
        }

        /// <summary>
        /// Prevents history from storing actions.
        /// </summary>
        public Command DisableHistoryCommand { get; private set; }

        private void OnDisableHistoryCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            var notebook = notebookWorkspaceViewModel.Notebook;
            foreach(var page in notebook.Pages)
            {
                page.History.UseHistory = false;
            }
        }

        /// <summary>
        /// Replays the entire history of the current page.
        /// </summary>
        public Command ReplayCommand { get; private set; }

        private void OnReplayCommandExecute()
        {
            var currentPage = CurrentPage;
            if(currentPage == null) { return; }

            var oldPageInteractionMode = (PageInteractionMode == PageInteractionMode.None) ? PageInteractionMode.Pen : PageInteractionMode;
            PageInteractionMode = PageInteractionMode.None;

            while(currentPage.History.UndoItems.Any()) { currentPage.History.Undo(); }

            var t = new Thread(() =>
                               {
                                   while(currentPage.History.RedoItems.Any())
                                   {
                                       var historyItemAnimationDelay = Convert.ToInt32(Math.Round(currentPage.History.CurrentAnimationDelay / 2.0));
                                       Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                             (DispatcherOperationCallback)delegate
                                                                                                          {
                                                                                                              currentPage.History.Redo(true);
                                                                                                              return null;
                                                                                                          },
                                                                             null);
                                       Thread.Sleep(historyItemAnimationDelay);
                                   }
                                   PageInteractionMode = oldPageInteractionMode;
                               });

            t.Start();
        }

        /// <summary>
        /// Undoes the last action.
        /// </summary>
        public Command UndoCommand { get; private set; }

        private void OnUndoCommandExecute()
        {
            CurrentPage.History.Undo();
        }

        private bool OnUndoCanExecute()
        {
            var page = CurrentPage;
            if(page == null)
            {
                return false;
            }
            
            var recordIndicator = page.History.RedoItems.FirstOrDefault() as AnimationIndicator;
            if(recordIndicator != null && recordIndicator.AnimationIndicatorType == AnimationIndicatorType.Record)
            {
                return false;
            }

            return page.History.CanUndo;
        }

        /// <summary>
        /// Redoes the last undone action.
        /// </summary>
        public Command RedoCommand { get; private set; }

        private void OnRedoCommandExecute()
        {
            CurrentPage.History.Redo();
        }

        private bool OnRedoCanExecute()
        {
            var page = CurrentPage;
            if(page == null)
            {
                return false;
            }

            return page.History.CanRedo;
        }

        /// <summary>
        /// Completely clears the history for the current page.
        /// </summary>
        public Command ClearPageHistoryCommand { get; private set; }

        private void OnClearPageHistoryCommandExecute()
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            if(currentPage == null) { return; }

            currentPage.History.ClearHistory();
        }

        /// <summary>
        /// Completely clears the non-animation history for the current page.
        /// </summary>
        public Command ClearPageNonAnimationHistoryCommand { get; private set; }

        private void OnClearPageNonAnimationHistoryCommandExecute()
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            if(currentPage == null) { return; }

            currentPage.History.ClearNonAnimationHistory();
        }

        /// <summary>
        /// Completely clears all histories for regular pages in a notebook.
        /// </summary>
        public Command ClearPagesHistoryCommand { get; private set; }

        private void OnClearPagesHistoryCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            var notebook = notebookWorkspaceViewModel.Notebook;
            foreach(var page in notebook.Pages)
            {
                page.History.ClearHistory();
            }
        }

        /// <summary>
        /// Completely clears all non-animation histories for regular pages in a notebook.
        /// </summary>
        public Command ClearPagesNonAnimationHistoryCommand { get; private set; }

        private void OnClearPagesNonAnimationHistoryCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            var notebook = notebookWorkspaceViewModel.Notebook;
            foreach(var page in notebook.Pages)
            {
                page.History.ClearNonAnimationHistory();
            }
        }

        /// <summary>
        /// Completely clears all histories for submissions in a notebook.
        /// </summary>
        public Command ClearSubmissionsHistoryCommand { get; private set; }

        private void OnClearSubmissionsHistoryCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            var notebook = notebookWorkspaceViewModel.Notebook;
            // TODO: Entities
            //foreach(var submission in notebook.Submissions.Keys.SelectMany(pageID => notebook.Submissions[pageID])) 
            //{
            //    submission.PageHistory.ClearHistory();
            //}
        }

        /// <summary>
        /// Completely clears all non-animation histories for regular pages in a notebook.
        /// </summary>
        public Command ClearSubmissionsNonAnimationHistoryCommand { get; private set; }

        private void OnClearSubmissionsNonAnimationHistoryCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            var notebook = notebookWorkspaceViewModel.Notebook;
            // TODO: Entities
            //foreach(var submission in notebook.Submissions.Keys.SelectMany(pageID => notebook.Submissions[pageID])) 
            //{
            //    submission.PageHistory.ClearNonAnimationHistory();
            //}
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
        /// Broadcast the current page of a SingleDisplay to all connected Students.
        /// </summary>
        public Command BroadcastPageCommand { get; private set; }

        private void OnBroadcastPageCommandExecute()
        {
            // TODO: Entities
            ////TODO: Steve - also broadcast to Projector
            //var page = NotebookPagesPanelViewModel.GetCurrentPage();
            //page.SerializedStrokes = StrokeDTO.SaveInkStrokes(page.InkStrokes);
            //var sPage = ObjectSerializer.ToString(page);
            //var zippedPage = CLPServiceAgent.Instance.Zip(sPage);
            //int index = (App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).Notebook.Pages.IndexOf(page);

            //if(App.Network.ClassList.Any())
            //{
            //    foreach(var student in App.Network.ClassList)
            //    {
            //        try
            //        {
            //            var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(App.Network.DefaultBinding, new EndpointAddress(student.CurrentMachineAddress));
            //            studentProxy.AddNewPage(zippedPage, index);
            //            (studentProxy as ICommunicationObject).Close();
            //        }
            //        catch(Exception ex)
            //        {
            //            Console.WriteLine(ex.Message);
            //        }
            //    }
            //    if(App.Network.ProjectorProxy != null)
            //    {
            //        try
            //        {
            //            App.Network.ProjectorProxy.AddNewPage(zippedPage, index);
            //        }
            //        catch(Exception)
            //        {
            //        }
            //    }
            //    else
            //    {
            //        Logger.Instance.WriteToLog("No Projector Found");
            //    }
            //}
            //else
            //{
            //    Logger.Instance.WriteToLog("No Students Found");
            //}
        }

        /// <summary>
        /// Replaces the current page on all Student Machines.
        /// </summary>
        public Command ReplacePageCommand { get; private set; }

        private void OnReplacePageCommandExecute()
        {
            // TODO: Entities
            ////TODO: Steve - also broadcast to Projector
            //var page = NotebookPagesPanelViewModel.GetCurrentPage();
            //page.SerializedStrokes = StrokeDTO.SaveInkStrokes(page.InkStrokes);
            //var sPage = ObjectSerializer.ToString(page);
            //var zippedPage = CLPServiceAgent.Instance.Zip(sPage);
            //var index = (App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).Notebook.Pages.IndexOf(page);

            //if(App.Network.ClassList.Count > 0)
            //{
            //    foreach(Person student in App.Network.ClassList)
            //    {
            //        try
            //        {
            //            var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(App.Network.DefaultBinding, new EndpointAddress(student.CurrentMachineAddress));
            //            studentProxy.ReplacePage(zippedPage, index);
            //            (studentProxy as ICommunicationObject).Close();
            //        }
            //        catch(Exception ex)
            //        {
            //            Console.WriteLine(ex.Message);
            //        }
            //    }
            //    if(App.Network.ProjectorProxy != null)
            //    {
            //        try
            //        {
            //            App.Network.ProjectorProxy.ReplacePage(zippedPage, index);
            //        }
            //        catch(Exception)
            //        {
            //        }
            //    }
            //    else
            //    {
            //        Logger.Instance.WriteToLog("No Projector Found");
            //    }
            //}
            //else
            //{
            //    Logger.Instance.WriteToLog("No Students Found");
            //}
        }


        /// <summary>
        /// Creates a submission for the current page.
        /// </summary>
        public Command CreatePageSubmissionCommand
        {
            get;
            private set;
        }

        private void OnCreatePageSubmissionCommandExecute()
        {
            var submission = CurrentPage.DuplicatePage();
            submission.ID = CurrentPage.ID;
            submission.VersionIndex = 1;
            submission.OwnerID = Person.TestSubmitter.ID;
            CurrentPage.Submissions.Add(submission);
        }

        public Command ShowTagsCommand { get; private set; }

        private void OnShowTagsCommandExecute()
        {
            // TODO: Entities
            var page = NotebookPagesPanelViewModel.GetCurrentPage();

            string tags = "";
            foreach(var t in page.Tags)
            {
                tags = tags + t.GetType().ToString() + " = " + t.Value + "\n";
            }

            var tagsView = new SimpleTextWindowView("Tags for this page", tags);
            tagsView.Owner = Application.Current.MainWindow;
            tagsView.ShowDialog();
        }

        #endregion //Testing

        #region Page Commands

        /// <summary>
        /// Adds a new page to the notebook.
        /// </summary>
        public Command<string> AddNewPageCommand { get; private set; }

        private void OnAddNewPageCommandExecute(string pageOrientation)
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            var notebookPanel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(notebookPanel == null || currentPage == null)
            {
                return;
            }
            var index = notebookPanel.Pages.IndexOf(currentPage);
            index++;
            var page = new CLPPage(App.MainWindowViewModel.CurrentUser);
            if(pageOrientation == "Portrait")
            {
                page.Height = CLPPage.PORTRAIT_HEIGHT;
                page.Width = CLPPage.PORTRAIT_WIDTH;
                page.InitialAspectRatio = page.Width / page.Height;
            }
            notebookPanel.Notebook.InsertPageAt(index, page);
        }

        public Command<string> AddNewProofPageCommand { get; private set; }

        private void OnAddNewProofPageCommandExecute(string pageOrientation)
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            var notebookPanel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(notebookPanel == null || currentPage == null)
            {
                return;
            }
            var index = notebookPanel.Pages.IndexOf(currentPage);
            index++;
            // TODO: Entities
            //var page = new CLPAnimationPage();
            //if(pageOrientation == "Portrait")
            //{
            //    page.Height = ACLPPageBase.PORTRAIT_HEIGHT;
            //    page.Width = ACLPPageBase.PORTRAIT_WIDTH;
            //    page.InitialAspectRatio = page.Width / page.Height;
            //}
            //page.ParentNotebookID = notebookPanel.Notebook.UniqueID;
            //notebookPanel.Notebook.InsertPageAt(index, page);
        }

        /// <summary>
        /// Moves the CurrentPage Up in the notebook.
        /// </summary>
        public Command MovePageUpCommand { get; private set; }

        private void OnMovePageUpCommandExecute()
        {
            var currentPage = CurrentPage;
            var notebookPanel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            var currentPageIndex = notebookPanel.Pages.IndexOf(currentPage);
            var previousPage = notebookPanel.Pages[currentPageIndex - 1];
            currentPage.PageNumber--;
            previousPage.PageNumber++;

            notebookPanel.Pages.MoveItemUp(currentPage);
            notebookPanel.CurrentPage = notebookPanel.Pages[currentPageIndex - 1];
        }

        private bool OnMovePageUpCanExecute()
        {
            var currentPage = CurrentPage;
            var notebookPanel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(notebookPanel == null || currentPage == null)
            {
                return false;
            }
            
            return notebookPanel.Pages.CanMoveItemUp(currentPage);
        }

        /// <summary>
        /// Moves the CurrentPage Down in the notebook.
        /// </summary>
        public Command MovePageDownCommand { get; private set; }

        private void OnMovePageDownCommandExecute()
        {
            var currentPage = CurrentPage;
            var notebookPanel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            var currentPageIndex = notebookPanel.Pages.IndexOf(currentPage);
            var nextPage = notebookPanel.Pages[currentPageIndex + 1];
            currentPage.PageNumber++;
            nextPage.PageNumber--;

            notebookPanel.Pages.MoveItemDown(currentPage);
            notebookPanel.CurrentPage = notebookPanel.Pages[currentPageIndex + 1];
        }

        private bool OnMovePageDownCanExecute()
        {
            var currentPage = CurrentPage;
            var notebookPanel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(notebookPanel == null || currentPage == null)
            {
                return false;
            }

            return notebookPanel.Pages.CanMoveItemDown(currentPage);
        }

        /// <summary>
        /// Converts current page between landscape and portrait.
        /// </summary>
        public Command SwitchPageLayoutCommand { get; private set; }
       
        private void OnSwitchPageLayoutCommandExecute()
        {
            var page = CurrentPage;

            if(Math.Abs(page.InitialAspectRatio - CLPPage.LANDSCAPE_WIDTH / CLPPage.LANDSCAPE_HEIGHT) < 0.01)
            {
                foreach(var pageObject in page.PageObjects)
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

                page.Width = CLPPage.PORTRAIT_WIDTH;
                page.Height = CLPPage.PORTRAIT_HEIGHT;
                page.InitialAspectRatio = page.Width / page.Height;
            }
            else if(Math.Abs(page.InitialAspectRatio - CLPPage.PORTRAIT_WIDTH / CLPPage.PORTRAIT_HEIGHT) < 0.01)
            {
                foreach(var pageObject in page.PageObjects)
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

                page.Width = CLPPage.LANDSCAPE_WIDTH;
                page.Height = CLPPage.LANDSCAPE_HEIGHT;
                page.InitialAspectRatio = page.Width / page.Height;
            }
        }

        public Command SwitchPageTypeCommand { get; private set; }

        public void OnSwitchPageTypeCommandExecute()
        {
            var page = CurrentPage;
            page.PageType = page.PageType == PageTypes.Animation ? PageTypes.Default : PageTypes.Animation;
        }

        /// <summary>
        /// Deletes current page from the notebook.
        /// </summary>
        public Command DeletePageCommand { get; private set; }

        private void OnDeletePageCommandExecute()
        {
            var page = CurrentPage;
            var panel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(page == null || panel == null)
            {
                return;
            }

            var index = panel.Notebook.Pages.IndexOf(page);
            if(index == -1)
            {
                return;
            }
            panel.Notebook.RemovePageAt(index);
        }

        /// <summary>
        /// Makes a duplicate of the current page.
        /// </summary>
        public Command CopyPageCommand { get; private set; }

        private void OnCopyPageCommandExecute()
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            var notebookPanel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(notebookPanel == null || currentPage == null)
            {
                return;
            }
            var index = notebookPanel.Pages.IndexOf(currentPage);
            index++;

            var newPage = currentPage.DuplicatePage();
            notebookPanel.Notebook.InsertPageAt(index, newPage);
        }

        /// <summary>
        /// Add 200 pixels to the height of the current page.
        /// </summary>
        public Command MakePageLongerCommand { get; private set; }
        
        private void OnMakePageLongerCommandExecute()
        {
            var page = CurrentPage;
            var initialHeight = page.Width / page.InitialAspectRatio;
            const int MAX_INCREASE_TIMES = 2;
            const double PAGE_INCREASE_AMOUNT = 200.0;
            if(page.Height < initialHeight + PAGE_INCREASE_AMOUNT * MAX_INCREASE_TIMES)
            {
                page.Height += PAGE_INCREASE_AMOUNT;
            }

            if(App.CurrentUserMode != App.UserMode.Instructor || 
               App.Network.ProjectorProxy == null)
            {
                return;
            }

            try
            {
                App.Network.ProjectorProxy.MakeCurrentPageLonger();
            }
            catch(Exception)
            {

            }
        }

        /// <summary>
        /// Trims the current page's excess height if free of ink strokes and pageObjects.
        /// </summary>
        public Command TrimPageCommand { get; private set; }

        private void OnTrimPageCommandExecute()
        {
            if((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay == null)
            {
                var page = CurrentPage;
                page.TrimPage();
            }
        }

        /// <summary>
        /// Completely clears a page of ink strokes and pageObjects.
        /// </summary>
        public Command ClearPageCommand { get; private set; }

        private void OnClearPageCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null ||
               notebookWorkspaceViewModel.CurrentDisplay != null)
            {
                return;
            }
            var page = CurrentPage;
            page.History.ClearHistory();
            page.PageObjects.Clear();
            page.InkStrokes.Clear();
            page.SerializedStrokes.Clear();
        }

        /// <summary>
        /// Bring up window to tag a page with Page Topics.
        /// </summary>
        public Command AddPageTopicCommand { get; private set; }

        private void OnAddPageTopicCommandExecute()
        {
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
        /// Bring up window to edit a page's metadata.
        /// </summary>
        public Command EditPageMetadataCommand { get; private set; }

        private void OnEditPageMetadataCommandExecute()
        {
            PageMetadataWindowView pageMetadataWindow = new PageMetadataWindowView();
            pageMetadataWindow.Owner = Application.Current.MainWindow;

            pageMetadataWindow.ChapterTitle.Text = CurrentPage.ChapterTitle;
            pageMetadataWindow.SubchapterTitle.Text = CurrentPage.SectionTitle;
            pageMetadataWindow.PageNumber.Text = CurrentPage.PageNumber.ToString();
            pageMetadataWindow.StudentWorkbookPageNumber.Text = CurrentPage.StudentWorkbookPageNumber;
            pageMetadataWindow.TeacherWorkbookPageNumber.Text = CurrentPage.TeacherWorkbookPageNumber;
            pageMetadataWindow.Curriculum.Text = CurrentPage.Curriculum;

            pageMetadataWindow.ShowDialog();
            if(pageMetadataWindow.DialogResult == true)
            {
                CurrentPage.ChapterTitle = pageMetadataWindow.ChapterTitle.Text;
                CurrentPage.SectionTitle = pageMetadataWindow.SubchapterTitle.Text;
                int pageNumber;
                if(int.TryParse(pageMetadataWindow.PageNumber.Text, out pageNumber)) {
                    CurrentPage.PageNumber = pageNumber;
                }
                CurrentPage.StudentWorkbookPageNumber = pageMetadataWindow.StudentWorkbookPageNumber.Text;
                CurrentPage.TeacherWorkbookPageNumber = pageMetadataWindow.TeacherWorkbookPageNumber.Text;
                CurrentPage.Curriculum = pageMetadataWindow.Curriculum.Text;
            }
        }

         /// <summary>
        /// Bring up window to set a notebook's curriculum.
        /// </summary>
        public Command SetNotebookCurriculumCommand { get; private set; }

        private void OnSetNotebookCurriculumCommandExecute()
        {
            var notebook = (App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).Notebook;
            NotebookMetadataWindowView notebookMetadataWindow = new NotebookMetadataWindowView();
            notebookMetadataWindow.Owner = Application.Current.MainWindow;

            notebookMetadataWindow.Curriculum.Text = notebook.Curriculum;

            notebookMetadataWindow.ShowDialog();
            if(notebookMetadataWindow.DialogResult == true)
            {
                notebook.Curriculum = notebookMetadataWindow.Curriculum.Text;
            }
        }

        #endregion //Page Commands

        #region Insert Commands

        private bool OnInsertPageObjectCanExecute() { return CurrentPage != null; }

        private bool OnInsertPageObjectCanExecute(string s) { return CurrentPage != null; }

        /// <summary>
        /// Gets the InsertTextBoxCommand command.
        /// </summary>
        public Command InsertTextBoxCommand { get; private set; }

        private void OnInsertTextBoxCommandExecute()
        {
            var textBox = new CLPTextBox(CurrentPage, string.Empty);
            ACLPPageBaseViewModel.AddPageObjectToPage(textBox);
        }

        /// <summary>
        /// Gets the InsertAggregationDataTableCommand command.
        /// </summary>
        public Command InsertAggregationDataTableCommand { get; private set; }

        private void OnInsertAggregationDataTableCommandExecute()
        {
            // TODO: Entities
            //CLPAggregationDataTable dataTable = new CLPAggregationDataTable(((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage);
            //ACLPPageBaseViewModel.AddPageObjectToPage(dataTable);
        }

        /// <summary>
        /// Gets the InsertStaticImageCommand command.
        /// </summary>
        public Command<string> InsertStaticImageCommand { get; private set; }

        private void OnInsertStaticImageCommandExecute(string fileName)
        {
            // TODO: Entities
            //var uri = new Uri("pack://application:,,,/Classroom Learning Partner;component/Images/Money/" + fileName);
            //var info = Application.GetResourceStream(uri);
            //var memoryStream = new MemoryStream();
            //info.Stream.CopyTo(memoryStream);

            //byte[] byteSource = memoryStream.ToArray();

            //var ByteSource = new List<byte>(byteSource);

            //MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            //byte[] hash = md5.ComputeHash(byteSource);
            //string imageID = Convert.ToBase64String(hash);

            //var page = ((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage;


            //if(!page.ImagePool.ContainsKey(imageID))
            //{
            //    page.ImagePool.Add(imageID, ByteSource);
            //}

            //var image = new CLPImage(imageID, page, 10, 10);

            //switch(fileName)
            //{
            //    case "penny.png":
            //        image.Height = 90;
            //        image.Width = 90;
            //        break;
            //    case "dime.png":
            //        image.Height = 80;
            //        image.Width = 80;
            //        break;
            //    case "nickel.png":
            //        image.Height = 100;
            //        image.Width = 100;
            //        break;
            //    case "quarter.png":
            //        image.Height = 120;
            //        image.Width = 120;
            //        break;
            //    default:
            //        image.Height = 128;
            //        image.Width = 300;
            //        break;
            //}

            //ACLPPageBaseViewModel.AddPageObjectToPage(image);
        }

        /// <summary>
        /// Gets the InsertImageCommand command.
        /// </summary>
        public Command InsertImageCommand { get; private set; }

        private void OnInsertImageCommandExecute()
        {
            // Configure open file dialog box
            var dlg = new Microsoft.Win32.OpenFileDialog
                      {
                          // Filter files by extension
                          Filter = "Images|*.png;*.jpg;*.jpeg;*.gif"
                      };

            var result = dlg.ShowDialog();
            if(result != true)
            {
                return;
            }

            // Open document
            var filename = dlg.FileName;
            if(File.Exists(filename))
            {
                var bytes = File.ReadAllBytes(filename);

                var md5 = new MD5CryptoServiceProvider();
                var hash = md5.ComputeHash(bytes);
                var imageHashID = Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-").Replace("=","");
                var newFileName = imageHashID + Path.GetExtension(filename);
                var newFilePath = Path.Combine(App.ImageCacheDirectory, newFileName);

                try
                {
                    File.Copy(filename, newFilePath);
                }
                catch(IOException)
                {
                    MessageBox.Show("Image already in ImagePool, using ImagePool instead.");
                }
                catch(Exception e)
                {
                    MessageBox.Show("Something went wrong copying the image to the ImagePool. See Error Log.");
                    Logger.Instance.WriteToLog("[IMAGEPOOL ERROR]: " + e.Message);
                    return;
                }

                var bitmapImage = CLPImage.GetImageFromPath(newFilePath);
                if(bitmapImage == null)
                {
                    MessageBox.Show("Failed to load image from ImageCache by fileName.");
                    return;
                }

                if(!App.MainWindowViewModel.ImagePool.ContainsKey(imageHashID))
                {
                    App.MainWindowViewModel.ImagePool.Add(imageHashID, bitmapImage);
                }

                var page = CurrentPage;

                var visualImage = System.Drawing.Image.FromFile(newFilePath);
                var image = new CLPImage(page, imageHashID, visualImage.Height, visualImage.Width);

                ACLPPageBaseViewModel.AddPageObjectToPage(image);
            }
            else
            {
                MessageBox.Show("Error opening image file. Please try again.");
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
                ((App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).RightPanel).IsVisible = false;
                ((App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).RightPanel as ViewModelBase).SaveAndCloseViewModel();
                (App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).RightPanel = null;
            }
            else //OpenPanel
            {
                if((App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).RightPanel == null)
                {
                    (App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).RightPanel = new WebcamPanelViewModel();
                }

                ((App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).RightPanel).IsVisible = true;
            }
        }

        void panelCloserTimer_Tick(object sender, EventArgs e)
        {
            if((App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).RightPanel != null)
            {
                ((App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).RightPanel as ViewModelBase).SaveAndCloseViewModel();
                (App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).RightPanel = null;
                panelCloserTimer.Stop();
            }
        }

        /// <summary>
        /// Gets the InsertImageStampCommand command.
        /// </summary>
        public Command InsertImageStampCommand { get; private set; }

        private void OnInsertImageStampCommandExecute()
        {
            CreateImageStamp(false);
        }

        /// <summary>
        /// Gets the InsertImageStampCommand command.
        /// </summary>
        public Command InsertImageContainerStampCommand { get; private set; }

        private void OnInsertImageContainerStampCommandExecute()
        {
            CreateImageStamp(true);
        }

        private void CreateImageStamp(bool isCollectionStamp)
        {
            // TODO: Entities
            //// Configure open file dialog box
            //Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.Filter = "Images|*.png;*.jpg;*.jpeg;*.gif"; // Filter files by extension

            //// Show open file dialog box
            //Nullable<bool> result = dlg.ShowDialog();

            //// Process open file dialog box results
            //if(result == true)
            //{
            //    // Open document
            //    string filename = dlg.FileName;
            //    if(File.Exists(filename))
            //    {
            //        byte[] bytes = File.ReadAllBytes(filename);
            //        var byteSource = new List<byte>(bytes);

            //        var md5 = new MD5CryptoServiceProvider();
            //        var hash = md5.ComputeHash(bytes);
            //        var imageID = Convert.ToBase64String(hash);

            //        var page = ((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage;

            //        if(!page.ImagePool.ContainsKey(imageID))
            //        {
            //            page.ImagePool.Add(imageID, byteSource);
            //        }

            //        var stamp = new CLPStamp(page, imageID, isCollectionStamp);

            //        ACLPPageBaseViewModel.AddPageObjectToPage(stamp);
            //    }
            //    else
            //    {
            //        MessageBox.Show("Error opening image file. Please try again.");
            //    }
            //}
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
            // TODO: Entities
            //CLPStamp stamp = new CLPStamp(((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage, string.Empty);
            //ACLPPageBaseViewModel.AddPageObjectToPage(stamp);
        }

        /// <summary>
        /// Gets the InsertBlankContainerStampCommand command.
        /// </summary>
        public Command InsertBlankContainerStampCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Method to invoke when the InsertBlankContainerStampCommand command is executed.
        /// </summary>
        private void OnInsertBlankContainerStampCommandExecute()
        {
            // TODO: Entities
            //var stamp = new CLPStamp(((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage,
            //    string.Empty,
            //    true);
            //ACLPPageBaseViewModel.AddPageObjectToPage(stamp);
        }

        /// <summary>
        /// Gets the InsertArrayCommand command.
        /// </summary>
        public Command<string> InsertArrayCommand { get; private set; }

        private void OnInsertArrayCommandExecute(string arrayType)
        {
            var page = CurrentPage;
            if(page == null)
            {
                return;
            }

            int rows, columns, dividend = 1, numberOfArrays = 1;
            switch(arrayType)
            {
                case "ARRAY":
                    var arrayCreationView = new ArrayCreationView {Owner = Application.Current.MainWindow};
                    arrayCreationView.ShowDialog();

                    if(arrayCreationView.DialogResult != true)
                    {
                        return;
                    }

                    try
                    {
                        rows = Convert.ToInt32(arrayCreationView.Rows.Text);
                    }
                    catch(FormatException)
                    {
                        rows = 1;
                    }

                    try
                    {
                        columns = Convert.ToInt32(arrayCreationView.Columns.Text);
                    }
                    catch(FormatException)
                    {
                        columns = 1;
                    }

                    //try
                    //{
                    //    numberOfArrays = Convert.ToInt32(arrayCreationView.NumberOfArrays.Text);
                    //}
                    //catch(FormatException)
                    //{
                    //    numberOfArrays = 1;
                    //}
                    break;
                case "FUZZYFACTORCARD":
                    var factorCreationView = new FuzzyFactorCardCreationView{ Owner = Application.Current.MainWindow};
                    factorCreationView.ShowDialog();

                    if(factorCreationView.DialogResult != true)
                    {
                        return;
                    }

                    try
                    {
                        dividend = Convert.ToInt32(factorCreationView.Product.Text);
                    }
                    catch(FormatException)
                    {
                        return;
                    }

                    try
                    {
                        rows = Convert.ToInt32(factorCreationView.Factor.Text);
                    }
                    catch(FormatException)
                    {
                        return;
                    }

                    columns = dividend / rows;
                    numberOfArrays = 1;
                    break;
                case "FFCREMAINDER":
                    var factorRemainderCreationView = new FuzzyFactorCardWithTilesCreationView
                    {
                        Owner = Application.Current.MainWindow
                    };
                    factorRemainderCreationView.ShowDialog();

                    if(factorRemainderCreationView.DialogResult != true)
                    {
                        return;
                    }

                    try
                    {
                        dividend = Convert.ToInt32(factorRemainderCreationView.Product.Text);
                    }
                    catch(FormatException)
                    {
                        return;
                    }

                    try
                    {
                        rows = Convert.ToInt32(factorRemainderCreationView.Factor.Text);
                    }
                    catch(FormatException)
                    {
                        return;
                    }

                    columns = dividend / rows;
                    numberOfArrays = 1;
                    break;
                case "ARRAYCARD":
                case "FACTORCARD":
                    //    var factorCreationView = new FactorCardCreationView{ Owner = Application.Current.MainWindow};
                    //    factorCreationView.ShowDialog();
                    //    if(factorCreationView.DialogResult != true)
                    //    {
                    //        return;
                    //    }

                    //    try
                    //    {
                    //        dividend = Convert.ToInt32(factorCreationView.Product.Text);
                    //    }
                    //    catch(FormatException)
                    //    {
                    //        return;
                    //    }

                    //    try
                    //    {
                    //        rows = Convert.ToInt32(factorCreationView.Factor.Text);
                    //    }
                    //    catch(FormatException)
                    //    {
                    //        return;
                    //    }

                    //    columns = dividend / rows;
                    //    numberOfArrays = 1;
                default:
                    return;
            }

            var arraysToAdd = new List<ACLPArrayBase>();
            foreach(var index in Enumerable.Range(1, numberOfArrays))
            {
                ACLPArrayBase array;
                switch(arrayType)
                {
                    case "ARRAY":
                        array = new CLPArray(page, columns, rows, ArrayTypes.Array);
                        break;
                    case "FUZZYFACTORCARD":
                        array = new FuzzyFactorCard(page, columns, rows, dividend);
                        // HACK: Find better way to set this
                        array.CreatorID = App.MainWindowViewModel.CurrentUser.ID;
                        array.OwnerID = App.MainWindowViewModel.CurrentUser.ID;
                        break;
                    case "FFCREMAINDER":
                        array = new FuzzyFactorCard(page, columns, rows, dividend, true);
                        // HACK: Find better way to set this
                        array.CreatorID = App.MainWindowViewModel.CurrentUser.ID;
                        (array as FuzzyFactorCard).RemainderTiles.CreatorID = array.CreatorID;
                        (array as FuzzyFactorCard).RemainderTiles.OwnerID = array.OwnerID;
                        break;
                    case "ARRAYCARD":
                        array = new CLPArray(page, columns, rows, ArrayTypes.ArrayCard);
                        break;
                    case "FACTORCARD":
                        array = new CLPArray(page, columns, rows, ArrayTypes.FactorCard);
                        break;
                    default:
                        return;
                }

                arraysToAdd.Add(array);
            }

            const double MIN_SIDE = 20.0;
            const double MIN_FFC_SIDE = 185.0;
            const double LABEL_LENGTH = 22.0;

            var arrayStacks = 1;
            var isHorizontallyAligned = page.Width / columns > page.Height / 4 * 3 / rows;
            var firstArray = arraysToAdd.First();
            firstArray.SizeArrayToGridLevel();
            var initialGridsquareSize = firstArray.GridSquareSize;
            var xPosition = 0.0;
            var yPosition = 150.0;

            //attempt to size newArray to lastArray
            //if fail, resize all other arrays to newArray
            //squareSize will be the grid size of the most recently placed array, or 0 if there are no non-background arrays
            double squareSize = 0.0;
            if(!(firstArray is FuzzyFactorCard))
            {
                foreach(var pageObject in page.PageObjects)
                {
                    if(pageObject is CLPArray || pageObject is FuzzyFactorCard && pageObject.CreatorID != Person.Author.ID)
                    {
                        squareSize = (pageObject as ACLPArrayBase).ArrayHeight / (pageObject as ACLPArrayBase).Rows;
                    }
                }
            }

            var minSide = (firstArray is FuzzyFactorCard)
                ? MIN_FFC_SIDE :
                MIN_SIDE;
            var defaultSquareSize = (firstArray is FuzzyFactorCard) ?
                Math.Max(45.0, (minSide / (Math.Min(rows, columns)))) :
                45.0;
            var initializedSquareSize = (squareSize > 0) ? Math.Max(squareSize, (minSide / (Math.Min(rows, columns)))) : defaultSquareSize;
            if((firstArray is FuzzyFactorCard)&& xPosition + initializedSquareSize * columns + LABEL_LENGTH * 3.0 + 12.0 > page.Width)
            {
                initializedSquareSize = minSide / (Math.Min(rows, columns));
            }

            while(xPosition + 2 * LABEL_LENGTH + initializedSquareSize * columns >= page.Width || yPosition + 2 * LABEL_LENGTH + initializedSquareSize * rows >= page.Height)
            {
                initializedSquareSize = Math.Abs(initializedSquareSize - 45.0) < .0001 ? 22.5 : initializedSquareSize / 4 * 3;
            }
            if(numberOfArrays > 1)
            {
                if(isHorizontallyAligned)
                {
                    while(xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * numberOfArrays + LABEL_LENGTH >= page.Width)
                    {
                        initializedSquareSize = Math.Abs(initializedSquareSize - 45.0) < .0001 ? 22.5 : initializedSquareSize / 4 * 3;

                        if(numberOfArrays < 5 || xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * numberOfArrays + LABEL_LENGTH < page.Width)
                        {
                            continue;
                        }

                        if(xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * Math.Ceiling((double)numberOfArrays / 2) + LABEL_LENGTH < page.Width &&
                           yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * 2 + LABEL_LENGTH < page.Height)
                        {
                            arrayStacks = 2;
                            break;
                        }

                        if(xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * Math.Ceiling((double)numberOfArrays / 3) + LABEL_LENGTH < page.Width &&
                           yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * 3 + LABEL_LENGTH < page.Height)
                        {
                            arrayStacks = 3;
                            break;
                        }
                    }
                }
                else
                {
                    yPosition = 100;
                    while(yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * numberOfArrays + LABEL_LENGTH >= page.Height)
                    {
                        initializedSquareSize = Math.Abs(initializedSquareSize - 45.0) < .0001 ? 22.5 : initializedSquareSize / 4 * 3;

                        if(numberOfArrays < 5 || yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * numberOfArrays + LABEL_LENGTH < page.Height)
                        {
                            continue;
                        }

                        if(yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * Math.Ceiling((double)numberOfArrays / 2) + LABEL_LENGTH < page.Height &&
                           xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * 2 + LABEL_LENGTH < page.Width)
                        {
                            arrayStacks = 2;
                            break;
                        }

                        if(yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * Math.Ceiling((double)numberOfArrays / 3) + LABEL_LENGTH < page.Height &&
                           xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * 3 + LABEL_LENGTH < page.Width)
                        {
                            arrayStacks = 3;
                            break;
                        }
                    }
                }
            }

            // If it doesn't fit, resize all other non-background arrays on page to match new array grid size
            if(squareSize > 0.0 && initializedSquareSize != squareSize)
            {
                Dictionary<string, Point> oldDimensions = new Dictionary<string, Point>();
                foreach(var pageObject in page.PageObjects)
                {
                    if(pageObject is CLPArray && pageObject.CreatorID != Person.Author.ID)
                    {
                        oldDimensions.Add(pageObject.ID, new Point(pageObject.Width, pageObject.Height));
                        if((pageObject as ACLPArrayBase).Rows * initializedSquareSize > MIN_SIDE && (pageObject as ACLPArrayBase).Columns * initializedSquareSize > MIN_SIDE)
                        {
                            if(pageObject.XPosition + (pageObject as ACLPArrayBase).Columns * initializedSquareSize + 2 * LABEL_LENGTH <= page.Width && pageObject.YPosition + (pageObject as ACLPArrayBase).Rows * initializedSquareSize + 2 * LABEL_LENGTH <= page.Height)
                            {
                                (pageObject as ACLPArrayBase).SizeArrayToGridLevel(initializedSquareSize);
                            }
                        }
                        else
                        {
                            (pageObject as ACLPArrayBase).SizeArrayToGridLevel(MIN_SIDE / Math.Min((pageObject as ACLPArrayBase).Rows, (pageObject as ACLPArrayBase).Columns));
                        }
                    }
                    initialGridsquareSize = initializedSquareSize;
                }
            }


            double MAX_HEIGHT = page.Height - 400.0;
            if(squareSize == 0.0)
            {
                initializedSquareSize = Math.Min(initialGridsquareSize, MAX_HEIGHT / rows);
            }

            firstArray.SizeArrayToGridLevel(initializedSquareSize);
            firstArray.XPosition = 0.0;
            if(295.0 + firstArray.Height < page.Height)
            {
                firstArray.YPosition = 295.0;
            }
            else
            {
                firstArray.YPosition = page.Height - firstArray.Height;
            }
            ACLPArrayBase.ApplyDistinctPosition(firstArray, App.MainWindowViewModel.CurrentUser.ID);
            xPosition = firstArray.XPosition;
            yPosition = firstArray.YPosition;

            //if there is exactly one other array on the page, keep track of it for placement
            ACLPArrayBase onlyArray = null;
            foreach(var pageObject in page.PageObjects)
            {
                if(pageObject is CLPArray)
                {
                    onlyArray = (onlyArray == null) ? pageObject as CLPArray : null;
                }
                else if(pageObject is FuzzyFactorCard)
                {
                    onlyArray = (onlyArray == null) ? pageObject as FuzzyFactorCard : null;
                }
            }

            //Position to not overlap with first array on page if possible
            if(onlyArray != null)
            {
                if(isHorizontallyAligned)
                {
                    const double GAP = 35.0;
                    if(!(onlyArray is FuzzyFactorCard && (onlyArray as FuzzyFactorCard).RemainderTiles != null) && onlyArray.XPosition + onlyArray.Width + (LABEL_LENGTH + columns * initializedSquareSize) * numberOfArrays + LABEL_LENGTH + GAP <= page.Width
                        && rows * initializedSquareSize + LABEL_LENGTH < page.Height)
                    {
                        xPosition = onlyArray.XPosition + onlyArray.Width + GAP;
                        yPosition = onlyArray.YPosition;
                    }
                    else if(onlyArray.XPosition + (LABEL_LENGTH + columns * initializedSquareSize) * numberOfArrays + LABEL_LENGTH <= page.Width
                        && onlyArray.YPosition + onlyArray.Height + rows * initializedSquareSize + LABEL_LENGTH + GAP < page.Height)
                    {
                        yPosition = onlyArray.YPosition + onlyArray.Height + GAP;
                        xPosition = onlyArray.XPosition;
                    }
                    else
                    {
                        yPosition = page.Height - rows * initializedSquareSize - 2 * LABEL_LENGTH;
                        xPosition = onlyArray.XPosition;
                    }
                }
                else
                {
                    const double GAP = 35.0;
                    if(!(onlyArray is FuzzyFactorCard && (onlyArray as FuzzyFactorCard).RemainderTiles != null) && onlyArray.YPosition + (LABEL_LENGTH + rows * initializedSquareSize) * numberOfArrays + LABEL_LENGTH <= page.Height
                        && onlyArray.XPosition + onlyArray.Width + columns * initializedSquareSize + LABEL_LENGTH + GAP < page.Width)
                    {
                        xPosition = onlyArray.XPosition + onlyArray.Width + GAP;
                        yPosition = onlyArray.YPosition;
                    }
                    else if(onlyArray.YPosition + onlyArray.Height + (LABEL_LENGTH + rows * initializedSquareSize) * numberOfArrays + LABEL_LENGTH + GAP <= page.Width
                        && onlyArray.XPosition + rows * initializedSquareSize + LABEL_LENGTH < page.Height)
                    {
                        yPosition = onlyArray.YPosition + onlyArray.Height + GAP;
                        xPosition = onlyArray.XPosition;
                    }
                }
            }

            if(arraysToAdd.Count == 1)
            {
                firstArray.XPosition = xPosition;
                firstArray.YPosition = yPosition;
                firstArray.SizeArrayToGridLevel(initializedSquareSize);

                if(firstArray.XPosition + firstArray.Width >= firstArray.ParentPage.Width)
                {
                    firstArray.XPosition = firstArray.ParentPage.Width - firstArray.Width;
                }
                if(firstArray.YPosition + firstArray.Height >= firstArray.ParentPage.Height)
                {
                    firstArray.YPosition = firstArray.ParentPage.Height - firstArray.Height;
                }

                ACLPPageBaseViewModel.AddPageObjectToPage(firstArray);

                if(arrayType == "FFCREMAINDER")
                {
                    if(xPosition + firstArray.Width + 20.0 + (firstArray as FuzzyFactorCard).RemainderTiles.Width <= page.Width)
                    {
                        (firstArray as FuzzyFactorCard).RemainderTiles.XPosition = xPosition + firstArray.Width + 20.0;
                    }
                    else
                    {
                        (firstArray as FuzzyFactorCard).RemainderTiles.XPosition = page.Width - (firstArray as FuzzyFactorCard).RemainderTiles.Width;
                    }
                    if(yPosition + (firstArray as FuzzyFactorCard).LabelLength + (firstArray as FuzzyFactorCard).RemainderTiles.Height <= page.Height)
                    {
                        (firstArray as FuzzyFactorCard).RemainderTiles.YPosition = yPosition + (firstArray as FuzzyFactorCard).LabelLength;
                    }
                    else
                    {
                        (firstArray as FuzzyFactorCard).RemainderTiles.YPosition = page.Height - (firstArray as FuzzyFactorCard).RemainderTiles.Height;
                    }
                }
            }
            else
            {
                initialGridsquareSize = initializedSquareSize;
                if(isHorizontallyAligned)
                {
                    while(xPosition + (firstArray.LabelLength + columns * initialGridsquareSize) * numberOfArrays + firstArray.LabelLength >= page.Width)
                    {
                        initialGridsquareSize = Math.Abs(initialGridsquareSize - 45.0) < .0001 ? 22.5 : initialGridsquareSize / 4 * 3;
                      
                        if(numberOfArrays < 5 ||
                            xPosition + (firstArray.LabelLength + columns * initialGridsquareSize) * numberOfArrays + firstArray.LabelLength < page.Width)
                        {
                            continue;
                        }

                        if(xPosition + (firstArray.LabelLength + columns * initialGridsquareSize) * Math.Ceiling((double)numberOfArrays / 2) + firstArray.LabelLength < page.Width &&
                            yPosition + (firstArray.LabelLength + rows * initialGridsquareSize) * 2 + firstArray.LabelLength < page.Height)
                        {
                            arrayStacks = 2;
                            break;
                        }

                        if(xPosition + (firstArray.LabelLength + columns * initialGridsquareSize) * Math.Ceiling((double)numberOfArrays / 3) + firstArray.LabelLength < page.Width &&
                            yPosition + (firstArray.LabelLength + rows * initialGridsquareSize) * 3 + firstArray.LabelLength < page.Height)
                        {
                            arrayStacks = 3;
                            break;
                        }
                    }
                }
                else
                {
                    yPosition = 100;
                    while(yPosition + (firstArray.LabelLength + rows * initialGridsquareSize) * numberOfArrays + firstArray.LabelLength >= page.Height)
                    {
                        initialGridsquareSize = Math.Abs(initialGridsquareSize - 45.0) < .0001 ? 22.5 : initialGridsquareSize / 4 * 3;

                        if(numberOfArrays < 5 ||
                            yPosition + (firstArray.LabelLength + rows * initialGridsquareSize) * numberOfArrays + firstArray.LabelLength < page.Height)
                        {
                            continue;
                        }

                        if(yPosition + (firstArray.LabelLength + rows * initialGridsquareSize) * Math.Ceiling((double)numberOfArrays / 2) + firstArray.LabelLength < page.Height &&
                            xPosition + (firstArray.LabelLength + columns * initialGridsquareSize) * 2 + firstArray.LabelLength < page.Width)
                        {
                            arrayStacks = 2;
                            break;
                        }

                        if(yPosition + (firstArray.LabelLength + rows * initialGridsquareSize) * Math.Ceiling((double)numberOfArrays / 3) + firstArray.LabelLength < page.Height &&
                            xPosition + (firstArray.LabelLength + columns * initialGridsquareSize) * 3 + firstArray.LabelLength < page.Width)
                        {
                            arrayStacks = 3;
                            break;
                        }
                    }
                }

                foreach(var array in arraysToAdd)
                {
                    var index = arraysToAdd.IndexOf(array) + 1;
                    if(isHorizontallyAligned)
                    {
                        if(arrayStacks == 2 &&
                            index == (int)Math.Ceiling((double)numberOfArrays / 2) + 1)
                        {
                            xPosition = firstArray.XPosition;
                            yPosition += firstArray.LabelLength + rows * initialGridsquareSize;
                        }
                        if(arrayStacks == 3 &&
                            (index == (int)Math.Ceiling((double)numberOfArrays / 3) + 1 || index == (int)Math.Ceiling((double)numberOfArrays / 3) * 2 + 1))
                        {
                            xPosition = firstArray.XPosition;
                            yPosition += firstArray.LabelLength + rows * initialGridsquareSize;
                        }
                        array.XPosition = xPosition;
                        array.YPosition = yPosition;
                        xPosition += firstArray.LabelLength + columns * initialGridsquareSize;
                        array.SizeArrayToGridLevel(initialGridsquareSize);
                    }
                    else
                    {
                        if(arrayStacks == 2 &&
                            index == (int)Math.Ceiling((double)numberOfArrays / 2) + 1)
                        {
                            xPosition += firstArray.LabelLength + columns * initialGridsquareSize;
                            yPosition = firstArray.YPosition;
                        }
                        if(arrayStacks == 3 &&
                            (index == (int)Math.Ceiling((double)numberOfArrays / 3) + 1 || index == (int)Math.Ceiling((double)numberOfArrays / 3) * 2 + 1))
                        {
                            xPosition += firstArray.LabelLength + columns * initialGridsquareSize;
                            yPosition = firstArray.YPosition;
                        }
                        array.XPosition = xPosition;
                        array.YPosition = yPosition;
                        yPosition += firstArray.LabelLength + rows * initialGridsquareSize;
                        array.SizeArrayToGridLevel(initialGridsquareSize);
                    }
                }

                ACLPPageBaseViewModel.AddPageObjectsToPage(page, arraysToAdd);
            }
           
            App.MainWindowViewModel.Ribbon.PageInteractionMode = PageInteractionMode.Select;
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
            if(CurrentPage == null)
            {
                return;
            }

            // If the page already has a Page Definition tag, start from that one
            ProductRelation productRelation = new ProductRelation();
            foreach(var tag in CurrentPage.Tags)
            {
                if(tag is PageDefinitionTag)
                {
                    productRelation = ProductRelation.fromString(tag.Value);
                    break;
                }
            }

            ProductRelationViewModel viewModel = new ProductRelationViewModel(productRelation);
            PageDefinitionView definitionView = new PageDefinitionView(viewModel);
            definitionView.Owner = Application.Current.MainWindow;
            definitionView.ShowDialog();

            if(definitionView.DialogResult == true)
            {
                // Update this page's definition tag
                PageDefinitionTag newTag = new PageDefinitionTag(CurrentPage, productRelation);
                CurrentPage.AddTag(newTag);
            }
        }

        /// <summary>
        /// Recursively sets the OwnerID of every page, stroke, and pageObject as Author.ID
        /// </summary>
        public Command SetEntireNotebookAsAuthoredCommand { get; private set; }

        private void OnSetEntireNotebookAsAuthoredCommandExecute()
        {
            var notebookPanel = NotebookPagesPanelViewModel.GetNotebookPagesPanelViewModel();
            if(notebookPanel == null)
            {
                return;
            }

            var notebook = notebookPanel.Notebook;
            notebook.Owner = Person.Author;
            foreach(var page in notebook.Pages)
            {
                page.Owner = Person.Author;
                foreach(var pageObject in page.PageObjects)
                {
                    pageObject.OwnerID = Person.Author.ID;
                    pageObject.CreatorID = Person.Author.ID;
                    pageObject.ParentPage = page;
                }
                foreach(var inkStroke in page.InkStrokes)
                {
                    inkStroke.SetStrokeOwnerID(Person.Author.ID);
                }
                foreach(var tag in page.Tags)
                {
                    tag.OwnerID = Person.Author.ID;
                    tag.ParentPage = page;
                }
            }
        }

        /// <summary>
        /// Gets the AnalyzeArrayCommand command.
        /// </summary>
        public Command AnalyzeArrayCommand { get; private set; }

        private void OnAnalyzeArrayCommandExecute()
        {
            // TODO: Entities
            //Logger.Instance.WriteToLog("Start of OnAnalyzeArrayCommandExecute()");

            // Get the page's math definition, or be sad if it doesn't have one
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            if(currentPage != null)
            {
                TagAnalysis.AnalyzeArray(currentPage);
            }
        }

        /// <summary>
        /// Gets the AnalyzeFuzzyFactorCardCommand command.
        /// </summary>
        public Command AnalyzeFuzzyFactorCardCommand
        {
            get;
            private set;
        }

        private void OnAnalyzeFuzzyFactorCardCommandExecute()
        {
            // Get the page's math definition, or be sad if it doesn't have one
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            if(currentPage != null)
            {
                TagAnalysis.AnalyzeFuzzyFactorCard(currentPage);
            }
        }

        /// <summary>
        /// Gets the AnalyzeStampsCommand command.
        /// </summary>
        public Command AnalyzeStampsCommand { get; private set; }

        private void OnAnalyzeStampsCommandExecute()
        {
            // TODO: Entities
            //// Get the page's math definition, or be sad if it doesn't have one
            //var page = ((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage;

            //PageAnalysis.AnalyzeStamps(page);
        }

        /// <summary>
        /// Gets the InsertAudioCommand command.
        /// </summary>
        public Command InsertAudioCommand { get; private set; }

        private void OnInsertAudioCommandExecute()
        {
            // TODO: Entities
            //CLPAudio audio = new CLPAudio(((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage);
            //ACLPPageBaseViewModel.AddPageObjectToPage(audio);
        }

        /// <summary>
        /// Gets the InsertProtractorCommand command.
        /// </summary>
        public Command InsertProtractorCommand { get; private set; }

        private void OnInsertProtractorCommandExecute()
        {
            var square = new Shape(CurrentPage, ShapeType.Protractor);
            square.Height = 200;
            square.Width = 2.0*square.Height;
            
            ACLPPageBaseViewModel.AddPageObjectToPage(square);
        }

        /// <summary>
        /// Gets the InsertSquareShapeCommand command.
        /// </summary>
        public Command InsertSquareShapeCommand { get; private set; }

        private void OnInsertSquareShapeCommandExecute()
        {
            var square = new Shape(CurrentPage, ShapeType.Rectangle);
            ACLPPageBaseViewModel.AddPageObjectToPage(square);
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
            var circle = new Shape(CurrentPage, ShapeType.Ellipse);
            ACLPPageBaseViewModel.AddPageObjectToPage(circle);
        }

        /// <summary>
        /// Gets the InsertHorizontalLineShapCommand command.
        /// </summary>
        public Command InsertHorizontalLineShapeCommand { get; private set; }

        private void OnInsertHorizontalLineShapeCommandExecute()
        {
            var line = new Shape(CurrentPage, ShapeType.HorizontalLine)
                       {
                           Height = 20.0
                       };
            ACLPPageBaseViewModel.AddPageObjectToPage(line);
        }

        /// <summary>
        /// Gets the InsertHorizontalLineShapCommand command.
        /// </summary>
        public Command InsertVerticalLineShapeCommand { get; private set; }

        private void OnInsertVerticalLineShapeCommandExecute()
        {
            var line = new Shape(CurrentPage, ShapeType.VerticalLine)
                       {
                           Width = 20.0
                       };
            ACLPPageBaseViewModel.AddPageObjectToPage(line);
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
            // TODO: Entities
            //CustomizeInkRegionView optionChooser = new CustomizeInkRegionView();
            //optionChooser.Owner = Application.Current.MainWindow;
            //optionChooser.ShowDialog();
            //if(optionChooser.DialogResult == true)
            //{
            //    CLPHandwritingAnalysisType selected_type = (CLPHandwritingAnalysisType)optionChooser.ExpectedType.SelectedIndex;
            //    CLPHandwritingRegion region = new CLPHandwritingRegion(selected_type, ((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage);
            //    ACLPPageBaseViewModel.AddPageObjectToPage(region);
            //}
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
            // TODO: Entities
            //CLPInkShapeRegion region = new CLPInkShapeRegion(((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage);
            //ACLPPageBaseViewModel.AddPageObjectToPage(region);
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
            // TODO: Entities
            //CLPGroupingRegion region = new CLPGroupingRegion(((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage);
            //ACLPPageBaseViewModel.AddPageObjectToPage(region);
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
            // TODO: Entities
            //CustomizeDataTableView optionChooser = new CustomizeDataTableView();
            //optionChooser.Owner = Application.Current.MainWindow;
            //optionChooser.ShowDialog();
            //if(optionChooser.DialogResult == true)
            //{
            //    CLPHandwritingAnalysisType selected_type = (CLPHandwritingAnalysisType)optionChooser.ExpectedType.SelectedIndex;

            //    int rows = 1;
            //    try { rows = Convert.ToInt32(optionChooser.Rows.Text); }
            //    catch(FormatException) { rows = 1; }

            //    int cols = 1;
            //    try { cols = Convert.ToInt32(optionChooser.Cols.Text); }
            //    catch(FormatException) { cols = 1; }

            //    CLPDataTable region = new CLPDataTable(rows, cols, selected_type, ((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage);
               
            //    ACLPPageBaseViewModel.AddPageObjectToPage(region);
            //}
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
            // TODO: Entities
            //CustomizeShadingRegionView optionChooser = new CustomizeShadingRegionView();
            //optionChooser.Owner = Application.Current.MainWindow;
            //optionChooser.ShowDialog();
            //if(optionChooser.DialogResult == true)
            //{
            //    //CLPHandwritingAnalysisType selected_type = (CLPHandwritingAnalysisType)optionChooser.ExpectedType.SelectedIndex;

            //    int rows = 0;
            //    try { rows = Convert.ToInt32(optionChooser.Rows.Text); }
            //    catch(FormatException) { rows = 0; }

            //    int cols = 0;
            //    try { cols = Convert.ToInt32(optionChooser.Cols.Text); }
            //    catch(FormatException) { cols = 0; }

            //    CLPShadingRegion region = new CLPShadingRegion(rows, cols, ((MainWindow.Workspace as NotebookWorkspaceViewModel).CurrentDisplay as CLPMirrorDisplay).CurrentPage);
            //    ACLPPageBaseViewModel.AddPageObjectToPage(region);
            //}
        }

        #endregion //Insert Commands

        #region Debug Commands

        /// <summary>
        /// Runs interpretation methods of all pageObjects on current page.
        /// </summary>
        public Command InterpretPageCommand { get; private set; }

        private void OnInterpretPageCommandExecute()
        {
            // TODO: Entities
            //var currentPage = CurrentPage;
            //foreach (var pageObject in currentPage.PageObjects)
            //{
            //    if (pageObject.GetType().IsSubclassOf(typeof(ACLPInkRegion)))
            //    {
            //        CLPServiceAgent.Instance.InterpretRegion(pageObject as ACLPInkRegion);
            //    }
            //}
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
