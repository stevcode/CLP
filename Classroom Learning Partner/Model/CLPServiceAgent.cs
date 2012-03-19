using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using System.Windows;
using Classroom_Learning_Partner.Model.CLPPageObjects;
using Classroom_Learning_Partner.ViewModels.PageObjects;
using MongoDB.Driver;
using Classroom_Learning_Partner.Views.Modal_Windows;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System.Windows.Input;
using System.Windows.Ink;
using Classroom_Learning_Partner.ViewModels.Displays;
using System.Collections.ObjectModel;


namespace Classroom_Learning_Partner.Model
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

        public void AddSubmission(CLPNotebook notebook, CLPPage page)
        {
            notebook.AddStudentSubmission(page.UniqueID, page);
        }

        public void OpenNotebook(string notebookName)
        {
            string filePath = App.NotebookDirectory + @"\" + notebookName + @".clp";
            if (File.Exists(filePath))
            {
                //alternatively, pull from database and build
                CLPNotebook notebook = CLPNotebook.Load(filePath);
                notebook.NotebookName = notebookName;

                int count = 0;
                foreach (var otherNotebook in App.MainWindowViewModel.OpenNotebooks)
                {
                    if (otherNotebook.UniqueID == notebook.UniqueID)
                    {
                        App.MainWindowViewModel.SelectedWorkspace = new NotebookWorkspaceViewModel(otherNotebook);
                        count++;
                        break;
                    }
                }

                if (count == 0)
                {
                    App.MainWindowViewModel.OpenNotebooks.Add(notebook);
                    App.MainWindowViewModel.SelectedWorkspace = new NotebookWorkspaceViewModel(notebook);
                }

                
            }
            else //else doesn't exist, error checking
            {
                //check if notebook exisist on server
            }
        }

        public void OpenNewNotebook()
        {
            bool NameChooserLoop = true;

            while (NameChooserLoop)
            {
                NotebookNamerWindowView nameChooser = new NotebookNamerWindowView();
                nameChooser.Owner = Application.Current.MainWindow;
                nameChooser.ShowDialog();
                if (nameChooser.DialogResult == true)
                {
                    string notebookName = nameChooser.NotebookName.Text;
                    string filePath = App.NotebookDirectory + @"\" + notebookName + @".clp";

                    if (!File.Exists(filePath))
                    {
                        CLPNotebook newNotebook = new CLPNotebook();
                        newNotebook.NotebookName = notebookName;
                        App.MainWindowViewModel.OpenNotebooks.Add(newNotebook);
                        App.MainWindowViewModel.SelectedWorkspace = new NotebookWorkspaceViewModel(newNotebook);
                        App.MainWindowViewModel.IsAuthoring = true;
                        App.MainWindowViewModel.AuthoringTabVisibility = Visibility.Visible;
                        
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

        public void SaveNotebook(CLPNotebook notebook)
        {
            //make async?
            //compare model w/ database
            string filePath = App.NotebookDirectory + @"\" + notebook.NotebookName + @".clp";
            notebook.Save(filePath);
            Console.WriteLine("Notebook saved locally");
            if (App.DatabaseUse == App.DatabaseMode.Using)
            {
                string s_notebook = ObjectSerializer.ToString(notebook);
                Console.WriteLine("Notebook serialized");
                App.Peer.Channel.SaveNotebookDB(s_notebook, App.Peer.UserName);//Server call
                Console.WriteLine("Notebook saving called on mesh");
            }
        }

        public void SaveNotebookDB(CLPNotebook notebook, string userName)
        {
            if (App.DatabaseUse == App.DatabaseMode.Using && App.CurrentUserMode == App.UserMode.Server)
            {
                //save to database
                MongoDatabase nb = App.DatabaseServer.GetDatabase("Notebooks");
                MongoCollection<BsonDocument> nbCollection = nb.GetCollection<BsonDocument>("Notebooks");
                var query = Query.EQ("ID", notebook.UniqueID);
                BsonDocument currentNotebook = nbCollection.FindOne(query);
                if (currentNotebook != null)
                {
                    //update with newer notebook version
                    currentNotebook["SaveDate"] = DateTime.Now.ToString();
                    currentNotebook["NotebookContent"] = ObjectSerializer.ToString(notebook);
                    nbCollection.Save(currentNotebook);
                }
                else
                {
                    nbCollection.Insert(createBsonNotebook(notebook, userName));
                }
            }
        }

        public void SaveNotebooksFromDBToHD(CLPNotebook notebook)
        {
            switch (App.CurrentUserMode)
            {
                case App.UserMode.Student:
                    string filePath = App.NotebookDirectory + @"\" + notebook.NotebookName + @".clp";
                    notebook.Save(filePath);
                    break;
            }
        }

        public void SavePageDB(CLPPage page)
        {
            if (App.DatabaseUse == App.DatabaseMode.Using && App.CurrentUserMode == App.UserMode.Server)
            {
                //save to database
                MongoDatabase nb = App.DatabaseServer.GetDatabase("Notebooks");
                MongoCollection<BsonDocument> pageCollection = nb.GetCollection<BsonDocument>("Pages");
                BsonDocument currentPage = new BsonDocument {
                    { "ID", page.UniqueID },
                    { "CreationDate", page.CreationDate },
                        { "PageContent", ObjectSerializer.ToString(page) }
                    };
                pageCollection.Insert(currentPage);
            }
        }

        public void ChooseNotebook(NotebookChooserWorkspaceViewModel notebookChooserVM)
        {
            if (!Directory.Exists(App.NotebookDirectory))
            {
                Directory.CreateDirectory(App.NotebookDirectory);
            }
            //normal operation - take what is already available
            foreach (string fullFile in Directory.GetFiles(App.NotebookDirectory, "*.clp"))
            {
                string notebookName = Path.GetFileNameWithoutExtension(fullFile);
                NotebookSelectorViewModel notebookSelector = new NotebookSelectorViewModel(notebookName);
                notebookChooserVM.NotebookSelectorViewModels.Add(notebookSelector);
            }
            //Jessie - grab notebookNames from database if using DB
        }

        public void Exit()
        {
            //ask to save notebooks, large window with checks for all notebooks (possibly also converter?)
            //sync with database
            //run network disconnect

            Environment.Exit(0);
        }


        public void SubmitPage(CLPPage page)
        {
            if (App.Peer.Channel != null)
            {
                //CLPHistory history = CLPHistory.GenerateHistorySinceLastSubmission(page);
                //string s_history = ObjectSerializer.ToString(history);

                //ObservableCollection<ICLPPageObject> pageObjects = CLPPage.PageObjectsSinceLastSubmission(page, history);
                //string s_pageObjects = ObjectSerializer.ToString(pageObjects);

                //List<string> inkStrokes = CLPPage.InkStrokesSinceLastSubmission(page, history);

                string oldSubmissionID = page.SubmissionID;
                page.SubmissionID = Guid.NewGuid().ToString();
                page.SubmissionTime = DateTime.Now;
                //App.Peer.Channel.SubmitPage(App.Peer.UserName, page.SubmissionID, page.SubmissionTime.ToString(), s_history, s_pageObjects, inkStrokes);

                string s_page = ObjectSerializer.ToString(page);
                App.Peer.Channel.SubmitFullPage(s_page, App.Peer.UserName);

                double size_standard = s_page.Length / 1024.0;
                Logger.Instance.WriteToLog("Submitting Page " + page.PageIndex + ": " + page.UniqueID + ", at " + page.SubmissionTime.ToShortTimeString());
                Logger.Instance.WriteToLog("Submission Size: " + size_standard.ToString());

                page.PageHistory.HistoryItems.Add(new CLPHistoryItem(HistoryItemType.Send, null, oldSubmissionID, page.SubmissionID));
            }
        }
        
        public void AddPageObjectToPage(string pageID, ICLPPageObject pageObject)
        {
            CLPPage page = GetPageFromID(pageID);
            AddPageObjectToPage(page, pageObject);
        }

        public void AddPageObjectToPage(CLPPage page, ICLPPageObject pageObject)
        {
            if (page != null)
            {
                pageObject.PageID = page.UniqueID;
                page.PageObjects.Add(pageObject);

                if (!page.PageHistory.IgnoreHistory)
                {
                    CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.AddPageObject, pageObject.UniqueID, null, null);
                    page.PageHistory.HistoryItems.Add(item);
                }
            }
        }

        public void RemovePageObjectFromPage(ICLPPageObject pageObject)
        {
            RemovePageObjectFromPage(pageObject.PageID, pageObject);
        }

        public void RemovePageObjectFromPage(string pageID, ICLPPageObject pageObject)
        {
            CLPPage page = GetPageFromID(pageID);
            RemovePageObjectFromPage(page, pageObject);
        }

        public void RemovePageObjectFromPage(CLPPage page, ICLPPageObject pageObject)
        {
            if (page != null)
            {
                foreach (ICLPPageObject po in page.PageObjects)
                {
                    if (po.UniqueID == pageObject.UniqueID)
                    {
                        page.PageObjects.Remove(po);
                        break;
                    }
                }

                

                if (!page.PageHistory.IgnoreHistory)
                {
                	CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.RemovePageObject, pageObject.UniqueID, ObjectSerializer.ToString(pageObject), null);
                    page.PageHistory.HistoryItems.Add(item);
                }
            }
        }

        public CLPPage GetPageFromID(string pageID)
        {
            foreach (var notebook in App.MainWindowViewModel.OpenNotebooks)
            {
                CLPPage page = notebook.GetNotebookPageByID(pageID);
                if (page != null)
                {
                    return page;
                }
            }
            return null;
        }     
        
        public void ChangePageObjectPosition(ICLPPageObject pageObject, Point pt)
        {
            CLPPage page = GetPageFromID(pageObject.PageID);
            if (!page.PageHistory.IgnoreHistory)
            {
                CLPHistoryItem item = new CLPHistoryItem(HistoryItemType.MovePageObject, pageObject.UniqueID, pageObject.Position.ToString(), pt.ToString());
                page.PageHistory.HistoryItems.Add(item);
            }

            pageObject.Position = pt;
        }

        public void ChangePageObjectDimensions(ICLPPageObject pageObject, double height, double width)
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

        public void RetrieveNotebooks(string username)
        {
            //Use username to retrieve all student Notebooks and broadcast over mesh network
            if (App.CurrentUserMode == App.UserMode.Server)
            {
                MongoDatabase nb = App.DatabaseServer.GetDatabase("Notebooks");
                MongoCollection<BsonDocument> notebookCollection = nb.GetCollection<BsonDocument>("Notebooks");
                var query = Query.EQ("User", username);
                var cursor = notebookCollection.Find(query).SetFields(Fields.Include("NotebookContent"));
                string s_notebook;
                foreach (BsonDocument notebook in cursor)
                {
                    s_notebook = notebook["NotebookContent"].AsString;
                    App.Peer.Channel.ReceiveNotebook(s_notebook, username);
                }
                Console.WriteLine("Notebooks broadcast to user " + username);
            }
        }

        public void DistributeNotebook(CLPNotebook notebook, string author)
        {
            if (App.DatabaseUse == App.DatabaseMode.Using)
            {
                App.Peer.Channel.DistributeNotebook(ObjectSerializer.ToString(notebook), author);
            }

        }

        public void DistributeNotebookServer(CLPNotebook notebook, string author)
        {
            //Get Notebooks collection, and save teacher's copy of notebook
            MongoDatabase nb = App.DatabaseServer.GetDatabase("Notebooks");
            MongoCollection<BsonDocument> nbCollection = nb.GetCollection<BsonDocument>("Notebooks");
            nbCollection.Insert(createBsonNotebook(notebook, author));

            //save notebook in MasterNotebook collection (needed?)
            MongoCollection<BsonDocument> masterNBCollection = nb.GetCollection<BsonDocument>("MasterNotebooks");
            masterNBCollection.Insert(createBsonNotebook(notebook, App.Peer.UserName));

            //Access students in class
            MongoCollection<BsonDocument> classesCollection = nb.GetCollection<BsonDocument>("Classes");
            var query = Query.EQ("name", "TestClass"); //change in future to refer to current class
            BsonDocument currentClass = classesCollection.FindOne(query);
            BsonArray students = currentClass["students"].AsBsonArray;

            //Prepair to allocate a copy of new notebook for each student in class
            System.Collections.ArrayList studentNotebookCopies = new System.Collections.ArrayList();
            foreach (string singleStudent in students)
            {
                //Create copy of notebook for each student
                studentNotebookCopies.Add(createBsonNotebook(notebook, singleStudent));
            }
            nbCollection.InsertBatch(studentNotebookCopies.ToArray());
            
        }

        private BsonDocument createBsonNotebook(CLPNotebook notebook, string userName)
        {
            BsonDocument currentNotebook = new BsonDocument {
                    { "ID", notebook.UniqueID },
                    {"User", userName}, 
                    { "CreationDate", notebook.CreationDate.ToString() },
                    {"SaveDate", DateTime.Now.ToString()},
                    { "NotebookName", notebook.NotebookName },
                    { "NotebookContent", ObjectSerializer.ToString(notebook) }
                    };
            return currentNotebook;
        }

        public void Initialize()
        {
        }

        //DONT REMOVE
        private void testNetworkBandwidth()
        {
            //int start = 1000;
            //int mult = 3000;
            //int currentSize;
            //DateTime currentTime;
            //string content = generateRandomString(start);
            //string increment = generateRandomString(mult);

            //for (int i = 0; i < 71; i++)
            //{
            //    currentSize = start + i * mult;
            //    if (i != 0)
            //    {
            //        content = content + increment;
            //    }
            //    for (int t = 0; t < 5; t++)
            //    {
            //        currentTime = DateTime.Now;
            //        App.Peer.Channel.TestNetworkSending(content, currentTime, i, currentSize, App.Peer.UserName);
            //        Logger.Instance.WriteToLog("-------------------------------------");
            //        Logger.Instance.WriteToLog("Item sent: " + i.ToString());
            //        Console.WriteLine("Item sent: " + i.ToString() + " trial " + t.ToString());
            //        Logger.Instance.WriteToLog("Size sent: " + currentSize.ToString());
            //        System.Threading.Thread.Sleep(5000);
            //    }
            //}
        }

        //for network testing only
        private String generateRandomString(int length)
        {
            //Initiate objects & vars
            Random random = new Random();
            String randomString = "";
            int randNumber;

            //Loop ‘length’ times to generate a random number or character
            for (int i = 0; i < length; i++)
            {
                if (random.Next(1, 3) == 1)
                    randNumber = random.Next(97, 123); //char {a-z}
                else
                    randNumber = random.Next(48, 58); //int {0-9}

                //append random char or digit to random string
                randomString = randomString + (char)randNumber;
            }
            //return the random string
            return randomString;
        }
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           