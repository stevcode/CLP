using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities.Ann;
using System.Windows.Threading;
using System.ServiceModel;
using iTextSharp.text;
using iTextSharp.text.pdf;

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
        }

        private void InitializeCommands()
        {
            //File Menu
            LogInAsNotebookOwnerCommand = new Command(OnLogInAsNotebookOwnerCommandExecute);
            RefreshNetworkCommand = new Command(OnRefreshNetworkCommandExecute);
            ToggleThumbnailsCommand = new Command(OnToggleThumbnailsCommandExecute);

            //Submission
            SubmitPageCommand = new Command(OnSubmitPageCommandExecute, OnInsertPageObjectCanExecute);
            GroupSubmitPageCommand = new Command(OnGroupSubmitPageCommandExecute, OnInsertPageObjectCanExecute);

            //History
            DisableHistoryCommand = new Command(OnDisableHistoryCommandExecute, OnClearHistoryCommandCanExecute);
            
            ClearPageHistoryCommand = new Command(OnClearPageHistoryCommandExecute, OnClearHistoryCommandCanExecute);
            ClearPageNonAnimationHistoryCommand = new Command(OnClearPageNonAnimationHistoryCommandExecute, OnClearHistoryCommandCanExecute);
            ClearPagesHistoryCommand = new Command(OnClearPagesHistoryCommandExecute, OnClearHistoryCommandCanExecute);
            ClearPagesNonAnimationHistoryCommand = new Command(OnClearPagesNonAnimationHistoryCommandExecute, OnClearHistoryCommandCanExecute);
            ClearSubmissionsHistoryCommand = new Command(OnClearSubmissionsHistoryCommandExecute, OnClearHistoryCommandCanExecute);
            ClearSubmissionsNonAnimationHistoryCommand = new Command(OnClearSubmissionsNonAnimationHistoryCommandExecute, OnClearHistoryCommandCanExecute);

            //Insert
            ToggleWebcamPanelCommand = new Command<bool>(OnToggleWebcamPanelCommandExecute);
            InsertStaticImageCommand = new Command<string>(OnInsertStaticImageCommandExecute, OnInsertPageObjectCanExecute);
            InsertAggregationDataTableCommand = new Command(OnInsertAggregationDataTableCommandExecute, OnInsertPageObjectCanExecute);
            InsertHandwritingRegionCommand = new Command(OnInsertHandwritingRegionCommandExecute, OnInsertPageObjectCanExecute);
            InsertInkShapeRegionCommand = new Command(OnInsertInkShapeRegionCommandExecute, OnInsertPageObjectCanExecute);
            InsertDataTableCommand = new Command(OnInsertDataTableCommandExecute, OnInsertPageObjectCanExecute);
            InsertShadingRegionCommand = new Command(OnInsertShadingRegionCommandExecute, OnInsertPageObjectCanExecute);
            InsertGroupingRegionCommand = new Command(OnInsertGroupingRegionCommandExecute, OnInsertPageObjectCanExecute);  
            
            //Testing
            TurnOffWebcamSharing = new Command(OnTurnOffWebcamSharingExecute);
            BroadcastPageCommand = new Command(OnBroadcastPageCommandExecute);
            SendPageToStudentCommand = new Command(OnSendPageToStudentCommandExecute);
            ReplacePageCommand = new Command(OnReplacePageCommandExecute);
            CreatePageSubmissionCommand = new Command(OnCreatePageSubmissionCommandExecute);
            MakeGroupsCommand = new Command(OnMakeGroupsCommandExecute);
            MakeExitTicketsCommand = new Command(OnMakeExitTicketsCommandExecute);
            MakeExitTicketsFromCurrentPageCommand = new Command(OnMakeExitTicketsFromCurrentPageCommandExecute);

            //Page
            ClearPageCommand = new Command(OnClearPageCommandExecute, OnInsertPageObjectCanExecute);

            //Metadata
            AddPageTopicCommand = new Command(OnAddPageTopicCommandExecute, OnInsertPageObjectCanExecute);
            EditPageMetadataCommand = new Command(OnEditPageMetadataCommandExecute, OnInsertPageObjectCanExecute);
            SetNotebookCurriculumCommand = new Command(OnSetNotebookCurriculumCommandExecute);

            //Debug
            

            //Authoring
            SetEntireNotebookAsAuthoredCommand = new Command(OnSetEntireNotebookAsAuthoredCommandExecute);
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "RibbonVM"; } }

        #region Properties

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
        public bool BroadcastInkToStudents
        {
            get { return GetValue<bool>(BroadcastInkToStudentsProperty); }
            set { SetValue(BroadcastInkToStudentsProperty, value); }
        }

        public static readonly PropertyData BroadcastInkToStudentsProperty = RegisterProperty("BroadcastInkToStudents", typeof(bool), false);

        

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

        #endregion //Convert to XAMLS?

        #region TextBox

        //public FontFamily CurrentFontFamily
        //{
        //    get { return GetValue<FontFamily>(CurrentFontFamilyProperty); }
        //    set
        //    {
        //        SetValue(CurrentFontFamilyProperty, value);
        //        if(LastFocusedTextBox != null)
        //        {
        //            if(!LastFocusedTextBox.isUpdatingVisualState)
        //            {
        //                LastFocusedTextBox.SetFont(-1.0, value, null);
        //            }
        //        }
        //    }
        //}

        //public double CurrentFontSize
        //{
        //    get { return GetValue<double>(CurrentFontSizeProperty); }
        //    set
        //    {
        //        SetValue(CurrentFontSizeProperty, value);
        //        if(LastFocusedTextBox != null)
        //        {
        //            if(!LastFocusedTextBox.isUpdatingVisualState)
        //            {
        //                LastFocusedTextBox.SetFont(CurrentFontSize, null, null);
        //            }
        //        }
        //    }
        //}

        //public Brush CurrentFontColor
        //{
        //    get { return GetValue<Brush>(CurrentFontColorProperty); }
        //    set
        //    {
        //        SetValue(CurrentFontColorProperty, value);
        //        if(LastFocusedTextBox != null)
        //        {
        //            if(!LastFocusedTextBox.isUpdatingVisualState)
        //            {
        //                LastFocusedTextBox.SetFont(-1.0, null, CurrentFontColor);
        //            }
        //        }
        //    }
        //}

        #endregion //TextBox

        #endregion //Bindings
        
        #region Methods

        public int MatchArrayGridSize(List<ACLPArrayBase> arraysToAdd)
        {
            int numberOfArrays = arraysToAdd.Count;
            var firstArray = arraysToAdd.First();
            int rows = firstArray.Rows;
            int columns = firstArray.Columns;

            const double MIN_SIDE = 20.0;
            const double MIN_FFC_SIDE = 185.0;
            const double LABEL_LENGTH = 22.0;

            var arrayStacks = 1;
            var isHorizontallyAligned = CurrentPage.Width / columns > CurrentPage.Height / 4 * 3 / rows;
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
                foreach(var pageObject in CurrentPage.PageObjects)
                {
                    if((pageObject is CLPArray || pageObject is FuzzyFactorCard) && pageObject.CreatorID != Person.Author.ID)
                    {
                        squareSize = (pageObject as ACLPArrayBase).ArrayHeight / (pageObject as ACLPArrayBase).Rows;
                    }
                }
            }

            var minSide = (firstArray is FuzzyFactorCard)
                ? MIN_FFC_SIDE :
                MIN_SIDE;
            var defaultSquareSize = (firstArray is FuzzyFactorCard) ?
                Math.Max(45.0, (MIN_FFC_SIDE / (Math.Min(rows, columns)))) : 
                45.0;
            var initializedSquareSize = (squareSize > 0) ? Math.Max(squareSize, (minSide / (Math.Min(rows, columns)))) : defaultSquareSize;
            if((firstArray is FuzzyFactorCard)&& xPosition + initializedSquareSize * columns + LABEL_LENGTH * 3.0 + 12.0 > CurrentPage.Width)
            {
                initializedSquareSize = minSide / (Math.Min(rows, columns));
            }

            while(xPosition + 2 * LABEL_LENGTH + initializedSquareSize * columns >= CurrentPage.Width || yPosition + 2 * LABEL_LENGTH + initializedSquareSize * rows >= CurrentPage.Height)
            {
                initializedSquareSize = Math.Abs(initializedSquareSize - 45.0) < .0001 ? 22.5 : initializedSquareSize / 4 * 3;
            }
            if(numberOfArrays > 1)
            {
                if(isHorizontallyAligned)
                {
                    while(xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * numberOfArrays + LABEL_LENGTH >= CurrentPage.Width)
                    {
                        initializedSquareSize = Math.Abs(initializedSquareSize - 45.0) < .0001 ? 22.5 : initializedSquareSize / 4 * 3;

                        if(numberOfArrays < 5 || xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * numberOfArrays + LABEL_LENGTH < CurrentPage.Width)
                        {
                            continue;
                        }

                        if(xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * Math.Ceiling((double)numberOfArrays / 2) + LABEL_LENGTH < CurrentPage.Width &&
                           yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * 2 + LABEL_LENGTH < CurrentPage.Height)
                        {
                            arrayStacks = 2;
                            break;
                        }

                        if(xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * Math.Ceiling((double)numberOfArrays / 3) + LABEL_LENGTH < CurrentPage.Width &&
                           yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * 3 + LABEL_LENGTH < CurrentPage.Height)
                        {
                            arrayStacks = 3;
                            break;
                        }
                    }
                }
                else
                {
                    yPosition = 100;
                    while(yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * numberOfArrays + LABEL_LENGTH >= CurrentPage.Height)
                    {
                        initializedSquareSize = Math.Abs(initializedSquareSize - 45.0) < .0001 ? 22.5 : initializedSquareSize / 4 * 3;

                        if(numberOfArrays < 5 || yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * numberOfArrays + LABEL_LENGTH < CurrentPage.Height)
                        {
                            continue;
                        }

                        if(yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * Math.Ceiling((double)numberOfArrays / 2) + LABEL_LENGTH < CurrentPage.Height &&
                           xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * 2 + LABEL_LENGTH < CurrentPage.Width)
                        {
                            arrayStacks = 2;
                            break;
                        }

                        if(yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * Math.Ceiling((double)numberOfArrays / 3) + LABEL_LENGTH < CurrentPage.Height &&
                           xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * 3 + LABEL_LENGTH < CurrentPage.Width)
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
                foreach(var pageObject in CurrentPage.PageObjects)
                {
                    if(pageObject is CLPArray && pageObject.CreatorID != Person.Author.ID)
                    {
                        oldDimensions.Add(pageObject.ID, new Point(pageObject.Width, pageObject.Height));
                        if((pageObject as ACLPArrayBase).Rows * initializedSquareSize > MIN_SIDE && (pageObject as ACLPArrayBase).Columns * initializedSquareSize > MIN_SIDE)
                        {
                            if(pageObject.XPosition + (pageObject as ACLPArrayBase).Columns * initializedSquareSize + 2 * LABEL_LENGTH <= CurrentPage.Width && pageObject.YPosition + (pageObject as ACLPArrayBase).Rows * initializedSquareSize + 2 * LABEL_LENGTH <= CurrentPage.Height)
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


            double MAX_HEIGHT = CurrentPage.Height - 400.0;
            if(squareSize == 0.0)
            {
                initializedSquareSize = Math.Min(initialGridsquareSize, MAX_HEIGHT / rows);
            }

            firstArray.SizeArrayToGridLevel(initializedSquareSize);

            return arrayStacks;
        }

        public void PlaceArrayNextToExistingArray(List<ACLPArrayBase> arraysToAdd)
        {
            const double LABEL_LENGTH = 22.0;
            int numberOfArrays = arraysToAdd.Count;
            var firstArray = arraysToAdd.First();
            int rows = firstArray.Rows;
            int columns = firstArray.Columns;
            var isHorizontallyAligned = CurrentPage.Width / columns > CurrentPage.Height / 4 * 3 / rows;
            double initializedSquareSize = firstArray.ArrayHeight / rows;
            var xPosition = firstArray.XPosition;
            var yPosition = firstArray.YPosition;

            //if there is exactly one other array on the page, keep track of it for placement
            ACLPArrayBase onlyArray = null;
            foreach(var pageObject in CurrentPage.PageObjects)
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
                    if(!(onlyArray is FuzzyFactorCard && (onlyArray as FuzzyFactorCard).RemainderTiles != null) && onlyArray.XPosition + onlyArray.Width + (LABEL_LENGTH + columns * initializedSquareSize) * numberOfArrays + LABEL_LENGTH + GAP <= CurrentPage.Width
                        && rows * initializedSquareSize + LABEL_LENGTH < CurrentPage.Height)
                    {
                        xPosition = onlyArray.XPosition + onlyArray.Width + GAP;
                        yPosition = onlyArray.YPosition;
                    }
                    else if(onlyArray.XPosition + (LABEL_LENGTH + columns * initializedSquareSize) * numberOfArrays + LABEL_LENGTH <= CurrentPage.Width
                        && onlyArray.YPosition + onlyArray.Height + rows * initializedSquareSize + LABEL_LENGTH + GAP < CurrentPage.Height)
                    {
                        yPosition = onlyArray.YPosition + onlyArray.Height + GAP;
                        xPosition = onlyArray.XPosition;
                    }
                    else
                    {
                        yPosition = CurrentPage.Height - rows * initializedSquareSize - 2 * LABEL_LENGTH;
                        xPosition = onlyArray.XPosition;
                    }
                }
                else
                {
                    const double GAP = 35.0;
                    if(!(onlyArray is FuzzyFactorCard && (onlyArray as FuzzyFactorCard).RemainderTiles != null) && onlyArray.YPosition + (LABEL_LENGTH + rows * initializedSquareSize) * numberOfArrays + LABEL_LENGTH <= CurrentPage.Height
                        && onlyArray.XPosition + onlyArray.Width + columns * initializedSquareSize + LABEL_LENGTH + GAP < CurrentPage.Width)
                    {
                        xPosition = onlyArray.XPosition + onlyArray.Width + GAP;
                        yPosition = onlyArray.YPosition;
                    }
                    else if(onlyArray.YPosition + onlyArray.Height + (LABEL_LENGTH + rows * initializedSquareSize) * numberOfArrays + LABEL_LENGTH + GAP <= CurrentPage.Width
                        && onlyArray.XPosition + rows * initializedSquareSize + LABEL_LENGTH < CurrentPage.Height)
                    {
                        yPosition = onlyArray.YPosition + onlyArray.Height + GAP;
                        xPosition = onlyArray.XPosition;
                    }
                }
            }

            firstArray.XPosition = xPosition;
            firstArray.YPosition = yPosition;
        }

        #endregion //Methods

        #region Commands

        #region File Menu

        /// <summary>
        /// Sets CurrentUser to the owner of the opened notebook.
        /// </summary>
        public Command LogInAsNotebookOwnerCommand { get; private set; }

        private void OnLogInAsNotebookOwnerCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if (notebookWorkspaceViewModel == null)
            {
                return;
            }

            MainWindow.CurrentUser = notebookWorkspaceViewModel.Notebook.Owner;
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

        #endregion //File Menu

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

        public Command MakeGroupsCommand { get; private set; }

        private void OnMakeGroupsCommandExecute()
        {
            //TODO CACHE
            //var groupCreationViewModel = new GroupCreationViewModel();
            //var groupCreationView = new GroupCreationView(groupCreationViewModel);
            //groupCreationView.Owner = Application.Current.MainWindow;
            //groupCreationView.WindowStartupLocation = WindowStartupLocation.Manual;
            //groupCreationView.Top = 0;
            //groupCreationView.ShowDialog();
            //if(groupCreationView.DialogResult == true)
            //{
            //    foreach(var group in groupCreationViewModel.Groups)
            //    {
            //        foreach(Person student in group.Members)
            //        {
            //            if(groupCreationViewModel.GroupType == "Temp")
            //            {
            //                student.TempDifferentiationGroup = group.Label;
            //            }
            //            else
            //            {
            //                student.CurrentDifferentiationGroup = group.Label;
            //            }
            //        }
            //    }
            //    try
            //    {
            //        App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.SaveClassSubject(MainWindowViewModel.ClassCacheDirectory);
            //    }
            //    catch
            //    {
            //        Logger.Instance.WriteToLog("Failed to save class subject after making groups.");
            //    }
            //}
        }

        public Command SendPageToStudentCommand { get; private set; }

        private void OnSendPageToStudentCommandExecute()
        {
            if(!App.MainWindowViewModel.AvailableUsers.Any()) 
            {
                Logger.Instance.WriteToLog("No Students Found");
                return;
            }

            var studentSelectorViewModel = new StudentSelectorViewModel();
            var studentSelectorView = new StudentSelectorView(studentSelectorViewModel);
            studentSelectorView.Owner = Application.Current.MainWindow;
            studentSelectorView.ShowDialog();
            if(studentSelectorView.DialogResult != true)
            {
                return;
            }

            CurrentPage.History.ClearHistory();
            CurrentPage.SerializedStrokes = StrokeDTO.SaveInkStrokes(CurrentPage.InkStrokes);
            CurrentPage.History.SerializedTrashedInkStrokes = StrokeDTO.SaveInkStrokes(CurrentPage.History.TrashedInkStrokes);
            var serializedCurrentPage = CLPServiceAgent.Instance.Zip(ObjectSerializer.ToString(CurrentPage));
            Parallel.ForEach(studentSelectorViewModel.SelectedStudents,
                student =>
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
                        var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(binding, 
                            new EndpointAddress(student.CurrentMachineAddress));
                        studentProxy.AddNewPage(serializedCurrentPage, 999);
                        (studentProxy as ICommunicationObject).Close();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
        }

        public Command MakeExitTicketsFromCurrentPageCommand { get; private set; }

        private void OnMakeExitTicketsFromCurrentPageCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            var notebook = notebookWorkspaceViewModel.Notebook;

            var exitTicketCreationViewModel = new ExitTicketCreationViewModel(CurrentPage);
            var exitTicketCreationView = new ExitTicketCreationView(exitTicketCreationViewModel);
            exitTicketCreationView.Owner = Application.Current.MainWindow;

            //App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Draw;
            exitTicketCreationView.ShowDialog();
            if(exitTicketCreationView.DialogResult == true)
            {
                SendExitTickets(exitTicketCreationViewModel);
            }
        }

        public Command MakeExitTicketsCommand { get; private set; }

        private void OnMakeExitTicketsCommandExecute()
        {
            var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            var notebook = notebookWorkspaceViewModel.Notebook;

            var exitTicketCreationViewModel = new ExitTicketCreationViewModel();
            var exitTicketCreationView = new ExitTicketCreationView(exitTicketCreationViewModel);
            exitTicketCreationView.Owner = Application.Current.MainWindow;

            //App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Draw;
            exitTicketCreationView.ShowDialog();
            if(exitTicketCreationView.DialogResult == true)
            {
                SendExitTickets(exitTicketCreationViewModel);
            }
        }

        private void SendExitTickets(ExitTicketCreationViewModel exitTicketCreationViewModel) 
        {
            //TODO CACHE
            //var notebookWorkspaceViewModel = MainWindow.Workspace as NotebookWorkspaceViewModel;
            //if(notebookWorkspaceViewModel == null)
            //{
            //    return;
            //}

            //var notebook = notebookWorkspaceViewModel.Notebook;
            //for(int i = 0; i < exitTicketCreationViewModel.ExitTickets.Count; i++)
            //{
            //    var exitTicket = exitTicketCreationViewModel.ExitTickets[i];
            //    notebook.Pages.Add(exitTicket);
            //    exitTicket.History.ClearHistory();
            //    exitTicket.SerializedStrokes = StrokeDTO.SaveInkStrokes(exitTicket.InkStrokes);
            //    exitTicket.History.SerializedTrashedInkStrokes = StrokeDTO.SaveInkStrokes(exitTicket.History.TrashedInkStrokes);

            //    foreach(Person student in exitTicketCreationViewModel.GroupCreationViewModel.Groups[i].Members)
            //    {
            //        student.TempDifferentiationGroup = exitTicket.DifferentiationLevel;
            //    }
            //}
            //try
            //{
            //    App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.SaveClassSubject(MainWindowViewModel.ClassCacheDirectory);
            //}
            //catch
            //{
            //    Logger.Instance.WriteToLog("Failed to save class subject after making exit tickets.");
            //}

            ////send exit tickets to projector
            //if(App.Network.ProjectorProxy != null)
            //{
            //    try
            //    {
            //        foreach(CLPPage exitTicket in exitTicketCreationViewModel.ExitTickets)
            //        {
            //            App.Network.ProjectorProxy.AddNewPage(CLPServiceAgent.Instance.Zip(ObjectSerializer.ToString(exitTicket)), 999);
            //        }
            //    }
            //    catch(Exception)
            //    {
            //    }
            //}

            ////send an exit ticket to each student
            //if(App.MainWindowViewModel.AvailableUsers.Any())
            //{
            //    Parallel.ForEach(App.MainWindowViewModel.AvailableUsers,
            //                        student =>
            //                        {
            //                            try
            //                            {
            //                                var binding = new NetTcpBinding
            //                                {
            //                                    Security =
            //                                    {
            //                                        Mode = SecurityMode.None
            //                                    }
            //                                };
            //                                var studentProxy = ChannelFactory<IStudentContract>.CreateChannel(binding, new EndpointAddress(student.CurrentMachineAddress));
            //                                CLPPage correctExitTicket = exitTicketCreationViewModel.ExitTickets.FirstOrDefault(x => x.DifferentiationLevel == student.TempDifferentiationGroup);
            //                                if(correctExitTicket == null)
            //                                {
            //                                    correctExitTicket = exitTicketCreationViewModel.ExitTickets.First();
            //                                }
            //                                //TODO: The number 999 is used in place of "infinity".
            //                                //Also I'm doing the serialization step per-student instead of per-exit-ticket which'll be somewhat slower.
            //                                studentProxy.AddNewPage(CLPServiceAgent.Instance.Zip(ObjectSerializer.ToString(correctExitTicket)), 999);
            //                                (studentProxy as ICommunicationObject).Close();
            //                            }
            //                            catch(Exception ex)
            //                            {
            //                                Console.WriteLine(ex.Message);
            //                            }
            //                        });
            //}
            //else
            //{
            //    Logger.Instance.WriteToLog("No Students Found");
            //}
        }

        #endregion //Testing

        #region Page Commands

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

        #endregion //Commands
    }
}
