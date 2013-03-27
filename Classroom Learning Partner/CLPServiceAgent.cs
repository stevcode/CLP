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
using CLP.Models.CLPHistoryItems;

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

                            ObservableCollection<List<byte>> byteStrokes = CLPPage.StrokesToBytes(page.InkStrokes);
                            ObservableCollection<ICLPPageObject> pageObjects = new ObservableCollection<ICLPPageObject>();

                            List<byte> image = new List<byte>();
                            if (page.PageIndex == 25)
                            {
                                foreach(ICLPPageObject pageObject in page.PageObjects)
                                {
                                    if(pageObject is CLPImage && pageObject.XPosition == 108 && pageObject.YPosition == 225)
                                    {
                                        image = page.ImagePool[(pageObject as CLPImage).ImageID];
                                        break;
                                    }
                                }
                            }

                            App.Network.InstructorProxy.AddStudentSubmission(byteStrokes, pageObjects, App.Network.CurrentUser, App.Network.CurrentGroup, notebookID, page.UniqueID, page.SubmissionID, page.SubmissionTime, isGroupSubmission, page.PageHeight, image);
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
                    notebook = CLP.Models.CLPNotebook.Load(filePath, true);
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

            DateTime saveTime = DateTime.Now;

            CLPNotebook notebook = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.Clone() as CLPNotebook;

            string time = saveTime.Year + "." + saveTime.Month + "." + saveTime.Day + "." +
                saveTime.Hour + "." + saveTime.Minute + "." + saveTime.Second;

            string filePathName = filePath + @"\" + time + "-" + appendedFileName + "-" + notebook.NotebookName + @".clp";
            notebook.Save(filePathName);
        }

        public void OpenNewNotebook()
        {
            bool NameChooserLoop = true;

            while(NameChooserLoop)
            {
                NotebookNamerWindowView nameChooser = new NotebookNamerWindowView();
                nameChooser.Owner = Application.Current.MainWindow;
                nameChooser.ShowDialog();
                if(nameChooser.DialogResult == true)
                {
                    string notebookName = nameChooser.NotebookName.Text;
                    string filePath = App.NotebookDirectory + @"\" + notebookName + @".clp";

                    if(!File.Exists(filePath))
                    {
                        CLP.Models.CLPNotebook newNotebook = new CLP.Models.CLPNotebook();
                        newNotebook.NotebookName = notebookName;
                        App.MainWindowViewModel.OpenNotebooks.Add(newNotebook);
                        App.MainWindowViewModel.SelectedWorkspace = new NotebookWorkspaceViewModel(newNotebook);
                        App.MainWindowViewModel.IsAuthoring = true;
                        App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Visible;

                        NameChooserLoop = false;
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
                    NameChooserLoop = false;
                }
            }
        }

        public void SaveNotebook(CLP.Models.CLPNotebook notebook)
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
            }
        }

        public void ChangePageObjectPosition(CLP.Models.ICLPPageObject pageObject, Point pt)
        {
            double oldXPos = pageObject.XPosition;
            double oldYPos = pageObject.YPosition;
            CLPPage page = pageObject.ParentPage;
            
            double xDiff = Math.Abs(oldXPos - pt.X);
            double yDiff = Math.Abs(oldYPos - pt.Y);
            double diff = xDiff + yDiff;
            if(diff > page.PageHistory.Sample_Rate)
            {
                page.PageHistory.push(new CLPHistoryMoveObject(page, pageObject, oldXPos, oldYPos, pt.X, pt.Y));
                pageObject.XPosition = pt.X;
                pageObject.YPosition = pt.Y;
                Console.WriteLine("x diff = " + (oldXPos - pt.X));
                Console.WriteLine("y diff = " + (oldYPos - pt.Y));
            }
        }

        public void ChangePageObjectDimensions(CLP.Models.ICLPPageObject pageObject, double height, double width)
        {
            double oldHeight = pageObject.Height;
            double oldWidth = pageObject.Width;
            CLPPage page = pageObject.ParentPage;
            double heightDiff = Math.Abs(oldHeight - height);
            double widthDiff = Math.Abs(oldWidth - width);
            double diff = heightDiff + widthDiff;
            if(diff > page.PageHistory.Sample_Rate){
                page.PageHistory.push(new CLPHistoryResizeObject(page, pageObject, oldHeight, oldWidth, height, width)); 
                pageObject.Height = height;
                pageObject.Width = width;
                Console.WriteLine("height diff = " + (oldHeight-height));
                Console.WriteLine("width diff = " + (oldWidth - width));
            }
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