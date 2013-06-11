using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
   class CLPProofPageViewModel : CLPPageViewModel
   {
       private static volatile object obj = new object();

       private MainWindowViewModel MainWindow
       {
           get { return App.MainWindowViewModel; }
       }

       #region Constructors

       public CLPProofPageViewModel(CLPPage page)
           : base(page)
       {
            PlayProofCommand = new Command(OnPlayProofCommandExecute);
            ReplayProofCommand = new Command(OnReplayProofCommandExecute);
            RecordProofCommand = new Command(OnRecordProofCommandExecute);
            InsertProofCommand = new Command(OnInsertProofCommandExecute);
            RewindProofCommand = new Command(OnRewindProofCommandExecute);
            ForwardProofCommand = new Command(OnForwardProofCommandExecute);
            PauseProofCommand = new Command<StackPanel>(OnPauseProofCommandExecute);
            StopProofCommand = new Command(OnStopProofCommandExecute);
            //OnPauseProofCommandExecutePure();
            CLPProofHistory proofPageHistory1 = (CLPProofHistory)page.PageHistory; 
            proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Pause;
            proofPageHistory1.IsPaused = true;
            lock(obj)
            {
                proofPageHistory1.Freeze();
            }
            ProofProgressCurrent = page.PageWidth *0.7175;
            ProofProgressVisible = "Hidden";
            ProofPresent = "Hidden";
       }

       #endregion //Constructors

       #region Properties
       
       /// <summary>
       /// Gets or sets the property value.
       /// </summary>
       [ViewModelToModel("Page")]
       public override ICLPHistory PageHistory
       {
           get { return GetValue<CLPProofHistory>(ProofPageHistoryProperty); }
           set { SetValue(ProofPageHistoryProperty, value); }
       }

       public static volatile  PropertyData ProofPageHistoryProperty = RegisterProperty("ProofPageHistory", typeof(CLPProofHistory));

       #endregion //Properties

       #region Commands
       
       public Command PlayProofCommand  { get; private set; }
       public Command ReplayProofCommand { get; private set; }
       public Command RecordProofCommand  { get; private set; }
       public Command InsertProofCommand { get; private set; }
       public Command RewindProofCommand { get; private set; }
       public Command ForwardProofCommand { get; private set; }
       public Command<StackPanel> PauseProofCommand { get; private set; }
       public Command StopProofCommand { get; private set; }

       //replays the entire proof from the beginning
       //disables editing of proof for duration of method
       private void OnReplayProofCommandExecute() {
           OnStopProofCommandExecute();
           OnPlayProofCommandExecute();
       }
      
       //enables editing of proof page
       //enables recording of proof page history
       private void OnRecordProofCommandExecute(){
           lock(obj)
           {
               CLPProofPage page = (CLPProofPage)(MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
               CLPProofHistory proofPageHistory1 = (CLPProofHistory)page.PageHistory;
               proofPageHistory1.Future.Clear();
               proofPageHistory1.IsPaused = false;
               proofPageHistory1.Unfreeze();
               EditingMode = InkCanvasEditingMode.Ink;
               proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Record;
               page.updateProgress();
           }
       }

       //enables editing of proof page
       //enables recording of proof page history
       private void OnInsertProofCommandExecute(){
           lock(obj)
           {
               CLPProofPage page = (CLPProofPage)(MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
               CLPProofHistory proofPageHistory1 = (CLPProofHistory)page.PageHistory;
               proofPageHistory1.IsPaused = false;
               proofPageHistory1.Unfreeze();
               EditingMode = InkCanvasEditingMode.Ink;
               proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Insert;
               page.updateProgress();
           }
       }

       //plays the entire proof backwards to the beginning
       //undoes pause if proof/page was paused
       //disables editing of proof for duration of method
       private void OnRewindProofCommandExecute() {
           Thread t = new Thread(() =>
           {
               PlayProof(CLPProofHistory.CLPProofPageAction.Rewind, -1, 25, 200);
           });
           t.Start();
       }
       
       //plays the entire proof forwards (more quickly than play) to the end
       //undoes pause if proof/page was paused
       //disables editing of proof for duration of method
       private void OnForwardProofCommandExecute() {
           Thread t = new Thread(() =>
           {
               PlayProof(CLPProofHistory.CLPProofPageAction.Forward, 1, 25, 200);
           });
           t.Start();
       }
      
       //sets isPaused property to its opposite (if true -> false, if false -> true)
       //Calls command for action being carried out before pause (if page was already paused)
       private void OnPauseProofCommandExecutePure() {
           CLPProofPage page = (CLPProofPage)(MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
           CLPProofHistory proofPageHistory1 = (CLPProofHistory)page.PageHistory;
           if(proofPageHistory1.IsPaused)
           {
               if(proofPageHistory1.ProofPageAction == CLPProofHistory.CLPProofPageAction.Play)
               {
                   //proofPageHistory1.isPaused = false;
                   this.OnPlayProofCommandExecute();
               }
               else if(proofPageHistory1.ProofPageAction == CLPProofHistory.CLPProofPageAction.Forward)
               {
                   //proofPageHistory1.isPaused = false;
                   this.OnForwardProofCommandExecute();
               }
               else if(proofPageHistory1.ProofPageAction == CLPProofHistory.CLPProofPageAction.Rewind)
               {
                   //proofPageHistory1.isPaused = false;
                   this.OnRewindProofCommandExecute();
               }
               else
               {
                   lock(obj)
                   {
                       proofPageHistory1.IsPaused = false;
                       proofPageHistory1.Unfreeze();
                   }
               }
           }
           else
           {
               proofPageHistory1.IsPaused = true;
               lock(obj)
               {
                   proofPageHistory1.Freeze();
               }
               //proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Pause;
           }
           lock(obj)
           {
               page.updateProgress();
           }
       }


       private void setButtonFocus(StackPanel ButtonsGrid, String s) {
           foreach(UIElement bt in ButtonsGrid.Children)
           {
               Console.WriteLine(((Button)bt).Name.Trim());
               if(((Button)bt).Name.Trim().Equals(s.Trim()))
               {
                   bt.Focus();
               }
           }
       
       }
       
       private void OnPauseProofCommandExecute(StackPanel ButtonsGrid)
       {
           CLPProofPage page = (CLPProofPage)(MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
           CLPProofHistory proofPageHistory1 = (CLPProofHistory)page.PageHistory;
           if(proofPageHistory1.IsPaused)
           {
               if(proofPageHistory1.ProofPageAction == CLPProofHistory.CLPProofPageAction.Play)
               {
                   setButtonFocus(ButtonsGrid, "PlayButton");
               }
               else if(proofPageHistory1.ProofPageAction == CLPProofHistory.CLPProofPageAction.Rewind)
               {
                   setButtonFocus(ButtonsGrid, "RewindButton");
               }
               else if(proofPageHistory1.ProofPageAction == CLPProofHistory.CLPProofPageAction.Insert)
               {
                   setButtonFocus(ButtonsGrid, "InsertButton");
               }
               else if(proofPageHistory1.ProofPageAction == CLPProofHistory.CLPProofPageAction.Record)
               {
                   setButtonFocus(ButtonsGrid, "RecordButton");
               }
               else if(proofPageHistory1.ProofPageAction == CLPProofHistory.CLPProofPageAction.Pause)
               {
                   //focus is already correct, do nothing
               }
               else
               {
                   //code should never come here
               }
           }
           else
           {
               //focus is already correct, do nothing
           }
           OnPauseProofCommandExecutePure();
       }
       
       //disables editing of proof page
       //disables recording of history 
       private void OnStopProofCommandExecute() {
           Thread t = new Thread(() =>
           {
               PlayProof(CLPProofHistory.CLPProofPageAction.Rewind, -1, 0, 0);
           });
           t.Start();
           //OnPauseProofCommandExecutePure();
           CLPProofPage page = (CLPProofPage)(MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
           CLPProofHistory proofPageHistory1 = (CLPProofHistory)page.PageHistory;
           proofPageHistory1.IsPaused = true;
           proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Pause;
           lock(obj)
           {
               proofPageHistory1.Freeze();
               page.updateProgress();
           }
           
           
       }

       //plays the entire proof forwards (more slowly than forward) to the end
       //undoes pause if proof/page was paused
       //disables editing of proof for duration of method
       private void OnPlayProofCommandExecute()
       {
           Thread t = new Thread(() =>
                { 
                PlayProof(CLPProofHistory.CLPProofPageAction.Play, 1, 200, 400); //was 50, 400
                });
           t.Start();
       
       }
      
       private void PlayProof(CLPProofHistory.CLPProofPageAction action, int direction, int smallPause, int largePause)
       {
           lock(obj){
               
               CLPProofPage page = (CLPProofPage)(MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
               CLPProofHistory proofPageHistory1 = (CLPProofHistory)page.PageHistory;
               Stack<CLPHistoryItem> from = null;
               Stack<CLPHistoryItem> to = null;
               if(direction >= 0)
               {
                   from = proofPageHistory1.Future;
                   to = proofPageHistory1.MetaPast;
               }
               else
               {
                   from = proofPageHistory1.MetaPast;
                   to = proofPageHistory1.Future;
               }
              
               
               int count = from.Count + to.Count;
               Console.WriteLine("This is the total: " + count); 


                proofPageHistory1.IsPaused = false;
                proofPageHistory1.ProofPageAction = action;
                proofPageHistory1.Freeze();
                base.EditingMode = InkCanvasEditingMode.None;
                int i = 0;
               try{
                    while(from.Count > 0)
                       {
                        count = from.Count + to.Count;
                        //Console.WriteLine("This is the new total on loop " + i + ": " + count);
                        i++;
                        if(proofPageHistory1.IsPaused)
                           {
                               //break;
                               base.EditingMode = InkCanvasEditingMode.Ink;
                               return;
                           }

                           CLPHistoryItem item = from.Pop();
                           to.Push(item);
                           if(!item.wasPaused && !item.singleCut)
                           {
                               if(item.ItemType == HistoryItemType.MoveObject || item.ItemType == HistoryItemType.ResizeObject)
                               {
                                   Thread.Sleep(smallPause); // make intervals between move-steps less painfully slow
                               }
                               else
                               {
                                   Thread.Sleep(largePause);
                               }
                           }
                           if(item != null)
                           {
                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                    (DispatcherOperationCallback)delegate(object arg)
                                    {
                                        if(direction >= 0)
                                        {
                                            proofPageHistory1.Freeze();
                                            item.Redo(page);
                                        }else{
                                            proofPageHistory1.Freeze();
                                            item.Undo(page);
                                        }
                                        page.updateProgress();
                                        return null;
                                    }, null);
                           }
                       }
                    ///////////////////
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                     (DispatcherOperationCallback)delegate(object arg)
                                     {
                                         proofPageHistory1.Unfreeze();
                                         base.EditingMode = InkCanvasEditingMode.Ink;
                                         proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Record;
                                         page.updateProgress();
                                         return null;
                                     }, null);
                    //////////////////
                    return;
                   }
                   catch(Exception e)
                   {
                       ///////////////////
                       Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                        (DispatcherOperationCallback)delegate(object arg)
                                        {
                                            proofPageHistory1.Unfreeze();
                                            base.EditingMode = InkCanvasEditingMode.Ink;
                                            proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Record;
                                            page.updateProgress();
                                            return null;
                                        }, null);
                       //////////////////
                       return;
                   }
           }
       }

      
       #endregion //Commands

       #region previousCommands
       /*private void OnPlayProofCommandExecute()
        {
            
           Thread t = new Thread(() =>
            {
                Console.WriteLine("play proof");
                try
                {
                    CLPProofPage page = (CLPProofPage)(MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
                    CLPProofHistory proofPageHistory1 = (CLPProofHistory)page.PageHistory;
                    proofPageHistory1.isPaused = false; 
                    proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Play; 
                    
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (DispatcherOperationCallback)delegate(object arg)
                    {
                        proofPageHistory1.Freeze();
                        base.EditingMode = InkCanvasEditingMode.None;
                        return null;
                    }, null);
                    Stack<CLPHistoryItem> metaPast = proofPageHistory1.MetaPast;
                    Stack<CLPHistoryItem> metaFuture = proofPageHistory1.Future;
                    while(metaFuture.Count > 0)
                    {
                        if(proofPageHistory1.isPaused)
                        {
                            //proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Play;
                            break;
                        }
                        
                        CLPHistoryItem item = metaFuture.Pop();
                        if(!item.wasPaused && !item.singleCut){
                            if(item.ItemType == HistoryItemType.MoveObject || item.ItemType == HistoryItemType.ResizeObject)
                            {
                                Thread.Sleep(50); // make intervals between move-steps less painfully slow
                            }
                            else
                            {
                                Thread.Sleep(400);
                            }
                        }
                        Console.WriteLine("This is the action being DONE: " + item.ItemType);
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (DispatcherOperationCallback)delegate(object arg)
                        {
                            if(item != null)
                            {
                                item.Redo(page);
                                metaPast.Push(item);
                            }
                            return null;
                        }, null);
                        
                    }
                    Thread.Sleep(400);
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (DispatcherOperationCallback)delegate(object arg)
                    {
                        proofPageHistory1.Unfreeze();
                        base.EditingMode = InkCanvasEditingMode.Ink;
                        return null;
                    }, null);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            t.Start();
        }*/
       /*private void OnRewindProofCommandExecute(){
           
            
          Thread t = new Thread(() =>
           {
               Console.WriteLine("rewind proof");
               try
               {
                   CLPProofPage page = (CLPProofPage)(MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
                   CLPProofHistory proofPageHistory1 = (CLPProofHistory)page.PageHistory;
                   proofPageHistory1.isPaused = false;
                   proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Rewind;

                   Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                   (DispatcherOperationCallback)delegate(object arg)
                   {
                       proofPageHistory1.Freeze();
                       base.EditingMode = InkCanvasEditingMode.None;
                       return null;
                   }, null);
                   Stack<CLPHistoryItem> metaFuture = proofPageHistory1.Future;
                   Stack<CLPHistoryItem> metaPast = proofPageHistory1.MetaPast;
                   while(metaPast.Count > 0) 
                    {
                        if(proofPageHistory1.isPaused)
                        {
                            //proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Rewind;
                            break;
                        }
                         
                       CLPHistoryItem item = metaPast.Pop();
                        if(!item.wasPaused && !item.singleCut)
                        {
                            if(item.ItemType == HistoryItemType.MoveObject || item.ItemType == HistoryItemType.ResizeObject)
                            {
                                Thread.Sleep(25); // make intervals between move-steps less painfully slow
                            }
                            else
                            {
                                Thread.Sleep(200);
                            }
                        }
                        Console.WriteLine("This is the action being UNDONE: " + item.ItemType);

                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (DispatcherOperationCallback)delegate(object arg)
                        {
                            if(item != null) // TODO (caseymc): find out why one of these would ever be null and fix
                            {
                                item.Undo(page);
                                metaFuture.Push(item);
                            }
                            return null;
                        }, null);   
                    }
                    Console.WriteLine("done undoing");
                   Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                   (DispatcherOperationCallback)delegate(object arg)
                   {
                       proofPageHistory1.Unfreeze();
                       base.EditingMode = InkCanvasEditingMode.Ink;
                       return null;
                   }, null);
               }
               catch(Exception e)
               {
                   Console.WriteLine(e.Message);
               }
           });
           t.Start();
      }*/
       /*private void OnForwardProofCommandExecute(){
           
          Thread t = new Thread(() =>
           {
               Console.WriteLine("Forward proof");
               try
               {
                   CLPProofPage page = (CLPProofPage)(MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
                   CLPProofHistory proofPageHistory1 = (CLPProofHistory)page.PageHistory;
                   proofPageHistory1.isPaused = false;
                   proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Forward; 

                   Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                   (DispatcherOperationCallback)delegate(object arg)
                   {
                       proofPageHistory1.Freeze();
                       base.EditingMode = InkCanvasEditingMode.None;
                       return null;
                   }, null);
                   Stack<CLPHistoryItem> metaPast = proofPageHistory1.MetaPast;
                   Stack<CLPHistoryItem> metaFuture = proofPageHistory1.Future;
                   while(metaFuture.Count > 0)
                   {
                       if(proofPageHistory1.isPaused)
                       {
                           proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Forward;
                           break;
                       }
                        
                       CLPHistoryItem item = metaFuture.Pop();
                       if(!item.wasPaused && !item.singleCut){
                           if(item.ItemType == HistoryItemType.MoveObject || item.ItemType == HistoryItemType.ResizeObject)
                           {
                               Thread.Sleep(25); // make intervals between move-steps less painfully slow
                           }
                           else
                           {
                               Thread.Sleep(200);
                           }
                       }
                       Console.WriteLine("This is the action being DONE: " + item.ItemType);
                       Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                       (DispatcherOperationCallback)delegate(object arg)
                       {
                           if(item != null)
                           {
                               item.Redo(page);
                               metaPast.Push(item);
                           }
                           return null;
                       }, null);
                        
                   }
                   Thread.Sleep(400);
                   Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                   (DispatcherOperationCallback)delegate(object arg)
                   {
                       proofPageHistory1.Unfreeze();
                       base.EditingMode = InkCanvasEditingMode.Ink;
                       return null;
                   }, null);
               }
               catch(Exception e)
               {
                   Console.WriteLine(e.Message);
               }
           });
           t.Start();
      }*/
       /*private void OnReplayProofCommandExecute() {
          
          Thread t = new Thread(() =>
           {
               Console.WriteLine("Replay");
               try
               {
                   CLPProofPage page = (CLPProofPage)(MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
                   CLPProofHistory proofPageHistory1 = (CLPProofHistory)page.PageHistory;
                   proofPageHistory1.isPaused = false;
                   proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Play;
                    
                   Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                   (DispatcherOperationCallback)delegate(object arg)
                   {
                       proofPageHistory1.Freeze();
                       base.EditingMode = InkCanvasEditingMode.None;
                       return null;
                   }, null);

                   Stack<CLPHistoryItem> metaPast = proofPageHistory1.MetaPast;
                   Stack<CLPHistoryItem> metaFuture = proofPageHistory1.Future;
                   while(metaPast.Count > 0) 
                   {
                       CLPHistoryItem item = metaPast.Pop();
                       Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                       (DispatcherOperationCallback)delegate(object arg)
                       {
                           if(item != null) // TODO (caseymc): find out why one of these would ever be null and fix
                           {
                               item.Undo(page);
                               metaFuture.Push(item);
                           }
                           return null;
                       }, null);     
                   }

                   Console.WriteLine("done undoing");
                   Thread.Sleep(400);
                   while(metaFuture.Count > 0)
                   {
                       if(proofPageHistory1.isPaused) {
                           //proofPageHistory1.ProofPageAction = CLPProofHistory.CLPProofPageAction.Play;
                           break;
                       }
                        

                       CLPHistoryItem item = metaFuture.Pop();
                       if(!item.wasPaused && !item.singleCut)
                       {
                           if(item.ItemType == HistoryItemType.MoveObject || item.ItemType == HistoryItemType.ResizeObject)
                           {
                               Thread.Sleep(50); // make intervals between move-steps less painfully slow
                           }
                           else
                           {
                               Thread.Sleep(400);
                           }
                       }
                       Console.WriteLine("This is the action being REDONE: " + item.ItemType);
                       Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                       (DispatcherOperationCallback)delegate(object arg)
                       {
                           if(item != null)
                           {
                               item.Redo(page);
                               metaPast.Push(item);
                           }
                            
                           return null;
                       }, null);
                       
                   }
                   Thread.Sleep(400);
                   Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                   (DispatcherOperationCallback)delegate(object arg)
                   {
                       proofPageHistory1.Unfreeze();
                       base.EditingMode = InkCanvasEditingMode.Ink;
                       return null;
                   }, null);
               }
               catch(Exception e)
               {
                   Console.WriteLine(e.Message);
               }
           });
           t.Start();
      }*/
       #endregion






   }
}
