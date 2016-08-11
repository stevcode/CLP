using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
using Catel.Threading;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.CustomControls;
using CLP.Entities;
using Path = System.Windows.Shapes.Path;

namespace Classroom_Learning_Partner.ViewModels
{
    public enum ConnectionStatuses
    {
        Offline,
        Connecting,
        Listening,
        Found,
        Connected,
        LoggedIn,
        LoggedOut,
        Disconnected
    }

    public class MajorRibbonViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindow
        {
            get { return App.MainWindowViewModel; }
        }

        private IPageInteractionService _pageInteractionService;
        private IDataService _dataService;

        public MajorRibbonViewModel()
        {
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();
            _dataService = DependencyResolver.Resolve<IDataService>();

            InitializeCommands();
            InitializeButtons();
            SetRibbonButtons();

            PageInteractionMode = _pageInteractionService.CurrentPageInteractionMode;
            CurrentLeftPanel = Panels.NotebookPages;

            InitializedAsync += MajorRibbonViewModel_InitializedAsync;
            ClosedAsync += MajorRibbonViewModel_ClosedAsync;
        }

        private Task MajorRibbonViewModel_InitializedAsync(object sender, EventArgs e)
        {
            _dataService.CurrentNotebookChanged += _dataService_CurrentNotebookChanged;

            return TaskHelper.Completed;
        }

        private Task MajorRibbonViewModel_ClosedAsync(object sender, ViewModelClosedEventArgs e)
        {
            _dataService.CurrentNotebookChanged += _dataService_CurrentNotebookChanged;

            return TaskHelper.Completed;
        }

        private void _dataService_CurrentNotebookChanged(object sender, EventArgs e)
        {
            // TODO: Change this to fire on CurrentPage change. See if commented out works to fire all CanExecutes.
            var catelCommand = (AddPageObjectToPageCommand as ICatelCommand);
            if (catelCommand != null)
            {
                catelCommand.RaiseCanExecuteChanged();
            }

            //var viewModelBase = this as ViewModelBase;
            //if (viewModelBase != null)
            //{
            //    var viewModelCommandManager = viewModelBase.GetViewModelCommandManager();
            //    viewModelCommandManager.InvalidateCommands();
            //}
        }

        /// <summary>Temp variable to refresh CanExecute command methods.</summary>
        public bool IsCurrentNotebookChanging
        {
            get { return GetValue<bool>(IsCurrentNotebookChangingProperty); }
            set { SetValue(IsCurrentNotebookChangingProperty, value); }
        }

        public static readonly PropertyData IsCurrentNotebookChangingProperty = RegisterProperty("IsCurrentNotebookChanging", typeof(bool), false);
        

        private void InitializeCommands()
        {
            ReconnectCommand = new Command(OnReconnectCommandExecute);
            ShowBackStageCommand = new Command(OnShowBackStageCommandExecute);
            ExitMultiDisplayCommand = new Command(OnExitMultiDisplayCommandExecute, OnExitMultiDisplayCanExecute);
            UndoCommand = new Command(OnUndoCommandExecute, OnUndoCanExecute);
            RedoCommand = new Command(OnRedoCommandExecute, OnRedoCanExecute);
            AddNewPageCommand = new Command(OnAddNewPageCommandExecute);
            AddNewAnimationPageCommand = new Command(OnAddNewAnimationPageCommandExecute);
            LongerPageCommand = new Command(OnLongerPageCommandExecute, OnLongerPageCanExecute);
            SubmitPageCommand = new Command(OnSubmitPageCommandExecute, OnSubmitPageCanExecute);
            AddPageObjectToPageCommand = new Command<string>(OnAddPageObjectToPageCommandExecute, OnAddPageObjectToPageCanExecute);

            TakePageScreenshotCommand = new Command(OnTakePageScreenshotCommandExecute, OnTakePageScreenshotCanExecute);

            ReverseSubmitPageCommand = new Command(OnReverseSubmitPageCommandExecute);
        }

        private void InitializeButtons()
        {
            //PageInteractionMode Toggles
            _setSelectModeButton = new GroupedRibbonButton("Select",
                                                           "PageInteractionMode",
                                                           "pack://application:,,,/Resources/Images/Hand32.png",
                                                           PageInteractionModes.Select.ToString());
            _setSelectModeButton.Checked += _button_Checked;

            _setDrawModeButton = new GroupedRibbonButton("Draw", "PageInteractionMode", "pack://application:,,,/Resources/Images/Pen32.png", PageInteractionModes.Draw.ToString());
            _setDrawModeButton.Checked += _button_Checked;

            _setEraseModeButton = new GroupedRibbonButton("Erase",
                                                          "PageInteractionMode",
                                                          "pack://application:,,,/Resources/Images/PointEraser32.png",
                                                          PageInteractionModes.Erase.ToString());
            _setEraseModeButton.Checked += _button_Checked;

            _setMarkModeButton = new GroupedRibbonButton("Mark", "PageInteractionMode", "pack://application:,,,/Resources/Images/AddCircle.png", PageInteractionModes.Mark.ToString());
            _setMarkModeButton.Checked += _button_Checked;

            _setLassoModeButton = new GroupedRibbonButton("Lasso",
                                                          "PageInteractionMode",
                                                          "pack://application:,,,/Resources/Images/Lasso32.png",
                                                          PageInteractionModes.Lasso.ToString());
            _setLassoModeButton.Checked += _button_Checked;

            _setCutModeButton = new GroupedRibbonButton("Cut", "PageInteractionMode", "pack://application:,,,/Resources/Images/Scissors32.png", PageInteractionModes.Cut.ToString());
            _setCutModeButton.Checked += _button_Checked;

            _setDividerCreationModeButton = new GroupedRibbonButton("Add Divider",
                                                                    "PageInteractionMode",
                                                                    "pack://application:,,,/Resources/Images/InkArray32.png",
                                                                    PageInteractionModes.DividerCreation.ToString());
            _setDividerCreationModeButton.Checked += _button_Checked;

            //Images
            //TODO: Better Icons
            _insertImageButton = new RibbonButton("Image", "pack://application:,,,/Resources/Images/AddImage.png", AddPageObjectToPageCommand, "IMAGE");

            //Stamps
            _insertGeneralStampButton = new RibbonButton("Stamp", "pack://application:,,,/Resources/Images/Stamp32.png", AddPageObjectToPageCommand, "BLANK_GENERAL_STAMP");
            _insertGroupStampButton = new RibbonButton("Group Stamp",
                                                       "pack://application:,,,/Resources/Images/CollectionStamp32.png",
                                                       AddPageObjectToPageCommand,
                                                       "BLANK_GROUP_STAMP");
            _insertImageGeneralStampButton = new RibbonButton("Image Stamp", "pack://application:,,,/Resources/Images/PictureStamp.png", AddPageObjectToPageCommand, "IMAGE_GENERAL_STAMP");
            //TODO: Better Icon
            _insertImageGroupStampButton = new RibbonButton("Image Group Stamp", "pack://application:,,,/Resources/Images/PictureStamp.png", AddPageObjectToPageCommand, "IMAGE_GROUP_STAMP");
            //TODO: Better Icon
            _insertPileButton = new RibbonButton("Stamp Group", "pack://application:,,,/Resources/Images/StampGroup32.png", AddPageObjectToPageCommand, "PILE");

            //Arrays
            _insertArrayButton = new RibbonButton("Array", "pack://application:,,,/Resources/Images/Array32.png", AddPageObjectToPageCommand, "ARRAY");
            _insert10x10ArrayButton = new RibbonButton("10x10 Array", "pack://application:,,,/Resources/Images/PresetArray32.png", AddPageObjectToPageCommand, "10X10");
            _insertArrayCardButton = new RibbonButton("Array Card", "pack://application:,,,/Resources/Images/ArrayCard32.png", AddPageObjectToPageCommand, "ARRAYCARD");
            _insertFactorCardButton = new RibbonButton("Factor Card", "pack://application:,,,/Resources/Images/FactorCard32.png", AddPageObjectToPageCommand, "FACTORCARD");
            _insertObscurableArrayButton = new RibbonButton("N Array", "pack://application:,,,/Resources/Images/FuzzyArray32.png", AddPageObjectToPageCommand, "OBSCURABLE_ARRAY");

            //Division Templates
            _insertDivisionTemplateButton = new RibbonButton("Division Template",
                                                             "pack://application:,,,/Resources/Images/DivisionTool32.png",
                                                             AddPageObjectToPageCommand,
                                                             "DIVISIONTEMPLATE");

            //NumberLine
            _insertNumberLineButton = new RibbonButton("Number Line", "pack://application:,,,/Resources/Images/NumberLine64New.png", AddPageObjectToPageCommand, "NUMBERLINE");
            _insertAutoNumberLineButton = new RibbonButton("Auto Number Line",
                                                           "pack://application:,,,/Resources/Images/NumberLineAuto64.png",
                                                           AddPageObjectToPageCommand,
                                                           "AUTO_NUMBERLINE");

            //Shapes
            _insertShapeButton = new DropDownRibbonButton("Shape", "pack://application:,,,/Resources/Images/Shapes64.png");
            var shapeDropDown = new ContextMenu();
            
            _insertSquareButton = new RibbonButton("Square", "pack://application:,,,/Resources/Images/AddSquare.png", AddPageObjectToPageCommand, "SQUARE", true);
            shapeDropDown.Items.Add(_insertSquareButton);
            _insertCircleButton = new RibbonButton("Circle", "pack://application:,,,/Resources/Images/AddCircle.png", AddPageObjectToPageCommand, "CIRCLE", true);
            shapeDropDown.Items.Add(_insertCircleButton);
            _insertTriangleButton = new RibbonButton("Triangle", "pack://application:,,,/Resources/Images/AddTriangle.png", AddPageObjectToPageCommand, "TRIANGLE", true);
            shapeDropDown.Items.Add(_insertTriangleButton);
            _insertHorizontalLineButton = new RibbonButton("Line", "pack://application:,,,/Resources/Images/HorizontalLineIcon.png", AddPageObjectToPageCommand, "HORIZONTALLINE", true);
            shapeDropDown.Items.Add(_insertHorizontalLineButton);

            _insertShapeButton.DropDown = shapeDropDown;

            #region Obsolete

            _insertProtractorButton = new RibbonButton("Protractor", "pack://application:,,,/Resources/Images/Protractor64.png", AddPageObjectToPageCommand, "PROTRACTOR");

            #endregion // Obsolete
            
            //Bin
            _insertBinButton = new RibbonButton("Bin", "pack://application:,,,/Resources/Images/Bin32.png", AddPageObjectToPageCommand, "BIN");

            //Text
            //TODO: Better Icons
            _insertTextBoxButton = new RibbonButton("Text", "pack://application:,,,/Resources/Images/MajorRibbon/TextBox512.png", AddPageObjectToPageCommand, "TEXTBOX");

            _insertMultipleChoiceTextBoxButton = new RibbonButton("Multiple Choice",
                                                                  "pack://application:,,,/Resources/Images/TempIcon32.png",
                                                                  AddPageObjectToPageCommand,
                                                                  "MULTIPLECHOICEBOX");

            // Recognition
            _insertRecognitionRegionButton = new RibbonButton("Answer Fill In", "pack://application:,,,/Resources/Images/LargeIcon.png", AddPageObjectToPageCommand, "ANSWERFILLIN");
        }

        private bool _isCheckedEventRunning = false;

        private void _button_Checked(object sender, RoutedEventArgs e)
        {
            _isCheckedEventRunning = true;
            var checkedButton = sender as GroupedRibbonButton;
            if (checkedButton == null)
            {
                return;
            }

            switch (checkedButton.GroupName)
            {
                case "PageInteractionMode":
                    PageInteractionMode = (PageInteractionModes)Enum.Parse(typeof (PageInteractionModes), checkedButton.AssociatedEnumValue);

                    if (App.MainWindowViewModel == null)
                    {
                        break;
                    }
                    var contextRibbon = NotebookWorkspaceViewModel.GetContextRibbon();
                    if (contextRibbon == null)
                    {
                        break;
                    }

                    _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();

                    switch (PageInteractionMode)
                    {
                        case PageInteractionModes.None:
                            _pageInteractionService.SetNoInteractionMode();
                            contextRibbon.Buttons.Clear();
                            break;
                        case PageInteractionModes.Select:
                            _pageInteractionService.SetSelectMode();
                            contextRibbon.Buttons.Clear();
                            break;
                        case PageInteractionModes.Draw:
                            _pageInteractionService.SetDrawMode();
                            contextRibbon.SetPenContextButtons();
                            break;
                        case PageInteractionModes.Erase:
                            _pageInteractionService.SetEraseMode();
                            contextRibbon.SetEraserContextButtons();
                            break;
                        case PageInteractionModes.Mark:
                            _pageInteractionService.SetMarkMode();
                            contextRibbon.SetMarkContextButtons();
                            break;
                        case PageInteractionModes.Lasso:
                            _pageInteractionService.SetLassoMode();
                            contextRibbon.Buttons.Clear();
                            break;
                        case PageInteractionModes.Cut:
                            _pageInteractionService.SetCutMode();
                            contextRibbon.Buttons.Clear();
                            break;
                        case PageInteractionModes.DividerCreation:
                            _pageInteractionService.SetDividerCreationMode();
                            contextRibbon.Buttons.Clear();
                            break;
                    }

                    break;
            }
            _isCheckedEventRunning = false;
        }

        #region Buttons

        public static Line Separater
        {
            get
            {
                var dict = new ResourceDictionary();
                var uri = new Uri(@"pack://application:,,,/Resources/CLPBrushes.xaml");
                dict.Source = uri;
                var grayEdgeColor = dict["GrayBorderColor"] as Brush;

                //Separater
                return new Line
                       {
                           Y1 = 0,
                           Y2 = 1,
                           Stretch = Stretch.Fill,
                           Margin = new Thickness(2),
                           Stroke = grayEdgeColor,
                           UseLayoutRounding = false,
                           SnapsToDevicePixels = false,
                           StrokeThickness = 1
                       };
            }
        }

        #region PageInteractionMode Toggle Buttons

        private GroupedRibbonButton _setSelectModeButton;
        private GroupedRibbonButton _setDrawModeButton;
        private GroupedRibbonButton _setEraseModeButton;
        private GroupedRibbonButton _setMarkModeButton;
        private GroupedRibbonButton _setLassoModeButton;
        private GroupedRibbonButton _setCutModeButton;
        private GroupedRibbonButton _setDividerCreationModeButton;

        #endregion //PageInteractionMode Toggles

        #region Insert PageObject Buttons

        //Images
        private RibbonButton _insertImageButton;

        //Stamps
        private RibbonButton _insertGeneralStampButton;
        private RibbonButton _insertGroupStampButton;
        private RibbonButton _insertImageGeneralStampButton;
        private RibbonButton _insertImageGroupStampButton;
        private RibbonButton _insertPileButton;

        //Arrays
        private RibbonButton _insertArrayButton;
        private RibbonButton _insert10x10ArrayButton;
        private RibbonButton _insertArrayCardButton;
        private RibbonButton _insertFactorCardButton;
        private RibbonButton _insertObscurableArrayButton;

        //Division Templates
        private RibbonButton _insertDivisionTemplateButton;

        //NumberLine
        private RibbonButton _insertNumberLineButton;
        private RibbonButton _insertAutoNumberLineButton;

        //Text
        private RibbonButton _insertTextBoxButton;

        private RibbonButton _insertMultipleChoiceTextBoxButton;

        //Shapes
        private DropDownRibbonButton _insertShapeButton;
        private RibbonButton _insertSquareButton;
        private RibbonButton _insertCircleButton;
        private RibbonButton _insertTriangleButton;
        private RibbonButton _insertHorizontalLineButton;
        private RibbonButton _insertProtractorButton;

        //Bin
        private RibbonButton _insertBinButton;

        // Recognition Regions
        private RibbonButton _insertRecognitionRegionButton;

        #endregion //Insert PageObject Buttons

        #endregion //Buttons

        #region Bindings

        /// <summary>SUMMARY</summary>
        public ConnectionStatuses ConnectionStatus
        {
            get { return GetValue<ConnectionStatuses>(ConnectionStatusProperty); }
            set { SetValue(ConnectionStatusProperty, value); }
        }

        public static readonly PropertyData ConnectionStatusProperty = RegisterProperty("ConnectionStatus", typeof (ConnectionStatuses), ConnectionStatuses.Listening);

        /// <summary>List of the buttons currently on the Ribbon.</summary>
        public ObservableCollection<UIElement> Buttons
        {
            get { return GetValue<ObservableCollection<UIElement>>(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        public static readonly PropertyData ButtonsProperty = RegisterProperty("Buttons", typeof (ObservableCollection<UIElement>), () => new ObservableCollection<UIElement>());

        #region Find Better Way

        /// <summary>Gets or sets the property value.</summary>
        public Panels? CurrentLeftPanel
        {
            get { return GetValue<Panels?>(CurrentLeftPanelProperty); }
            set { SetValue(CurrentLeftPanelProperty, value); }
        }

        public static readonly PropertyData CurrentLeftPanelProperty = RegisterProperty("CurrentLeftPanel", typeof (Panels?));

        /// <summary>Right Panel.</summary>
        public Panels? CurrentRightPanel
        {
            get { return GetValue<Panels?>(CurrentRightPanelProperty); }
            set { SetValue(CurrentRightPanelProperty, value); }
        }

        public static readonly PropertyData CurrentRightPanelProperty = RegisterProperty("CurrentRightPanel", typeof (Panels?));

        #endregion //Find Better Way

        /// <summary>Gets or sets the property value.</summary>
        public bool BlockStudentPenInput
        {
            get { return GetValue<bool>(BlockStudentPenInputProperty); }
            set
            {
                SetValue(BlockStudentPenInputProperty, value);

                var discoveredStudentAddresses = App.Network.DiscoveredStudents.Addresses.ToList();
                if (discoveredStudentAddresses.Any())
                {
                    Parallel.ForEach(discoveredStudentAddresses,
                                     address =>
                                     {
                                         try
                                         {
                                             var binding = new NetTcpBinding
                                                           {
                                                               Security =
                                                               {
                                                                   Mode = SecurityMode.None
                                                               }
                                                           };
                                             var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(binding, address);
                                             studentProxy.TogglePenDownMode(value);
                                             (studentProxy as ICommunicationObject).Close();
                                         }
                                         catch (Exception e)
                                         {
                                             Console.WriteLine(e.Message);
                                         }
                                     });
                }
                //if (App.MainWindowViewModel.AvailableUsers.Any())
                //{
                //    Parallel.ForEach(App.MainWindowViewModel.AvailableUsers,
                //                     student =>
                //                     {
                //                         try
                //                         {
                //                             var binding = new NetTcpBinding
                //                             {
                //                                 Security =
                //                                 {
                //                                     Mode = SecurityMode.None
                //                                 }
                //                             };
                //                             var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(binding, new EndpointAddress(student.CurrentMachineAddress));
                //                             studentProxy.TogglePenDownMode(value);
                //                             (studentProxy as ICommunicationObject).Close();
                //                         }
                //                         catch (Exception ex)
                //                         {
                //                             Console.WriteLine(ex.Message);
                //                         }
                //                     });
                //}
                else
                {
                    Logger.Instance.WriteToLog("No Students Found");
                }
            }
        }

        public static readonly PropertyData BlockStudentPenInputProperty = RegisterProperty("BlockStudentPenInput", typeof (bool), false);

        /// <summary>Toggles the animation ribbon.</summary>
        public bool IsAnimationRibbonForcedVisible
        {
            get { return GetValue<bool>(IsAnimationRibbonForcedVisibleProperty); }
            set { SetValue(IsAnimationRibbonForcedVisibleProperty, value); }
        }

        public static readonly PropertyData IsAnimationRibbonForcedVisibleProperty = RegisterProperty("IsAnimationRibbonForcedVisible", typeof(bool), false, OnIsAnimationRibbonForcedVisibleChanged);

        private static void OnIsAnimationRibbonForcedVisibleChanged(object sender, AdvancedPropertyChangedEventArgs args)
        {
            var majorRibbon = sender as MajorRibbonViewModel;
            if (majorRibbon == null)
            {
                return;
            }

            var animationControlRibbon = NotebookWorkspaceViewModel.GetAnimationControlRibbon();
            if (animationControlRibbon == null)
            {
                return;
            }

            animationControlRibbon.IsNonAnimationPlaybackEnabled = (bool)args.NewValue;
        }

        #endregion //Bindings

        #region Properties

        /// <summary>Interaction Mode for the current page.</summary>
        public PageInteractionModes PageInteractionMode
        {
            get { return GetValue<PageInteractionModes>(PageInteractionModeProperty); }
            set
            {
                SetValue(PageInteractionModeProperty, value);
                if (_isCheckedEventRunning)
                {
                    return;
                }

                switch (value)
                {
                    case PageInteractionModes.None:
                        break;
                    case PageInteractionModes.Select:
                        _setSelectModeButton.IsChecked = true;
                        break;
                    case PageInteractionModes.Draw:
                        _setDrawModeButton.IsChecked = true;
                        break;
                    case PageInteractionModes.Erase:
                        _setEraseModeButton.IsChecked = true;
                        break;
                    case PageInteractionModes.Mark:
                        _setMarkModeButton.IsChecked = true;
                        break;
                    case PageInteractionModes.Lasso:
                        _setLassoModeButton.IsChecked = true;
                        break;
                    case PageInteractionModes.Cut:
                        _setCutModeButton.IsChecked = true;
                        break;
                    case PageInteractionModes.DividerCreation:
                        _setDividerCreationModeButton.IsChecked = true;
                        break;
                }
            }
        }

        public static readonly PropertyData PageInteractionModeProperty = RegisterProperty("PageInteractionMode", typeof (PageInteractionModes), PageInteractionModes.Draw);

        #endregion //Properties

        #region Commands

        /// <summary>Restarts the network.</summary>
        public Command ReconnectCommand { get; private set; }

        private void OnReconnectCommandExecute()
        {
            if (MessageBox.Show("Are you sure you want to restart the network connection?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            App.Network.StopNetworking();
            App.Network.StartNetworking();

            //CLPServiceAgent.Instance.NetworkReconnect();  ?
        }

        /// <summary>Brings up the BackStage.</summary>
        public Command ShowBackStageCommand { get; private set; }

        private void OnShowBackStageCommandExecute()
        {
            MainWindow.BackStage.CurrentNavigationPane = NavigationPanes.Info;
            MainWindow.IsBackStageVisible = true;
        }

        /// <summary>If viewing a MultiDisplay, switches to SingleDisplay and closes Displays Panel.</summary>
        public Command ExitMultiDisplayCommand { get; private set; }

        private void OnExitMultiDisplayCommandExecute()
        {
            var notebookWorkspace = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if (notebookWorkspace == null)
            {
                return;
            }

            notebookWorkspace.CurrentDisplay = null;
            CurrentRightPanel = null;

            if (App.Network.ProjectorProxy == null)
            {
                return;
            }

            try
            {
                const string DISPLAY_ID = "SingleDisplay";
                App.Network.ProjectorProxy.SwitchProjectorDisplay(DISPLAY_ID, -1);
            }
            catch (Exception)
            {
                //
            }
        }

        private bool OnExitMultiDisplayCanExecute()
        {
            var notebookWorkspace = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if (notebookWorkspace == null)
            {
                return false;
            }

            return notebookWorkspace.CurrentDisplay != null;
        }

        #region History Commands

        /// <summary>Undoes the last action.</summary>
        public Command UndoCommand { get; private set; }

        private void OnUndoCommandExecute() { _dataService.CurrentPage.History.Undo(); }

        private bool OnUndoCanExecute()
        {
            var page = _dataService.CurrentPage;
            if (page == null)
            {
                return false;
            }

            var recordIndicator = page.History.RedoItems.FirstOrDefault() as AnimationIndicator;
            if (recordIndicator != null &&
                recordIndicator.AnimationIndicatorType == AnimationIndicatorType.Record)
            {
                return false;
            }

            return page.History.CanUndo;
        }

        /// <summary>Redoes the last undone action.</summary>
        public Command RedoCommand { get; private set; }

        private void OnRedoCommandExecute() { _dataService.CurrentPage.History.Redo(); }

        private bool OnRedoCanExecute()
        {
            var page = _dataService.CurrentPage;
            if (page == null)
            {
                return false;
            }

            return page.History.CanRedo;
        }

        #endregion //History Commands

        #region Sharing Commands

        /// <summary>Submits current page to the teacher.</summary>
        public Command SubmitPageCommand { get; private set; }

        private void OnSubmitPageCommandExecute()
        {
            var currentPage = _dataService.CurrentPage;

            currentPage.TrimPage();
            var page = currentPage;
            var submission = currentPage.NextVersionCopy();

            var tBackground = new Thread(() =>
                               {
                                   var existingTags = submission.Tags.Where(t => t.Category != Category.Definition && !(t is TempArraySkipCountingTag)).ToList();
                                   foreach (var tempArraySkipCountingTag in existingTags)
                                   {
                                       submission.RemoveTag(tempArraySkipCountingTag);
                                   }

                                   HistoryAnalysis.GenerateHistoryActions(submission);

                                   string sPage = string.Empty;
                                   try
                                   {
                                       sPage = ObjectSerializer.ToString(submission);
                                   }
                                   catch (Exception e)
                                   {
                                       Logger.Instance.WriteToLog("Failed To stringify submission");
                                       Logger.Instance.WriteToLog("[UNHANDLED ERROR] - " + e.Message + " " + (e.InnerException != null ? "\n" + e.InnerException.Message : null));
                                       Logger.Instance.WriteToLog("[HResult]: " + e.HResult);
                                       Logger.Instance.WriteToLog("[Source]: " + e.Source);
                                       Logger.Instance.WriteToLog("[Method]: " + e.TargetSite);
                                       Logger.Instance.WriteToLog("[StackTrace]: " + e.StackTrace);
                                   }

                                   Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                       {
                                                           try
                                                           {
                                                               page.Submissions.Add(submission);
                                                               page.IsCached = true;
                                                           }
                                                           catch (Exception e)
                                                           {
                                                               Logger.Instance.WriteToLog("[ERROR] Error adding submission to current page on submit: " +
                                                                                          e.Message);
                                                           }

                                                           return null;
                                                       },
                                                       null);

                                   var dataService = DependencyResolver.Resolve<IDataService>();
                                   if (dataService == null)
                                   {
                                       Logger.Instance.WriteToLog("notebook service null on submission");
                                       return;
                                   }
                                   if (string.IsNullOrEmpty(sPage))
                                   {
                                       Logger.Instance.WriteToLog("sPage null or empty on submission");
                                       return;
                                   }
                                   if (App.Network.InstructorProxy == null)
                                   {
                                       Logger.Instance.WriteToLog("Instructor NOT Available for Student Submission");
                                       return;
                                   }

                                   try
                                   {
                                       //var sPage = ObjectSerializer.ToString(submission);
                                       var zippedPage = sPage.CompressWithGZip();

                                       App.Network.InstructorProxy.AddSerializedSubmission(zippedPage, dataService.CurrentNotebook.ID);
                                   }
                                   catch (Exception ex)
                                   {
                                       Logger.Instance.WriteToLog("Error Sending Submission: " + ex.Message);
                                       return;
                                   }
                               })
                    {
                        IsBackground = true
                    };
            tBackground.Start();
        }

        private bool OnSubmitPageCanExecute()
        {
            var notebookWorkspace = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if (notebookWorkspace == null ||
                _dataService.CurrentPage == null ||
                notebookWorkspace.PagesAddedThisSession.Contains(_dataService.CurrentPage))
            {
                return false;
            }

            return !_dataService.CurrentPage.IsCached;
        }

        #endregion //Sharing Commands

        #region Page Commands

        /// <summary>Adds new page to current notebook.</summary>
        public Command AddNewPageCommand { get; private set; }

        private void OnAddNewPageCommandExecute()
        {
            var notebookWorkspace = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if (notebookWorkspace == null)
            {
                return;
            }

            var newPage = new CLPPage(App.MainWindowViewModel.CurrentUser);
            notebookWorkspace.Notebook.AddPage(newPage);
            notebookWorkspace.PagesAddedThisSession.Add(newPage);
        }

        /// <summary>
        /// Adds a new animation page to the current notebook.
        /// </summary>
        public Command AddNewAnimationPageCommand { get; private set; }

        private void OnAddNewAnimationPageCommandExecute()
        {
            var notebookWorkspace = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if (notebookWorkspace == null)
            {
                return;
            }

            var newPage = new CLPPage(App.MainWindowViewModel.CurrentUser)
                          {
                              PageType = PageTypes.Animation
                          };
            notebookWorkspace.Notebook.AddPage(newPage);
        }

        /// <summary>Doubles height of the current page.</summary>
        public Command LongerPageCommand { get; private set; }

        private void OnLongerPageCommandExecute()
        {
            var currentPage = _dataService.CurrentPage;

            if (currentPage == null)
            {
                return;
            }

            var initialHeight = currentPage.Width / currentPage.InitialAspectRatio;
            currentPage.Height = initialHeight * 2;

            if (App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Teacher ||
                App.Network.ProjectorProxy == null)
            {
                return;
            }

            try
            {
                App.Network.ProjectorProxy.MakeCurrentPageLonger();
            }
            catch (Exception) { }
        }

        private bool OnLongerPageCanExecute()
        {
            var currentPage = _dataService.CurrentPage;

            if (currentPage == null)
            {
                return false;
            }

            var initialHeight = currentPage.Width / currentPage.InitialAspectRatio;
            return currentPage.Height < initialHeight * 2;
        }

        /// <summary>Sets the Version 0 page to the state of the selected submission.</summary>
        public Command ReverseSubmitPageCommand { get; private set; }

        private void OnReverseSubmitPageCommandExecute()
        {
            //var submission = CurrentPage;
            //var notebookService = DependencyResolver.Resolve<INotebookService>();
            //if (notebookService == null ||
            //    submission == null)
            //{
            //    return;
            //}

            //var parentPage = notebookService.CurrentNotebook.GetPageByCompositeKeys(submission.ID, submission.OwnerID, submission.DifferentiationLevel, 0);
            //submission.SerializedStrokes = StrokeDTO.SaveInkStrokes(submission.InkStrokes);
            //submission.History.SerializedTrashedInkStrokes = StrokeDTO.SaveInkStrokes(submission.History.TrashedInkStrokes);
            //var copy = submission.DeepCopy();
            //if (copy == null ||
            //    parentPage == null)
            //{
            //    return;
            //}

            //copy.LastVersionIndex = submission.VersionIndex;
            //copy.SubmissionType = SubmissionTypes.Unsubmitted;
            //copy.VersionIndex = 0;
            //copy.History.VersionIndex = 0;
            //copy.History.LastVersionIndex = submission.VersionIndex;
            //foreach (var pageObject in copy.PageObjects)
            //{
            //    pageObject.VersionIndex = 0;
            //    pageObject.LastVersionIndex = submission.VersionIndex;
            //    pageObject.ParentPage = copy;
            //}

            //foreach (var pageObject in copy.History.TrashedPageObjects)
            //{
            //    pageObject.VersionIndex = 0;
            //    pageObject.LastVersionIndex = submission.VersionIndex;
            //    pageObject.ParentPage = copy;
            //}

            //foreach (var tag in copy.Tags)
            //{
            //    tag.VersionIndex = 0;
            //    tag.LastVersionIndex = submission.VersionIndex;
            //    tag.ParentPage = copy;
            //}

            //foreach (var serializedStroke in copy.SerializedStrokes)
            //{
            //    //TODO: Stroke Version Index should be uint
            //    serializedStroke.VersionIndex = 0;
            //}

            //foreach (var serializedStroke in copy.History.SerializedTrashedInkStrokes)
            //{
            //    serializedStroke.VersionIndex = 0;
            //}

            //copy.Submissions = parentPage.Submissions;
            //var pageIndex = notebookService.CurrentNotebook.Pages.IndexOf(parentPage);
            //notebookService.CurrentNotebook.Pages.RemoveAt(pageIndex);
            //notebookService.CurrentNotebook.Pages.Insert(pageIndex, copy);
        }

        #endregion //Page Commands

        #region Insert PageObject Commands

        /// <summary>Adds pageObject to the Current Page.</summary>
        public Command<string> AddPageObjectToPageCommand { get; private set; }

        private void OnAddPageObjectToPageCommandExecute(string pageObjectType)
        {
            var currentPage = _dataService.CurrentPage;

            if (currentPage == null)
            {
                return;
            }

            switch (pageObjectType)
            {
                //Image
                case "IMAGE":
                    CLPImageViewModel.AddImageToPage(currentPage);
                    break;

                //Stamps
                case "BLANK_GENERAL_STAMP":
                    StampViewModel.AddBlankGeneralStampToPage(currentPage);
                    break;
                case "BLANK_GROUP_STAMP":
                    StampViewModel.AddBlankGroupStampToPage(currentPage);
                    break;
                case "IMAGE_GENERAL_STAMP":
                    StampViewModel.AddImageGeneralStampToPage(currentPage);
                    break;
                case "IMAGE_GROUP_STAMP":
                    StampViewModel.AddImageGroupStampToPage(currentPage);
                    break;
                case "PILE":
                    StampViewModel.AddPileToPage(currentPage);
                    break;

                //Arrays
                case "ARRAY":
                    CLPArrayViewModel.AddArrayToPage(currentPage, ArrayTypes.Array);
                    break;
                case "10X10":
                    CLPArrayViewModel.AddArrayToPage(currentPage, ArrayTypes.TenByTen);
                    break;
                case "ARRAYCARD":
                    CLPArrayViewModel.AddArrayToPage(currentPage, ArrayTypes.ArrayCard);
                    break;
                case "FACTORCARD":
                    CLPArrayViewModel.AddArrayToPage(currentPage, ArrayTypes.FactorCard);
                    break;
                case "OBSCURABLE_ARRAY":
                    CLPArrayViewModel.AddArrayToPage(currentPage, ArrayTypes.ObscurableArray);
                    break;

                //Number Line 
                case "NUMBERLINE":
                    NumberLineViewModel.AddNumberLineToPage(currentPage);
                    break;
                case "AUTO_NUMBERLINE":
                    NumberLineViewModel.AddNumberLineToPage(currentPage);
                    break;

                //Division Template
                case "DIVISIONTEMPLATE":
                    DivisionTemplateViewModel.AddDivisionTemplateToPage(currentPage);
                    break;

                //Shapes
                case "SQUARE":
                    ShapeViewModel.AddShapeToPage(currentPage, ShapeType.Rectangle);
                    break;
                case "CIRCLE":
                    ShapeViewModel.AddShapeToPage(currentPage, ShapeType.Ellipse);
                    break;
                case "TRIANGLE":
                    ShapeViewModel.AddShapeToPage(currentPage, ShapeType.Triangle);
                    break;
                case "HORIZONTALLINE":
                    ShapeViewModel.AddShapeToPage(currentPage, ShapeType.HorizontalLine);
                    break;
                case "VERTICALLINE":
                    ShapeViewModel.AddShapeToPage(currentPage, ShapeType.VerticalLine);
                    break;
                case "PROTRACTOR":
                    ShapeViewModel.AddShapeToPage(currentPage, ShapeType.Protractor);
                    break;
                case "RIGHT_DIAGONAL":
                    ShapeViewModel.AddShapeToPage(currentPage, ShapeType.RightDiagonal);
                    break;
                case "RIGHT_DIAGONAL_DASHED":
                    ShapeViewModel.AddShapeToPage(currentPage, ShapeType.RightDiagonalDashed);
                    break;
                case "LEFT_DIAGONAL":
                    ShapeViewModel.AddShapeToPage(currentPage, ShapeType.LeftDiagonal);
                    break;
                case "LEFT_DIAGONAL_DASHED":
                    ShapeViewModel.AddShapeToPage(currentPage, ShapeType.LeftDiagonalDashed);
                    break;

                //Bin
                case "BIN":
                    BinViewModel.AddBinToPage(currentPage);
                    break;

                //Text
                case "TEXTBOX":
                    CLPTextBoxViewModel.AddTextBoxToPage(currentPage);
                    break;

                case "MULTIPLECHOICEBOX":
                    MultipleChoiceViewModel.AddMultipleChoiceToPage(currentPage);
                    break;

                // Recognition
                case "ANSWERFILLIN":
                    InterpretationRegionViewModel.AddInterpretationRegionToPage(currentPage);
                    break;
            }

            PageInteractionMode = PageInteractionModes.Select;
        }

        private bool OnAddPageObjectToPageCanExecute(string pageObjectType)
        {
            return _dataService.CurrentPage != null;
        }

        #endregion //Insert PageObject Commands

        #region Extras

        /// <summary>Takes a screenshot of the current page.</summary>
        public Command TakePageScreenshotCommand { get; private set; }

        private void OnTakePageScreenshotCommandExecute()
        {
            var currentPage = _dataService.CurrentPage;
            var pageViewModel = currentPage.GetAllViewModels().First(x => (x is ACLPPageBaseViewModel) && !(x as ACLPPageBaseViewModel).IsPagePreview);

            var viewManager = ServiceLocator.Default.ResolveType<IViewManager>();
            var views = viewManager.GetViewsOfViewModel(pageViewModel);
            var pageView = views.FirstOrDefault(view => view is CLPPageView) as CLPPageView;
            if (pageView == null)
            {
                return;
            }

            var bitmapImage = pageView.ToBitmapImage(currentPage.Width, dpi: 300);

            var thumbnailsFolderPath = Catel.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Page Screenshots");
            var thumbnailFilePath = Catel.IO.Path.Combine(thumbnailsFolderPath,
                                                 "Page - " + currentPage.PageNumber + ";" + currentPage.DifferentiationLevel + ";" + currentPage.VersionIndex + ";" +
                                                 DateTime.Now.ToString("yyyy-M-d,hh.mm.ss") + ".png");

            if (!Directory.Exists(thumbnailsFolderPath))
            {
                Directory.CreateDirectory(thumbnailsFolderPath);
            }

            var pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using (var outputStream = new MemoryStream())
            {
                pngEncoder.Save(outputStream);
                File.WriteAllBytes(thumbnailFilePath, outputStream.ToArray());
            }
        }

        private bool OnTakePageScreenshotCanExecute()
        {
            return _dataService.CurrentPage != null;
        }

        #endregion // Extras

        #endregion //Commands

        #region Methods

        public void SetRibbonButtons()
        {
            // Page Interaction Modes
            Buttons.Add(_setSelectModeButton);
            Buttons.Add(_setDrawModeButton);
            Buttons.Add(_setEraseModeButton);
            //Buttons.Add(_setMarkModeButton);
            Buttons.Add(Separater);
            Buttons.Add(_setLassoModeButton);
            Buttons.Add(_setCutModeButton);
            Buttons.Add(_setDividerCreationModeButton);

            // Insert Math Tools
            Buttons.Add(Separater);
            Buttons.Add(_insertArrayButton);
            //Buttons.Add(_insertObscurableArrayButton);
            Buttons.Add(_insertBinButton);
            Buttons.Add(_insertDivisionTemplateButton);
            Buttons.Add(_insertNumberLineButton);
            //Buttons.Add(_insertAutoNumberLineButton);
            Buttons.Add(_insertShapeButton);
            Buttons.Add(_insertGeneralStampButton);
            Buttons.Add(_insertPileButton);
            //Buttons.Add(_insertGroupStampButton);
            //Buttons.Add(_insert10x10ArrayButton);
            //Buttons.Add(_insertArrayCardButton);
            //Buttons.Add(_insertFactorCardButton);

            // Insert Text Box
            //Buttons.Add(Separater);
            //Buttons.Add(_insertImageButton);
            //Buttons.Add(_insertTextBoxButton);
            //Buttons.Add(_insertRecognitionRegionButton);
            //Buttons.Add(_insertMultipleChoiceTextBoxButton);
        }

        #endregion //Methods
    }
}