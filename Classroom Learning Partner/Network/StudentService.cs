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
        void AddWebcamImage(List<byte> image);

        [OperationContract]
        void ForceLogOut(string machineName);
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
                                                                                        //TODO: Steve - AutoSave here
                                                                                        App.MainWindowViewModel.IsPenDownActivated = isPenDownModeEnabled;
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

        public void ForceLogOut(string machineName)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        MessageBox.Show("Some one else logged in with your name on machine " + machineName +
                                                                                                        ". You will now be logged out.",
                                                                                                        "Double Login",
                                                                                                        MessageBoxButton.OK);

                                                                                        var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
                                                                                        if (notebookService != null)
                                                                                        {
                                                                                            notebookService.OpenNotebooks.Clear();
                                                                                            notebookService.CurrentNotebook = null;
                                                                                        }
                                                                                        App.MainWindowViewModel.SetWorkspace();

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        #endregion

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
                                                                                        if (notebookService != null)
                                                                                        {
                                                                                            notebookService.CurrentClassPeriod = classPeriod;
                                                                                        }

                                                                                        App.MainWindowViewModel.AvailableUsers = new ObservableCollection<Person>(classPeriod.ClassSubject.StudentList.OrderBy(x => x.FullName));

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
                Logger.Instance.WriteToLog("Failed to load partial notebook.");
                return;
            }
            notebook.CurrentPage = notebook.Pages.First();

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        var notebookService = ServiceLocator.Default.ResolveType<INotebookService>();
                                                                                        if (notebookService == null)
                                                                                        {
                                                                                            return null;
                                                                                        }

                                                                                        notebookService.OpenNotebooks.Add(notebook);
                                                                                        notebookService.CurrentNotebook = notebook;
                                                                                        App.MainWindowViewModel.Workspace = new BlankWorkspaceViewModel();
                                                                                        App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void AddHistoryItem(string compositePageID, string zippedHistoryItem) { }

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

            page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
            page.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(page.History.SerializedTrashedInkStrokes);
            page.Owner = App.MainWindowViewModel.CurrentUser;

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

            page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
            page.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(page.History.SerializedTrashedInkStrokes);
            page.Owner = App.MainWindowViewModel.CurrentUser;

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