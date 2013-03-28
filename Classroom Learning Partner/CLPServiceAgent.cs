using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ProtoBuf;
using System.Windows.Controls;
using Catel.MVVM;
using Catel.MVVM.Views;
using Catel.IoC;
using Catel.Windows.Controls;
using System.ServiceModel;
using System.Collections.ObjectModel;

namespace Classroom_Learning_Partner
{
    //Sealed to allow the compiler to perform special optimizations during JIT
    public sealed class CLPServiceAgent
    {
        private CLPServiceAgent()
        {

        }

        //readonly allows thread-safety and means it can only be allocated once.
        private static readonly CLPServiceAgent _instance = new CLPServiceAgent();
        public static CLPServiceAgent Instance { get { return _instance; } }

        public void Initialize()
        {
        }

        public void Exit()
        {
            //ask to save notebooks, large window with checks for all notebooks (possibly also converter?)
            //sync with database
            //run network disconnect
            if (_autoSaveThread != null)
            {
                _autoSaveThread.Join(1500);
            }
            
            Environment.Exit(0);
        }

        public IView GetViewFromViewModel(IViewModel viewModel)
        {
            var viewManager = ServiceLocator.Instance.ResolveType<IViewManager>();
            var views = viewManager.GetViewsOfViewModel(viewModel);

            return views[0];
        }

        #region Notebook

        public void SubmitPage(CLP.Models.CLPPage page, string notebookID, bool isGroupSubmission)
        {
            if(App.Network.InstructorProxy != null)
            {
                Thread t = new Thread(() =>
                    {
                        try
                        {
                            string oldSubmissionID = page.SubmissionID;
                            page.SubmissionID = Guid.NewGuid().ToString();
                            page.SubmissionTime = DateTime.Now;
                            page.TrimPage();

                            var sPage = ObjectSerializer.ToString(page);

                            ObservableCollection<List<byte>> byteStrokes = CLPPage.StrokesToBytes(page.InkStrokes);
                            ObservableCollection<ICLPPageObject> pageObjects = new ObservableCollection<ICLPPageObject>();

                            App.Network.InstructorProxy.AddSerializedSubmission(sPage, App.Network.CurrentUser, App.Network.CurrentGroup, page.SubmissionTime, isGroupSubmission, notebookID, page.SubmissionID);
                         //   App.Network.InstructorProxy.AddStudentSubmission(byteStrokes, pageObjects, App.Network.CurrentUser, App.Network.CurrentGroup, notebookID, page.UniqueID, page.SubmissionID, page.SubmissionTime, isGroupSubmission, page.PageHeight);
                        }
                        catch(System.Exception ex)
                        {
                            Logger.Instance.WriteToLog("Error Sending Submission: " + ex.Message);
                        }
                    });
                t.IsBackground = true;
                t.Start();
            }
            else
            {
                Console.WriteLine("Instructor NOT Available");
            }
        }

        public void AddSubmission(CLP.Models.CLPNotebook notebook, CLP.Models.CLPPage page)
        {
            notebook.AddStudentSubmission(page.UniqueID, page);
        }

        public void GetNotebookNames(NotebookChooserWorkspaceViewModel notebookChooserVM)
        {
            if(!Directory.Exists(App.NotebookDirectory))
            {
                Directory.CreateDirectory(App.NotebookDirectory);
            }
            //normal operation - take what is already available
            foreach(string fullFile in Directory.GetFiles(App.NotebookDirectory, "*.clp"))
            {
                string notebookName = Path.GetFileNameWithoutExtension(fullFile);
                notebookChooserVM.NotebookNames.Add(notebookName);
            }
            //Jessie - grab notebookNames from database if using DB
        }

        private Thread _autoSaveThread;
        public void OpenNotebook(string notebookName)
        {

            string filePath = App.NotebookDirectory + @"\" + notebookName + @".clp";
            if(File.Exists(filePath))
            {

                DateTime start = DateTime.Now;
                CLP.Models.CLPNotebook notebook = null;

                //Steve - Conversion happens here
                try
                {
                    notebook = CLP.Models.CLPNotebook.Load(filePath);
                }
                catch(Exception ex)
                {
                    Logger.Instance.WriteToLog("[ERROR] - Notebook could not be loaded: " + ex.Message);
                }

                DateTime end = DateTime.Now;
                TimeSpan span = end.Subtract(start);
                Logger.Instance.WriteToLog("Time to open notebook (In Seconds): " + span.TotalSeconds);
                if(notebook != null)
                {
                    notebook.NotebookName = notebookName;

                    foreach(CLPPage page in notebook.Pages)
                    {
                        foreach(ICLPPageObject pageObject in page.PageObjects)
                        {
                            pageObject.ParentPage = page;
                        }
                        if(notebook.Submissions.ContainsKey(page.UniqueID))
                        {
                            foreach(CLPPage submission in notebook.Submissions[page.UniqueID])
                            {
                                foreach(ICLPPageObject pageObject in submission.PageObjects)
                                {
                                    pageObject.ParentPage = submission;
                                }
                            }
                        }
                    }

                    int count = 0;
                    foreach(var otherNotebook in App.MainWindowViewModel.OpenNotebooks)
                    {
                        if(otherNotebook.UniqueID == notebook.UniqueID && otherNotebook.NotebookName == notebook.NotebookName)
                        {
                            App.MainWindowViewModel.SelectedWorkspace = new NotebookWorkspaceViewModel(otherNotebook);
                            count++;
                            break;
                        }
                    }

                    if(count == 0)
                    {
                        App.MainWindowViewModel.OpenNotebooks.Add(notebook);
                        if(App.CurrentUserMode == App.UserMode.Instructor || App.CurrentUserMode == App.UserMode.Student || App.CurrentUserMode == App.UserMode.Projector)
                        {
                            App.MainWindowViewModel.SelectedWorkspace = new NotebookWorkspaceViewModel(notebook);
                        }
                    }

                    if(App.CurrentUserMode == App.UserMode.Student)
                    {
                        _autoSaveThread = new Thread(new ThreadStart(AutoSaveNotebook));
                        _autoSaveThread.IsBackground = true;
                        _autoSaveThread.Start();
                    }

                }
                else
                {
                    MessageBox.Show("Notebook could not be opened. Check error log.");
                }
            }
            else //else doesn't exist, error checking
            {
                //check if notebook exisist on server
            }
        }

        private void AutoSaveNotebook()
        {
            while(true)
            {
                Thread.Sleep(120000); //AutoSave every 2.5 minutes.
                QuickSaveNotebook("AUTOSAVE");
            }
        }

        public void QuickSaveNotebook(string appendedFileName)
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\AutoSavedNotebooks";

            if(!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            var saveTime = DateTime.Now;

            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel != null)
            {
                var notebook = notebookWorkspaceViewModel.Notebook.Clone() as CLPNotebook;

                string time = saveTime.Year + "." + saveTime.Month + "." + saveTime.Day + "." +
                              saveTime.Hour + "." + saveTime.Minute + "." + saveTime.Second;

                if(notebook != null)
                {
                    string filePathName = filePath + @"\" + time + "-" + appendedFileName + "-" + notebook.NotebookName + @".clp";
                    notebook.Save(filePathName);
                }
                else
                {
                    Logger.Instance.WriteToLog("FAILED TO CLONE NOTEBOOK FOR AUTOSAVE!");
                }
            }
        }

        public void OpenNewNotebook()
        {
            bool nameChooserLoop = true;

            while(nameChooserLoop)
            {
                var nameChooser = new NotebookNamerWindowView {Owner = Application.Current.MainWindow};
                nameChooser.ShowDialog();
                if(nameChooser.DialogResult == true)
                {
                    string notebookName = nameChooser.NotebookName.Text;
                    string filePath = App.NotebookDirectory + @"\" + notebookName + @".clp";

                    if(!File.Exists(filePath))
                    {
                        var newNotebook = new CLPNotebook {NotebookName = notebookName};
                        App.MainWindowViewModel.OpenNotebooks.Add(newNotebook);
                        App.MainWindowViewModel.SelectedWorkspace = new NotebookWorkspaceViewModel(newNotebook);
                        App.MainWindowViewModel.IsAuthoring = true;
                        App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Visible;

                        nameChooserLoop = false;
                        //Send empty notebook to db
                        //ObjectSerializer.ToString(newNotebookViewModel)
                    }
                    else
                    {
                        MessageBox.Show("A Notebook with that name already exists. Please choose a different name.");
                    }
                }
                else
                {
                    nameChooserLoop = false;
                }
            }
        }

        public void SaveNotebook(CLPNotebook notebook)
        {
            string filePath = App.NotebookDirectory + @"\" + notebook.NotebookName + @".clp";
            if(App.CurrentUserMode == App.UserMode.Student)
            {
                notebook.Submissions.Clear();
            }
            notebook.Save(filePath);
        }

        #endregion //Notebook

        #region Page

        public void AddPageObjectToPage(CLP.Models.ICLPPageObject pageObject)
        {
            CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(pageObject.ParentPageID);

            AddPageObjectToPage(parentPage, pageObject);
        }

        public void AddPageObjectToPage(CLP.Models.CLPPage page, CLP.Models.ICLPPageObject pageObject)
        {
            if (page != null)
            {
                pageObject.IsBackground = App.MainWindowViewModel.IsAuthoring;
                page.PageObjects.Add(pageObject);

                //if (!page.PageHistory.IgnoreHistory)
                //{
                //    CLP.Models.CLPHistoryItem item = new CLP.Models.CLPHistoryItem(CLP.Models.HistoryItemType.AddPageObject, pageObject.UniqueID, null, null);
                //    page.PageHistory.HistoryItems.Add(item);                  
                //}
            }
        }

        public void RemovePageObjectFromPage(CLP.Models.ICLPPageObject pageObject)
        {
            CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(pageObject.ParentPageID);

            RemovePageObjectFromPage(parentPage, pageObject);
        }

        public void RemovePageObjectFromPage(CLP.Models.CLPPage page, CLP.Models.ICLPPageObject pageObject)
        {
            if (page != null)
            {
                pageObject.OnRemoved();
                page.PageObjects.Remove(pageObject);

                //if (!page.PageHistory.IgnoreHistory)
                //{
                //    CLP.Models.CLPHistoryItem item = new CLP.Models.CLPHistoryItem(CLP.Models.HistoryItemType.RemovePageObject, pageObject.UniqueID, ObjectSerializer.ToString(pageObject), null);
                //    page.PageHistory.HistoryItems.Add(item);
                //}
            }
        }

        public void ChangePageObjectPosition(CLP.Models.ICLPPageObject pageObject, Point pt)
        {

            //if (!page.PageHistory.IgnoreHistory)
            //{
            //    //steve - fix for lack of Position
            //   //CLP.Models.CLPHistoryItem item = new CLP.Models.CLPHistoryItem(CLP.Models.HistoryItemType.MovePageObject, pageObject.UniqueID, pageObject.Position.ToString(), pt.ToString());
            //   //page.PageHistory.HistoryItems.Add(item);
            //}

            pageObject.XPosition = pt.X;
            pageObject.YPosition = pt.Y;
        }

        public void ChangePageObjectDimensions(CLP.Models.ICLPPageObject pageObject, double height, double width)
        {
            //Commented out for now because not useful at all. Just uncomment to start using.
            //CLPPage page = GetPageFromID(pageObject.PageID);
            //if (!page.PageHistory.IgnoreHistory)
            //{
            //    double oldHeight = pageObject.Height;
            //    double oldWidth = pageObject.Width;
            //    Tuple<double, double> oldValue = new Tuple<double, double>(oldHeight, oldWidth);
            //    Tuple<double, double> newValue = new Tuple<double, double>(height, width);

            //    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.ResizePageObject, pageObject.UniqueID, oldValue.ToString(), newValue.ToString());
            //    page.PageHistory.HistoryItems.Add(item);
            //}

            pageObject.Height = height;
            pageObject.Width = width;
        }

        public void InterpretRegion(ACLPInkRegion inkRegion) {
            inkRegion.DoInterpretation();

            Logger.Instance.WriteToLog(inkRegion.ParentPage.SubmitterName);
            Logger.Instance.WriteToLog((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.NotebookName);
            Logger.Instance.WriteToLog(inkRegion.ParentPage.PageIndex.ToString());
            Logger.Instance.WriteToLog(inkRegion.StoredAnswer);
        }

        #endregion //Page

        #region Network

        private Thread _networkThread;

        public void NetworkSetup()
        {
            _networkThread = new Thread(App.Network.Run) { IsBackground = true };
            _networkThread.Start();
        }

        public void NetworkReconnect()
        {
            App.Network.Stop();
            _networkThread.Join();
            _networkThread = null;

            Person tempPerson = App.Network.CurrentUser;
            Group tempGroup = App.Network.CurrentGroup;

            App.Network.Dispose();
            App.Network = null;
            App.Network = new CLPNetwork();
            App.Network.CurrentUser = tempPerson;
            App.Network.CurrentGroup = tempGroup;
            _networkThread = new Thread(App.Network.Run) { IsBackground = true };
            _networkThread.Start();
        }

        public void NetworkDisconnect()
        {
            App.Network.Stop();
            _networkThread.Join();
        }

        #endregion //Network
    }
}