using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Threading;
using Catel.Data;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface IProjectorContract : INotebookContract
    {
        [OperationContract]
        void SwitchProjectorDisplay(string displayID, int displayNumber);

        [OperationContract]
        void AddPageToDisplay(string pageID, string pageOwnerID, string differentiationLevel, int pageVersionIndex);

        [OperationContract]
        void RemovePageFromDisplay(string pageID, string pageOwnerID, string differentiationLevel, int pageVersionIndex);

        [OperationContract]
        void AddSerializedSubmission(string zippedPage, string notebookID);

        [OperationContract]
        void ScrollPage(string pageID, string submissionID, double offset);
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class ProjectorService : IProjectorContract
    {
        public void SwitchProjectorDisplay(string displayID, int displayNumber)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
                                                                                        if(notebookWorkspaceViewModel == null)
                                                                                        {
                                                                                            return null;
                                                                                        }
                                                                                        if(App.CurrentUserMode == App.UserMode.Projector)
                                                                                        {
                                                                                            if(displayID == "SingleDisplay")
                                                                                            {
                                                                                                notebookWorkspaceViewModel.CurrentDisplay = null;
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                var isNewDisplay = true;
                                                                                                foreach(var gridDisplay in
                                                                                                    notebookWorkspaceViewModel.Displays.Where(gridDisplay =>
                                                                                                                                              gridDisplay.ID == displayID &&
                                                                                                                                              gridDisplay is GridDisplay))
                                                                                                {
                                                                                                    notebookWorkspaceViewModel.CurrentDisplay = gridDisplay;

                                                                                                    isNewDisplay = false;
                                                                                                    break;
                                                                                                }

                                                                                                if(isNewDisplay)
                                                                                                {
                                                                                                    var newGridDisplay = new GridDisplay
                                                                                                                         {
                                                                                                                             ID = displayID,
                                                                                                                             DisplayNumber = displayNumber,
                                                                                                                             NotebookID = notebookWorkspaceViewModel.Notebook.ID
                                                                                                                         };
                                                                                                    notebookWorkspaceViewModel.Notebook.Displays.Add(newGridDisplay);
                                                                                                    notebookWorkspaceViewModel.CurrentDisplay = null;
                                                                                                    notebookWorkspaceViewModel.CurrentDisplay = newGridDisplay;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void AddPageToDisplay(string pageID, string pageOwnerID, string differentiationLevel, int pageVersionIndex)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        if(App.CurrentUserMode == App.UserMode.Projector)
                                                                                        {
                                                                                            foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                                                                                            {
                                                                                           


                                                                                                var page = notebook.GetPageByCompositeKeys(pageID, pageOwnerID, pageVersionIndex);

                                                                                                if(page == null)
                                                                                                {
                                                                                                    continue;
                                                                                                }

                                                                                                var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
                                                                                                if(notebookWorkspaceViewModel == null)
                                                                                                {
                                                                                                    break;
                                                                                                }

                                                                                                if(notebookWorkspaceViewModel.CurrentDisplay == null)
                                                                                                {
                                                                                                    notebookWorkspaceViewModel.Notebook.CurrentPage = page;
                                                                                                    break;
                                                                                                }

                                                                                                notebookWorkspaceViewModel.CurrentDisplay.AddPageToDisplay(page);
                                                                                                break;
                                                                                            }
                                                                                        }
                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void RemovePageFromDisplay(string pageID, string pageOwnerID, string differentiationLevel, int pageVersionIndex)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        if(App.CurrentUserMode == App.UserMode.Projector)
                                                                                        {
                                                                                            foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                                                                                            {
                                                                                                var page = notebook.GetPageByCompositeKeys(pageID, pageOwnerID, pageVersionIndex);

                                                                                                if(page == null)
                                                                                                {
                                                                                                    continue;
                                                                                                }

                                                                                                var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
                                                                                                if(notebookWorkspaceViewModel != null)
                                                                                                {
                                                                                                    notebookWorkspaceViewModel.CurrentDisplay.RemovePageFromDisplay(page);
                                                                                                }
                                                                                                break;
                                                                                            }
                                                                                        }
                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void AddSerializedSubmission(string zippedPage, string notebookID)
        {
            var unZippedPage = CLPServiceAgent.Instance.UnZip(zippedPage);
            var submission = ObjectSerializer.ToObject(unZippedPage) as CLPPage;

            if(submission == null)
            {
                Logger.Instance.WriteToLog("Failed to receive student submission. Page or Submitter is null.");
                return;
            }
            submission.InkStrokes = StrokeDTO.LoadInkStrokes(submission.SerializedStrokes);
            submission.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(submission.History.SerializedTrashedInkStrokes);
            var currentNotebook = App.MainWindowViewModel.OpenNotebooks.FirstOrDefault(notebook => notebookID == notebook.ID && notebook.OwnerID == Person.Emily.ID);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        try
                                                                                        {
                                                                                            if(currentNotebook != null)
                                                                                            {
                                                                                                var page = currentNotebook.Pages.FirstOrDefault(x => x.ID == submission.ID);
                                                                                                if(page == null)
                                                                                                {
                                                                                                    return null;
                                                                                                }
                                                                                                page.Submissions.Add(submission);
                                                                                            }
                                                                                            //TODO: QuickSave
                                                                                        }
                                                                                        catch(Exception e)
                                                                                        {
                                                                                            Logger.Instance.WriteToLog("[ERROR] Recieved Submission from wrong notebook: " + e.Message);
                                                                                        }

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void ScrollPage(string pageID, string submissionID, double offset)
        {
            // TODO: Fix for lack of single/mirror display
            //var currentPage = NotebookPagesPanelViewModel.GetCurrentPage();
            //if(currentPage == null || currentPage.ID != pageID)
            //{
            //    return;
            //}

            //if(submissionID != "" &&
            //   submissionID != currentPage.SubmissionID)
            //{
            //    return;
            //}

            //var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            //if(notebookWorkspaceViewModel == null)
            //{
            //    return;
            //}

            //var mirrorDisplay = notebookWorkspaceViewModel.CurrentDisplay as SingleDisplay;
            //var mirrorDisplayViewModels = CLPServiceAgent.Instance.GetViewModelsFromModel(mirrorDisplay);
            //var mirrorDisplayView = CLPServiceAgent.Instance.GetViewFromViewModel(mirrorDisplayViewModels.FirstOrDefault()) as SingleDisplayView;

            //if(mirrorDisplayView == null)
            //{
            //    return;
            //}

            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //                                           (DispatcherOperationCallback)delegate
            //                                                                        {
            //                                                                            mirrorDisplayView.MirrorDisplayScroller.ScrollToVerticalOffset(offset);
            //                                                                            return null;
            //                                                                        }, null);
        }

        #region INotebookContract Members

        public void OpenClassPeriod(string zippedClassPeriod, string zippedClassSubject)
        {
            var unZippedClassPeriod = CLPServiceAgent.Instance.UnZip(zippedClassPeriod);
            var classPeriod = ObjectSerializer.ToObject(unZippedClassPeriod) as ClassPeriod;
            if(classPeriod == null)
            {
                Logger.Instance.WriteToLog("Failed to load classperiod.");
                return;
            }

            var unZippedClassSubject = CLPServiceAgent.Instance.UnZip(zippedClassSubject);
            var classSubject = ObjectSerializer.ToObject(unZippedClassSubject) as ClassSubject;
            if(classSubject == null)
            {
                Logger.Instance.WriteToLog("Failed to load classperiod.");
                return;
            }

            classPeriod.ClassSubject = classSubject;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        App.MainWindowViewModel.CurrentClassPeriod = classPeriod;
                                                                                        App.MainWindowViewModel.AvailableUsers = classPeriod.ClassSubject.StudentList;

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void OpenPartialNotebook(string zippedNotebook)
        {
            var unZippedNotebook = CLPServiceAgent.Instance.UnZip(zippedNotebook);
            var notebook = ObjectSerializer.ToObject(unZippedNotebook) as Notebook;
            if(notebook == null)
            {
                Logger.Instance.WriteToLog("Failed to load notebook.");
                return;
            }
            notebook.CurrentPage = notebook.Pages.First();
            foreach(var page in notebook.Pages)
            {
                page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
                page.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(page.History.SerializedTrashedInkStrokes);
            }

            foreach(var page in notebook.Pages)
            {
                foreach(var notebookName in MainWindowViewModel.AvailableLocalNotebookNames)
                {
                    var notebookInfo = notebookName.Split(';');
                    if(notebookInfo.Length != 4 ||
                       notebookInfo[3] == Person.Author.ID ||
                       notebookInfo[3] == Person.Emily.ID ||
                       notebookInfo[3] == Person.EmilyProjector.ID)
                    {
                        continue;
                    }

                    var folderPath = Path.Combine(App.NotebookCacheDirectory, notebookName);
                    if(!Directory.Exists(folderPath))
                    {
                        continue;
                    }

                    var submissionsPath = Path.Combine(folderPath, "Pages");
                    if(!Directory.Exists(submissionsPath))
                    {
                        continue;
                    }

                    var submissionPaths = Directory.EnumerateFiles(submissionsPath, "*.xml");

                    foreach(var submissionPath in submissionPaths)
                    {
                        var submissionFileName = Path.GetFileNameWithoutExtension(submissionPath);
                        var submissionInfo = submissionFileName.Split(';');
                        if(submissionInfo.Length == 5 &&
                           submissionInfo[2] == page.ID &&
                           submissionInfo[4] != "0")
                        {
                            var submission = ModelBase.Load<CLPPage>(submissionPath, SerializationMode.Xml);
                            page.Submissions.Add(submission);
                        }
                    }
                }
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        App.MainWindowViewModel.OpenNotebooks.Add(notebook);
                                                                                        App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void AddHistoryItem(string compositePageID, string zippedHistoryItem)
        {
            var compositeKeys = compositePageID.Split(';');
            var pageID = compositeKeys[0];
            var pageOwnerID = compositeKeys[1];
            var versionIndex = -1;
            Int32.TryParse(compositeKeys[2], out versionIndex);

            var unzippedHistoryItem = CLPServiceAgent.Instance.UnZip(zippedHistoryItem);
            var historyItem = ObjectSerializer.ToObject(unzippedHistoryItem) as IHistoryItem;
            if(historyItem == null)
            {
                Logger.Instance.WriteToLog("Failed to apply historyItem to projector.");
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate
                {
                    foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                    {
                        var page = notebook.GetPageByCompositeKeys(pageID, pageOwnerID, versionIndex);

                        if(page == null)
                        {
                            continue;
                        }

                        historyItem.ParentPage = page;
                        historyItem.UnpackHistoryItem();
                        page.History.RedoItems.Clear();
                        page.History.RedoItems.Add(historyItem);
                        page.History.Redo();

                        break;
                    }
                    return null;
                }, null);
        }

        public void AddNewPage(string zippedPage, int index)
        {
            var unZippedPage = CLPServiceAgent.Instance.UnZip(zippedPage);
            var page = ObjectSerializer.ToObject(unZippedPage) as CLPPage;

            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
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
                                                                                            notebookWorkspaceViewModel.Notebook.AddCLPPageToNotebook(page);
                                                                                        }

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void ReplacePage(string zippedPage, int index)
        {
            var unZippedPage = CLPServiceAgent.Instance.UnZip(zippedPage);
            var page = ObjectSerializer.ToObject(unZippedPage) as CLPPage;

            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
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