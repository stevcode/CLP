using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Catel;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class AnimationControlRibbonViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly IPageInteractionService _pageInteractionService;
        private readonly IRoleService _roleService;

        #region Constructor

        public AnimationControlRibbonViewModel(IDataService dataService, IPageInteractionService pageInteractionService, IRoleService roleService)
        {
            Argument.IsNotNull(() => dataService);
            Argument.IsNotNull(() => pageInteractionService);
            Argument.IsNotNull(() => roleService);

            _dataService = dataService;
            _pageInteractionService = pageInteractionService;
            _roleService = roleService;

            CurrentPage = _dataService.CurrentPage;

            InitializeEventSubscriptions();
            InitializeCommands();
        }

        #endregion //Constructor

        #region Model

        [Model(SupportIEditableObject = false)]
        public CLPPage CurrentPage
        {
            get => GetValue<CLPPage>(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        #endregion //Model

        #region Bindings

        /// <summary>Multiplier for playback speed of animation.</summary>
        public double CurrentPlaybackSpeed
        {
            get => GetValue<double>(CurrentPlaybackSpeedProperty);
            set => SetValue(CurrentPlaybackSpeedProperty, value);
        }

        public static readonly PropertyData CurrentPlaybackSpeedProperty = RegisterProperty("CurrentPlaybackSpeed", typeof(double), 1.0);

        /// <summary>If an animation is currently recording or not.</summary>
        public bool IsRecording
        {
            get => GetValue<bool>(IsRecordingProperty);
            set => SetValue(IsRecordingProperty, value);
        }

        public static readonly PropertyData IsRecordingProperty = RegisterProperty("IsRecording", typeof(bool), false);

        /// <summary>If an animation is currently playing or not.</summary>
        public bool IsPlaying
        {
            get => GetValue<bool>(IsPlayingProperty);
            set => SetValue(IsPlayingProperty, value);
        }

        public static readonly PropertyData IsPlayingProperty = RegisterProperty("IsPlaying", typeof(bool), false);

        /// <summary>Forces Playback on non-animation pages.</summary>
        public bool IsNonAnimationPlaybackEnabled
        {
            get => GetValue<bool>(IsNonAnimationPlaybackEnabledProperty);
            set => SetValue(IsNonAnimationPlaybackEnabledProperty, value);
        }

        public static readonly PropertyData IsNonAnimationPlaybackEnabledProperty =
            RegisterProperty("IsNonAnimationPlaybackEnabled", typeof(bool), false, OnIsNonAnimationPlaybackEnabledChanged);

        private static void OnIsNonAnimationPlaybackEnabledChanged(object sender, AdvancedPropertyChangedEventArgs advancedPropertyChangedEventArgs)
        {
            var animationControlRibbonViewModel = sender as AnimationControlRibbonViewModel;
            if (animationControlRibbonViewModel == null)
            {
                return;
            }

            animationControlRibbonViewModel._isClosing = true;
            animationControlRibbonViewModel.CurrentPage.History.IsNonAnimationPlaybackEnabled = animationControlRibbonViewModel.IsNonAnimationPlaybackEnabled;
            animationControlRibbonViewModel.RaisePropertyChanged(nameof(IsPlaybackEnabled));
            animationControlRibbonViewModel.RaisePropertyChanged(nameof(IsVisible));
            animationControlRibbonViewModel._isClosing = false;
        }

        public bool IsPlaybackEnabled
        {
            get
            {
                if (CurrentPage == null)
                {
                    return false;
                }

                return CurrentPage.History.IsAnimation || IsNonAnimationPlaybackEnabled;
            }
        }

        public bool IsVisible
        {
            get
            {
                if (CurrentPage == null)
                {
                    return false;
                }

                return CurrentPage.PageType == PageTypes.Animation || IsNonAnimationPlaybackEnabled;
            }
        }

        public Visibility ResearcherOnlyVisibility => _roleService.Role == ProgramRoles.Researcher ? Visibility.Visible : Visibility.Collapsed;

        #endregion // Bindings

        #region Events

        private void InitializeEventSubscriptions()
        {
            _roleService.RoleChanged += _roleService_RoleChanged;
            _dataService.CurrentPageChanged += _dataService_CurrentPageChanged;
        }

        private void _roleService_RoleChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(ResearcherOnlyVisibility));
        }

        private void _dataService_CurrentPageChanged(object sender, EventArgs e)
        {
            _isPageChangingHack = true;
            RaisePropertyChanged(nameof(IsPlaybackEnabled));
            RaisePropertyChanged(nameof(IsVisible));

            var previousPage = CurrentPage;

            CurrentPage = _dataService.CurrentPage;
            if (CurrentPage != null)
            {
                CurrentPage.History.IsNonAnimationPlaybackEnabled = IsNonAnimationPlaybackEnabled;
            }

            if (previousPage == null ||
                !IsPlaying)
            {
                return;
            }

            Stop(previousPage);
        }

        #endregion // Events

        #region ViewModelBase Overrides

        private bool _isClosing;
        private bool _isPageChangingHack;

        protected override async Task OnClosingAsync()
        {
            _roleService.RoleChanged -= _roleService_RoleChanged;
            _dataService.CurrentPageChanged -= _dataService_CurrentPageChanged;
            _isClosing = true;
            Stop(CurrentPage);
            await base.OnClosingAsync();
        }

        #endregion // ViewModelBase Overrides

        #region Commands

        private void InitializeCommands()
        {
            SlowUndoCommand = new Command(OnSlowUndoCommandExecute, OnSlowUndoCanExecute);
            SlowRedoCommand = new Command(OnSlowRedoCommandExecute, OnSlowRedoCanExecute);
            RecordAnimationCommand = new Command(OnRecordAnimationCommandExecute);
            RewindAnimationCommand = new Command(OnRewindAnimationCommandExecute);
            PlayAnimationCommand = new Command(OnPlayAnimationCommandExecute);
            PlayBackwardsCommand = new Command(OnPlayBackwardsCommandExecute);
            SliderChangedCommand = new Command<RoutedPropertyChangedEventArgs<double>>(OnSliderChangedCommandExecute);
            MakeAnimationFromHistoryCommand = new Command(OnMakeAnimationFromHistoryCommandExecute);
            ClearAnimationPageCommand = new Command(OnClearAnimationPageCommandExecute);
            UndoCommand = new Command(OnUndoCommandExecute, OnUndoCanExecute);
        }

        private PageInteractionModes _oldPageInteractionMode = PageInteractionModes.Draw;

        #region History Commands

        /// <summary>Undoes the last action.</summary>
        public Command SlowUndoCommand { get; private set; }

        private void OnSlowUndoCommandExecute()
        {
            SlowUndo(CurrentPage);
        }

        private bool OnSlowUndoCanExecute()
        {
            var page = CurrentPage;
            return page != null && page.History.CanUndo && !IsPlaying && !IsRecording;
        }

        public void SlowUndo(CLPPage page)
        {
            if (IsRecording ||
                IsPlaying ||
                page == null ||
                _pageInteractionService == null)
            {
                return;
            }

            page.History.IsAnimating = true;
            IsPlaying = true;
            _oldPageInteractionMode = _pageInteractionService.CurrentPageInteractionMode == PageInteractionModes.None
                                          ? PageInteractionModes.Draw
                                          : _pageInteractionService.CurrentPageInteractionMode;
            _pageInteractionService.SetNoInteractionMode();

            var t = new Thread(() =>
                               {
                                   var undoAction = page.History.UndoActions.First() as IHistoryBatch;
                                   var ticksToUndo = undoAction == null ? 1 : undoAction.CurrentBatchTickIndex;
                                   var currentTick = ticksToUndo;
                                   while (currentTick > 0 && IsPlaying)
                                   {
                                       currentTick--;
                                       var historyActionAnimationDelay = Convert.ToInt32(Math.Round(page.History.CurrentAnimationDelay / CurrentPlaybackSpeed));
                                       Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                             (DispatcherOperationCallback)delegate
                                                                                                          {
                                                                                                              page.History.Undo(true);
                                                                                                              return null;
                                                                                                          },
                                                                             null);
                                       Thread.Sleep(historyActionAnimationDelay);
                                   }

                                   Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                         (DispatcherOperationCallback)delegate
                                                                                                      {
                                                                                                          IsPlaying = false;
                                                                                                          _pageInteractionService.SetPageInteractionMode(_oldPageInteractionMode);
                                                                                                          page.History.IsAnimating = false;
                                                                                                          return null;
                                                                                                      },
                                                                         null);
                               });

            t.Start();
        }

        /// <summary>Redoes the last undone action.</summary>
        public Command SlowRedoCommand { get; private set; }

        private void OnSlowRedoCommandExecute()
        {
            SlowRedo(CurrentPage);
        }

        private bool OnSlowRedoCanExecute()
        {
            var page = CurrentPage;
            return page != null && page.History.CanRedo && !IsPlaying && !IsRecording;
        }

        public void SlowRedo(CLPPage page)
        {
            if (IsRecording ||
                IsPlaying ||
                page == null ||
                _pageInteractionService == null)
            {
                return;
            }

            page.History.IsAnimating = true;
            IsPlaying = true;
            _oldPageInteractionMode = _pageInteractionService.CurrentPageInteractionMode == PageInteractionModes.None
                                          ? PageInteractionModes.Draw
                                          : _pageInteractionService.CurrentPageInteractionMode;
            _pageInteractionService.SetNoInteractionMode();

            var t = new Thread(() =>
                               {
                                   var redoAction = page.History.RedoActions.First() as IHistoryBatch;
                                   var ticksToRedo = redoAction == null ? 1 : redoAction.NumberOfBatchTicks - redoAction.CurrentBatchTickIndex;
                                   var currentTick = ticksToRedo;
                                   while (currentTick > 0 && IsPlaying)
                                   {
                                       currentTick--;
                                       var historyActionAnimationDelay = Convert.ToInt32(Math.Round(page.History.CurrentAnimationDelay / CurrentPlaybackSpeed));
                                       Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                             (DispatcherOperationCallback)delegate
                                                                                                          {
                                                                                                              page.History.Redo(true);
                                                                                                              return null;
                                                                                                          },
                                                                             null);
                                       Thread.Sleep(historyActionAnimationDelay);
                                   }

                                   Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                         (DispatcherOperationCallback)delegate
                                                                                                      {
                                                                                                          IsPlaying = false;
                                                                                                          _pageInteractionService.SetPageInteractionMode(_oldPageInteractionMode);
                                                                                                          page.History.IsAnimating = false;
                                                                                                          return null;
                                                                                                      },
                                                                         null);
                               });

            t.Start();
        }

        #endregion //History Commands

        /// <summary>Begins recording page interations for use in an animation.</summary>
        public Command RecordAnimationCommand { get; private set; }

        private void OnRecordAnimationCommandExecute()
        {
            Record(CurrentPage);
        }

        public void Record(CLPPage page)
        {
            if (IsNonAnimationPlaybackEnabled || page == null)
            {
                return;
            }

            IsPlaying = false;
            page.History.IsAnimating = false;
            if (IsRecording)
            {
                Stop(page);
                RaisePropertyChanged(nameof(IsPlaybackEnabled));
                return;
            }

            IsRecording = true;
            if (page.History.IsAnimation)
            {
                var eraseRedoAnimation = MessageBox.Show("Do you want to start recording from here?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                if (!eraseRedoAnimation)
                {
                    IsRecording = false;
                    RaisePropertyChanged(nameof(IsPlaybackEnabled));
                    return;
                }

                page.History.RedoActions.Clear();
                var firstUndoAction = page.History.UndoActions.FirstOrDefault() as AnimationIndicatorHistoryAction;
                if (firstUndoAction != null &&
                    firstUndoAction.AnimationIndicatorType == AnimationIndicatorType.Stop)
                {
                    page.History.UndoActions.Remove(firstUndoAction);
                }
                page.History.UpdateTicks();
                RaisePropertyChanged(nameof(IsPlaybackEnabled));
            }
            else
            {
                page.History.UndoActions.Clear();
                page.History.RedoActions.Clear();
                page.History.AddHistoryAction(new AnimationIndicatorHistoryAction(page, App.MainWindowViewModel.CurrentUser, AnimationIndicatorType.Record));
                RaisePropertyChanged(nameof(IsPlaybackEnabled));
            }
        }

        /// <summary>Stops Recording, if recording. Stops Playing, if playing. Then rewinds animation to beginning.</summary>
        public Command RewindAnimationCommand { get; private set; }

        private void OnRewindAnimationCommandExecute()
        {
            Rewind(CurrentPage);
        }

        public void Rewind(CLPPage page)
        {
            if (page == null ||
                _pageInteractionService == null)
            {
                return;
            }

            if (IsPlaying || IsRecording)
            {
                Stop(page);
            }

            if (!page.History.IsAnimation &&
                !IsNonAnimationPlaybackEnabled)
            {
                return;
            }

            _oldPageInteractionMode = _pageInteractionService.CurrentPageInteractionMode == PageInteractionModes.None
                                          ? PageInteractionModes.Draw
                                          : _pageInteractionService.CurrentPageInteractionMode;
            _pageInteractionService.SetNoInteractionMode();
            page.History.IsAnimating = true;

            IsPlaying = true;
            while (page.History.UndoActions.Any())
            {
                var animationIndicator = page.History.UndoActions.First() as AnimationIndicatorHistoryAction;
                page.History.Undo();
                if (animationIndicator != null &&
                    animationIndicator.AnimationIndicatorType == AnimationIndicatorType.Record &&
                    !IsNonAnimationPlaybackEnabled)
                {
                    break;
                }
            }
            IsPlaying = false;
            _pageInteractionService.SetPageInteractionMode(_oldPageInteractionMode);
            page.History.IsAnimating = false;
        }

        /// <summary>Plays the animation in the History.</summary>
        public Command PlayAnimationCommand { get; private set; }

        private void OnPlayAnimationCommandExecute()
        {
            Play(CurrentPage);
        }

        public void Play(CLPPage page)
        {
            if (IsRecording ||
                page == null ||
                _pageInteractionService == null)
            {
                return;
            }

            if (IsPlaying)
            {
                IsPlaying = false;
                return;
            }

            page.History.IsAnimating = true;
            IsPlaying = true;
            _oldPageInteractionMode = _pageInteractionService.CurrentPageInteractionMode == PageInteractionModes.None
                                          ? PageInteractionModes.Draw
                                          : _pageInteractionService.CurrentPageInteractionMode;
            _pageInteractionService.SetNoInteractionMode();

            var t = new Thread(() =>
                               {
                                   while (page.History.RedoActions.Any() && IsPlaying)
                                   {
                                       var historyActionAnimationDelay = Convert.ToInt32(Math.Round(page.History.CurrentAnimationDelay / CurrentPlaybackSpeed));
                                       Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                             (DispatcherOperationCallback)delegate
                                                                                                          {
                                                                                                              page.History.Redo(true);
                                                                                                              return null;
                                                                                                          },
                                                                             null);
                                       Thread.Sleep(historyActionAnimationDelay);
                                   }

                                   Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                         (DispatcherOperationCallback)delegate
                                                                                                      {
                                                                                                          IsPlaying = false;
                                                                                                          _pageInteractionService.SetPageInteractionMode(_oldPageInteractionMode);
                                                                                                          page.History.IsAnimating = false;
                                                                                                          return null;
                                                                                                      },
                                                                         null);
                               });

            t.Start();
        }

        public void Stop(CLPPage page)
        {
            if (page == null ||
                _pageInteractionService == null)
            {
                return;
            }

            if (IsRecording)
            {
                page.History.AddHistoryAction(new AnimationIndicatorHistoryAction(page, App.MainWindowViewModel.CurrentUser, AnimationIndicatorType.Stop));
            }
            IsPlaying = false;
            IsRecording = false;
            page.History.IsAnimating = false;

            if (_pageInteractionService.CurrentPageInteractionMode == PageInteractionModes.None)
            {
                _pageInteractionService.SetPageInteractionMode(_oldPageInteractionMode);
            }
        }

        /// <summary>
        ///     Plays the animation in reverse
        /// </summary>
        public Command PlayBackwardsCommand { get; private set; }

        private void OnPlayBackwardsCommandExecute()
        {
            PlayBackwards(CurrentPage);
        }

        private void PlayBackwards(CLPPage page)
        {
            if (IsRecording ||
                page == null ||
                _pageInteractionService == null)
            {
                return;
            }

            if (IsPlaying)
            {
                IsPlaying = false;
                return;
            }

            page.History.IsAnimating = true;
            IsPlaying = true;
            _oldPageInteractionMode = _pageInteractionService.CurrentPageInteractionMode == PageInteractionModes.None
                                          ? PageInteractionModes.Draw
                                          : _pageInteractionService.CurrentPageInteractionMode;
            _pageInteractionService.SetNoInteractionMode();

            var t = new Thread(() =>
                               {
                                   while (page.History.UndoActions.Any() && IsPlaying)
                                   {
                                       var historyActionAnimationDelay = Convert.ToInt32(Math.Round(page.History.CurrentAnimationDelay / CurrentPlaybackSpeed));
                                       Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                             (DispatcherOperationCallback)delegate
                                                                                                          {
                                                                                                              page.History.Undo(true);
                                                                                                              return null;
                                                                                                          },
                                                                             null);
                                       Thread.Sleep(historyActionAnimationDelay);
                                   }

                                   Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                         (DispatcherOperationCallback)delegate
                                                                                                      {
                                                                                                          IsPlaying = false;
                                                                                                          _pageInteractionService.SetPageInteractionMode(_oldPageInteractionMode);
                                                                                                          page.History.IsAnimating = false;
                                                                                                          return null;
                                                                                                      },
                                                                         null);
                               });

            t.Start();
        }

        /// <summary>Plays the animation through to the specified point.</summary>
        public Command<RoutedPropertyChangedEventArgs<double>> SliderChangedCommand { get; private set; }

        private void OnSliderChangedCommandExecute(RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsPlaying ||
                IsRecording ||
                _isClosing ||
                _pageInteractionService == null ||
                CurrentPage == null)
            {
                return;
            }

            if (_isPageChangingHack)
            {
                _isPageChangingHack = false;
                return;
            }

            //CurrentPage.History.MoveToHistoryPoint(e.OldValue, e.NewValue);
        }

        /// <summary>
        ///     SUMMARY
        /// </summary>
        public Command MakeAnimationFromHistoryCommand { get; private set; }

        private void OnMakeAnimationFromHistoryCommandExecute()
        {
            if (MessageBox.Show("Do you want to make an animation of what you just did?", "Forgot to Record?", MessageBoxButton.YesNo) == MessageBoxResult.No ||
                IsRecording ||
                IsPlaying)
            {
                return;
            }

            CurrentPage.History.ForceIntoAnimation(CurrentPage, App.MainWindowViewModel.CurrentUser);
            RaisePropertyChanged(nameof(IsPlaybackEnabled));
        }

        /// <summary>
        ///     Clears the page of the animation and all ink and pageObjects that aren't background.
        /// </summary>
        public Command ClearAnimationPageCommand { get; private set; }

        private void OnClearAnimationPageCommandExecute()
        {
            if (MessageBox.Show("Are you sure you want clear the page of everything?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            var pageObjectsToDelete = CurrentPage.PageObjects.Where(p => p.CreatorID == App.MainWindowViewModel.CurrentUser.ID).ToList();
            foreach (var pageObject in pageObjectsToDelete)
            {
                CurrentPage.PageObjects.Remove(pageObject);
            }

            CurrentPage.InkStrokes.Clear();
            CurrentPage.History.ClearHistory();
            RaisePropertyChanged(nameof(IsPlaybackEnabled));
        }

        /// <summary>
        /// </summary>
        public Command UndoCommand { get; private set; }

        private void OnUndoCommandExecute()
        {
            CurrentPage.History.Undo();
            CurrentPage.History.RedoActions.Clear();
        }

        private bool OnUndoCanExecute()
        {
            return !CurrentPage.History.RedoActions.Any() && CurrentPage.History.UndoActions.Any();
        }

        #endregion //Commands
    }
}