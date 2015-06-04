using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
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
        void AddSerializedSubmission(string zippedPage, string notebookID);

        [OperationContract]
        void ScrollPage(double percentOffset);
        
        [OperationContract]
        void MakeCurrentPageLonger();

        [OperationContract]
        void RewindCurrentPage();
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class ProjectorService : IProjectorContract
    {
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
                                                                                                var singleDisplayView = CLPServiceAgent.Instance.GetViewFromViewModel(notebookWorkspaceViewModel.SingleDisplay);
                                                                                                screenShotByteSource = CLPServiceAgent.Instance.UIElementToImageByteArray(singleDisplayView as UIElement);
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                var displayViewModels = CLPServiceAgent.Instance.GetViewModelsFromModel(notebookWorkspaceViewModel.CurrentDisplay as IModel);
                                                                                                foreach(var gridDisplayView in from displayViewModel in displayViewModels
                                                                                                                               where displayViewModel is GridDisplayViewModel && (displayViewModel as GridDisplayViewModel).IsDisplayPreview == false
                                                                                                                               select CLPServiceAgent.Instance.GetViewFromViewModel(displayViewModel))
                                                                                                {
                                                                                                    screenShotByteSource = CLPServiceAgent.Instance.UIElementToImageByteArray(gridDisplayView as UIElement);
                                                                                                }
                                                                                            }

                                                                                            if(screenShotByteSource == null)
                                                                                            {
                                                                                                return null;
                                                                                            }

                                                                                            var bitmapImage = new BitmapImage();
                                                                                            bitmapImage.BeginInit();
                                                                                            bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                                                                                            bitmapImage.StreamSource = new MemoryStream(screenShotByteSource);
                                                                                            bitmapImage.EndInit();
                                                                                            bitmapImage.Freeze();

                
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
                                                                                            var isNewDisplay = true;
                                                                                            foreach(var display in
                                                                                                notebookWorkspaceViewModel.Displays.Where(display =>
                                                                                                                                          display.ID == displayID))
                                                                                            {
                                                                                                notebookWorkspaceViewModel.CurrentDisplay = null;
                                                                                                notebookWorkspaceViewModel.CurrentDisplay = display;

                                                                                                isNewDisplay = false;
                                                                                                break;
                                                                                            }

                                                                                            if(isNewDisplay)
                                                                                            {
                                                                                                var newGridDisplay = new GridDisplay
                                                                                                                     {
                                                                                                                         ID = displayID,
                                                                                                                         DisplayNumber = displayNumber,
                                                                                                                         NotebookID = notebookWorkspaceViewModel.Notebook.ID,
                                                                                                                         ParentNotebook = notebookWorkspaceViewModel.Notebook
                                                                                                                     };
                                                                                                notebookWorkspaceViewModel.Notebook.Displays.Add(newGridDisplay);
                                                                                                notebookWorkspaceViewModel.CurrentDisplay = null;
                                                                                                notebookWorkspaceViewModel.CurrentDisplay = newGridDisplay;
                                                                                            }
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

            var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
            if (notebookService == null)
            {
                return;
            }

            CLPPage page = null;
            foreach (var notebook in notebookService.OpenNotebooks)
            {
                page = notebook.GetPageByCompositeKeys(pageID, pageOwnerID, differentiationLevel, pageVersionIndex);

                if(page != null)
                {
                    break;
                }
            }

            if(page == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        var newGridDisplay = new GridDisplay
                                                                                                                {
                                                                                                                    ID = displayID,
                                                                                                                    DisplayNumber = displayNumber,
                                                                                                                    NotebookID = notebookWorkspaceViewModel.Notebook.ID,
                                                                                                                    ParentNotebook = notebookWorkspaceViewModel.Notebook
                                                                                                                };
                                                                                        newGridDisplay.AddPageToDisplay(page);
                                                                                        notebookWorkspaceViewModel.Notebook.Displays.Add(newGridDisplay);
                                                                                        notebookWorkspaceViewModel.CurrentDisplay = null;
                                                                                        notebookWorkspaceViewModel.CurrentDisplay = newGridDisplay;

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

            var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
            if (notebookService == null)
            {
                return;
            }

            CLPPage page = null;
            foreach (var notebook in notebookService.OpenNotebooks)
            {
                page = notebook.GetPageByCompositeKeys(pageID, pageOwnerID, differentiationLevel, pageVersionIndex);

                if(page != null)
                {
                    break;
                }
            }

            if(page == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        if(displayID == "SingleDisplay")
                                                                                        {
                                                                                            notebookWorkspaceViewModel.CurrentDisplay = null;
                                                                                            notebookWorkspaceViewModel.Notebook.CurrentPage = page;
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            var display = notebookWorkspaceViewModel.Displays.First(x => x.ID == displayID);
                                                                                            if(display == null)
                                                                                            {
                                                                                                return null;
                                                                                            }
                                                                                            display.AddPageToDisplay(page);
                                                                                        }
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

            var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
            if (notebookService == null)
            {
                return;
            }

            CLPPage page = null;
            foreach (var notebook in notebookService.OpenNotebooks)
            {
                page = notebook.GetPageByCompositeKeys(pageID, pageOwnerID, differentiationLevel, pageVersionIndex);

                if(page != null)
                {
                    break;
                }
            }

            if(page == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        if(displayID == "SingleDisplay")
                                                                                        {
                                                                                            notebookWorkspaceViewModel.CurrentDisplay = null;
                                                                                            notebookWorkspaceViewModel.Notebook.CurrentPage = page;
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            var display = notebookWorkspaceViewModel.Displays.First(x => x.ID == displayID);
                                                                                            if(display == null)
                                                                                            {
                                                                                                return null;
                                                                                            }
                                                                                            display.RemovePageFromDisplay(page);
                                                                                        }
                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void AddSerializedSubmission(string zippedPage, string notebookID)
        {
            var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
            if (notebookService == null)
            {
                return;
            }

            var unZippedPage = CLPServiceAgent.Instance.UnZip(zippedPage);
            var submission = ObjectSerializer.ToObject(unZippedPage) as CLPPage;

            if (submission == null ||
                submission.Owner == null)
            {
                Logger.Instance.WriteToLog("Failed to receive student submission. Page or Submitter is null.");
                return;
            }
            submission.InkStrokes = StrokeDTO.LoadInkStrokes(submission.SerializedStrokes);
            submission.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(submission.History.SerializedTrashedInkStrokes);

            var currentNotebook =
                notebookService.OpenNotebooks.FirstOrDefault(notebook => notebookID == notebook.ID && notebook.OwnerID == App.MainWindowViewModel.CurrentUser.ID);

            if (currentNotebook == null)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                       {
                                                           try
                                                           {
                                                               var page =
                                                                       currentNotebook.Pages.FirstOrDefault(
                                                                                                            x =>
                                                                                                            x.ID == submission.ID &&
                                                                                                            x.DifferentiationLevel ==
                                                                                                            submission.DifferentiationLevel);
                                                               if (page == null)
                                                               {
                                                                   return null;
                                                               }
                                                               page.Submissions.Add(submission);
                                                           }
                                                           catch (Exception e)
                                                           {
                                                               Logger.Instance.WriteToLog("[ERROR] Recieved Submission from wrong notebook: " +
                                                                                          e.Message);
                                                           }

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

            var singleDisplayView = CLPServiceAgent.Instance.GetViewFromViewModel(notebookWorkspaceViewModel.SingleDisplay) as SingleDisplayView;
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
                                                                                        var page = notebookWorkspaceViewModel.Notebook.CurrentPage;
                                                                                        var initialHeight = page.Width / page.InitialAspectRatio;
                                                                                        const int MAX_INCREASE_TIMES = 2;
                                                                                        const double PAGE_INCREASE_AMOUNT = 200.0;
                                                                                        if(page.Height < initialHeight + PAGE_INCREASE_AMOUNT * MAX_INCREASE_TIMES)
                                                                                        {
                                                                                            page.Height += PAGE_INCREASE_AMOUNT;
                                                                                        }
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
                                                                                        var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
                                                                                        if (notebookService == null)
                                                                                        {
                                                                                            return null;
                                                                                        }

                                                                                        notebookService.CurrentClassPeriod = classPeriod;
                                                                                        App.MainWindowViewModel.AvailableUsers = classPeriod.ClassSubject.StudentList;
                                                                                        App.MainWindowViewModel.CurrentUser = notebookService.CurrentClassPeriod.ClassSubject.Teacher;

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void OpenPartialNotebook(string zippedNotebook)
        {
            //var unZippedNotebook = CLPServiceAgent.Instance.UnZip(zippedNotebook);
            //var notebook = ObjectSerializer.ToObject(unZippedNotebook) as Notebook;
            //if(notebook == null)
            //{
            //    Logger.Instance.WriteToLog("Failed to load notebook.");
            //    return;
            //}

            //App.MainWindowViewModel.CurrentUser = App.MainWindowViewModel.CurrentClassPeriod.ClassSubject.Teacher;

            //notebook.CurrentPage = notebook.Pages.First();
            //foreach(var page in notebook.Pages)
            //{
            //    page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
            //    page.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(page.History.SerializedTrashedInkStrokes);
            //}

            //foreach(var page in notebook.Pages)
            //{
            //    foreach(var notebookName in MainWindowViewModel.AvailableLocalNotebookNames)
            //    {
            //        var notebookInfo = notebookName.Split(';');
            //        if(notebookInfo.Length != 4 ||
            //           notebookInfo[3] == Person.Author.ID ||
            //           notebookInfo[3] == App.MainWindowViewModel.CurrentUser.ID)
            //        {
            //            continue;
            //        }

            //        var folderPath = Path.Combine(MainWindowViewModel.NotebookCacheDirectory, notebookName);
            //        if(!Directory.Exists(folderPath))
            //        {
            //            continue;
            //        }

            //        var submissionsPath = Path.Combine(folderPath, "Pages");
            //        if(!Directory.Exists(submissionsPath))
            //        {
            //            continue;
            //        }

            //        var submissionPaths = Directory.EnumerateFiles(submissionsPath, "*.xml");

            //        foreach(var submissionPath in submissionPaths)
            //        {
            //            var submissionFileName = Path.GetFileNameWithoutExtension(submissionPath);
            //            var submissionInfo = submissionFileName.Split(';');
            //            if(submissionInfo.Length == 5 &&
            //               submissionInfo[2] == page.ID &&
            //               submissionInfo[4] != "0")
            //            {
            //                var submission = ModelBase.Load<CLPPage>(submissionPath, SerializationMode.Xml);
            //                page.Submissions.Add(submission);
            //            }
            //        }
            //    }
            //}

            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //                                           (DispatcherOperationCallback)delegate
            //                                                                        {
            //                                                                            var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
            //                                                                            if (notebookService == null)
            //                                                                            {
            //                                                                                return null;
            //                                                                            }

            //                                                                            notebookService.OpenNotebooks.Add(notebook);
            //                                                                            notebookService.CurrentNotebook = notebook;
            //                                                                            App.MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
            //                                                                            App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);

            //                                                                            return null;
            //                                                                        },
            //                                           null);
        }

        public void AddHistoryItem(string compositePageID, string zippedHistoryItem)
        {
            var compositeKeys = compositePageID.Split(';');
            var pageID = compositeKeys[0];
            var pageOwnerID = compositeKeys[1];
            var differentiationLevel = compositeKeys[2];
            var versionIndex = Convert.ToUInt32(compositeKeys[3]);

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
                                                                                        var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
                                                                                        if (notebookService == null)
                                                                                        {
                                                                                            return null;
                                                                                        }

                                                                                        foreach (var notebook in notebookService.OpenNotebooks)
                                                                                        {
                                                                                            var page = notebook.GetPageByCompositeKeys(pageID, pageOwnerID, differentiationLevel, versionIndex);

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
                                                                                    },
                                                       null);
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
                                                                                            notebookWorkspaceViewModel.Notebook.Pages.Insert(index, page);
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            notebookWorkspaceViewModel.Notebook.Pages.Add(page);
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