using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Catel.Data;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Models;
using Catel.MVVM;
using Catel.MVVM.Views;
using Catel.IoC;
using Catel.Windows.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Ink;
using System.Windows.Media.Imaging;

namespace Classroom_Learning_Partner
{
    //Sealed to allow the compiler to perform special optimizations during JIT
    public sealed class CLPServiceAgent
    {
        private static System.Timers.Timer _autoSaveTimer = new System.Timers.Timer();
        private CLPServiceAgent()
        {
            //_autoSaveTimer.Interval = 123000;
            //_autoSaveTimer.Elapsed += _autoSaveTimer_Elapsed;
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
            //_autoSaveTimer.Stop();
            
            Environment.Exit(0);
        }

        public IView GetViewFromViewModel(IViewModel viewModel)
        {
            var viewManager = ServiceLocator.Default.ResolveType<IViewManager>();
            var views = viewManager.GetViewsOfViewModel(viewModel);

            return views[0];
        }

        public List<IViewModel> GetViewModelsFromModel(IModel model)
        {
            var viewModelManger = ServiceLocator.Default.ResolveType<IViewModelManager>();
            var result = viewModelManger.GetViewModelsOfModel(model).ToList();
            return result;
        }

        public byte[] GetJpgImage(UIElement source, double scale = 1.0, int quality = 30)
        {
            var actualHeight = source.RenderSize.Height;
            var actualWidth = source.RenderSize.Width;

            var renderHeight = actualHeight * scale;
            var renderWidth = actualWidth * scale;

            var renderTarget = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
            var sourceBrush = new VisualBrush(source);

            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();

            using(drawingContext)
            {
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(actualWidth, actualHeight)));
            }
            renderTarget.Render(drawingVisual);

            var jpgEncoder = new JpegBitmapEncoder { QualityLevel = quality };
            jpgEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

            byte[] imageArray;

            using(var outputStream = new MemoryStream())
            {
                jpgEncoder.Save(outputStream);
                imageArray = outputStream.ToArray();
            }

            return imageArray;
        }

        #region Notebook

        public void SubmitPage(ICLPPage page, string notebookID, bool isGroupSubmission)
        {
            if(App.Network.InstructorProxy != null)
            {
                var t = new Thread(() =>
                    {
                        try
                        {
                            page.SubmissionID = Guid.NewGuid().ToString();
                            page.SubmissionTime = DateTime.Now;
                            page.TrimPage();

                            var sPage = ObjectSerializer.ToString(page);

                            var byteStrokes = StrokeDTO.SaveInkStrokes(page.InkStrokes);
                            var pageObjects = new ObservableCollection<ICLPPageObject>();

                            App.Network.InstructorProxy.AddSerializedSubmission(sPage, App.Network.CurrentUser, App.Network.CurrentGroup, page.SubmissionTime, isGroupSubmission, notebookID, page.SubmissionID);
                            //App.Network.InstructorProxy.AddStudentSubmission(byteStrokes, pageObjects, App.Network.CurrentUser, App.Network.CurrentGroup, notebookID, page.UniqueID, page.SubmissionID, page.SubmissionTime, isGroupSubmission, page.PageHeight);
                        }
                        catch(Exception ex)
                        {
                            Logger.Instance.WriteToLog("Error Sending Submission: " + ex.Message);
                        }
                    }) {IsBackground = true};
                t.Start();
            }
            else
            {
                Console.WriteLine("Instructor NOT Available");
            }
        }

        public void AddSubmission(CLPNotebook notebook, ICLPPage page)
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

        public void OpenNotebook(string notebookName)
        {

            string filePath = App.NotebookDirectory + @"\" + notebookName + @".clp";
            if(File.Exists(filePath))
            {

                var start = DateTime.Now;
                CLPNotebook notebook = null;

                try
                {
                    
                    ModelBase.GlobalLeanAndMeanModel = true;
                    notebook = CLPNotebook.Load(filePath);
                    ModelBase.GlobalLeanAndMeanModel = false;
                }
                catch(Exception ex)
                {
                    Logger.Instance.WriteToLog("[ERROR] - Notebook could not be loaded: " + ex.Message);
                }

                var end = DateTime.Now;
                var span = end.Subtract(start);
                Logger.Instance.WriteToLog("Time to open notebook (In Seconds): " + span.TotalSeconds);
                if(notebook != null)
                {
                    notebook.NotebookName = notebookName;

                    foreach(var page in notebook.Pages)
                    {
                        page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes); 

                        foreach(ICLPPageObject pageObject in page.PageObjects)
                        {
                            pageObject.ParentPage = page;
                        }
                        foreach(var clpHistoryItem in page.PageHistory.UndoItems)
                        {
                            clpHistoryItem.ParentPage = page;
                        }
                        foreach(var clpHistoryItem in page.PageHistory.RedoItems)
                        {
                            clpHistoryItem.ParentPage = page;
                        }
                        if(notebook.Submissions.ContainsKey(page.UniqueID))
                        {
                            foreach(var submission in notebook.Submissions[page.UniqueID])
                            {
                                submission.InkStrokes = StrokeDTO.LoadInkStrokes(submission.SerializedStrokes);

                                foreach(ICLPPageObject pageObject in submission.PageObjects)
                                {
                                    pageObject.ParentPage = submission;
                                }
                                foreach(var clpHistoryItem in page.PageHistory.UndoItems)
                                {
                                    clpHistoryItem.ParentPage = page;
                                }
                                foreach(var clpHistoryItem in page.PageHistory.RedoItems)
                                {
                                    clpHistoryItem.ParentPage = page;
                                }
                            }
                        }
                    }

                    notebook.InitializeAfterDeserialize();

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
                       // _autoSaveTimer.Start();
                    }

                }
                else
                {
                    MessageBox.Show("Notebook could not be opened. Check error log.");
                }
            }
        }

        private bool _isAutoSaving;
        void _autoSaveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _isAutoSaving = true;
            QuickSaveNotebook("AUTOSAVE");
            _isAutoSaving = false;
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
            else
            {
                Logger.Instance.WriteToLog("***NOTEBOOK WORKSPACE VIEWMODEL BECAME NULL***");
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
            //if(App.CurrentUserMode == App.UserMode.Student)
            //{
            //    notebook.Submissions.Clear();
            //}

            //_autoSaveTimer.Stop();

            //while(_isAutoSaving)
            //{
            //}
            
            notebook.Save(filePath);

            //_autoSaveTimer.Start();
        }

        #endregion //Notebook

        #region Page

        public void InterpretRegion(ACLPInkRegion inkRegion) {
            inkRegion.DoInterpretation();

            Logger.Instance.WriteToLog(inkRegion.ParentPage.Submitter.FullName);
            var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            if(notebookWorkspaceViewModel != null)
            {
                Logger.Instance.WriteToLog(notebookWorkspaceViewModel.Notebook.NotebookName);
            }
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

            var tempPerson = App.Network.CurrentUser;
            var tempGroup = App.Network.CurrentGroup;

            App.Network.Dispose();
            App.Network = null;
            App.Network = new CLPNetwork {CurrentUser = tempPerson, CurrentGroup = tempGroup};
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