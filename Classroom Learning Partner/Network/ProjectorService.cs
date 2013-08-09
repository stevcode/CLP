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
        void AddStudentSubmission(ObservableCollection<StrokeDTO> byteStrokes,
            ObservableCollection<ICLPPageObject> pageObjects,
            Person submitter, Group groupSubmitter,
            string notebookID, string pageID, string submissionID, DateTime submissionTime,
            bool isGroupSubmission, double pageHeight);

        [OperationContract]
        void AddSerializedSubmission(string sPage, Person submitter, Group groupSubmitter,
            DateTime submissionTime, bool isGroupSubmission, String notebookID, String submissionID);

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
                            var page = notebook.GetNotebookPageByID(pageID);

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

        public void AddStudentSubmission(ObservableCollection<StrokeDTO> byteStrokes, 
            ObservableCollection<ICLPPageObject> pageObjects,
            Person submitter, Group groupSubmitter,
            string notebookID, string pageID, string submissionID, DateTime submissionTime,
            bool isGroupSubmission, double pageHeight)
        {
            CLPPage submission = null;
            CLPNotebook currentNotebook = null;

            foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
            {
                if(notebookID == notebook.UniqueID)
                {
                    currentNotebook = notebook;
                    submission = notebook.GetNotebookPageByID(pageID).Clone() as ICLPPage;
                    break;
                }
            }

            if(submission != null)
            {
                submission.SerializedStrokes = byteStrokes;
                submission.InkStrokes = CLPPage.LoadInkStrokes(byteStrokes);

                submission.IsSubmission = true;
                submission.IsGroupSubmission = true;
                submission.SubmissionID = submissionID;
                submission.SubmissionTime = submissionTime;
                submission.SubmitterName = submitter.FullName;
                submission.Submitter = submitter;
                submission.GroupSubmitter = groupSubmitter;
                submission.PageHeight = pageHeight;

                foreach(ICLPPageObject pageObject in pageObjects)
                {
                    submission.PageObjects.Add(pageObject);
                }

                foreach(ICLPPageObject pageObject in submission.PageObjects)
                {
                    pageObject.ParentPage = submission;
                    if(pageObject is ISubmittable)
                    {
                        ICLPPageObject o = pageObject;
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (DispatcherOperationCallback)delegate
                                {
                                    var submittable = o as ISubmittable;
                                    if(submittable != null)
                                    {
                                        submittable.AfterSubmit(isGroupSubmission, currentNotebook);
                                    }
                                    return null;
                                }, null);
                    }
                }

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate
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

        public void AddSerializedSubmission(string sPage, Person submitter, Group groupSubmitter,
            DateTime submissionTime, bool isGroupSubmission, String notebookID, String submissionID)
        {
            var submission = ObjectSerializer.ToObject(sPage) as CLPPage;
            if(submission != null)
            {
                submission.IsSubmission = true;
                submission.IsGroupSubmission = isGroupSubmission;
                submission.SubmissionID = submissionID;
                submission.SubmissionTime = submissionTime;
                submission.SubmitterName = submitter.FullName;
                submission.Submitter = submitter;
                submission.GroupSubmitter = groupSubmitter;
                submission.InkStrokes = CLPPage.LoadInkStrokes(submission.SerializedStrokes);

                foreach(ICLPPageObject pageObject in submission.PageObjects)
                {
                    pageObject.ParentPage = submission;
                }
            }

            var currentNotebook = App.MainWindowViewModel.OpenNotebooks.FirstOrDefault(notebook => notebookID == notebook.UniqueID);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate
                    {
                    try
                    {
                        CLPServiceAgent.Instance.AddSubmission(currentNotebook, submission);
                        //CLPServiceAgent.Instance.QuickSaveNotebook("RECIEVE-" + userName);
                    }
                    catch(Exception e)
                    {
                        Logger.Instance.WriteToLog("[ERROR] Recieved Submission from wrong notebook: " + e.Message);
                    }

                    return null;
                }, null);
        }

        public void ScrollPage(string pageID, string submissionID, double offset)
        {
            if (App.MainWindowViewModel.SelectedWorkspace is NotebookWorkspaceViewModel)
            {
                if((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay is LinkedDisplayViewModel)
                {
                    var currentPage = ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage;

                    if(currentPage.UniqueID == pageID)
                    {
                        if(submissionID == "" || submissionID == currentPage.SubmissionID)
                        {
                            var viewManager = ServiceLocator.Default.ResolveType<IViewManager>();
                            var views = viewManager.GetViewsOfViewModel((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel);

                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (DispatcherOperationCallback)delegate
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

        public void ModifyPageInkStrokes(List<StrokeDTO> strokesAdded, List<StrokeDTO> strokesRemoved, string pageID)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate
                    {
                    foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                    {
                        CLPPage page = notebook.GetNotebookPageByID(pageID);

                        if(page == null)
                        {
                            page = notebook.GetSubmissionByID(pageID);
                        }

                        if(page != null)
                        {
                            StrokeCollection strokesToRemove = CLPPage.LoadInkStrokes(new ObservableCollection<StrokeDTO>(strokesRemoved));

                            var strokes =
                                from externalStroke in strokesToRemove
                                from stroke in page.InkStrokes
                                where stroke.GetStrokeUniqueID() == externalStroke.GetStrokeUniqueID()
                                select stroke;

                            StrokeCollection actualStrokesToRemove = new StrokeCollection(strokes.ToList());

                            page.InkStrokes.Remove(actualStrokesToRemove);

                            StrokeCollection strokesToAdd = CLPPage.LoadInkStrokes(new ObservableCollection<StrokeDTO>(strokesAdded));
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
