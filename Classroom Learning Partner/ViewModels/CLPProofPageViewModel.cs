using Catel.MVVM;
using CLP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
using Classroom_Learning_Partner.Views.Modal_Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
using Classroom_Learning_Partner.Views.Modal_Windows;
using System.Threading;

namespace Classroom_Learning_Partner.ViewModels
{
   class CLPProofPageViewModel : CLPPageViewModel
   {
       #region Constructors
       public CLPProofPageViewModel(CLPPage page):base(page)
        {
            PlayProofCommand = new Command(OnPlayProofCommandExecute);
            ReplayProofCommand = new Command(OnReplayProofCommandExecute);
            RecordProofCommand = new Command(OnRecordProofCommandExecute);
            EditProofCommand = new Command(OnEditProofCommandExecute);
            RewindProofCommand = new Command(OnRewindProofCommandExecute);
            ForwardProofCommand = new Command(OnForwardProofCommandExecute);
            PauseProofCommand = new Command(OnPauseProofCommandExecute);
            DoneProofCommand = new Command(OnDoneProofCommandExecute);
        }
       #endregion //Constructors

       #region Properties
       [ViewModelToModel("Page")]
       public override CLPHistory PageHistory
       {
           get { return GetValue<CLPProofHistory>(ProofPageHistoryProperty); }
           set { SetValue(ProofPageHistoryProperty, value); }
       }
       /// <summary>
       /// Register the PageHistory property so it is known in the class.
       /// </summary>
       public static readonly PropertyData ProofPageHistoryProperty = RegisterProperty("ProofPageHistory", typeof(CLPProofHistory));
       #endregion //Properties






       #region Commands
       public Command PlayProofCommand  { get; private set; }
       public Command ReplayProofCommand { get; private set; }
       public Command RecordProofCommand  { get; private set; }
       public Command EditProofCommand { get; private set; }
       public Command RewindProofCommand { get; private set; }
       public Command ForwardProofCommand { get; private set; }
       public Command PauseProofCommand { get; private set; }
       public Command DoneProofCommand { get; private set; }

       public MainWindowViewModel MainWindow
       {
           get { return App.MainWindowViewModel; }
       }
       
       private void OnReplayProofCommandExecute() {
            Thread t = new Thread(() =>
            {
                Console.WriteLine("Replay");
                try
                {
                    CLPPage page = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
                    CLPHistory pageHistory = page.PageHistory;

                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (DispatcherOperationCallback)delegate(object arg)
                    {
                        pageHistory.Freeze();
                        return null;
                    }, null);

                    Stack<CLPHistoryItem> metaFuture = new Stack<CLPHistoryItem>();
                    Stack<CLPHistoryItem> metaPast = new Stack<CLPHistoryItem>(new Stack<CLPHistoryItem>(pageHistory.MetaPast));

                    while(metaPast.Count > 0) 
                    {
                        CLPHistoryItem item = metaPast.Pop();
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (DispatcherOperationCallback)delegate(object arg)
                        {
                            if(item != null) // TODO (caseymc): find out why one of these would ever be null and fix
                            {
                                item.Undo(page);
                            }
                            return null;
                        }, null);
                        metaFuture.Push(item);
                    }

                    Console.WriteLine("done undoing");
                    Thread.Sleep(400);
                    while(metaFuture.Count > 0)
                    {
                        CLPHistoryItem item = metaFuture.Pop();
                        if(item.ItemType == HistoryItemType.MoveObject || item.ItemType == HistoryItemType.ResizeObject)
                        {
                            Thread.Sleep(50); // make intervals between move-steps less painfully slow
                        }
                        else
                        {
                            Thread.Sleep(400);
                        }
                        Console.WriteLine("This is the action being REDONE: " + item.ItemType);
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (DispatcherOperationCallback)delegate(object arg)
                        {
                            if(item != null)
                            {
                                item.Redo(page);
                            }
                            return null;
                        }, null);
                    }
                    Thread.Sleep(400);
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (DispatcherOperationCallback)delegate(object arg)
                    {
                        pageHistory.Unfreeze();
                        return null;
                    }, null);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            t.Start();
       }
       private void OnRecordProofCommandExecute(){
           this.PageHistory.Unfreeze();
       }
       private void OnEditProofCommandExecute(){
           this.PageHistory.Unfreeze();
       }
       private void OnRewindProofCommandExecute(){}
       private void OnForwardProofCommandExecute(){}
       private void OnPauseProofCommandExecute(){
           ((CLPProofHistory)PageHistory).isPaused = true;
       }
       private void OnDoneProofCommandExecute() {
       this.PageHistory.Freeze();
       base.EditingMode = InkCanvasEditingMode.None;
       }
       private void OnPlayProofCommandExecute()
        {
            /*CLPPage page = (MainWindow.SelectedWorkspace as NotebookWorkspaceViewModel).CurrentPage;
            Thread ty = new Thread(() =>
            {
                try
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (DispatcherOperationCallback)delegate(object arg)
                    {
                        page.PageHistory.redo();
                        return null;
                    }, null);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
            ty.Start(); */
            //this.ProofPageHistory.PlayProof();
        }
       #endregion //Commands

    }
}
