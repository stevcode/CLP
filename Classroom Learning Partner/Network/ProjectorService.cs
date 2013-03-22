using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Threading;
using Catel.IoC;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;
using Classroom_Learning_Partner.Views;
using System.Security.Cryptography;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface IProjectorContract : INotebookContract
    {
        [OperationContract]
        void SwitchProjectorDisplay(string displayType, List<string> displayPages);

        [OperationContract]
        void AddPageToDisplay(string pageID);

        [OperationContract]
        void AddStudentSubmission(ObservableCollection<List<byte>> byteStrokes,
            ObservableCollection<ICLPPageObject> pageObjects,
            Person submitter, Group groupSubmitter,
            string notebookID, string pageID, string submissionID, DateTime submissionTime,
            bool isGroupSubmission, double pageHeight, List<byte> image);

        [OperationContract]
        void ScrollPage(string pageID, string submissionID, double offset);
    }

    public class ProjectorService : IProjectorContract
    {
        public ProjectorService() { }

        public void SwitchProjectorDisplay(string displayType, List<string> displayPages)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    if(App.CurrentUserMode == App.UserMode.Projector)
                    {
                        if(displayType == "LinkedDisplay")
                        {
                            (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).LinkedDisplay;

                            AddPageToDisplay(displayPages[0]);
                        }
                        else
                        {
                            bool isNewDisplay = true;
                            foreach(GridDisplayViewModel gridDisplay in (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).GridDisplays)
                            {
                                if(gridDisplay.DisplayID == displayType)
                                {
                                    gridDisplay.DisplayedPages.Clear();
                                    (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay = gridDisplay;

                                    isNewDisplay = false;
                                    break;
                                }
                            }

                            if(isNewDisplay)
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
                    if(App.CurrentUserMode == App.UserMode.Projector)
                    {
                        foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                        {
                            CLP.Models.CLPPage page = notebook.GetNotebookPageByID(pageID);

                            if(page == null)
                            {
                                page = notebook.GetSubmissionByID(pageID);
                            }

                            if(page != null)
                            {
                                (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay.AddPageToDisplay(page);
                                break;
                            }
                        }
                    }
                    return null;
                }, null);
        }

        public void AddStudentSubmission(ObservableCollection<List<byte>> byteStrokes, 
            ObservableCollection<ICLPPageObject> pageObjects,
            Person submitter, Group groupSubmitter,
            string notebookID, string pageID, string submissionID, DateTime submissionTime,
            bool isGroupSubmission, double pageHeight, List<byte> image)
        {
            CLPPage submission = null;
            CLPNotebook currentNotebook = null;

            foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
            {
                if(notebookID == notebook.UniqueID)
                {
                    currentNotebook = notebook;
                    submission = notebook.GetNotebookPageByID(pageID).Clone() as CLPPage;
                    break;
                }
            }

            if(submission != null)
            {
                submission.ByteStrokes = byteStrokes;
                submission.InkStrokes = CLPPage.BytesToStrokes(byteStrokes);

                submission.IsSubmission = true;
                submission.IsGroupSubmission = true;
                submission.SubmissionID = submissionID;
                submission.SubmissionTime = submissionTime;
                submission.SubmitterName = submitter.FullName;
                submission.Submitter = submitter;
                submission.GroupSubmitter = groupSubmitter;
                submission.PageHeight = pageHeight;

                if(submission.PageIndex == 25)
                {
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    byte[] hash = md5.ComputeHash(image.ToArray());
                    string imageID = Convert.ToBase64String(hash);

                    if(!submission.ImagePool.ContainsKey(imageID))
                    {
                        submission.ImagePool.Add(imageID, image);
                    }
                    CLPImage imagePO = new CLPImage(imageID, submission);
                    imagePO.IsBackground = true;
                    imagePO.Height = 450;
                    imagePO.Width = 600;
                    imagePO.YPosition = 225;
                    imagePO.XPosition = 108;

                    submission.PageObjects.Add(imagePO);
                }

                foreach(ICLPPageObject pageObject in pageObjects)
                {
                    submission.PageObjects.Add(pageObject);
                }

                foreach(ICLPPageObject pageObject in submission.PageObjects)
                {
                    pageObject.ParentPage = submission;
                    if(pageObject is ISubmittable)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (DispatcherOperationCallback)delegate(object arg)
                            {
                                (pageObject as ISubmittable).AfterSubmit(isGroupSubmission, currentNotebook);
                                return null;
                            }, null);
                    }
                }

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    try
                    {
                        CLPServiceAgent.Instance.AddSubmission(currentNotebook, submission);
                    }
                    catch(Exception e)
                    {
                        Logger.Instance.WriteToLog("[ERROR] Recieved Submission from wrong notebook: " + e.Message);
                    }

                    return null;
                }, null);
            }

            //CLPServiceAgent.Instance.QuickSaveNotebook("RECIEVE-" + userName);
        }

        public void ScrollPage(string pageID, string submissionID, double offset)
        {
            if (App.MainWindowViewModel.SelectedWorkspace is NotebookWorkspaceViewModel)
            {
                if((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay is LinkedDisplayViewModel)
                {
                    CLPPage currentPage = ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;

                    if(currentPage.UniqueID == pageID)
                    {
                        if(submissionID == "" || submissionID == currentPage.SubmissionID)
                        {
                            var viewManager = ServiceLocator.Instance.ResolveType<IViewManager>();
                            var views = viewManager.GetViewsOfViewModel((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel);

                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (DispatcherOperationCallback)delegate(object arg)
                            {
                                (views[0] as LinkedDisplayView).MirrorDisplayScroller.ScrollToVerticalOffset(offset);
                                return null;
                            }, null);
                        }
                    }
                }
            }
        }

        #region INotebookContract Members

        public void ModifyPageInkStrokes(List<List<byte>> strokesAdded, List<List<byte>> strokesRemoved, string pageID)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                    {
                        CLP.Models.CLPPage page = notebook.GetNotebookPageByID(pageID);

                        if(page == null)
                        {
                            page = notebook.GetSubmissionByID(pageID);
                        }

                        if(page != null)
                        {
                            StrokeCollection strokesToRemove = CLPPage.BytesToStrokes(new ObservableCollection<List<byte>>(strokesRemoved));

                            var strokes =
                                from externalStroke in strokesToRemove
                                from stroke in page.InkStrokes
                                where stroke.GetStrokeUniqueID() == externalStroke.GetStrokeUniqueID()
                                select stroke;

                            StrokeCollection actualStrokesToRemove = new StrokeCollection(strokes.ToList());

                            page.InkStrokes.Remove(actualStrokesToRemove);

                            StrokeCollection strokesToAdd = CLPPage.BytesToStrokes(new ObservableCollection<List<byte>>(strokesAdded));
                            page.InkStrokes.Add(strokesToAdd);
                            break;
                        }
                    }
                    return null;
                }, null);
        }

        public void AddNewPage(string s_page, int index) { }

        public void ReplacePage(string s_page, int index) { }
        
        #endregion
    }
}
