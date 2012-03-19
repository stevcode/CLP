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
        void Connect(string userName);

        [OperationContract(IsOneWay = true)]
        void Disconnect(string userName);

        [OperationContract(IsOneWay = true)]
        void SubmitPage(string page, string userName);

        [OperationContract(IsOneWay = true)]
        void SaveNotebookDB(string s_notebook, string userName);

        [OperationContract(IsOneWay = true)]
        void SavePage(string page, string userName, DateTime submitTime);

        [OperationContract(IsOneWay = true)]
        void DistributeNotebook(string s_notebook, string author);

        [OperationContract(IsOneWay = true)]
        void ReceiveNotebook(string page, string userName);
        

        [OperationContract(IsOneWay = true)]
        void LaserUpdate(Point pt);

        [OperationContract(IsOneWay = true)]
        void TurnOffLaser();

        [OperationContract(IsOneWay = true)]
        void BroadcastInk(List<string> strokesAdded, List<string> strokesRemoved, string pageUniqueID);

        [OperationContract(IsOneWay = true)]
        void SwitchProjectorDisplay(string displayType, List<string> gridDisplayPages);

        [OperationContract(IsOneWay = true)]
        void AddPageToDisplay(string stringPage);

        [OperationContract(IsOneWay = true)]
        void RemovePageFromGridDisplay(string pageID);

        [OperationContract(IsOneWay = true)]
        void AddPageObjectToPage(string pageID, string stringPageObject);
    }

    public interface ICLPMeshNetworkChannel : ICLPMeshNetworkContract, IClientChannel
    {
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CLPMeshNetworkService : ICLPMeshNetworkContract
    {
        int pagecount = 0;


        public void Connect(string userName)
        {
            if (App.CurrentUserMode == App.UserMode.Server && App.DatabaseUse == App.DatabaseMode.Using)
            {
                Console.WriteLine("Instructor/Student Machine Connected: " + userName);
                //Users Notebooks to user machine
                //Currently username is the machine name -> CHANGE when using actual names
                CLPServiceAgent.Instance.RetrieveNotebooks(userName);
            }
          
        }

        public void Disconnect(string userName)
        {
            if (App.CurrentUserMode == App.UserMode.Server)
            {
                Console.WriteLine("Machine Disconnected: " + userName);
            }
        }
     
        public void SubmitPage(string s_page, string userName)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
             {
                    if (App.CurrentUserMode == App.UserMode.Instructor)
                    {
                        Console.WriteLine("page received");
                        Console.WriteLine(s_page);

                        CLPPage page = (ObjectSerializer.ToObject(s_page) as CLPPage);
                        page.IsSubmission = true;
                        page.SubmitterName = userName;
                        CLPServiceAgent.Instance.AddSubmission(page);
                    }
                    else if (App.CurrentUserMode == App.UserMode.Server)
                    {
                        pagecount++;
                        Console.WriteLine("Page Recieved, Current Count: " + pagecount.ToString());
                        //Database call
                        if (App.DatabaseUse == App.DatabaseMode.Using)
                        {
                            CLPPage page = (ObjectSerializer.ToObject(s_page) as CLPPage);
                            CLPServiceAgent.Instance.SavePageDB(page, s_page, userName, true);
                        }
                    }
             return null;
             }, null);   

        }
        public void SavePage(string s_page, string userName, DateTime submitTime)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (DispatcherOperationCallback)delegate(object arg)
            {
                if (App.CurrentUserMode == App.UserMode.Server && App.DatabaseUse == App.DatabaseMode.Using)
                {

                    //Database call
                    TimeSpan difference = DateTime.Now.Subtract(submitTime);
                    double kbSize = s_page.Length / 1024.0;
                    Logger.Instance.WriteToLog("RecvSave " + kbSize.ToString() + " " + difference.ToString() + " " + userName);
                    if (App.DatabaseUse == App.DatabaseMode.Using)
                    {
                        CLPPage page = (ObjectSerializer.ToObject(s_page) as CLPPage);
                        CLPServiceAgent.Instance.SavePageDB(page, s_page, userName, false);
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
                CLPServiceAgent.Instance.SaveNotebookDB(notebook, userName);
               
            }
        }
        public void DistributeNotebook(string s_notebook, string author)
        {
            if (App.CurrentUserMode == App.UserMode.Server && App.DatabaseUse == App.DatabaseMode.Using)
            {
                CLPNotebook notebook = (ObjectSerializer.ToObject(s_notebook) as CLPNotebook);
                CLPServiceAgent.Instance.DistributeNotebookServer(notebook, author);
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
                CLPServiceAgent.Instance.SaveNotebooksFromDBToHD(notebook);
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
        public void BroadcastInk(List<string> strokesAdded, List<string> strokesRemoved, string pageUniqueID)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        //foreach (var pageViewModel in App.CurrentNotebookViewModel.PageViewModels)
                        //{
                        //    if (pageViewModel.Page.UniqueID == pageUniqueID)
                        //    {
                        //        foreach (var stringStroke in strokesAdded)
                        //        {
                        //            Stroke stroke = CLPPageViewModel.StringToStroke(stringStroke);
                        //            pageViewModel.OtherStrokes.Add(stroke);
                        //        }
                        //        foreach (var stringStroke in strokesRemoved)
                        //        {
                        //            Stroke sentStroke = CLPPageViewModel.StringToStroke(stringStroke);
                        //            foreach (var stroke in pageViewModel.OtherStrokes.ToList())
                        //            {
                        //                string a = sentStroke.GetPropertyData(CLPPage.StrokeIDKey) as string;
                        //                string b = stroke.GetPropertyData(CLPPage.StrokeIDKey) as string;
                        //                if (a == b)
                        //                {
                        //                    pageViewModel.OtherStrokes.Remove(stroke);
                        //                }
                        //            }
                        //            pageViewModel.OtherStrokes.Remove(CLPPageViewModel.StringToStroke(stringStroke));
                        //        }
                        //    }
                        //}

                    }

                    return null;
                }, null);

        }

        public void SwitchProjectorDisplay(string displayType, List<string> gridDisplayPages)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        if (displayType == "LinkedDisplay")
                        {
                            //(App.MainWindowViewModel.SelectedWorkspace as ProjectorWorkspaceViewModel).SelectedDisplay = (App.MainWindowViewModel.SelectedWorkspace as ProjectorWorkspaceViewModel).LinkedDisplay;
                            //(App.MainWindowViewModel.SelectedWorkspace as ProjectorWorkspaceViewModel).LinkedDisplay.IsActive = true;
                            //(App.MainWindowViewModel.SelectedWorkspace as ProjectorWorkspaceViewModel).LinkedDisplay.IsOnProjector = true;
                            //(App.MainWindowViewModel.SelectedWorkspace as ProjectorWorkspaceViewModel).GridDisplay.IsActive = false;
                            //(App.MainWindowViewModel.SelectedWorkspace as ProjectorWorkspaceViewModel).GridDisplay.IsOnProjector = false;
                        }
                        else
                        {
                            //(App.MainWindowViewModel.SelectedWorkspace as ProjectorWorkspaceViewModel).SelectedDisplay = (App.MainWindowViewModel.SelectedWorkspace as ProjectorWorkspaceViewModel).GridDisplay;
                            //(App.MainWindowViewModel.SelectedWorkspace as ProjectorWorkspaceViewModel).GridDisplay.IsActive = true;
                            //(App.MainWindowViewModel.SelectedWorkspace as ProjectorWorkspaceViewModel).GridDisplay.IsOnProjector = true;
                            //(App.MainWindowViewModel.SelectedWorkspace as ProjectorWorkspaceViewModel).LinkedDisplay.IsActive = false;
                            //(App.MainWindowViewModel.SelectedWorkspace as ProjectorWorkspaceViewModel).LinkedDisplay.IsOnProjector = false;

                            //(App.MainWindowViewModel.SelectedWorkspace as ProjectorWorkspaceViewModel).GridDisplay.DisplayedPages.Clear();
                            //foreach (var stringPage in gridDisplayPages)
                            //{
                                //CLPPage page = ObjectSerializer.ToObject(stringPage) as CLPPage;
                                //bool isAlreadyInCurrentNotebook = false;
                                //foreach (var pageViewModel in App.CurrentNotebookViewModel.PageViewModels)
                                //{
                                //    if (pageViewModel.Page.UniqueID == page.UniqueID)
                                //    {
                                //        isAlreadyInCurrentNotebook = true;
                                //    }
                                //}

                                //if (isAlreadyInCurrentNotebook)
                                //{
                                //    (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).GridDisplay.DisplayPages.Add(new CLPPageViewModel(page, App.CurrentNotebookViewModel));
                                //}
                                //else
                                //{
                                //    CLPPageViewModel newPageViewModel = new CLPPageViewModel(page, App.CurrentNotebookViewModel);
                                //    App.CurrentNotebookViewModel.PageViewModels.Add(newPageViewModel);
                                //    (App.MainWindowViewModel.Workspace as ProjectorWorkspaceViewModel).GridDisplay.DisplayPages.Add(newPageViewModel);
                                //}
                            //}
                        }

                    }
                    return null;
                }, null);
        }

        public void AddPageToDisplay(string stringPage)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        CLPPage page = ObjectSerializer.ToObject(stringPage) as CLPPage;
                        //bool isAlreadyInCurrentNotebook = false;
                        //foreach (var pageViewModel in App.CurrentNotebookViewModel.PageViewModels)
                        //{
                        //    if (page.IsSubmission)
                        //    {
                        //        page.UniqueID = page.SubmissionID;
                        //    }
                        //    if (pageViewModel.Page.UniqueID == page.UniqueID)
                        //    {
                        //        isAlreadyInCurrentNotebook = true;
                        //    }
                            
                        //}

                        //if (isAlreadyInCurrentNotebook)
                        //{
                        //    AppMessages.AddPageToDisplay.Send(App.CurrentNotebookViewModel.GetPageByID(page.UniqueID));
                        //}
                        //else
                        //{
                        //    CLPPageViewModel newPageViewModel = new CLPPageViewModel(page, App.CurrentNotebookViewModel);
                        //    App.CurrentNotebookViewModel.PageViewModels.Add(newPageViewModel);
                        //    AppMessages.AddPageToDisplay.Send(newPageViewModel);
                        //}
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
                        
                        //foreach (var pageViewModel in App.CurrentNotebookViewModel.PageViewModels)
                        //{
                        //    if (pageViewModel.Page.UniqueID == pageID)
                        //    {
                        //        object pageObject = ObjectSerializer.ToObject(stringPageObject);


                        //        CLPPageObjectBaseViewModel pageObjectViewModel;
                        //        if (pageObject is CLPImage)
                        //        {
                        //            pageObjectViewModel = new CLPImageViewModel(pageObject as CLPImage, pageViewModel);
                        //        }
                        //        else if (pageObject is CLPImageStamp)
                        //        {
                        //            pageObjectViewModel = new CLPImageStampViewModel(pageObject as CLPImageStamp, pageViewModel);
                        //        }
                        //        else if (pageObject is CLPBlankStamp)
                        //        {
                        //            pageObjectViewModel = new CLPBlankStampViewModel(pageObject as CLPBlankStamp, pageViewModel);
                        //        }
                        //        else if (pageObject is CLPTextBox)
                        //        {
                        //            pageObjectViewModel = new CLPTextBoxViewModel(pageObject as CLPTextBox, pageViewModel);
                        //        }
                        //        else if (pageObject is CLPSnapTileContainer)
                        //        {
                        //            pageObjectViewModel = new CLPSnapTileContainerViewModel(pageObject as CLPSnapTileContainer, pageViewModel);
                        //        }
                        //        else
                        //        {
                        //            pageObjectViewModel = null;
                        //        }

                        //        pageViewModel.PageObjectContainerViewModels.Add(new PageObjectContainerViewModel(pageObjectViewModel));
                        //        pageViewModel.Page.PageObjects.Add(pageObjectViewModel.PageObject);
                        //        break;
                        //    }
                        //}
                    }
                                

                    return null;
                }, null);
        }
    }
}
