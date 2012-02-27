using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.ServiceModel;
using System.Windows.Threading;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using System.Windows.Ink;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using Classroom_Learning_Partner.ViewModels.PageObjects;



namespace Classroom_Learning_Partner.Model
{
    [ServiceContract(CallbackContract = typeof(ICLPMeshNetworkContract))]
    public interface ICLPMeshNetworkContract
    {
        [OperationContract(IsOneWay = true)]
        void Connect(string machineName, string userName);

        [OperationContract(IsOneWay = true)]
        void Disconnect(string userName);

        [OperationContract(IsOneWay = true)]
        void SubmitPage(string page, string userName, DateTime submitTime);

        [OperationContract(IsOneWay = true)]
        void SaveNotebookDB(string s_notebook, string userName);

        [OperationContract(IsOneWay = true)]
        void DistributeNotebook(string s_notebook, string author);

        [OperationContract(IsOneWay=true)]
        void RetrieveNotebooks(string userName);

        [OperationContract(IsOneWay = true)]
        void ReceiveNotebook(string page, string userName);
        

        [OperationContract(IsOneWay = true)]
        void LaserUpdate(Point pt);

        [OperationContract(IsOneWay = true)]
        void TurnOffLaser();

        [OperationContract(IsOneWay = true)]
        void BroadcastInk(List<string> strokesAdded, List<string> strokesRemoved, Tuple<bool, string, string> pageUniqueID);

        [OperationContract(IsOneWay = true)]
        void SwitchProjectorDisplay(string displayType, List<Tuple<bool, string, string>> gridDisplayPageIDs);

        [OperationContract(IsOneWay = true)]
        void AddPageToDisplay(Tuple<bool, string, string> pageID);

        [OperationContract(IsOneWay = true)]
        void RemovePageFromGridDisplay(string pageID);

        [OperationContract(IsOneWay = true)]
        void AddPageObjectToPage(string pageID, string stringPageObject);

        [OperationContract(IsOneWay = true)]
        void TestNetworkSending(string content, DateTime sentTime, int id, int size, string username);

    }

    public interface ICLPMeshNetworkChannel : ICLPMeshNetworkContract, IClientChannel
    {
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CLPMeshNetworkService : ICLPMeshNetworkContract
    {
        private ICLPServiceAgent CLPService = new CLPServiceAgent();
        int pagecount = 0;


        public void TestNetworkSending(string content, DateTime sentTime, int id, int size, string username)
        {
            if (App.UserMode.Instructor == App.CurrentUserMode)
            {
                TimeSpan difference = DateTime.Now.Subtract(sentTime);
                //Print results
                double kbSize = size / 1024.0;
                Logger.Instance.WriteToLog(kbSize.ToString() + " " + difference.ToString() + " " + username);
                //Logger.Instance.WriteToLog("-------------------------------------");
                //Logger.Instance.WriteToLog("Item sent: " + id.ToString());
                //Logger.Instance.WriteToLog("Size sent: " + kbSize.ToString());
                //Logger.Instance.WriteToLog("From     : " + username);
                //Logger.Instance.WriteToLog("Took     : " + difference.ToString());
            }
        }

        public void Connect(string machineName, string userName)
        {
            if (App.CurrentUserMode == App.UserMode.Server && App.DatabaseUse == App.DatabaseMode.Using)
            {
                Console.WriteLine("Instructor/Student Machine Connected: " + userName);
            }
          
        }

        public void Disconnect(string userName)
        {
            if (App.CurrentUserMode == App.UserMode.Server)
            {
                Console.WriteLine("Machine Disconnected: " + userName);
            }
        }
     
        public void SubmitPage(string s_page, string userName, DateTime submitTime)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
             {
                    if (App.CurrentUserMode == App.UserMode.Instructor || App.CurrentUserMode == App.UserMode.Projector)
                    {
                        TimeSpan difference = DateTime.Now.Subtract(submitTime);
                        double kbSize = s_page.Length / 1024.0;
                        Logger.Instance.WriteToLog("-------------------------------------");
                        Console.WriteLine("Instructor received page at " + DateTime.Now.ToString());
                        Logger.Instance.WriteToLog("Instructor received page at " + DateTime.Now.ToString());
                        Logger.Instance.WriteToLog("RecvSubmission " + kbSize.ToString() + " " + difference.ToString() + " " + userName);
                        //Logger.Instance.WriteToLog("Instructor received page at " + DateTime.Now.ToString());
                        //Console.WriteLine(s_page);

                        CLPPage page = (ObjectSerializer.ToObject(s_page) as CLPPage);
                        //Logger.Instance.WriteToLog("Instructor done desiralizing page at " + DateTime.Now.ToString());
                        page.IsSubmission = true;
                        page.SubmitterName = userName;
                        CLPService.AddSubmission(page);
                    }
                    else if (App.CurrentUserMode == App.UserMode.Server)
                    {
                        pagecount++;
                        Console.WriteLine("Page Recieved, Current Count: " + pagecount.ToString());
                        //Database call
                        if (App.DatabaseUse == App.DatabaseMode.Using)
                        {
                            CLPPage page = (ObjectSerializer.ToObject(s_page) as CLPPage);
                            CLPService.SavePageDB(page, userName);
                        }
                    }
             return null;
             }, null);   

        }

        public void SaveNotebookDB(string s_notebook, string userName)
        {
            //recieve notebook
            //App.PeerNode.channel

            if (App.CurrentUserMode == App.UserMode.Server && App.DatabaseUse == App.DatabaseMode.Using)
            {
                Console.WriteLine("Notebook save requtest received");
                //Console.WriteLine(s_notebook);
                //DB call
                CLPNotebook notebook = (ObjectSerializer.ToObject(s_notebook) as CLPNotebook);
                CLPService.SaveNotebookDB(notebook, userName);
               
            }
        }
        public void DistributeNotebook(string s_notebook, string author)
        {
            if (App.CurrentUserMode == App.UserMode.Server && App.DatabaseUse == App.DatabaseMode.Using)
            {
                CLPNotebook notebook = (ObjectSerializer.ToObject(s_notebook) as CLPNotebook);
                CLPService.DistributeNotebookServer(notebook, author);
            }
        }

        public void RetrieveNotebooks(string userName)
        {
            if (App.CurrentUserMode == App.UserMode.Server && App.DatabaseUse == App.DatabaseMode.Using)
            {
                Console.WriteLine("Instructor/Student Machine requests notebooks: " + userName);
                //Users Notebooks to user machine
                CLPService.RetrieveNotebooks(userName);
                
            }
        }
        public void ReceiveNotebook(string s_notebook, string userName)
        {
            Console.WriteLine("ReceiveNotebooks called");
            //Console.WriteLine(s_notebook);
            //string[] splitUserNotebook = s_notebook.Split(new char[] { '#' }, 2);
            if (userName == App.Peer.UserName && App.CurrentUserMode != App.UserMode.Server)
            {
                Console.WriteLine("ReceiveNotebooks - recieved one notebook");
                CLPNotebook notebook = (ObjectSerializer.ToObject(s_notebook) as CLPNotebook);
                CLPService.SaveNotebooksFromDBToHD(notebook);
                //reload notebook chooser window if already open
                //if (App.MainWindowViewModel.Workspace.GetType().Equals(new NotebookChooserWorkspaceViewModel().GetType()))
                //{
                //    App.MainWindowViewModel.Workspace = new NotebookChooserWorkspaceViewModel();
                //}
                
               
            }
        }

        public void LaserUpdate(Point pt)
        {

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        AppMessages.UpdateLaserPointerPosition.Send(pt);
                    }
                    return null;
                }, null);

        }

        public void TurnOffLaser()
        {

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        AppMessages.TurnOffLaser.Send();
                    }
                    return null;
                }, null);
        }
        public void BroadcastInk(List<string> strokesAdded, List<string> strokesRemoved, Tuple<bool, string, string> pageUniqueID)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        //is submission
                        if (pageUniqueID.Item1)
                        {
                            foreach (var pageViewModel in App.CurrentNotebookViewModel.SubmissionViewModels[pageUniqueID.Item2])
                            {
                                if (pageViewModel.Page.SubmissionID == pageUniqueID.Item3)
                                {
                                    foreach (var stringStroke in strokesAdded)
                                    {
                                        Stroke stroke = CLPPageViewModel.StringToStroke(stringStroke);
                                        pageViewModel.OtherStrokes.Add(stroke);
                                    }
                                    foreach (var stringStroke in strokesRemoved)
                                    {
                                        Stroke sentStroke = CLPPageViewModel.StringToStroke(stringStroke);
                                        foreach (var stroke in pageViewModel.OtherStrokes.ToList())
                                        {
                                            string a = sentStroke.GetPropertyData(CLPPage.StrokeIDKey) as string;
                                            string b = stroke.GetPropertyData(CLPPage.StrokeIDKey) as string;
                                            if (a == b)
                                            {
                                                pageViewModel.OtherStrokes.Remove(stroke);
                                            }
                                        }
                                        pageViewModel.OtherStrokes.Remove(CLPPageViewModel.StringToStroke(stringStroke));
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var pageViewModel in App.CurrentNotebookViewModel.PageViewModels)
                            {
                                if (pageViewModel.Page.UniqueID == pageUniqueID.Item2)
                                {
                                    foreach (var stringStroke in strokesAdded)
                                    {
                                        Stroke stroke = CLPPageViewModel.StringToStroke(stringStroke);
                                        pageViewModel.OtherStrokes.Add(stroke);
                                    }
                                    foreach (var stringStroke in strokesRemoved)
                                    {
                                        Stroke sentStroke = CLPPageViewModel.StringToStroke(stringStroke);
                                        foreach (var stroke in pageViewModel.OtherStrokes.ToList())
                                        {
                                            string a = sentStroke.GetPropertyData(CLPPage.StrokeIDKey) as string;
                                            string b = stroke.GetPropertyData(CLPPage.StrokeIDKey) as string;
                                            if (a == b)
                                            {
                                                pageViewModel.OtherStrokes.Remove(stroke);
                                            }
                                        }
                                        pageViewModel.OtherStrokes.Remove(CLPPageViewModel.StringToStroke(stringStroke));
                                    }
                                }
                            }
                        }
                    }

                    return null;
                }, null);

        }

        public void SwitchProjectorDisplay(string displayType, List<Tuple<bool,string,string>> gridDisplayPageIDs)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        if (displayType == "LinkedDisplay")
                        {
                            (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).Display = (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).LinkedDisplay;
                            (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).LinkedDisplay.IsActive = true;
                            (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).LinkedDisplay.IsOnProjector = true;
                            (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).GridDisplay.IsActive = false;
                            (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).GridDisplay.IsOnProjector = false;
                        }
                        else
                        {
                            (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).Display = (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).GridDisplay;
                            (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).GridDisplay.IsActive = true;
                            (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).GridDisplay.IsOnProjector = true;
                            (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).LinkedDisplay.IsActive = false;
                            (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).LinkedDisplay.IsOnProjector = false;

                            (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).GridDisplay.DisplayPages.Clear();
                            foreach (Tuple<bool, string, string> pageIDs in gridDisplayPageIDs)
                            {
                                //is submission
                                if (pageIDs.Item1)
                                {
                                    foreach (var pageViewModel in App.CurrentNotebookViewModel.SubmissionViewModels[pageIDs.Item2])
                                    {
                                        if (pageViewModel.Page.SubmissionID == pageIDs.Item3)
                                        {
                                            (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).GridDisplay.DisplayPages.Add(pageViewModel);
                                        }
                                    } 
                                }
                                else
                                {
                                    foreach (var pageViewModel in App.CurrentNotebookViewModel.PageViewModels)
                                    {
                                        if (pageViewModel.Page.UniqueID == pageIDs.Item2)
                                        {
                                            (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).GridDisplay.DisplayPages.Add(pageViewModel);
                                        }
                                    }   
                                }
                                

                            }
                        }

                    }
                    return null;
                }, null);
        }

        public void AddPageToDisplay(Tuple<bool, string, string> pageID)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        //is submission
                        if (pageID.Item1)
                        {
                            foreach (var pageViewModel in App.CurrentNotebookViewModel.SubmissionViewModels[pageID.Item2])
                            {
                                if (pageViewModel.Page.SubmissionID == pageID.Item3)
                                {
                                    AppMessages.AddPageToDisplay.Send(pageViewModel);
                                }
                            }
                        }
                        else
                        {
                            foreach (var pageViewModel in App.CurrentNotebookViewModel.PageViewModels)
                            {
                                if (pageViewModel.Page.UniqueID == pageID.Item2)
                                {
                                    AppMessages.AddPageToDisplay.Send(pageViewModel);
                                }
                            }
                        }
                    }
                    return null;
                }, null);
        }


        public void RemovePageFromGridDisplay(string pageID)
        {
            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //    (DispatcherOperationCallback)delegate(object arg)
            //    {
            //        if (App.CurrentUserMode == App.UserMode.Projector)
            //        {


            //            if (isAlreadyInCurrentNotebook)
            //            {
            //                AppMessages.AddPageToDisplay.Send(App.CurrentNotebookViewModel.GetPageByID(page.UniqueID));
            //            }
            //            else
            //            {
            //                CLPPageViewModel newPageViewModel = new CLPPageViewModel(page, App.CurrentNotebookViewModel);
            //                App.CurrentNotebookViewModel.PageViewModels.Add(newPageViewModel);
            //                AppMessages.AddPageToDisplay.Send(newPageViewModel);
            //            }
            //        }
            //        return null;
            //    }, null);
        }


        public void AddPageObjectToPage(string pageID, string stringPageObject)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {

                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {

                        foreach (var pageViewModel in App.CurrentNotebookViewModel.PageViewModels)
                        {
                            if (pageViewModel.Page.UniqueID == pageID)
                            {
                                object pageObject = ObjectSerializer.ToObject(stringPageObject);


                                CLPPageObjectBaseViewModel pageObjectViewModel;
                                if (pageObject is CLPImage)
                                {
                                    pageObjectViewModel = new CLPImageViewModel(pageObject as CLPImage, pageViewModel);
                                }
                                else if (pageObject is CLPStamp)
                                {
                                    pageObjectViewModel = new CLPStampViewModel(pageObject as CLPStamp, pageViewModel);
                                }
                                else if (pageObject is CLPTextBox)
                                {
                                    pageObjectViewModel = new CLPTextBoxViewModel(pageObject as CLPTextBox, pageViewModel);
                                }
                                else if (pageObject is CLPSnapTile)
                                {
                                    pageObjectViewModel = new CLPSnapTileViewModel(pageObject as CLPSnapTile, pageViewModel);
                                }
                                else if (pageObject is CLPSquare)
                                {
                                    pageObjectViewModel = new CLPSquareViewModel(pageObject as CLPSquare, pageViewModel);
                                }
                                else if (pageObject is CLPCircle)
                                {
                                    pageObjectViewModel = new CLPCircleViewModel(pageObject as CLPCircle, pageViewModel);
                                }
                                else
                                {
                                    pageObjectViewModel = null;
                                }

                                pageViewModel.PageObjectContainerViewModels.Add(new PageObjectContainerViewModel(pageObjectViewModel));
                                pageViewModel.Page.PageObjects.Add(pageObjectViewModel.PageObject);
                                break;
                            }
                        }
                    }
                                

                    return null;
                }, null);
        }
    }
}
