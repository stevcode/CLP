using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Threading;
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
        void RemovePageFromDisplay(string pageID);

        [OperationContract]
        void AddSerializedSubmission(string zippedPage, string submissionID,
            DateTime submissionTime, string notebookID, string zippedSubmitter);

        [OperationContract]
        void ScrollPage(string pageID, string submissionID, double offset);
    }

    public class ProjectorService : IProjectorContract
    {
        public void SwitchProjectorDisplay(string displayType, List<string> displayPages)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate
                                             {
                                                 var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
                                                 if(notebookWorkspaceViewModel == null)
                                                 {
                                                     return null;
                                                 }
                    if(App.CurrentUserMode == App.UserMode.Projector)
                    {
                        if(displayType == "MirrorDisplay")
                        {

                                notebookWorkspaceViewModel.SelectedDisplay = notebookWorkspaceViewModel.MirrorDisplay;


                            AddPageToDisplay(displayPages[0]);
                        }
                        else
                        {
                            var isNewDisplay = true;
                            foreach(var gridDisplay in notebookWorkspaceViewModel.Displays.Where(gridDisplay => gridDisplay.UniqueID == displayType && gridDisplay is CLPGridDisplay)) 
                            {
                                (gridDisplay as CLPGridDisplay).Pages.Clear();
                                notebookWorkspaceViewModel.SelectedDisplay = gridDisplay;

                                isNewDisplay = false;
                                break;
                            }

                            if(isNewDisplay)
                            {
                                var newGridDisplay = new CLPGridDisplay
                                                     {
                                                         UniqueID = displayType
                                                     };
                                newGridDisplay.Pages.Clear();
                                notebookWorkspaceViewModel.Notebook.Displays.Add(newGridDisplay);

                                notebookWorkspaceViewModel.SelectedDisplay = newGridDisplay;
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
                (DispatcherOperationCallback)delegate
                                             {
                    if(App.CurrentUserMode == App.UserMode.Projector)
                    {
                        foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                        {
                            var page = notebook.GetNotebookPageByID(pageID) ?? notebook.GetSubmissionByID(pageID);

                            if(page == null)
                            {
                                continue;
                            }

                            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
                            if(notebookWorkspaceViewModel != null)
                            {
                                notebookWorkspaceViewModel.SelectedDisplay.AddPageToDisplay(page);
                            }
                            break;
                        }
                    }
                    return null;
                }, null);
        }

        public void RemovePageFromDisplay(string pageID)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate
                {
                    if(App.CurrentUserMode == App.UserMode.Projector)
                    {
                        foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                        {
                            var page = notebook.GetNotebookPageByID(pageID) ?? notebook.GetSubmissionByID(pageID);

                            if(page == null)
                            {
                                continue;
                            }

                            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
                            if(notebookWorkspaceViewModel != null)
                            {
                                notebookWorkspaceViewModel.SelectedDisplay.RemovePageFromDisplay(page);
                            }
                            break;
                        }
                    }
                    return null;
                }, null);
        }

        public void AddSerializedSubmission(string zippedPage, string submissionID,
            DateTime submissionTime, string notebookID, string zippedSubmitter)
        {
            var unZippedPage = CLPServiceAgent.Instance.UnZip(zippedPage);
            var page = ObjectSerializer.ToObject(unZippedPage);
            ICLPPage submission = null;
            if(page is CLPPage)
            {
                submission = page as CLPPage;
            }
            else if(page is CLPAnimationPage)
            {
                submission = page as CLPAnimationPage;
            }

            var unZippedSubmitter = CLPServiceAgent.Instance.UnZip(zippedSubmitter);
            var submitter = ObjectSerializer.ToObject(unZippedSubmitter) as Person;

            if(submission == null || submitter == null)
            {
                Logger.Instance.WriteToLog("Failed to receive student submission. Page or Submitter is null.");
                return;
            }
            submission.SubmissionType = SubmissionType.Single;
            submission.SubmissionID = submissionID;
            submission.SubmissionTime = submissionTime;
            submission.Submitter = submitter;

            ACLPPageBase.Deserialize(submission);

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
                                                               Logger.Instance.WriteToLog("[ERROR] Recieved Submission from wrong notebook: " +
                                                                                          e.Message);
                                                           }

                                                           return null;
                                                       },
                                                       null);
        }

        public void ScrollPage(string pageID, string submissionID, double offset)
        {
            var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            if(currentPage == null || currentPage.UniqueID != pageID)
            {
                return;
            }

            if(submissionID != "" &&
               submissionID != currentPage.SubmissionID)
            {
                return;
            }

            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            var mirrorDisplay = notebookWorkspaceViewModel.SelectedDisplay as CLPMirrorDisplay;
            var mirrorDisplayViewModels = CLPServiceAgent.Instance.GetViewModelsFromModel(mirrorDisplay);
            var mirrorDisplayView = CLPServiceAgent.Instance.GetViewFromViewModel(mirrorDisplayViewModels.FirstOrDefault()) as MirrorDisplayView;

            if(mirrorDisplayView == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        mirrorDisplayView.MirrorDisplayScroller.ScrollToVerticalOffset(offset);
                                                                                        return null;
                                                                                    }, null);
        }

        #region INotebookContract Members

        public void ModifyPageInkStrokes(List<StrokeDTO> strokesAdded, List<StrokeDTO> strokesRemoved, string pageID)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate
                {
                    foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                    {
                        var page = notebook.GetNotebookPageByID(pageID);

                        if(page == null)
                        {
                            continue;
                        }

                        var strokesToRemove = StrokeDTO.LoadInkStrokes(new ObservableCollection<StrokeDTO>(strokesRemoved));

                        var strokes =
                            from externalStroke in strokesToRemove
                            from stroke in page.InkStrokes
                            where stroke.GetStrokeUniqueID() == externalStroke.GetStrokeUniqueID()
                            select stroke;

                        var actualStrokesToRemove = new StrokeCollection(strokes.ToList());

                        page.InkStrokes.Remove(actualStrokesToRemove);

                        var strokesToAdd = StrokeDTO.LoadInkStrokes(new ObservableCollection<StrokeDTO>(strokesAdded));
                        page.InkStrokes.Add(strokesToAdd);
                        break;
                    }
                    return null;
                }, null);
        }

        public void AddNewPage(string zippedPage, int index)
        {
            var unZippedPage = CLPServiceAgent.Instance.UnZip(zippedPage);
            var page = ObjectSerializer.ToObject(unZippedPage) as ICLPPage;

            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null ||
               page == null)
            {
                Logger.Instance.WriteToLog("Failed to add broadcasted page.");
                return;
            }
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        if(index < notebookWorkspaceViewModel.Notebook.Pages.Count)
                                                                                        {
                                                                                            notebookWorkspaceViewModel.Notebook.InsertPageAt(index, page);
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            notebookWorkspaceViewModel.Notebook.AddPage(page);
                                                                                        }
                                                           return null;
                                                       },
                                                       null);
        }

        public void ReplacePage(string zippedPage, int index)
        {
            var unZippedPage = CLPServiceAgent.Instance.UnZip(zippedPage);
            var page = ObjectSerializer.ToObject(unZippedPage) as ICLPPage;

            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null ||
               page == null)
            {
                Logger.Instance.WriteToLog("Failed to add broadcasted page.");
                return;
            }
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                       {
                                                           notebookWorkspaceViewModel.Notebook.RemovePageAt(index);
                                                           notebookWorkspaceViewModel.Notebook.InsertPageAt(index, page);

                                                           return null;
                                                       },
                                                       null);
        }
        
        #endregion
    }
}
