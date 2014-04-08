using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
   class CLPAnimationPageViewModel : ACLPPageBaseViewModel
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

       public override string Title { get { return "AnimationPageVM"; } }

       #region Overrides of ViewModelBase

       protected override void OnClosing()
       {
           _isClosing = true;
           StopAnimation();
           base.OnClosing();
       }

       private bool _isClosing = false;

       #endregion

       #endregion //Constructor

       #region Properties

       /// <summary>
       /// Multiplier for playback speed of animation.
       /// </summary>
       public double CurrentPlaybackSpeed
       {
           get { return GetValue<double>(CurrentPlaybackSpeedProperty); }
           set { SetValue(CurrentPlaybackSpeedProperty, value); }
       }

       public static readonly PropertyData CurrentPlaybackSpeedProperty = RegisterProperty("CurrentPlaybackSpeed", typeof(double), 1.0);      

       /// <summary>
       /// If an animation is currently recording or not.
       /// </summary>
       public bool IsRecording
       {
           get { return GetValue<bool>(IsRecordingProperty); }
           set { SetValue(IsRecordingProperty, value); }
       }

       public static readonly PropertyData IsRecordingProperty = RegisterProperty("IsRecording", typeof(bool), false);

       /// <summary>
       /// If an animation is currently playing or not.
       /// </summary>
       public bool IsPlaying
       {
           get { return GetValue<bool>(IsPlayingProperty); }
           set { SetValue(IsPlayingProperty, value); }
       }

       public static readonly PropertyData IsPlayingProperty = RegisterProperty("IsPlaying", typeof(bool), false);

       #endregion //Properties

       #region Commands

       private PageInteractionMode _oldPageInteractionMode = PageInteractionMode.Pen;

       /// <summary>
       /// Begins recording page interations for use in an animation.
       /// </summary>
       public Command RecordAnimationCommand { get; private set; }

       private void OnRecordAnimationCommandExecute()
       {
           IsPlaying = false;
           if(IsRecording)
           {
               StopAnimation();
               return;
           }

           IsRecording = true;
           // TODO: Entities
           //if(PageHistory.IsAnimation)
           //{
           //    var eraseRedoAnimation = MessageBox.Show("Do you wish to Record from this spot? If you do, any animation after this point will be erased!",
           //                                             "", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
           //    if(!eraseRedoAnimation)
           //    {
           //        return;
           //    }

           //    PageHistory.RedoItems.Clear();
           //    var firstUndoItem = PageHistory.UndoItems.FirstOrDefault() as CLPAnimationIndicator;
           //    if(firstUndoItem != null && firstUndoItem.AnimationIndicatorType == AnimationIndicatorType.Stop)
           //    {
           //        PageHistory.UndoItems.Remove(firstUndoItem);
           //    }
           //    PageHistory.UpdateTicks();
           //}
           //else
           //{
           //    PageHistory.AddHistoryItem(new CLPAnimationIndicator(Page, AnimationIndicatorType.Record));
           //}
       }

       /// <summary>
       /// Stops Recording, if recording. Stops Playing, if playing. Then rewinds animation to beginning.
       /// </summary>
       public Command RewindAnimationCommand { get; private set; }

       private void OnRewindAnimationCommandExecute()
       {
           if(IsPlaying || IsRecording)
           {
               StopAnimation();
           }

           // TODO: Entities
           //if(!PageHistory.IsAnimation) 
           //{
           //    return;
           //}

           //_oldPageInteractionMode = PageInteractionMode;
           //PageInteractionMode = PageInteractionMode.None;

           //IsPlaying = true;
           //while(PageHistory.UndoItems.Any())
           //{
           //    var clpAnimationIndicator = PageHistory.UndoItems.First() as CLPAnimationIndicator;
           //    PageHistory.Undo();
           //    if(clpAnimationIndicator != null && clpAnimationIndicator.AnimationIndicatorType == AnimationIndicatorType.Record)
           //    {
           //        break;
           //    }
           //}
           IsPlaying = false;
           PageInteractionMode = _oldPageInteractionMode;
       }

       /// <summary>
       /// Plays the animation in the History.
       /// </summary>
       public Command PlayAnimationCommand { get; private set; }

       private void OnPlayAnimationCommandExecute()
       {
           // TODO: Entities
           //if(IsRecording)
           //{
           //    return;
           //}

           //if(IsPlaying)
           //{
           //    IsPlaying = false;
           //    return;
           //}
           //
           //var t = new Thread(() =>
           //                       {
           //                           InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
           //                           IsPlaying = true;
           //                           _oldPageInteractionMode = (PageInteractionMode == PageInteractionMode.None) ? PageInteractionMode.Pen : PageInteractionMode;
           //                           PageInteractionMode = PageInteractionMode.None;

           //                           while(PageHistory.RedoItems.Any() && IsPlaying)
           //                           {
           //                               var historyItemAnimationDelay = Convert.ToInt32(Math.Round(Page.PageHistory.CurrentAnimationDelay / CurrentPlaybackSpeed));
           //                               Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
           //                                                                     (DispatcherOperationCallback)delegate
           //                                                                                                      {
           //                                                                                                          PageHistory.Redo(true);
           //                                                                                                          return null;
           //                                                                                                      }, null);
           //                               Thread.Sleep(historyItemAnimationDelay);
           //                           }
           //                           IsPlaying = false;
           //                           PageInteractionMode = _oldPageInteractionMode;
           //                           InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
           //                       });

           //t.Start();
       }

       private void StopAnimation()
       {
           PageInteractionMode = _oldPageInteractionMode;
           // TODO: Entities
           //if(IsRecording)
           //{
           //    PageHistory.AddHistoryItem(new CLPAnimationIndicator(Page, AnimationIndicatorType.Stop)); 
           //}
           IsPlaying = false;
           IsRecording = false;
       }

       /// <summary>
       /// Plays the animation through to the specified point.
       /// </summary>
       public Command<RoutedPropertyChangedEventArgs<double>> SliderChangedCommand { get; private set; }

       private void OnSliderChangedCommandExecute(RoutedPropertyChangedEventArgs<double> e)
       {
           // TODO: Entities
           //if(IsPlaying || IsRecording || _isClosing)
           //{
           //    return;
           //}
           //PageHistory.MoveToHistoryPoint(e.OldValue, e.NewValue);
       }

       #endregion //Commands
   }
}
