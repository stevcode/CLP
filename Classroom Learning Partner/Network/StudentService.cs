using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Threading;
using Catel.IoC;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner
{
    [ServiceContract]
    public interface IStudentContract : INotebookContract
    {
        [OperationContract]
        void TogglePenDownMode(bool isPenDownModeEnabled);

        [OperationContract]
        void OtherAttemptedLogin(string machineName);
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class StudentService : IStudentContract
    {
        #region IStudentContract Members

        public void TogglePenDownMode(bool isPenDownModeEnabled)
        {
            UIHelper.RunOnUI(() => App.MainWindowViewModel.IsPenDownActivated = isPenDownModeEnabled);
        }

        public void OtherAttemptedLogin(string machineName)
        {
            UIHelper.RunOnUI(
                             () =>
                                 MessageBox.Show($"Someone else tried to log in with your name from machine {machineName}. Make sure you are logged in as the correct person.",
                                                 "Attempted Incorrect Login",
                                                 MessageBoxButton.OK));
        }

        #endregion

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
            var pageOwnerID = App.MainWindowViewModel.CurrentUser.ID;
            var differentiationLevel = compositeKeys[2]; // TODO: Make owner's current differentiation level
            var versionIndex = Convert.ToUInt32(compositeKeys[3]);

            var unzippedHistoryAction = zippedHistoryAction.DecompressFromGZip();
            var historyAction = ObjectSerializer.ToObject(unzippedHistoryAction) as IHistoryAction;
            if (historyAction == null)
            {
                CLogger.AppendToLog("Failed to apply historyAction to projector.");
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
                                                           pageToRedo.History.Redo();
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
            //    CLogger.AppendToLog("Failed to add broadcasted page.");
            //    return;
            //}

            //page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
            //page.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(page.History.SerializedTrashedInkStrokes);
            //page.Owner = App.MainWindowViewModel.CurrentUser;

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
            //    CLogger.AppendToLog("Failed to add broadcasted page.");
            //    return;
            //}

            //page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
            //page.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(page.History.SerializedTrashedInkStrokes);
            //page.Owner = App.MainWindowViewModel.CurrentUser;

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
    }
}