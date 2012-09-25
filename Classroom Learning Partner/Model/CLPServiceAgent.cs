using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ProtoBuf;

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

        public void AddSubmission(CLP.Models.CLPNotebook notebook, CLP.Models.CLPPage page)
        {
            notebook.AddStudentSubmission(page.UniqueID, page);
        }

        public void OpenNotebook(string notebookName)
        {

            string filePath = App.NotebookDirectory + @"\" + notebookName + @".clp";
            if (File.Exists(filePath))
            {
                
                DateTime start = DateTime.Now;
                CLP.Models.CLPNotebook notebook = null;

                //Steve - Conversion happens here
                try
                {
                    notebook = CLP.Models.CLPNotebook.Load(filePath, true);
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteToLog("[ERROR] - Notebook could not be loaded: " + ex.Message);
                }
                
                DateTime end = DateTime.Now;
                TimeSpan span = end.Subtract(start);
                Logger.Instance.WriteToLog("Time to open notebook (In Milliseconds): " + span.TotalMilliseconds);
                Logger.Instance.WriteToLog("Time to open notebook (In Seconds): " + span.TotalSeconds);
                Logger.Instance.WriteToLog("Time to open notebook (In Minutes): " + span.TotalMinutes);
                if (notebook != null)
                {
                    notebook.NotebookName = notebookName;

                    int count = 0;
                    foreach (var otherNotebook in App.MainWindowViewModel.OpenNotebooks)
                    {
                        if (otherNotebook.UniqueID == notebook.UniqueID && otherNotebook.NotebookName == notebook.NotebookName)
                        {
                            App.MainWindowViewModel.SelectedWorkspace = new NotebookWorkspaceViewModel(otherNotebook);
                            count++;
                            break;
                        }
                    }

                    if (count == 0)
                    {
                        App.MainWindowViewModel.OpenNotebooks.Add(notebook);
                        if (App.CurrentUserMode == App.UserMode.Instructor || App.CurrentUserMode == App.UserMode.Student)
                        {
                            App.MainWindowViewModel.SelectedWorkspace = new NotebookWorkspaceViewModel(notebook);
                        }
                        else
                        {
                            App.MainWindowViewModel.SelectedWorkspace = new ProjectorWorkspaceViewModel();
                        }

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
            //make async?
            //compare model w/ database
            DateTime startLocalSave = DateTime.Now;
            string filePath = App.NotebookDirectory + @"\" + notebook.NotebookName + @".clp";
            if (App.CurrentUserMode == App.UserMode.Student)
            {
                notebook.Submissions.Clear();
            }
            notebook.Save(filePath);
            TimeSpan timeToSaveLocal = DateTime.Now.Subtract(startLocalSave);
            //System.Threading.Thread

            if (App.DatabaseUse == App.DatabaseMode.Using && App.CurrentUserMode == App.UserMode.Student && App.Peer.Channel != null)
            {

                int numPagesSaved = 0;
                DateTime startSavingTime= DateTime.Now;
                HistoryItemType lastItem = HistoryItemType.EraseInk;
                int count = 0;
                foreach(CLP.Models.CLPPage page in notebook.Pages)
                {


                    if (!page.PageHistory.IsSaved())
                    {
                        numPagesSaved++;
                        DateTime now = DateTime.Now;
                        CLP.Models.CLPPage p = page;
                        //submit page, removing history first

                        CLP.Models.CLPHistory tempHistory = CLP.Models.CLPHistory.removeHistoryFromPage(page);

                        //Serialize using protobuf
                        MemoryStream stream = new MemoryStream();
                        Serializer.PrepareSerializer<CLP.Models.CLPPage>();
                        Serializer.Serialize<CLP.Models.CLPPage>(stream, p);
                        string s_page_pb = Convert.ToBase64String(stream.ToArray());
                        //string s_page = ObjectSerializer.ToString(notebook);

                        //Actual send


                        //CLPServiceAgent.TimeCallBack(Tuple.Create<string, string>(s_page_pb, notebook.NotebookName));
                        // System.Threading.Thread thread = new System.Threading.Thread(() =>
                        // {
                        System.Threading.ThreadPool.QueueUserWorkItem(state =>
                        {
                            App.Peer.Channel.SavePage(s_page_pb, App.Peer.UserName, now, notebook.NotebookName, p.PageIndex);
                        });
                        //Logger is not thread safe
                        //So, page likely was sent, but no guarantee
                        Logger.Instance.WriteToLog("Page " + p.PageIndex.ToString() + " sent to server(save), size: " + (s_page_pb.Length / 1024.0).ToString() + " kB,  Last history item " + lastItem.ToString());
                        //});
                        //thread.Start();
                        //replace history:
                        CLP.Models.CLPHistory.replaceHistoryInPage(tempHistory, page);
                        CLP.Models.CLPHistoryItem item = new CLP.Models.CLPHistoryItem(CLP.Models.HistoryItemType.Save, null, null, null);
                        page.PageHistory.HistoryItems.Add(item);
                    }
                    else
                    {
                        Logger.Instance.WriteToLog("Page " + page.PageIndex.ToString() + " no changed registered, ");
                    }

                }
               
                Logger.Instance.WriteToLog("Network Saving " + numPagesSaved.ToString() + " took " + DateTime.Now.Subtract(startSavingTime).ToString()
                    + ",  Local Save took " + timeToSaveLocal.ToString());
                Logger.Instance.WriteToLog("===================");
            }

        }



        public void SaveNotebookDB(CLP.Models.CLPNotebook notebook, string userName)
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
                    currentNotebook["SaveDate"] = BsonDateTime.Create(DateTime.UtcNow);
                    currentNotebook["NotebookContent"] = ObjectSerializer.ToString(notebook);
                    nbCollection.Save(currentNotebook);
                }
                else
                {
                    nbCollection.Insert(createBsonNotebook(notebook, userName));
                }
            }
        }

        public void SaveNotebooksFromDBToHD(CLP.Models.CLPNotebook notebook)
        {
            switch (App.CurrentUserMode)
            {
                case App.UserMode.Student:
                    string filePath = App.NotebookDirectory + @"\" + notebook.NotebookName + @".clp";
                    notebook.Save(filePath);
                    break;
            }
        }

        public void SavePageDB(CLP.Models.CLPPage page, string userName, bool isSubmission, DateTime saveDate, string notebookName)
        {
            string s_page = ObjectSerializer.ToString(page);
            if (App.DatabaseUse == App.DatabaseMode.Using && App.CurrentUserMode == App.UserMode.Server)
            {

                //save to database
                MongoDatabase nb = App.DatabaseServer.GetDatabase("Notebooks");
                MongoCollection<BsonDocument> pageCollection;
                if (isSubmission)
                {
                    pageCollection = nb.GetCollection<BsonDocument>("Pages");
                    pageCollection.Insert(createBsonPage(page, s_page, userName, saveDate, notebookName));
                }
                else
                {
                    pageCollection = nb.GetCollection<BsonDocument>("SavedPages");
                    var query = Query.And(Query.EQ("ID", page.UniqueID), Query.EQ("User", userName));
                    BsonDocument currentPage = pageCollection.FindOne(query);
                    if (currentPage != null)
                    {
                        //update with newer notebook version
                        currentPage["SaveDate"] = BsonDateTime.Create(saveDate);
                        currentPage["PageContent"] = s_page;
                        pageCollection.Save(currentPage);
                    }
                    else
                    {
                        //create new page- page for this student has never been saved before 
                        pageCollection.Insert(createBsonPage(page, s_page, userName, saveDate, notebookName));
                    }
                }

            }
        }

        public void GetNotebookNames(NotebookChooserWorkspaceViewModel notebookChooserVM)
        {
            if (!Directory.Exists(App.NotebookDirectory))
            {
                Directory.CreateDirectory(App.NotebookDirectory);
            }
            //normal operation - take what is already available
            foreach (string fullFile in Directory.GetFiles(App.NotebookDirectory, "*.clp"))
            {
                string notebookName = Path.GetFileNameWithoutExtension(fullFile);
                notebookChooserVM.NotebookNames.Add(notebookName);
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


        //Method for logging size of submissions with various serialization methods
        //Used for testing
        private void serializationSizes(CLP.Models.CLPPage page, string notebookName)
        {
            //Size tests
            List<double> sizes = new List<double>();


            //remove history before serializing 
            CLP.Models.CLPHistory tempHistory = CLP.Models.CLPHistory.removeHistoryFromPage(page);

            string oldSubmissionID = page.SubmissionID;
            page.SubmissionID = Guid.NewGuid().ToString();
            page.SubmissionTime = DateTime.Now;


            //ProtoBufTest - Page
            //Serialize using protobuf
            MemoryStream stream = new MemoryStream();
            Serializer.PrepareSerializer<CLP.Models.CLPPage>();
            Serializer.Serialize<CLP.Models.CLPPage>(stream, page);
            string s_page_pb = Convert.ToBase64String(stream.ToArray());

            // Add BFPage
            string s_page = ObjectSerializer.ToString(page);
            double size_standard = s_page.Length / 1024.0;
            sizes.Add(size_standard);
            
            //Add PB Page
            sizes.Add(s_page_pb.Length / 1024.0);

            //Test deserialize 
            //Stream stream2 = new MemoryStream(Convert.FromBase64String(s_page_pb));
            //CLPPage page2 = new CLPPage();
            //page2 = Serializer.Deserialize<CLPPage>(stream2);
            //App.PageTypeModel.Deserialize(stream2, page2, typeof(CLPPage));

            //BF History
            string s_history = ObjectSerializer.ToString(tempHistory);
            sizes.Add(s_history.Length / 1024.0);

            //ProtoBufTest - History
            App.PageTypeModel[typeof(CLP.Models.CLPHistory)].CompileInPlace();
            MemoryStream stream3 = new MemoryStream();
            App.PageTypeModel.Serialize(stream3, tempHistory);
            //Serializer.Serialize<CLPHistory>(stream3, tempHistory);
            string s_history_pb = Convert.ToBase64String(stream3.ToArray());
            sizes.Add(s_history_pb.Length / 1024.0);

            //Test deserialize 
            // Stream stream4 = new MemoryStream(Convert.FromBase64String(s_history_pb));
            //CLPHistory history = new CLPHistory();
            //CLPHistory history = Serializer.Deserialize<CLPHistory>(stream4);
            //App.PageTypeModel.Deserialize(stream4, history, typeof(CLPHistory));

            //put the history back into the page
            CLP.Models.CLPHistory.replaceHistoryInPage(tempHistory, page);


            //log sizes
            Logger.Instance.WriteToLog("==== Serialization Size (protobuf) (in .5 kB) for page " + page.PageIndex.ToString());
            Logger.Instance.WriteToLog("Page w/o  History " + sizes[0].ToString() + " " + sizes[1].ToString());
            Logger.Instance.WriteToLog("Full      History " + sizes[2].ToString() + " " + sizes[3].ToString());
            //Logger.Instance.WriteToLog("Segmented History " + sizes[4].ToString() + " " + sizes[5].ToString());
            //Logger.Instance.WriteToLog("Num Full History Items " + sizes[6].ToString());
            //Logger.Instance.WriteToLog("Num Seg History  Items " + sizes[7].ToString());
        }

        public void SubmitPage(CLP.Models.CLPPage page, string notebookName)
        {
            if (App.Peer.Channel != null)
            {

                //remove history before sending
                CLP.Models.CLPHistory tempHistory = CLP.Models.CLPHistory.removeHistoryFromPage(page);

                string oldSubmissionID = page.SubmissionID;
                page.SubmissionID = Guid.NewGuid().ToString();
                page.SubmissionTime = DateTime.Now;
                

                //ProtoBufTest - Page
                //Serialize using protobuf
                MemoryStream stream = new MemoryStream();
                Serializer.PrepareSerializer<CLP.Models.CLPPage>();
                Serializer.Serialize<CLP.Models.CLPPage>(stream, page);
                string s_page_pb = Convert.ToBase64String(stream.ToArray());
                double pbPageSize = (s_page_pb.Length / 1024.0);

                string sPage = ObjectSerializer.ToString(page);

                //Submit Page using PB
                App.Peer.Channel.SubmitFullPage(sPage, App.Peer.UserName, notebookName);

                //put the history back into the page
                CLP.Models.CLPHistory.replaceHistoryInPage(tempHistory, page);

                page.PageHistory.HistoryItems.Add(new CLP.Models.CLPHistoryItem(CLP.Models.HistoryItemType.Submit, null, oldSubmissionID, page.SubmissionID));
                page.PageHistory.HistoryItems.Add(new CLP.Models.CLPHistoryItem(CLP.Models.HistoryItemType.Save, null, null, null)); 

                //log sizes
                Logger.Instance.WriteToLog("==== Serialization Size (protobuf) (in .5 kB) for page " + page.PageIndex.ToString() + " : " + pbPageSize);
            }
        }

        public void AddPageObjectToPage(CLP.Models.ICLPPageObject pageObject)
        {
            AddPageObjectToPage(pageObject.ParentPage, pageObject);
        }

        public void AddPageObjectToPage(CLP.Models.CLPPage page, CLP.Models.ICLPPageObject pageObject)
        {
            if (page != null)
            {
                pageObject.IsBackground = App.MainWindowViewModel.IsAuthoring;
                page.PageObjects.Add(pageObject);

                if (!page.PageHistory.IgnoreHistory)
                {
                    CLP.Models.CLPHistoryItem item = new CLP.Models.CLPHistoryItem(CLP.Models.HistoryItemType.AddPageObject, pageObject.UniqueID, null, null);
                    page.PageHistory.HistoryItems.Add(item);                  
                }
            }
        }

        public void RemovePageObjectFromPage(CLP.Models.ICLPPageObject pageObject)
        {
            RemovePageObjectFromPage(pageObject.ParentPage, pageObject);
        }

        public void RemovePageObjectFromPage(CLP.Models.CLPPage page, CLP.Models.ICLPPageObject pageObject)
        {
            if (page != null)
            {
                //page.PageObjects.Remove(pageObject);
                foreach(CLP.Models.ICLPPageObject po in page.PageObjects)
                {
                    if (po.UniqueID == pageObject.UniqueID)
                    {
                        page.PageObjects.Remove(po);
                        break;
                    }
                }
                if (!page.PageHistory.IgnoreHistory)
                {
                    CLP.Models.CLPHistoryItem item = new CLP.Models.CLPHistoryItem(CLP.Models.HistoryItemType.RemovePageObject, pageObject.UniqueID, ObjectSerializer.ToString(pageObject), null);
                    page.PageHistory.HistoryItems.Add(item);
                }
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

        public void DistributeNotebook(CLP.Models.CLPNotebook notebook, string author)
        {
            if (App.DatabaseUse == App.DatabaseMode.Using)
            {
                App.Peer.Channel.DistributeNotebook(ObjectSerializer.ToString(notebook), author);
            }

        }

        public void DistributeNotebookServer(CLP.Models.CLPNotebook notebook, string author)
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

        private BsonDocument createBsonNotebook(CLP.Models.CLPNotebook notebook, string userName)
        {
            BsonDocument currentNotebook = new BsonDocument {
                    { "ID", notebook.UniqueID },
                    {"User", userName}, 
                    { "CreationDate", BsonDateTime.Create(notebook.CreationDate.ToUniversalTime()) },
                    {"SaveDate", BsonDateTime.Create(DateTime.UtcNow)},
                    { "NotebookName", notebook.NotebookName },
                    { "NotebookContent", ObjectSerializer.ToString(notebook) }
                    };
            return currentNotebook;
        }
        private BsonDocument createBsonHistory(string s_history, string pageID, string userName, DateTime saveDate)
        {
            return new BsonDocument {
                    { "ID", pageID },
                    {"SaveDate", BsonDateTime.Create(saveDate) },
                    {"User", userName},
                        { "HistoryContent", s_history }
                    };
        }
        private BsonDocument createBsonPage(CLP.Models.CLPPage page, string s_page, string userName, DateTime saveDate, string notebookName)
        {
            return new BsonDocument {
                    { "ID", page.UniqueID },
                    {"ParentNotebookID", page.ParentNotebookID},
                    {"NotebookName", notebookName},
                    {"PageNumber", page.PageIndex},
                    { "CreationDate", BsonDateTime.Create(page.CreationDate.ToUniversalTime()) },
                    {"SaveDate", BsonDateTime.Create(saveDate) },
                    {"PageTopics", BsonArray.Create(page.PageTopics.ToList())},
                    {"User", userName},
                    { "PageContent", s_page }
                    };
        }
        public void Initialize()
        {
        }

        //Function that reads in all notebooks in notebook folder and saves them to database
        //Used for saving old data to db

        public void ImportLocalNotebooksFromDB()
        {
            Logger.Instance.WriteToLog("ImportLocalNotebooksFromDB Called");
            string[] filePaths = Directory.GetFiles(App.NotebookDirectory + @"\");
            CLP.Models.CLPNotebook notebook;
            DateTime start, end;
            TimeSpan span;
            string[] parts, dateParts;
            DateTime date;
            string userName, notebookName;
            foreach (string nbPath in filePaths)
            {
                //pull of info from path
                parts = Path.GetFileNameWithoutExtension(nbPath).Split('-');
                dateParts = parts[0].Split('.');
                date = new DateTime(Convert.ToInt32(dateParts[0]), Convert.ToInt32(dateParts[1]), Convert.ToInt32(dateParts[2]));
                userName = parts[2];


                //load notebook
                start = DateTime.Now;
                notebook = CLP.Models.CLPNotebook.Load(nbPath);
                end = DateTime.Now;
                span = end.Subtract(start);
                Logger.Instance.WriteToLog("Time to open " + userName + "  "+ parts[1] + "  notebook (In Seconds): " + span.TotalSeconds);
                notebookName = notebook.NotebookName;
                //Save to DB
                foreach(CLP.Models.CLPPage page in notebook.Pages)
                {
                    //Okay to save teacher history? Who knows
                    CLP.Models.CLPHistory tempHistory = CLP.Models.CLPHistory.removeHistoryFromPage(page);
                    CLPServiceAgent.Instance.SaveHistoryDB(tempHistory, page.UniqueID, userName, date);
                    CLPServiceAgent.Instance.SavePageDB(page, userName, false, date, notebookName);
                }
                Logger.Instance.WriteToLog("Done saving " + userName + "  " + parts[1] + "  notebook");
            }
        }

        public void SaveHistoryDB(CLP.Models.CLPHistory history, string pageID, string userName, DateTime saveDate)
        {
            string s_history = ObjectSerializer.ToString(history);
            if (App.DatabaseUse == App.DatabaseMode.Using && App.CurrentUserMode == App.UserMode.Server)
            {

                //save to database
                MongoDatabase nb = App.DatabaseServer.GetDatabase("Notebooks");
                MongoCollection<BsonDocument> historyCollection;

                historyCollection = nb.GetCollection<BsonDocument>("SavedHistories");
                var query = Query.And(Query.EQ("ID", pageID), Query.EQ("User", userName));
                BsonDocument currentPage = historyCollection.FindOne(query);
                if (currentPage != null)
                {
                    //update with newer notebook version
                    currentPage["SaveDate"] = BsonDateTime.Create(saveDate);
                    currentPage["HistoryContent"] = s_history;
                    historyCollection.Save(currentPage);
                }
                else
                {
                    //create new history- history for this student has never been saved before 
                    historyCollection.Insert(createBsonHistory(s_history, pageID, userName, saveDate));
                }
                

            }
            
        }

        internal void RunDBQueryForPages()
        {

            if (App.DatabaseUse == App.DatabaseMode.Using){
                Logger.Instance.WriteToLog("RunDBQueryForPages called");
                DateTime start = DateTime.Now;

                MongoDatabase nb = App.DatabaseServer.GetDatabase("Notebooks");
                MongoCollection<BsonDocument> pageCollection = nb.GetCollection<BsonDocument>("SavedPages");
               // var query = Query.And(Query.EQ("ID", page.UniqueID), Query.EQ("User", userName));
                var query = Query.EQ("User", "Teacher");
                MongoCursor cursor = pageCollection.Find(query).SetSortOrder(SortBy.Ascending("PageNumber"));
                Logger.Instance.WriteToLog("Query for Teacher pages takes" + DateTime.Now.Subtract(start).TotalSeconds.ToString() +  " seconds");

                CLP.Models.CLPNotebook newNotebook = new CLP.Models.CLPNotebook();
                newNotebook.NotebookName = "NotebookFromDBQuery";
                newNotebook.Pages.RemoveAt(0);
                start = DateTime.Now;
                foreach (BsonDocument page in cursor)
                {

                    newNotebook.AddPage(ObjectSerializer.ToObject(page["PageContent"].ToString()) as CLP.Models.CLPPage);
                    
                }
                Logger.Instance.WriteToLog("Adding all Teacher pages takes " + DateTime.Now.Subtract(start).TotalSeconds.ToString() +  " seconds");

                start = DateTime.Now;
                query = Query.NE("User", "Teacher");
                cursor = pageCollection.Find(query).SetSortOrder(SortBy.Ascending("PageNumber"));
                Logger.Instance.WriteToLog("Query for student pages takes" + DateTime.Now.Subtract(start).TotalSeconds.ToString() + " seconds");
                start = DateTime.Now;
                foreach (BsonDocument page in cursor)
                {

                    newNotebook.AddStudentSubmission(page["ID"].ToString(), ObjectSerializer.ToObject(page["PageContent"].ToString()) as CLP.Models.CLPPage);
                   
                }

                Logger.Instance.WriteToLog("Adding all student pages takes " + DateTime.Now.Subtract(start).TotalSeconds.ToString() + " seconds");

                App.MainWindowViewModel.OpenNotebooks.Add(newNotebook);
                App.MainWindowViewModel.SelectedWorkspace = new NotebookWorkspaceViewModel(newNotebook);
            }
            //else
            //{
            ////do something else
            //}
        }

        internal void SaveAllHistories(CLP.Models.CLPNotebook notebook)
        {
            if (App.DatabaseUse == App.DatabaseMode.Using && App.CurrentUserMode == App.UserMode.Student)
            {

                Logger.Instance.WriteToLog("Save All Histories");
                CLP.Models.CLPPage tempP;
                foreach(CLP.Models.CLPPage page in notebook.Pages)
                {
                    if (true) //In the future, check to see if history has been saved 
                    {
                        tempP = page;
                        //submit page, removing history first
                        DateTime now = DateTime.Now;
                        CLP.Models.CLPHistory segmentedHistory = CLP.Models.CLPHistory.GetSegmentedHistory(tempP);
                        
                        //Serialize history using protobuf
                        MemoryStream stream = new MemoryStream();
                        Serializer.PrepareSerializer<CLPHistory>();
                        Serializer.Serialize<CLP.Models.CLPHistory>(stream, segmentedHistory);
                        string s_history_pb = Convert.ToBase64String(stream.ToArray());
                        //string s_page = ObjectSerializer.ToString(notebook);

                        //Actual send

                        System.Threading.ThreadPool.QueueUserWorkItem(state =>
                        {
                            App.Peer.Channel.SaveHistory(s_history_pb, App.Peer.UserName, now, notebook.NotebookName, tempP.UniqueID, tempP.PageIndex);
                        });

                        Logger.Instance.WriteToLog("Page " + tempP.PageIndex.ToString() + " history sent to server(save), size: " + (s_history_pb.Length / 1024.0).ToString() + " kB");
                        System.Threading.Thread.Sleep(250);
                        //replace history:
                        CLP.Models.CLPHistory.replaceHistoryInPage(segmentedHistory, page);

                    }
                }

                Logger.Instance.WriteToLog("===================");
            }
        }
    }
}