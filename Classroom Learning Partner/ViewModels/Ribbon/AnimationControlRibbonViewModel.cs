﻿using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class AnimationControlRibbonViewModel : ViewModelBase
    {
        private IPageInteractionService _pageInteractionService;

        #region Constructor

        public AnimationControlRibbonViewModel(Notebook notebook)
        {
            Notebook = notebook;
            _pageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();

            SlowUndoCommand = new Command(OnSlowUndoCommandExecute, OnSlowUndoCanExecute);
            SlowRedoCommand = new Command(OnSlowRedoCommandExecute, OnSlowRedoCanExecute);
            RecordAnimationCommand = new Command(OnRecordAnimationCommandExecute);
            RewindAnimationCommand = new Command(OnRewindAnimationCommandExecute);
            PlayAnimationCommand = new Command(OnPlayAnimationCommandExecute);
            SliderChangedCommand = new Command<RoutedPropertyChangedEventArgs<double>>(OnSliderChangedCommandExecute);
        }

        public override string Title
        {
            get { return "AnimationControlRibbonVM"; }
        }

        #endregion //Constructor

        #region Overrides of ViewModelBase

        protected override void OnClosing()
        {
            _isClosing = true;
            Stop(CurrentPage);
            base.OnClosing();
        }

        private bool _isClosing;

        #endregion

        #region Model

        /// <summary>The Model of the ViewModel.</summary>
        [Model(SupportIEditableObject = false)]
        public Notebook Notebook
        {
            get { return GetValue<Notebook>(NotebookProperty); }
            private set { SetValue(NotebookProperty, value); }
        }

        public static readonly PropertyData NotebookProperty = RegisterProperty("Notebook", typeof (Notebook));

        /// <summary>A property mapped to a property on the Model SingleDisplay.</summary>
        [ViewModelToModel("Notebook")]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set
            {
                SetValue(CurrentPageProperty, value);
                RaisePropertyChanged("CanPlayback");
            }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof (CLPPage), null, OnCurrentPageChanged);

        private static void OnCurrentPageChanged(object sender, AdvancedPropertyChangedEventArgs advancedPropertyChangedEventArgs)
        {
            var animationControlRibbonViewModel = sender as AnimationControlRibbonViewModel;
            var previousPage = advancedPropertyChangedEventArgs.OldValue as CLPPage;
            if (animationControlRibbonViewModel == null ||
                previousPage == null ||
                !animationControlRibbonViewModel.IsPlaying)
            {
                return;
            }

            animationControlRibbonViewModel.Stop(previousPage);
        }

        #endregion //Model

        #region Properties

        /// <summary>Multiplier for playback speed of animation.</summary>
        public double CurrentPlaybackSpeed
        {
            get { return GetValue<double>(CurrentPlaybackSpeedProperty); }
            set { SetValue(CurrentPlaybackSpeedProperty, value); }
        }

        public static readonly PropertyData CurrentPlaybackSpeedProperty = RegisterProperty("CurrentPlaybackSpeed", typeof (double), 1.0);

        /// <summary>If an animation is currently recording or not.</summary>
        public bool IsRecording
        {
            get { return GetValue<bool>(IsRecordingProperty); }
            set { SetValue(IsRecordingProperty, value); }
        }

        public static readonly PropertyData IsRecordingProperty = RegisterProperty("IsRecording", typeof (bool), false);

        /// <summary>If an animation is currently playing or not.</summary>
        public bool IsPlaying
        {
            get { return GetValue<bool>(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        public static readonly PropertyData IsPlayingProperty = RegisterProperty("IsPlaying", typeof (bool), false);

        /// <summary>Toggles visibility of animation controls for researcher Replay.</summary>
        public bool IsNonAnimationPlayback
        {
            get { return GetValue<bool>(IsNonAnimationPlaybackProperty); }
            set
            {
                SetValue(IsNonAnimationPlaybackProperty, value);
                RaisePropertyChanged("CanPlayback");
            }
        }

        public static readonly PropertyData IsNonAnimationPlaybackProperty = RegisterProperty("IsNonAnimationPlayback", typeof (bool), false);

        public bool CanPlayback
        {
            get { return CurrentPage != null && (CurrentPage.History.IsAnimation || IsNonAnimationPlayback); }
        }

        #endregion //Properties

        #region Commands

        private PageInteractionModes _oldPageInteractionMode = PageInteractionModes.Draw;

        #region History Commands

        /// <summary>Undoes the last action.</summary>
        public Command SlowUndoCommand { get; private set; }

        private void OnSlowUndoCommandExecute() { CurrentPage.History.Undo(true); }

        private bool OnSlowUndoCanExecute()
        {
            var page = CurrentPage;
            return page != null && page.History.CanUndo;
        }

        /// <summary>Redoes the last undone action.</summary>
        public Command SlowRedoCommand { get; private set; }

        private void OnSlowRedoCommandExecute() { CurrentPage.History.Redo(true); }

        private bool OnSlowRedoCanExecute()
        {
            var page = CurrentPage;
            return page != null && page.History.CanRedo;
        }

        #endregion //History Commands

        /// <summary>Begins recording page interations for use in an animation.</summary>
        public Command RecordAnimationCommand { get; private set; }

        private void OnRecordAnimationCommandExecute() { Record(CurrentPage); }

        public void Record(CLPPage page)
        {
            if (IsNonAnimationPlayback || page == null)
            {
                return;
            }

            IsPlaying = false;
            page.History.IsAnimating = false;
            if (IsRecording)
            {
                Stop(page);
                return;
            }

            IsRecording = true;
            if (page.History.IsAnimation)
            {
                var eraseRedoAnimation =
                    MessageBox.Show("Do you wish to Record from this spot? If you do, any animation after this point will be erased!", "", MessageBoxButton.YesNo) ==
                    MessageBoxResult.Yes;
                if (!eraseRedoAnimation)
                {
                    return;
                }

                page.History.RedoItems.Clear();
                var firstUndoItem = page.History.UndoItems.FirstOrDefault() as AnimationIndicator;
                if (firstUndoItem != null &&
                    firstUndoItem.AnimationIndicatorType == AnimationIndicatorType.Stop)
                {
                    page.History.UndoItems.Remove(firstUndoItem);
                }
                page.History.UpdateTicks();
            }
            else
            {
                page.History.AddHistoryItem(new AnimationIndicator(page, App.MainWindowViewModel.CurrentUser, AnimationIndicatorType.Record));
            }
            RaisePropertyChanged("CanPlayback");
        }

        /// <summary>Stops Recording, if recording. Stops Playing, if playing. Then rewinds animation to beginning.</summary>
        public Command RewindAnimationCommand { get; private set; }

        private void OnRewindAnimationCommandExecute() { Rewind(CurrentPage); }

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
                !IsNonAnimationPlayback)
            {
                return;
            }

            _oldPageInteractionMode = _pageInteractionService.CurrentPageInteractionMode == PageInteractionModes.None
                                          ? PageInteractionModes.Draw
                                          : _pageInteractionService.CurrentPageInteractionMode;
            _pageInteractionService.SetNoInteractionMode();
            page.History.IsAnimating = true;

            IsPlaying = true;
            while (page.History.UndoItems.Any())
            {
                var animationIndicator = page.History.UndoItems.First() as AnimationIndicator;
                page.History.Undo();
                if (animationIndicator != null &&
                    animationIndicator.AnimationIndicatorType == AnimationIndicatorType.Record &&
                    !IsNonAnimationPlayback)
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

        private void OnPlayAnimationCommandExecute() { Play(CurrentPage); }

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
                                   while (page.History.RedoItems.Any() && IsPlaying)
                                   {
                                       var historyItemAnimationDelay = Convert.ToInt32(Math.Round(page.History.CurrentAnimationDelay / CurrentPlaybackSpeed));
                                       Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                             (DispatcherOperationCallback)delegate
                                                                                                          {
                                                                                                              page.History.Redo(true);
                                                                                                              return null;
                                                                                                          },
                                                                             null);
                                       Thread.Sleep(historyItemAnimationDelay);
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
                page.History.AddHistoryItem(new AnimationIndicator(page, App.MainWindowViewModel.CurrentUser, AnimationIndicatorType.Stop));
                RaisePropertyChanged("CanPlayback");
            }
            IsPlaying = false;
            IsRecording = false;
            page.History.IsAnimating = false;

            if (_pageInteractionService.CurrentPageInteractionMode == PageInteractionModes.None)
            {
                _pageInteractionService.SetPageInteractionMode(_oldPageInteractionMode);
            }
        }

        /// <summary>Plays the animation through to the specified point.</summary>
        public Command<RoutedPropertyChangedEventArgs<double>> SliderChangedCommand { get; private set; }

        private void OnSliderChangedCommandExecute(RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsPlaying ||
                IsRecording ||
                _isClosing)
            {
                return;
            }

            CurrentPage.History.MoveToHistoryPoint(e.OldValue, e.NewValue);
        }

        #endregion //Commands
    }
}