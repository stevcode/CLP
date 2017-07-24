using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM.Views;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Views;
using CLP.Entities;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface IProjectorContract : INotebookContract
    {
        [OperationContract]
        void FreezeProjector(bool isFreezing);

        [OperationContract]
        void SwitchProjectorDisplay(string displayID, int displayNumber);

        [OperationContract]
        void CreateGridDisplayAndAddPage(string displayID, int displayNumber, string pageID, string pageOwnerID, string differentiationLevel, uint pageVersionIndex);

        [OperationContract]
        void AddPageToDisplay(string pageID, string pageOwnerID, string differentiationLevel, uint pageVersionIndex, string displayID);

        [OperationContract]
        void RemovePageFromDisplay(string pageID, string pageOwnerID, string differentiationLevel, uint pageVersionIndex, string displayID);

        [OperationContract]
        void ScrollPage(double percentOffset);
        
        [OperationContract]
        void MakeCurrentPageLonger();

        [OperationContract]
        void RewindCurrentPage();


        [OperationContract]
        string AddStudentSubmission(string submissionJson, string notebookID);
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class ProjectorService : IProjectorContract
    {
        #region Message Constants

        public const string MESSAGE_NO_DATA_SERVICE = "no data service";
        public const string MESSAGE_NO_NETWORK_SERVICE = "no network service";
        public const string MESSAGE_SUCCESSFUL_STUDENT_LOG_IN = "successful student log in";
        public const string MESSAGE_STUDENT_NOT_IN_ROSTER = "student not in roster";
        public const string MESSAGE_NOTEBOOK_NOT_LOADED_BY_TEACHER = "notebook not loaded by teacher";
        public const string MESSAGE_SUBMISSION_NOT_DESERIALIZED = "submission not deserialized";
        public const string MESSAGE_PAGE_NOT_LOADED_IN_NOTEBOOK = "page not loaded in notebook";
        public const string MESSAGE_SUBMISSION_SUCCESSFUL = "submission successful";

        #endregion // Message Constants

        public void FreezeProjector(bool isFreezing)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        //take snapshot
                                                                                        if(isFreezing)
                                                                                        {
                                                                                            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
                                                                                            if(notebookWorkspaceViewModel == null)
                                                                                            {
                                                                                                return null;
                                                                                            }

                                                                                            byte[] screenShotByteSource = null;
                                                                                            if(notebookWorkspaceViewModel.CurrentDisplay == null)
                                                                                            {
                                                                                                var singleDisplayView = notebookWorkspaceViewModel.SingleDisplay.GetFirstView();
                                                                                                screenShotByteSource = (singleDisplayView as UIElement).ToImageByteArray();
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                var displayViewModels = (notebookWorkspaceViewModel.CurrentDisplay as IModel).GetAllViewModels();
                                                                                                foreach(var gridDisplayView in from displayViewModel in displayViewModels
                                                                                                                               where displayViewModel is GridDisplayViewModel && (displayViewModel as GridDisplayViewModel).IsDisplayPreview == false
                                                                                                                               select displayViewModel.GetFirstView())
                                                                                                {
                                                                                                    screenShotByteSource = (gridDisplayView as UIElement).ToImageByteArray();
                                                                                                }
                                                                                            }

                                                                                            if(screenShotByteSource == null)
                                                                                            {
                                                                                                return null;
                                                                                            }

                                                                                            var bitmapImage = screenShotByteSource.ToBitmapImage();

                                                                                            App.MainWindowViewModel.FrozenDisplayImageSource = bitmapImage;
                                                                                        }
                                                                                        App.MainWindowViewModel.IsProjectorFrozen = isFreezing;

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void SwitchProjectorDisplay(string displayID, int displayNumber)
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null ||
               App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Projector)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        if(displayID == "SingleDisplay")
                                                                                        {
                                                                                            notebookWorkspaceViewModel.CurrentDisplay = null;
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            //var isNewDisplay = true;
                                                                                            //foreach(var display in
                                                                                            //    notebookWorkspaceViewModel.Displays.Where(display =>
                                                                                            //                                              display.ID == displayID))
                                                                                            //{
                                                                                            //    notebookWorkspaceViewModel.CurrentDisplay = null;
                                                                                            //    notebookWorkspaceViewModel.CurrentDisplay = display;

                                                                                            //    isNewDisplay = false;
                                                                                            //    break;
                                                                                            //}

                                                                                            //if(isNewDisplay)
                                                                                            //{
                                                                                            //    var newGridDisplay = new GridDisplay
                                                                                            //                         {
                                                                                            //                             ID = displayID,
                                                                                            //                             DisplayNumber = displayNumber,
                                                                                            //                             NotebookID = notebookWorkspaceViewModel.Notebook.ID,
                                                                                            //                             ParentNotebook = notebookWorkspaceViewModel.Notebook
                                                                                            //                         };
                                                                                            //    notebookWorkspaceViewModel.Notebook.Displays.Add(newGridDisplay);
                                                                                            //    notebookWorkspaceViewModel.CurrentDisplay = null;
                                                                                            //    notebookWorkspaceViewModel.CurrentDisplay = newGridDisplay;
                                                                                            //}
                                                                                        }

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void CreateGridDisplayAndAddPage(string displayID, int displayNumber, string pageID, string pageOwnerID, string differentiationLevel, uint pageVersionIndex)
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null ||
               App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Projector)
            {
                return;
            }

            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return;
            }

            CLPPage page = null;
            //foreach (var notebookInfo in dataService.LoadedNotebooksInfo)
            //{
            //    var notebook = notebookInfo.Notebook;
            //    if (notebook == null)
            //    {
            //        continue;
            //    }
            //    page = notebook.GetPageByCompositeKeys(pageID, pageOwnerID, differentiationLevel, pageVersionIndex);

            //    if(page != null)
            //    {
            //        break;
            //    }
            //}

            if(page == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        //var newGridDisplay = new GridDisplay
                                                                                        //                        {
                                                                                        //                            ID = displayID,
                                                                                        //                            DisplayNumber = displayNumber,
                                                                                        //                            NotebookID = notebookWorkspaceViewModel.Notebook.ID,
                                                                                        //                            ParentNotebook = notebookWorkspaceViewModel.Notebook
                                                                                        //                        };
                                                                                        //newGridDisplay.AddPageToDisplay(page);
                                                                                        //notebookWorkspaceViewModel.Notebook.Displays.Add(newGridDisplay);
                                                                                        //notebookWorkspaceViewModel.CurrentDisplay = null;
                                                                                        //notebookWorkspaceViewModel.CurrentDisplay = newGridDisplay;

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void AddPageToDisplay(string pageID, string pageOwnerID, string differentiationLevel, uint pageVersionIndex, string displayID)
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null ||
               App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Projector)
            {
                return;
            }

            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return;
            }

            CLPPage page = null;
            //foreach (var notebookInfo in dataService.LoadedNotebooksInfo)
            //{
            //    var notebook = notebookInfo.Notebook;
            //    if (notebook == null)
            //    {
            //        continue;
            //    }
            //    page = notebook.GetPageByCompositeKeys(pageID, pageOwnerID, differentiationLevel, pageVersionIndex);

            //    if(page != null)
            //    {
            //        break;
            //    }
            //}

            if(page == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        //if(displayID == "SingleDisplay")
                                                                                        //{
                                                                                        //    notebookWorkspaceViewModel.CurrentDisplay = null;
                                                                                        //    notebookWorkspaceViewModel.Notebook.CurrentPage = page;
                                                                                        //}
                                                                                        //else
                                                                                        //{
                                                                                        //    var display = notebookWorkspaceViewModel.Displays.First(x => x.ID == displayID);
                                                                                        //    if(display == null)
                                                                                        //    {
                                                                                        //        return null;
                                                                                        //    }
                                                                                        //    display.AddPageToDisplay(page);
                                                                                        //}
                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void RemovePageFromDisplay(string pageID, string pageOwnerID, string differentiationLevel, uint pageVersionIndex, string displayID)
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null ||
               App.MainWindowViewModel.CurrentProgramMode != ProgramModes.Projector)
            {
                return;
            }

            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return;
            }

            CLPPage page = null;
            //foreach (var notebookInfo in dataService.LoadedNotebooksInfo)
            //{
            //    var notebook = notebookInfo.Notebook;
            //    if (notebook == null)
            //    {
            //        continue;
            //    }
            //    page = notebook.GetPageByCompositeKeys(pageID, pageOwnerID, differentiationLevel, pageVersionIndex);

            //    if(page != null)
            //    {
            //        break;
            //    }
            //}

            if(page == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        //if(displayID == "SingleDisplay")
                                                                                        //{
                                                                                        //    notebookWorkspaceViewModel.CurrentDisplay = null;
                                                                                        //    notebookWorkspaceViewModel.Notebook.CurrentPage = page;
                                                                                        //}
                                                                                        //else
                                                                                        //{
                                                                                        //    var display = notebookWorkspaceViewModel.Displays.First(x => x.ID == displayID);
                                                                                        //    if(display == null)
                                                                                        //    {
                                                                                        //        return null;
                                                                                        //    }
                                                                                        //    display.RemovePageFromDisplay(page);
                                                                                        //}
                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void ScrollPage(double percentOffset)
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            var singleDisplayView = notebookWorkspaceViewModel.SingleDisplay.GetFirstView() as SingleDisplayView;
            if(singleDisplayView == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        var adjustedOffset = percentOffset * singleDisplayView.SingleDisplayScroller.ExtentHeight;
                                                                                        singleDisplayView.SingleDisplayScroller.ScrollToVerticalOffset(adjustedOffset);
                                                                                        return null;
                                                                                    }, null);
        }

        public void MakeCurrentPageLonger()
        {
            var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        //var page = notebookWorkspaceViewModel.Notebook.CurrentPage;
                                                                                        //var initialHeight = page.Width / page.InitialAspectRatio;
                                                                                        //const int MAX_INCREASE_TIMES = 2;
                                                                                        //const double PAGE_INCREASE_AMOUNT = 200.0;
                                                                                        //if(page.Height < initialHeight + PAGE_INCREASE_AMOUNT * MAX_INCREASE_TIMES)
                                                                                        //{
                                                                                        //    page.Height += PAGE_INCREASE_AMOUNT;
                                                                                        //}
                                                                                        return null;
                                                                                    }, null);
        }

        #region Animation Commands

        public void RewindCurrentPage()
        {
            //var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            //if(notebookWorkspaceViewModel == null)
            //{
            //    return;
            //}

            //var pageViewModel = CLPServiceAgent.Instance.GetViewModelsFromModel(notebookWorkspaceViewModel.Notebook.CurrentPage).First(x => (x is CLPAnimationPageViewModel) && !(x as ACLPPageBaseViewModel).IsPagePreview) as CLPAnimationPageViewModel;
            //if(pageViewModel == null)
            //{
            //    return;
            //}

            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //                                           (DispatcherOperationCallback)delegate
            //                                                                        {

            //                                                                            CLPAnimationPageViewModel.Rewind(pageViewModel);

            //                                                                            return null;
            //                                                                        },
            //                                           null);
        }

        #endregion //Animation Commands

        #region INotebookContract Members

        public void AddHistoryAction(string compositePageID, string zippedHistoryAction)
        {
            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return;
            }

            var compositeKeys = compositePageID.Split(';');
            var pageID = compositeKeys[0];
            var pageOwnerID = compositeKeys[1];
            var differentiationLevel = compositeKeys[2];
            var versionIndex = Convert.ToUInt32(compositeKeys[3]);

            var unzippedHistoryAction = zippedHistoryAction.DecompressFromGZip();
            var historyAction = ObjectSerializer.ToObject(unzippedHistoryAction) as IHistoryAction;
            if(historyAction == null)
            {
                Logger.Instance.WriteToLog("Failed to apply historyAction to projector.");
                return;
            }

            CLPPage pageToRedo = null;
            //foreach (var notebookInfo in dataService.LoadedNotebooksInfo)
            //{
            //    var notebook = notebookInfo.Notebook;
            //    if (notebook == null)
            //    {
            //        continue;
            //    }

            //    var page = notebook.GetPageByCompositeKeys(pageID, pageOwnerID, differentiationLevel, versionIndex);
            //    if (page == null)
            //    {
            //        continue;
            //    }

            //    pageToRedo = page;
            //}

            if (pageToRedo == null)
            {
                return;
            }

            historyAction.ParentPage = pageToRedo;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        historyAction.UnpackHistoryAction();
                                                                                        pageToRedo.History.RedoActions.Clear();
                                                                                        pageToRedo.History.RedoActions.Add(historyAction);

                                                                                        var tempIsAnimating = pageToRedo.History.IsAnimating;
                                                                                        pageToRedo.History.IsAnimating = true;
                                                                                        pageToRedo.History.Redo(true);
                                                                                        pageToRedo.History.IsAnimating = tempIsAnimating;

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void AddNewPage(string zippedPage, int index)
        {
            //var unZippedPage = zippedPage.DecompressFromGZip();
            //var page = ObjectSerializer.ToObject(unZippedPage) as CLPPage;

            //var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            //if(notebookWorkspaceViewModel == null ||
            //   page == null)
            //{
            //    Logger.Instance.WriteToLog("Failed to add broadcasted page.");
            //    return;
            //}
            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //                                           (DispatcherOperationCallback)delegate
            //                                                                        {
            //                                                                            if(index < notebookWorkspaceViewModel.Notebook.Pages.Count)
            //                                                                            {
            //                                                                                notebookWorkspaceViewModel.Notebook.Pages.Insert(index, page);
            //                                                                            }
            //                                                                            else
            //                                                                            {
            //                                                                                notebookWorkspaceViewModel.Notebook.Pages.Add(page);
            //                                                                            }

            //                                                                            return null;
            //                                                                        },
            //                                           null);
        }

        public void ReplacePage(string zippedPage, int index)
        {
            //var unZippedPage = zippedPage.DecompressFromGZip();
            //var page = ObjectSerializer.ToObject(unZippedPage) as CLPPage;

            //var notebookWorkspaceViewModel = App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel;
            //if(notebookWorkspaceViewModel == null ||
            //   page == null)
            //{
            //    Logger.Instance.WriteToLog("Failed to add broadcasted page.");
            //    return;
            //}
            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //                                           (DispatcherOperationCallback)delegate
            //                                                                        {
            //                                                                            notebookWorkspaceViewModel.Notebook.RemovePageAt(index);
            //                                                                            notebookWorkspaceViewModel.Notebook.InsertPageAt(index, page);

            //                                                                            return null;
            //                                                                        },
            //                                           null);
        }

        #endregion

        public string AddStudentSubmission(string submissionJson, string notebookID)
        {
            var dataService = ServiceLocator.Default.ResolveType<IDataService>();
            if (dataService == null)
            {
                return MESSAGE_NO_DATA_SERVICE;
            }

            var submission = ASerializableBase.FromJsonString<CLPPage>(submissionJson);
            if (submission == null)
            {
                return MESSAGE_SUBMISSION_NOT_DESERIALIZED;
            }

            var studentID = submission.Owner.ID;
            var studentNotebook = dataService.LoadedNotebooks.FirstOrDefault(n => n.ID == notebookID && n.Owner.ID == studentID);
            if (studentNotebook == null)
            {
                return MESSAGE_NOTEBOOK_NOT_LOADED_BY_TEACHER;
            }

            var studentPage = studentNotebook.Pages.FirstOrDefault(p => p.ID == submission.ID);
            if (studentPage == null)
            {
                return MESSAGE_PAGE_NOT_LOADED_IN_NOTEBOOK;
            }

            var teacherNotebook = dataService.LoadedNotebooks.FirstOrDefault(n => n.ID == notebookID && !n.Owner.IsStudent);
            if (teacherNotebook == null)
            {
                return MESSAGE_NOTEBOOK_NOT_LOADED_BY_TEACHER;
            }

            var teacherPage = teacherNotebook.Pages.FirstOrDefault(p => p.ID == submission.ID);
            if (teacherPage == null)
            {
                return MESSAGE_PAGE_NOT_LOADED_IN_NOTEBOOK;
            }

            UIHelper.RunOnUI(() =>
            {
                studentPage.Submissions.Add(submission);
                if (teacherPage == null)
                {
                    return;
                }

                var pageViewModels = teacherPage.GetAllViewModels();
                foreach (var pageViewModel in pageViewModels)
                {
                    var pageVM = pageViewModel as ACLPPageBaseViewModel;
                    if (pageVM == null)
                    {
                        continue;
                    }
                    pageVM.UpdateSubmissionCount();
                }
            });

            return MESSAGE_SUBMISSION_SUCCESSFUL;
        }
    }
}