using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Threading;
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
                Logger.Instance.WriteToLog("Failed to load partial notebook.");
                return;
            }
            notebook.CurrentPage = notebook.Pages.First();

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       (DispatcherOperationCallback)delegate
                                                                                    {
                                                                                        App.MainWindowViewModel.OpenNotebooks.Add(notebook);
                                                                                        App.MainWindowViewModel.Workspace = new NotebookWorkspaceViewModel(notebook);

                                                                                        return null;
                                                                                    },
                                                       null);
        }

        public void ModifyPageInkStrokes(List<StrokeDTO> strokesAdded, List<StrokeDTO> strokesRemoved, string pageID)
        {
            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            //    (DispatcherOperationCallback)delegate
            //                                 {
            //        foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
            //        {
            //            var page = notebook.GetNotebookPageByID(pageID);

            //            if(page == null)
            //            {
            //                continue;
            //            }

            //            var strokesToRemove = StrokeDTO.LoadInkStrokes(new ObservableCollection<StrokeDTO>(strokesRemoved));

            //            var strokes =
            //                from externalStroke in strokesToRemove
            //                from stroke in page.InkStrokes
            //                where stroke.GetStrokeUniqueID() == externalStroke.GetStrokeUniqueID()
            //                select stroke;

            //            var actualStrokesToRemove = new StrokeCollection(strokes.ToList());

            //            page.InkStrokes.Remove(actualStrokesToRemove);

            //            var strokesToAdd = StrokeDTO.LoadInkStrokes(new ObservableCollection<StrokeDTO>(strokesAdded));
            //            page.InkStrokes.Add(strokesToAdd);
            //            break;
            //        }
            //        return null;
            //    }, null);
        }

        public void AddHistoryItem(string pageID, string zippedHistoryItem) { }

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