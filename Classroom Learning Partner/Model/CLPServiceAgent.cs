﻿using System;
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


namespace Classroom_Learning_Partner.Model
{
    
    public class CLPServiceAgent
    {
        private CLPServiceAgent()
        {
        }

        private static CLPServiceAgent _instance = new CLPServiceAgent();
        public static CLPServiceAgent Instance { get { return _instance; } }

        public void AddSubmission(CLPPage page)
        {
            //App.CurrentNotebookViewModel.AddStudentSubmission(page.UniqueID, new CLPPageViewModel(page, App.CurrentNotebookViewModel));
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
                        App.MainWindowViewModel.CurrentNotebookIndex = App.MainWindowViewModel.OpenNotebooks.IndexOf(otherNotebook);
                        count++;
                        break;
                    }
                }

                if (count == 0)
                {
                    App.MainWindowViewModel.OpenNotebooks.Add(notebook);
                    App.MainWindowViewModel.CurrentNotebookIndex = App.MainWindowViewModel.OpenNotebooks.Count - 1;
                }

                App.MainWindowViewModel.SelectedWorkspace = new NotebookWorkspaceViewModel();
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
                        App.MainWindowViewModel.CurrentNotebookIndex = App.MainWindowViewModel.OpenNotebooks.Count - 1;
                        App.MainWindowViewModel.SelectedWorkspace = new NotebookWorkspaceViewModel();
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


        //public void ConvertNotebookToXPS(CLPNotebookViewModel notebookVM)
        //{
        //    throw new NotImplementedException();
        //}


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
                string s_page = ObjectSerializer.ToString(page);
                App.Peer.Channel.SubmitPage(s_page, App.Peer.UserName);
            }
           
        }


        //public void SendLaserPosition(Point pt)
        //{
        //    //want to wrap this to check if Channel is null, will throw an exception if the "projector" isn't on. 
        //    if (App.Peer.Channel != null)
        //    {
        //        App.Peer.Channel.LaserUpdate(pt);
        //    }
        //}

        //public void TurnOffLaser()
        //{
        //    if (App.Peer.Channel != null)
        //    {
        //        App.Peer.Channel.TurnOffLaser();
        //    }
        //}
        private bool undoRedo = false;
        public void AddPageObjectToPage(CLPPageObjectBase pageObject, bool undo)
        {
            undoRedo = undo;
            Point p = pageObject.Position;
            AddPageObjectToPage(pageObject);
            undoRedo = false;
        }
        public void AddPageObjectToPage(CLPPageObjectBase pageObject)
        {
            //AppMessages.RequestCurrentDisplayedPage.Send((pageViewModel) =>
            //{
            //    CLPPageObjectBaseViewModel pageObjectViewModel;
            //    if (pageObject is CLPImage)
            //    {
            //        pageObjectViewModel = new CLPImageViewModel(pageObject as CLPImage, pageViewModel);
            //    }
            //    else if (pageObject is CLPImageStamp)
            //    {
            //        pageObjectViewModel = new CLPImageStampViewModel(pageObject as CLPImageStamp, pageViewModel);
            //    }
            //    else if (pageObject is CLPBlankStamp)
            //    {
            //        pageObjectViewModel = new CLPBlankStampViewModel(pageObject as CLPBlankStamp, pageViewModel);
            //    }
            //    else if (pageObject is CLPTextBox)
            //    {
            //        pageObjectViewModel = new CLPTextBoxViewModel(pageObject as CLPTextBox, pageViewModel);
            //    }
            //    else if (pageObject is CLPSnapTileContainer)
            //    {
            //        pageObjectViewModel = new CLPSnapTileContainerViewModel(pageObject as CLPSnapTileContainer, pageViewModel);
            //    }
            //    else
            //    {
            //        pageObjectViewModel = null;
            //    }

            //    pageViewModel.PageObjectContainerViewModels.Add(new PageObjectContainerViewModel(pageObjectViewModel));
            //    pageViewModel.Page.PageObjects.Add(pageObjectViewModel.PageObject);
                
            //    if (!undoRedo)
            //    {
            //        CLPHistoryItem item = new CLPHistoryItem("ADD");
            //        pageViewModel.HistoryVM.AddHistoryItem(pageObject, item);
            //    }
                //DATABASE add pageobject to current page
            //});
        }
        
        public void RemovePageObjectFromPage(CLPPageObjectBaseViewModel pageObject, bool undo)
        {
            undoRedo = undo;
            RemovePageObjectFromPage(pageObject);
            undoRedo = false;
        }
        public void RemovePageObjectFromPage(ICLPPageObject pageObject)
        {
            //Steve - will not work with grid display
            ((App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).SelectedDisplay as LinkedDisplayViewModel).DisplayedPage.PageObjects.Remove(pageObject);
            //pageObjectContainerViewModel.PageObjectViewModel.PageViewModel.PageObjectContainerViewModels.Remove(pageObjectContainerViewModel);
            //pageObjectContainerViewModel.PageObjectViewModel.PageViewModel.Page.PageObjects.Remove(pageObjectContainerViewModel.PageObjectViewModel.PageObject);
            ////AppMessages.RequestCurrentDisplayedPage.Send((pageViewModel) =>
            ////{
            ////    pageViewModel.PageObjectContainerViewModels.Remove(pageObjectContainerViewModel);
            ////    pageViewModel.Page.PageObjects.Remove(pageObjectContainerViewModel.PageObjectViewModel.PageObject);
            ////    //DATABASE remove page object from current page
            ////});
            //if (!undoRedo)
            //{
            //    CLPHistoryItem item = new CLPHistoryItem("ERASE");
            //    pageObjectContainerViewModel.PageObjectViewModel.PageViewModel.HistoryVM.AddHistoryItem(pageObjectContainerViewModel.PageObjectViewModel.PageObject, item);
            //}
        }
        public void RemovePageObjectFromPage(CLPPageObjectBaseViewModel pageObject)
        {
            //foreach (var container in pageObject.PageViewModel.PageObjectContainerViewModels)
            //{
            //    if (container.PageObjectViewModel.PageObject.UniqueID == pageObject.PageObject.UniqueID)
            //    {
            //        RemovePageObjectFromPage(container);
            //        break;
            //    }
            //}
        }
        
        public void RemoveStrokeFromPage(Stroke stroke, CLPPageViewModel page)
        {
            Stroke s = null;
            foreach (var v in page.InkStrokes)
            {
                
                if(stroke.GetPropertyData(CLPPage.StrokeIDKey).ToString().Equals(v.GetPropertyData(CLPPage.StrokeIDKey).ToString()) )
                    {
                        s = v;
                        break;
                    }
            }
            if(s != null)
                page.InkStrokes.Remove(s);

        }
        public void RemoveStrokeFromPage(Stroke stroke, CLPPageViewModel page, bool isUndo)
        {
            page.undoFlag = isUndo;
            RemoveStrokeFromPage(stroke, page);
            page.undoFlag = false;
        }
        public void AddStrokeToPage(Stroke stroke, CLPPageViewModel page)
        {
            page.InkStrokes.Add(stroke);
            
        }
        public void AddStrokeToPage(Stroke stroke, CLPPageViewModel page, bool isUndo)
        {
            page.undoFlag = isUndo;
            AddStrokeToPage(stroke, page);
            page.undoFlag = false;
        }
        public void ChangePageObjectPosition(PageObjectContainerViewModel pageObjectContainerViewModel, Point pt)
        {
            Point oldLocation = pageObjectContainerViewModel.Position;
            pageObjectContainerViewModel.Position = pt;
            
            //if (!undoRedo)
            //{
            //    CLPHistoryItem item = new CLPHistoryItem("MOVE");
            //    item.OldValue = oldLocation.ToString();
            //    item.NewValue = pt.ToString();
            //    pageObjectContainerViewModel.PageObjectViewModel.PageViewModel.HistoryVM.AddHistoryItem(pageObjectContainerViewModel.PageObjectViewModel.PageObject, item);
            //}


            //if (pageObjectContainerViewModel.PageObjectViewModel is CLPSnapTileContainerViewModel)
            //{
            //    CLPSnapTileContainerViewModel snapTileVM = pageObjectContainerViewModel.PageObjectViewModel as CLPSnapTileContainerViewModel;
            //    if (snapTileVM.NextTile != null)
            //    {
            //        foreach (var container in snapTileVM.PageViewModel.PageObjectContainerViewModels)
            //        {
            //            if (container.PageObjectViewModel is CLPSnapTileContainerViewModel)
            //            {
            //                if ((container.PageObjectViewModel as CLPSnapTileContainerViewModel).PageObject.UniqueID == snapTileVM.NextTile.PageObject.UniqueID)
            //                {
            //                    container.Position = new Point(pageObjectContainerViewModel.Position.X, pageObjectContainerViewModel.Position.Y + CLPSnapTileContainer.TILE_HEIGHT);
            //                    container.PageObjectViewModel.Position = new Point(pageObjectContainerViewModel.Position.X, pageObjectContainerViewModel.Position.Y + CLPSnapTileContainer.TILE_HEIGHT);
            //                    container.PageObjectViewModel.PageObject.Position = new Point(pageObjectContainerViewModel.Position.X, pageObjectContainerViewModel.Position.Y + CLPSnapTileContainer.TILE_HEIGHT);
            //                }
            //            }
                        
            //        }
            //    }
            //}
            //send change to projector and students?
            //DATABASE change page object's position
        }
        public void ChangePageObjectPosition(CLPPageObjectBaseViewModel pageObject, Point pt, bool isUndo)
        {
            //undoRedo = isUndo;
            //foreach (var container in pageObject.PageViewModel.PageObjectContainerViewModels)
            //{
            //    if (container.PageObjectViewModel.PageObject.UniqueID == pageObject.PageObject.UniqueID)
            //    {
            //        ChangePageObjectPosition(container, pt);
            //        break;
            //    }
            //}
            //undoRedo = false;

        }
       
        public void ChangePageObjectDimensions(PageObjectContainerViewModel pageObjectContainerViewModel, double height, double width)
        {
            double oldHeight = pageObjectContainerViewModel.Height;
            double oldWidth = pageObjectContainerViewModel.Width;
            Tuple<double, double> oldValue = new Tuple<double, double>(oldHeight, oldWidth);
            Tuple<double, double> newValue = new Tuple<double, double>(height, width);
            pageObjectContainerViewModel.Height = height;
            pageObjectContainerViewModel.Width = width;
            //pageObjectContainerViewModel.PageObjectViewModel.PageObject.Height = height;
            //pageObjectContainerViewModel.PageObjectViewModel.PageObject.Width = width;
            //DATABASE change page object's dimensions
            //if (!undoRedo)
            //{
            //    CLPHistoryItem item = new CLPHistoryItem("RESIZE");
            //    item.OldValue = oldValue.ToString();
            //    item.NewValue = newValue.ToString();
            //    pageObjectContainerViewModel.PageObjectViewModel.PageViewModel.HistoryVM.AddHistoryItem(pageObjectContainerViewModel.PageObjectViewModel.PageObject, item);
            //}
        }
        public void ChangePageObjectDimensions(CLPPageObjectBaseViewModel pageObject, double height, double width, bool isUndo)
        {
            //undoRedo = isUndo;
            //foreach (var container in pageObject.PageViewModel.PageObjectContainerViewModels)
            //{
            //    if (container.PageObjectViewModel.PageObject.UniqueID == pageObject.PageObject.UniqueID)
            //    {
            //        ChangePageObjectDimensions(container, height, width);
            //        break;
            //    }
            //}
            //undoRedo = false;
        }
        public void SendInkCanvas(System.Windows.Controls.InkCanvas ink)
        {
            //AppMessages.RequestCurrentDisplayedPage.Send((pageViewModel) =>
            //{
            //    pageViewModel.HistoryVM.InkCanvas = ink;
            //});
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
    }
}
