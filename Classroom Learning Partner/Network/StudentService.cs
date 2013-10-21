using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Threading;
using Catel.Windows;
using CLP.Models;
using Classroom_Learning_Partner.ViewModels;
using System.Security.Cryptography;

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

    public class StudentService : IStudentContract
    {
        #region IStudentContract Members

        public void TogglePenDownMode(bool isPenDownModeEnabled)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate
                                             {
                    //TODO: Steve - AutoSave here
                    if(isPenDownModeEnabled)
                    {
                        PleaseWaitHelper.Show("The Teacher has disabled the pen.");
                    }
                    else
                    {
                        PleaseWaitHelper.Hide();
                    }
                    return null;
                }, null);
        }

        public void AddWebcamImage(List<byte> image)
        {
            var page = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetPageAt(24, -1);

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(image.ToArray());
            string imageID = Convert.ToBase64String(hash);

            if(!page.ImagePool.ContainsKey(imageID))
            {
                page.ImagePool.Add(imageID, image);
            }
            CLPImage imagePO = new CLPImage(imageID, page, 10, 10);
            imagePO.IsBackground = true;
            imagePO.Height = 450;
            imagePO.Width = 600;
            imagePO.YPosition = 225;
            imagePO.XPosition = 108;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (DispatcherOperationCallback)delegate(object arg)
                    {
                        page.PageObjects.Add(imagePO);

                        return null;
                    }, null);
        }

        #endregion

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

        public void AddHistoryItem(string pageID, string zippedHistoryItem)
        {
            
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
