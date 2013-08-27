using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
   class CLPAnimationPageViewModel : ACLPPageBaseViewModel
   {
       #region Constructor

       public CLPAnimationPageViewModel(CLPAnimationPage page)
           : base(page)
       {
           RecordAnimationCommand = new Command(OnRecordAnimationCommandExecute);
           RewindAnimationCommand = new Command(OnRewindAnimationCommandExecute);
           PlayAnimationCommand = new Command(OnPlayAnimationCommandExecute);
           StopAnimationCommand = new Command(OnStopAnimationCommandExecute);
           SliderChangedCommand = new Command<RoutedPropertyChangedEventArgs<double>>(OnSliderChangedCommandExecute);
       }

       public override string Title { get { return "AnimationPageVM"; } }

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

       #endregion //Properties

       #region Commands

       private PageInteractionMode _oldPageInteractionMode = PageInteractionMode.Pen;
       private bool _isPaused = true;
       private bool _isRecording = false;

       /// <summary>
       /// Begins recording page interations for use in an animation.
       /// </summary>
       public Command RecordAnimationCommand { get; private set; }

       private void OnRecordAnimationCommandExecute()
       {
           if(_isRecording)
           {
               return;
           }

           _isRecording = true;
           if(PageHistory.IsAnimation)
           {
               var eraseRedoAnimation = MessageBox.Show("Do you wish to Record from this spot? If you do, any animation after this point will be erased!",
                                                        "", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
               if(!eraseRedoAnimation)
               {
                   return;
               }

               PageHistory.RedoItems.Clear();
               var firstUndoItem = PageHistory.UndoItems.FirstOrDefault() as CLPAnimationIndicator;
               if(firstUndoItem != null && firstUndoItem.AnimationIndicatorType == AnimationIndicatorType.Stop)
               {
                   PageHistory.UndoItems.Remove(firstUndoItem);
               }
               PageHistory.UpdateTicks();
           }
           else
           {
               PageHistory.AddHistoryItem(new CLPAnimationIndicator(Page, AnimationIndicatorType.Record));
           }
       }

       /// <summary>
       /// Stops Recording, if recording, then rewinds animation to beginning.
       /// </summary>
       public Command RewindAnimationCommand { get; private set; }

       private void OnRewindAnimationCommandExecute()
       {
           if(!_isPaused || _isRecording)
           {
               OnStopAnimationCommandExecute();
           }

           if(!PageHistory.IsAnimation) 
           {
               return;
           }

           _oldPageInteractionMode = PageInteractionMode;
           PageInteractionMode = PageInteractionMode.None;

           _isPaused = false;
           while(PageHistory.UndoItems.Any())
           {
               var clpAnimationIndicator = PageHistory.UndoItems.First() as CLPAnimationIndicator;
               PageHistory.Undo();
               if(clpAnimationIndicator != null && clpAnimationIndicator.AnimationIndicatorType == AnimationIndicatorType.Record)
               {
                   break;
               }
           }
           _isPaused = true;
           PageInteractionMode = _oldPageInteractionMode;
       }

       /// <summary>
       /// Plays the animation in the History.
       /// </summary>
       public Command PlayAnimationCommand { get; private set; }

       private void OnPlayAnimationCommandExecute()
       {
           if(_isRecording || !_isPaused)
           {
               return;
           }

           var t = new Thread(() =>
                                  {
                                      InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
                                      _isPaused = false;
                                      _oldPageInteractionMode = (PageInteractionMode == PageInteractionMode.None) ? PageInteractionMode.Pen : PageInteractionMode;
                                      PageInteractionMode = PageInteractionMode.None;

                                      while(PageHistory.RedoItems.Any() && !_isPaused)
                                      {
                                          var historyItemAnimationDelay = Convert.ToInt32(Math.Round(Page.PageHistory.CurrentAnimationDelay / CurrentPlaybackSpeed));
                                          Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                                (DispatcherOperationCallback)delegate
                                                                                                                 {
                                                                                                                     PageHistory.Redo(true);
                                                                                                                     return null;
                                                                                                                 }, null);
                                          Thread.Sleep(historyItemAnimationDelay);
                                      }
                                      _isPaused = true;
                                      PageInteractionMode = _oldPageInteractionMode;
                                      InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
                                  });

           t.Start();
       }

       /// <summary>
       /// Stops the animation in the History.
       /// </summary>
       public Command StopAnimationCommand { get; private set; }

       private void OnStopAnimationCommandExecute()
       {
           PageInteractionMode = _oldPageInteractionMode;
           if(_isRecording)
           {
               PageHistory.AddHistoryItem(new CLPAnimationIndicator(Page, AnimationIndicatorType.Stop)); 
           }
           _isPaused = true;
           _isRecording = false;
       }

       /// <summary>
       /// Plays the animation through to the specified point.
       /// </summary>
       public Command<RoutedPropertyChangedEventArgs<double>> SliderChangedCommand { get; private set; }

       private void OnSliderChangedCommandExecute(RoutedPropertyChangedEventArgs<double> e)
       {
           if(!_isPaused || _isRecording)
           {
               return;
           }
           PageHistory.MoveToHistoryPoint(e.OldValue, e.NewValue);
       }

       #endregion //Commands
   }
}
