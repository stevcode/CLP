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
        void ToggleAutoNumberLine(bool isAutoNumberLineEnabled);

        [OperationContract]
        void AddWebcamImage(List<byte> image);

        [OperationContract]
        void OtherAttemptedLogin(string machineName);
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class StudentService : IStudentContract
    {
        #region IStudentContract Members

        public void TogglePenDownMode(bool isPenDownModeEnabled)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        App.MainWindowViewModel.IsPenDownActivated = isPenDownModeEnabled;
                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void ToggleAutoNumberLine(bool isAutoNumberLineEnabled)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                       {
                                                           App.MainWindowViewModel.CanUseAutoNumberLine = isAutoNumberLineEnabled;
                                                           return null;
                                                       },
                                                       null);
        }

        public void AddWebcamImage(List<byte> image)
        {
            //var page = (App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).Notebook.GetPageAt(24, -1);

            //MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            //byte[] hash = md5.ComputeHash(image.ToArray());
            //string imageID = Convert.ToBase64String(hash);

            //if(!page.ImagePool.ContainsKey(imageID))
            //{
            //    page.ImagePool.Add(imageID, image);
            //}
            //CLPImage imagePO = new CLPImage(imageID, page, 10, 10);
            //imagePO.IsBackground = true;
            //imagePO.Height = 450;
            //imagePO.Width = 600;
            //imagePO.YPosition = 225;
            //imagePO.XPosition = 108;

            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //        (DispatcherOperationCallback)delegate(object arg)
            //        {
            //            page.PageObjects.Add(imagePO);

            //            return null;
            //        }, null);
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

        public void OpenClassPeriod(string zippedClassPeriod, string zippedClassSubject)
        {
            var unZippedClassPeriod = zippedClassPeriod.DecompressFromGZip();
            var classPeriod = ObjectSerializer.ToObject(unZippedClassPeriod) as ClassPeriod;
            if(classPeriod == null)
            {
                Logger.Instance.WriteToLog("Failed to load classperiod.");
                return;
            }

            var unZippedClassSubject = zippedClassSubject.DecompressFromGZip();
            var classSubject = ObjectSerializer.ToObject(unZippedClassSubject) as ClassInformation;
            if(classSubject == null)
            {
                Logger.Instance.WriteToLog("Failed to load classperiod.");
                return;
            }

            classPeriod.ClassInformation = classSubject;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        //var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
                                                                                        //if (notebookService != null)
                                                                                        //{
                                                                                        //    notebookService.CurrentClassPeriod = classPeriod;
                                                                                        //}

                                                                                        //App.MainWindowViewModel.AvailableUsers = new ObservableCollection<Person>(classPeriod.ClassInformation.StudentList.OrderBy(x => x.FullName));

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void OpenPartialNotebook(string zippedNotebook)
        {
            var unZippedNotebook = zippedNotebook.DecompressFromGZip();
            var notebook = ObjectSerializer.ToObject(unZippedNotebook) as Notebook;
            if(notebook == null)
            {
                Logger.Instance.WriteToLog("Failed to load partial notebook.");
                return;
            }
            notebook.CurrentPage = notebook.Pages.First();

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        //var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
                                                                                        //if (notebookService == null)
                                                                                        //{
                                                                                        //    return null;
                                                                                        //}

                                                                                        //notebookService.OpenNotebooks.Add(notebook);
                                                                                        //notebookService.CurrentNotebook = notebook;
                                                                                        //App.MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
                                                                                        //App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void AddHistoryItem(string compositePageID, string zippedHistoryItem)
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

            var unzippedHistoryItem = zippedHistoryItem.DecompressFromGZip();
            var historyItem = ObjectSerializer.ToObject(unzippedHistoryItem) as IHistoryItem;
            if (historyItem == null)
            {
                Logger.Instance.WriteToLog("Failed to apply historyItem to projector.");
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

            historyItem.ParentPage = pageToRedo;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                       {
                                                           historyItem.UnpackHistoryItem();
                                                           pageToRedo.History.RedoItems.Clear();
                                                           pageToRedo.History.RedoItems.Add(historyItem);

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
            //    Logger.Instance.WriteToLog("Failed to add broadcasted page.");
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
            //    Logger.Instance.WriteToLog("Failed to add broadcasted page.");
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