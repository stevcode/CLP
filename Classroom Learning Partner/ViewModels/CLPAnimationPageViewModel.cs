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
       }

       public override string Title { get { return "AnimationPageVM"; } }

       #endregion //Constructor

       #region Properties

       /// <summary>
       /// SUMMARY
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
       private bool _isPaused = false;

       /// <summary>
       /// Begins recording page interations for use in an animation.
       /// </summary>
       public Command RecordAnimationCommand { get; private set; }

       private void OnRecordAnimationCommandExecute()
       {
           
       }

       /// <summary>
       /// Stops Recording, if recording, then rewinds animation to beginning.
       /// </summary>
       public Command RewindAnimationCommand { get; private set; }

       private void OnRewindAnimationCommandExecute()
       {
           OnStopAnimationCommandExecute();
           _isPaused = false;
           _oldPageInteractionMode = PageInteractionMode;
           PageInteractionMode = PageInteractionMode.None;

           while(Page.PageHistory.UndoItems.Any())
           {
               Page.PageHistory.Undo();
           }
           PageInteractionMode = _oldPageInteractionMode;
       }

       /// <summary>
       /// Plays the animation in the History.
       /// </summary>
       public Command PlayAnimationCommand { get; private set; }

       private void OnPlayAnimationCommandExecute()
       {
           var t = new Thread(() =>
                                  {
                                      //InkStrokes.StrokesChanged -= InkStrokes_StrokesChanged;
                                      _isPaused = false;
                                      _oldPageInteractionMode = PageInteractionMode;
                                      PageInteractionMode = PageInteractionMode.None;

                                      while(Page.PageHistory.RedoItems.Any() && !_isPaused)
                                      {
                                          var historyItemAnimationDelay = Convert.ToInt32(Math.Round(Page.PageHistory.CurrentAnimationDelay / CurrentPlaybackSpeed));
                                          Application.Current.Dispatcher.Invoke(DispatcherPriority.DataBind,
                                                                                (DispatcherOperationCallback)delegate
                                                                                                                 {
                                                                                                                     Page.PageHistory.Redo(true);
                                                                                                                     return null;
                                                                                                                 }, null);
                                          Thread.Sleep(historyItemAnimationDelay);
                                      }

                                      PageInteractionMode = _oldPageInteractionMode;
                                    //  InkStrokes.StrokesChanged += InkStrokes_StrokesChanged;
                                  });

           t.Start();
       }

       /// <summary>
       /// Stops the animation in the History.
       /// </summary>
       public Command StopAnimationCommand { get; private set; }

       private void OnStopAnimationCommandExecute()
       {
           _isPaused = true;
       }

       #region OldStuff
       public void Slider_ValueChanged_1b(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
       {
         
               //Stack<CLPHistoryItem> past = proofPageHistory1.MetaPast;
               //Stack<CLPHistoryItem> future = proofPageHistory1.Future;
               //double pastCount = past.Count;
               //double futureCount = future.Count;
               //double oldpos = pastCount;
               //double newpos = Math.Round((e.NewValue * (pastCount + futureCount)) / 100);
               //int diff = Convert.ToInt32(newpos - oldpos);
               //int diffabs = Math.Abs(diff);

               //if(diffabs > 0)
               //{
               //    if(diff < 0)
               //    {
               //        PlayProof(CLPProofHistory.CLPProofPageAction.Play, -1, 0, 0, diffabs);
               //    }
               //    else if(diff > 0)
               //    {
               //        PlayProof(CLPProofHistory.CLPProofPageAction.Play, 1, 0, 0, diffabs);
               //    }
               //    else
               //    {
               //        return;
               //    }
               //}   
       }

       #endregion

       #endregion //Commands

       #region Methods

       public void updateProgress()
       {
           //try
           //{
           //    //CLPAnimationPage page = (CLPAnimationPage)(MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
           //    CLPProofHistory proofPageHistory1 = (CLPProofHistory)PageHistory;
           //    double FutureItemsNumber = proofPageHistory1.Future.Count;
           //    double pastItemsNumber = proofPageHistory1.MetaPast.Count;
           //    double totalItemsNumber = FutureItemsNumber + pastItemsNumber;

           //    if(totalItemsNumber == 0)
           //    {

           //        ProofPresent = "Hidden";
           //        ProofProgressCurrent = 0;
           //        SliderProgressCurrent = 0;
           //        return;
           //    }
           //    else
           //    {
           //        ProofPresent = "Visible";
           //        ProofProgressCurrent =

           //            (pastItemsNumber * PageWidth * 0.7175) /
           //            totalItemsNumber;
           //        SliderProgressCurrent = (pastItemsNumber * 100) /
           //            totalItemsNumber;

           //    }

           //    if(proofPageHistory1.ProofPageAction.Equals(CLPProofHistory.CLPProofPageAction.Record))
           //    {
           //        ProofProgressVisible = "Hidden";
           //    }
           //    else
           //    {
           //        ProofProgressVisible = "Visible";

           //    }


           //}
           //catch(Exception e)
           //{
           //    Console.WriteLine(e.Message);
           //}
       }

       #endregion //Methods
   }
}
