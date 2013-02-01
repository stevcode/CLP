using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Threading;
using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;

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
        void AddStudentSubmissionViaString(string sPage, string userName, string notebookName);
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
                            CLP.Models.CLPPage page = notebook.GetNotebookPageByID(pageID);

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

        public void AddStudentSubmissionViaString(string sPage, string userName, string notebookName)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    CLPPage page = (ObjectSerializer.ToObject(sPage) as CLPPage);

                    foreach(ICLPPageObject pageObject in page.PageObjects)
                    {
                        pageObject.ParentPage = page;
                    }

                    page.IsSubmission = true;
                    page.SubmitterName = userName;

                    try
                    {
                        foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                        {
                            if(page.ParentNotebookID == notebook.UniqueID)
                            {
                                CLPServiceAgent.Instance.AddSubmission(notebook, page);
                                //TODO: Steve - AutoSave Here
                                break;
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Logger.Instance.WriteToLog("[ERROR] Recieved Submission from wrong notebook: " + e.Message);
                    }

                    return null;
                }, null);
        }

        #region INotebookContract Members

        public void ModifyPageInkStrokes(List<List<byte>> strokesAdded, List<List<byte>> strokesRemoved, string pageID)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    foreach(var notebook in App.MainWindowViewModel.OpenNotebooks)
                    {
                        CLP.Models.CLPPage page = notebook.GetNotebookPageByID(pageID);

                        if(page == null)
                        {
                            page = notebook.GetSubmissionByID(pageID);
                        }

                        if(page != null)
                        {
                            StrokeCollection strokesToRemove = CLPPage.BytesToStrokes(new ObservableCollection<List<byte>>(strokesRemoved));

                            var strokes =
                                from externalStroke in strokesToRemove
                                from stroke in page.InkStrokes
                                where stroke.GetStrokeUniqueID() == externalStroke.GetStrokeUniqueID()
                                select stroke;

                            StrokeCollection actualStrokesToRemove = new StrokeCollection(strokes.ToList());

                            page.InkStrokes.Remove(actualStrokesToRemove);

                            StrokeCollection strokesToAdd = CLPPage.BytesToStrokes(new ObservableCollection<List<byte>>(strokesAdded));
                            page.InkStrokes.Add(strokesToAdd);
                            break;
                        }
                    }
                    return null;
                }, null);
        }

        #endregion
    }
}
