using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Threading;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;
using ProtoBuf;

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
        void SubmitFullPage(string page, string userName, string notebookName);

        [OperationContract(IsOneWay = true)]
        void SubmitPage(string userName, string submissionID, string submissionTime, string s_history, string s_pageObjects, List<string> inkStrokes);

        [OperationContract(IsOneWay = true)]
        void SaveNotebookDB(string s_notebook, string userName);

        [OperationContract(IsOneWay = true)]
        void SavePage(string page, string userName, DateTime submitTime, string notebookName, int pageNumber);

        [OperationContract(IsOneWay = true)]
        void SaveHistory(string s_history, string userName, DateTime time, string notebookName, string pageID, int pageNumber);

        [OperationContract(IsOneWay = true)]
        void DistributeNotebook(string s_notebook, string author);

        [OperationContract(IsOneWay = true)]
        void ReceiveNotebook(string page, string userName);

        [OperationContract(IsOneWay = true)]
        void BroadcastInk(List<List<byte>> strokesAdded, List<List<byte>> strokesRemoved, string pageID, bool broadcastInkToStudents);

        [OperationContract(IsOneWay = true)]
        void SwitchProjectorDisplay(string displayType, List<string> displayPages);

        [OperationContract(IsOneWay = true)]
        void AddPageToDisplay(string pageID);

        [OperationContract(IsOneWay = true)]
        void ChangePageObjectsOnPage(string pageID, List<string> added, List<string> removedIDs);
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

        public void SaveHistory(string s_history, string userName, DateTime submitTime, string notebookName, string pageID, int pageNumber)
        {

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (DispatcherOperationCallback)delegate(object arg)
            {

         if (App.CurrentUserMode == App.UserMode.Server)
                {
                    //Deserialize Using Protobuf
                    Stream stream = new MemoryStream(Convert.FromBase64String(s_history));
                    CLP.Models.CLPHistory history = new CLP.Models.CLPHistory();
                    history = Serializer.Deserialize<CLP.Models.CLPHistory>(stream);

                    //Interpolate History to make it bigger again
                    
                    TimeSpan difference = DateTime.Now.Subtract(submitTime);
                    double kbSize = s_history.Length / 1024.0;
                    Logger.Instance.WriteToLog("RecvSaveHistory " + kbSize.ToString() + " " + difference.ToString() + " " + userName + " page num " + pageNumber.ToString());
                    CLP.Models.CLPHistory interpolatedHistory = CLP.Models.CLPHistory.InterpolateHistory(history);
                    //Database call
                    if (App.DatabaseUse == App.DatabaseMode.Using)
                    {
                        //save as submission and as page save
                        CLPServiceAgent.Instance.SaveHistoryDB(interpolatedHistory, pageID, userName, submitTime);

                    }
                }
                return null;
            }, null);
        }

        public void SubmitFullPage(string s_page, string userName, string notebookName)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {

                        

                    if (App.CurrentUserMode == App.UserMode.Instructor || App.CurrentUserMode == App.UserMode.Projector)
                    {
                        //Deserialize Using Protobuf
                        //Stream stream = new MemoryStream(Convert.FromBase64String(s_page));
                        //CLP.Models.CLPPage page = new CLP.Models.CLPPage();
                        //page = Serializer.Deserialize<CLP.Models.CLPPage>(stream);

                        CLPPage page = (ObjectSerializer.ToObject(s_page) as CLPPage);

                        foreach(ICLPPageObject pageObject in page.PageObjects)
                        {
                            pageObject.ParentPage = page;
                        }
                        //interpolate the history to make it bigger again - claire
                        //History is sent separately- Jessie
                        //CLPHistory interpolatedHistory = CLPHistory.InterpolateHistory(page.PageHistory);
                        //CLPHistory.ReplaceHistoryItems(page.PageHistory, interpolatedHistory);

                        page.IsSubmission = true;
                        page.SubmitterName = userName;

                        try
                        {
                            foreach (var notebook in App.MainWindowViewModel.OpenNotebooks)
                            {
                                if (page.ParentNotebookID == notebook.UniqueID)
                                {
                                    CLPServiceAgent.Instance.AddSubmission(notebook, page);
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.WriteToLog("[ERROR] Recieved Submission from wrong notebook: " + e.Message);
                        }
                    }
                    else if (App.CurrentUserMode == App.UserMode.Server)
                    {
                        //Deserialize Using Protobuf
                        Stream stream = new MemoryStream(Convert.FromBase64String(s_page));
                        CLP.Models.CLPPage page = new CLP.Models.CLPPage();
                        page = Serializer.Deserialize<CLP.Models.CLPPage>(stream);
                        pagecount++;

                        double kbSize = s_page.Length / 1024.0;
                        Logger.Instance.WriteToLog("RecvSubmission " + kbSize.ToString() + " "+ DateTime.Now.ToString()+ " " + userName);
                        //Database call
                        if (App.DatabaseUse == App.DatabaseMode.Using)
                        {
                            //save as submission and as page save
                            CLPServiceAgent.Instance.SavePageDB(page, userName, true, DateTime.Now, notebookName);
                            CLPServiceAgent.Instance.SavePageDB(page, userName, false, DateTime.Now, notebookName);
                        }
                    }
                    return null;
                }, null);
        }

        public void SubmitPage(string userName, string submissionID, string submissionTime, string s_history, string s_pageObjects, List<string> inkStrokes)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
             {


                 //foreach (var notebook in App.MainWindowViewModel.OpenNotebooks)
                 //{
                 //    if (page.ParentNotebookID == notebook.UniqueID)
                 //    {
                 //        CLPServiceAgent.Instance.AddSubmission(notebook, page);
                 //        break;
                 //    }
                 //}

                 return null;
             }, null);
        }

        public void SavePage(string s_page, string userName, DateTime submitTime, string notebookName, int pageNumber)
        {
            
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (DispatcherOperationCallback)delegate(object arg)
            {
                if (App.CurrentUserMode == App.UserMode.Server && App.DatabaseUse == App.DatabaseMode.Using)
                {

                    //Database call
                    TimeSpan difference = DateTime.Now.Subtract(submitTime);
                    double kbSize = s_page.Length / 1024.0;
                    Logger.Instance.WriteToLog("RecvSave " + kbSize.ToString() + " " + difference.ToString() + " " + userName + " pageNum: " + pageNumber.ToString());
                    if (App.DatabaseUse == App.DatabaseMode.Using)
                    {
                        //CLPPage page = (ObjectSerializer.ToObject(s_page) as CLPPage);
                        //Deserialize Using Protobuf
                        Stream stream = new MemoryStream(Convert.FromBase64String(s_page));
                        CLP.Models.CLPPage page = new CLP.Models.CLPPage();
                        page = Serializer.Deserialize<CLP.Models.CLPPage>(stream);
                        CLPServiceAgent.Instance.SavePageDB(page,userName, false, DateTime.Now, notebookName);
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
                CLP.Models.CLPNotebook notebook = (ObjectSerializer.ToObject(s_notebook) as CLP.Models.CLPNotebook);
                CLPServiceAgent.Instance.SaveNotebookDB(notebook, userName);

            }
        }

        public void DistributeNotebook(string s_notebook, string author)
        {
            if (App.CurrentUserMode == App.UserMode.Server && App.DatabaseUse == App.DatabaseMode.Using)
            {
                CLP.Models.CLPNotebook notebook = (ObjectSerializer.ToObject(s_notebook) as CLP.Models.CLPNotebook);
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
                CLP.Models.CLPNotebook notebook = (ObjectSerializer.ToObject(s_notebook) as CLP.Models.CLPNotebook);
                CLPServiceAgent.Instance.SaveNotebooksFromDBToHD(notebook);
                //reload notebook chooser window if already open
                //if (App.MainWindowViewModel.Workspace.GetType().Equals(new NotebookChooserWorkspaceViewModel().GetType()))
                //{
                //    App.MainWindowViewModel.Workspace = new NotebookChooserWorkspaceViewModel();
                //}
            }
        }

        public void BroadcastInk(List<List<byte>> strokesAdded, List<List<byte>> strokesRemoved, string pageID, bool broadcastInkToStudents)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        foreach (var notebook in App.MainWindowViewModel.OpenNotebooks)
                        {
                            CLP.Models.CLPPage page = notebook.GetNotebookPageByID(pageID);

                            if (page == null)
                            {
                                page = notebook.GetSubmissionByID(pageID);
                            }

                            if(page != null)
                            {
                                StrokeCollection strokesToRemove = CLPPage.BytesToStrokes(new ObservableCollection<List<byte>>(strokesRemoved));

                                var strokes =
                                    from externalStroke in strokesToRemove
                                    from stroke in page.InkStrokes
                                    where (stroke.GetPropertyData(CLPPage.StrokeIDKey) as string) == (externalStroke.GetPropertyData(CLPPage.StrokeIDKey) as string)
                                    select stroke;

                                StrokeCollection actualStrokesToRemove = new StrokeCollection(strokes.ToList());

                                page.InkStrokes.Remove(actualStrokesToRemove);

                                StrokeCollection strokesToAdd = CLPPage.BytesToStrokes(new ObservableCollection<List<byte>>(strokesAdded));
                                page.InkStrokes.Add(strokesToAdd);
                            }


                            //Steve - Fix for page.Inkstrokes
                            //if (page != null)
                            //{
                            //    StrokeCollection removedStrokes = CLPPage.StringsToStrokes(new ObservableCollection<string>(strokesRemoved));

                            //    foreach (var strokeToRemove in removedStrokes)
                            //    {
                            //        int strokeIndex = -1;
                            //        foreach (var stroke in page.InkStrokes)
                            //        {
                            //            if ((stroke.GetPropertyData(CLPPage.StrokeIDKey) as string) == (strokeToRemove.GetPropertyData(CLPPage.StrokeIDKey) as string))
                            //            {
                            //                strokeIndex = page.InkStrokes.IndexOf(stroke);
                            //                break;
                            //            }
                            //        }
                            //        try
                            //        {
                            //            page.InkStrokes.RemoveAt(strokeIndex);
                            //        }
                            //        catch (System.Exception ex)
                            //        {
                            //            Logger.Instance.WriteToLog("[ERROR] - Failed to remove stroke from page on Projector. " + ex.Message);
                            //        }
                            //    }

                            //    StrokeCollection addedStrokes = CLPPage.StringsToStrokes(new ObservableCollection<string>(strokesAdded));
                            //    page.InkStrokes.Add(addedStrokes);
                            //    break;
                            //}
                        }
                    }

                    if(App.CurrentUserMode == App.UserMode.Student && broadcastInkToStudents)
                    {
                        foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                        {
                            CLP.Models.CLPPage page = notebook.GetNotebookPageByID(pageID);

                            if(page == null)
                            {
                                page = notebook.GetSubmissionByID(pageID);
                            }

                            //Steve - fix for page.InkStrokes
                            //if(page != null)
                            //{
                            //    StrokeCollection removedStrokes = CLPPage.StringsToStrokes(new ObservableCollection<string>(strokesRemoved));

                            //    foreach(var strokeToRemove in removedStrokes)
                            //    {
                            //        int strokeIndex = -1;
                            //        foreach(var stroke in page.InkStrokes)
                            //        {
                            //            if((stroke.GetPropertyData(CLPPage.StrokeIDKey) as string) == (strokeToRemove.GetPropertyData(CLPPage.StrokeIDKey) as string))
                            //            {
                            //                strokeIndex = page.InkStrokes.IndexOf(stroke);
                            //                break;
                            //            }
                            //        }
                            //        try
                            //        {
                            //            page.InkStrokes.RemoveAt(strokeIndex);
                            //        }
                            //        catch(System.Exception ex)
                            //        {
                            //            Logger.Instance.WriteToLog("[ERROR] - Failed to remove stroke from page on Projector. " + ex.Message);
                            //        }
                            //    }

                            //    StrokeCollection addedStrokes = CLPPage.StringsToStrokes(new ObservableCollection<string>(strokesAdded));
                            //    page.InkStrokes.Add(addedStrokes);
                            //    break;
                            //}
                        }
                    }
                    return null;
                }, null);
        }

        public void SwitchProjectorDisplay(string displayType, List<string> displayPages)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        if (displayType == "LinkedDisplay")
                        {
                            (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).LinkedDisplay;

                            AddPageToDisplay(displayPages[0]);
                        }
                        else
                        {
                            bool isNewDisplay = true;
                            foreach(GridDisplayViewModel gridDisplay in (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).GridDisplays)
                            {
                                if (gridDisplay.DisplayID == displayType)
                                {
                                    gridDisplay.DisplayedPages.Clear();
                                    (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay = gridDisplay;

                                    isNewDisplay = false;
                                    break;
                                }
                            }

                            if (isNewDisplay)
                            {
                                GridDisplayViewModel newGridDisplay = new GridDisplayViewModel();
                                newGridDisplay.DisplayID = displayType;
                                newGridDisplay.DisplayedPages.Clear();
                                (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).GridDisplays.Add(newGridDisplay);

                                (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay = newGridDisplay;
                            }

                            foreach(var pageID in displayPages)
                            {
                                AddPageToDisplay(pageID);
                            }
                        }
                    }
                    return null;
                }, null);
        }

        public void AddPageToDisplay(string pageID)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        foreach (var notebook in App.MainWindowViewModel.OpenNotebooks)
                        {
                            CLP.Models.CLPPage page = notebook.GetNotebookPageByID(pageID);

                            if (page == null)
                            {
                                page = notebook.GetSubmissionByID(pageID);
                            }

                            if (page != null)
                            {
                            	(App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay.AddPageToDisplay(page);
                                break;
                            }
                        }
                    }
                    return null;
                }, null);
        }

        public void ChangePageObjectsOnPage(string pageID, List<string> added, List<string> removedIDs)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if (App.CurrentUserMode == App.UserMode.Projector)
                    {
                        foreach (var notebook in App.MainWindowViewModel.OpenNotebooks)
                        {
                            CLP.Models.CLPPage page = notebook.GetNotebookPageByID(pageID);

                            if (page == null)
                            {
                                page = notebook.GetSubmissionByID(pageID);
                            }

                            if (page != null)
                            {
                                foreach (string pageObjectString in added)
                                {
                                    CLP.Models.ICLPPageObject pageObject = ObjectSerializer.ToObject(pageObjectString) as CLP.Models.ICLPPageObject;
                                    CLPServiceAgent.Instance.AddPageObjectToPage(page, pageObject);
                                }
                                foreach (string id in removedIDs)
                                {
                                    CLP.Models.ICLPPageObject objectToRemove = null;
                                    foreach(CLP.Models.ICLPPageObject pageObject in page.PageObjects)
                                    {
                                        if (pageObject.UniqueID == id)
                                        {
                                            objectToRemove = pageObject;
                                            break;
                                        }
                                    }
                                    if (objectToRemove != null)
                                    {
                                        CLPServiceAgent.Instance.RemovePageObjectFromPage(page, objectToRemove);
                                    }
                                }
                                
                                break;
                            }
                        }
                        
                    }
                    return null;
                }, null);
        }
    }
}