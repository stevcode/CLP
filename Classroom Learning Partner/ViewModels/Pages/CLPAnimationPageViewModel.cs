using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    internal class CLPAnimationPageViewModel : ACLPPageBaseViewModel
    {
        #region Constructor

        public CLPAnimationPageViewModel(CLPPage page)
            : base(page)
        {
            RecordAnimationCommand = new Command(OnRecordAnimationCommandExecute);
            RewindAnimationCommand = new Command(OnRewindAnimationCommandExecute);
            PlayAnimationCommand = new Command(OnPlayAnimationCommandExecute);
            SliderChangedCommand = new Command<RoutedPropertyChangedEventArgs<double>>(OnSliderChangedCommandExecute);
        }

        public override string Title
        {
            get { return "AnimationPageVM"; }
        }

        #region Overrides of ViewModelBase

        protected override void OnClosing()
        {
            _isClosing = true;
            StopAnimation();
            base.OnClosing();
        }

        private bool _isClosing;

        #endregion

        #endregion //Constructor

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

        #endregion //Properties

        #region Commands

        private PageInteractionModes _oldPageInteractionMode = PageInteractionModes.Draw;

        /// <summary>Begins recording page interations for use in an animation.</summary>
        public Command RecordAnimationCommand { get; private set; }

        private void OnRecordAnimationCommandExecute()
        {
            IsPlaying = false;
            History.IsAnimating = false;
            if (IsRecording)
            {
                StopAnimation();
                return;
            }

            IsRecording = true;
            if (History.IsAnimation)
            {
                var eraseRedoAnimation =
                    MessageBox.Show("Do you wish to Record from this spot? If you do, any animation after this point will be erased!", "", MessageBoxButton.YesNo) ==
                    MessageBoxResult.Yes;
                if (!eraseRedoAnimation)
                {
                    return;
                }

                History.RedoItems.Clear();
                var firstUndoItem = History.UndoItems.FirstOrDefault() as AnimationIndicator;
                if (firstUndoItem != null &&
                    firstUndoItem.AnimationIndicatorType == AnimationIndicatorType.Stop)
                {
                    History.UndoItems.Remove(firstUndoItem);
                }
                History.UpdateTicks();
            }
            else
            {
                History.AddHistoryItem(new AnimationIndicator(Page, App.MainWindowViewModel.CurrentUser, AnimationIndicatorType.Record));
            }
        }

        /// <summary>Stops Recording, if recording. Stops Playing, if playing. Then rewinds animation to beginning.</summary>
        public Command RewindAnimationCommand { get; private set; }

        private void OnRewindAnimationCommandExecute() { Rewind(this); }

        public static void Rewind(CLPAnimationPageViewModel pageViewModel)
        {
            if (pageViewModel.IsPlaying ||
                pageViewModel.IsRecording)
            {
                pageViewModel.StopAnimation();
            }

            if (!pageViewModel.History.IsAnimation)
            {
                return;
            }

            pageViewModel._oldPageInteractionMode = pageViewModel.PageInteractionService.CurrentPageInteractionMode == PageInteractionModes.None
                                                        ? PageInteractionModes.Draw
                                                        : pageViewModel.PageInteractionService.CurrentPageInteractionMode;
            pageViewModel.PageInteractionService.SetNoInteractionMode();
            pageViewModel.History.IsAnimating = true;

            pageViewModel.IsPlaying = true;
            while (pageViewModel.History.UndoItems.Any())
            {
                var animationIndicator = pageViewModel.History.UndoItems.First() as AnimationIndicator;
                pageViewModel.History.Undo();
                if (animationIndicator != null &&
                    animationIndicator.AnimationIndicatorType == AnimationIndicatorType.Record)
                {
                    break;
                }
            }
            pageViewModel.IsPlaying = false;
            pageViewModel.PageInteractionService.SetPageInteractionMode(pageViewModel._oldPageInteractionMode);
            pageViewModel.History.IsAnimating = false;
        }

        /// <summary>Plays the animation in the History.</summary>
        public Command PlayAnimationCommand { get; private set; }

        private void OnPlayAnimationCommandExecute() { Play(this); }

        public static void Play(CLPAnimationPageViewModel pageViewModel)
        {
            if (pageViewModel.IsRecording)
            {
                return;
            }

            if (pageViewModel.IsPlaying)
            {
                pageViewModel.IsPlaying = false;
                return;
            }

            pageViewModel.History.IsAnimating = true;
            pageViewModel.IsPlaying = true;
            pageViewModel._oldPageInteractionMode = pageViewModel.PageInteractionService.CurrentPageInteractionMode == PageInteractionModes.None
                                                        ? PageInteractionModes.Draw
                                                        : pageViewModel.PageInteractionService.CurrentPageInteractionMode;
            pageViewModel.PageInteractionService.SetNoInteractionMode();

            var t = new Thread(() =>
                               {
                                   while (pageViewModel.History.RedoItems.Any() &&
                                          pageViewModel.IsPlaying)
                                   {
                                       var historyItemAnimationDelay =
                                           Convert.ToInt32(Math.Round(pageViewModel.Page.History.CurrentAnimationDelay / pageViewModel.CurrentPlaybackSpeed));
                                       Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                             (DispatcherOperationCallback)delegate
                                                                                                          {
                                                                                                              pageViewModel.History.Redo(true);
                                                                                                              return null;
                                                                                                          },
                                                                             null);
                                       Thread.Sleep(historyItemAnimationDelay);
                                   }

                                   Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                         (DispatcherOperationCallback)delegate
                                                                                                      {
                                                                                                          pageViewModel.IsPlaying = false;
                                                                                                          pageViewModel.PageInteractionService.SetPageInteractionMode(pageViewModel._oldPageInteractionMode);
                                                                                                          pageViewModel.History.IsAnimating = false;
                                                                                                          return null;
                                                                                                      },
                                                                         null);
                               });

            t.Start();
        }

        private void StopAnimation() { Stop(this); }

        public static void Stop(CLPAnimationPageViewModel pageViewModel)
        {
            pageViewModel.PageInteractionService.SetPageInteractionMode(pageViewModel._oldPageInteractionMode);
            if (pageViewModel.IsRecording)
            {
                pageViewModel.History.AddHistoryItem(new AnimationIndicator(pageViewModel.Page, App.MainWindowViewModel.CurrentUser, AnimationIndicatorType.Stop));
            }
            pageViewModel.IsPlaying = false;
            pageViewModel.IsRecording = false;
            pageViewModel.History.IsAnimating = false;
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
            History.MoveToHistoryPoint(e.OldValue, e.NewValue);
        }

        #endregion //Commands
    }
}