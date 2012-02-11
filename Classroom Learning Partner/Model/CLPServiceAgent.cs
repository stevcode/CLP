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
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Input;
using System.Windows.Ink;


namespace Classroom_Learning_Partner.Model
{
    public interface ICLPServiceAgent
    {
        void SetWorkspace();

        void AddPageAt(CLPPage page, int notebookIndex, int submissionIndex);
        void RemovePageAt(int pageIndex);
        void DuplicatePageAt(int pageIndex);

        void AddSubmission(CLPPage page);
        void DistributeNotebook(CLPNotebookViewModel notebookVM, string author);
        void OpenNotebook(string notebookName);
        void OpenNewNotebook();
        void SaveNotebook(CLPNotebookViewModel notebookVM);
        void SaveNotebookDB(CLPNotebook notebookVM, string userName);
        void SavePageDB(CLPPage pageVM);
        void SaveNotebooksFromDBToHD(CLPNotebook notebookVM);
        void ChooseNotebook(NotebookChooserWorkspaceViewModel notebookChooserVM);
        void ConvertNotebookToXPS(CLPNotebookViewModel notebookVM);

        void SubmitPage(CLPPageViewModel pageVM);
        void Exit();

        void SendLaserPosition(Point pt);


        void AddPageObjectToPage(CLPPageObjectBase pageObject);
        void RemovePageObjectFromPage(PageObjectContainerViewModel pageObjectContainerViewModel);
        void RemoveStrokeFromPage(Stroke stroke, CLPPageViewModel page);
        void ChangePageObjectPosition(PageObjectContainerViewModel pageObjectContainerViewModel, Point pt);
        void ChangePageObjectDimensions(PageObjectContainerViewModel pageObjectContainerViewModel, double height, double width);

        void SendInkCanvas(System.Windows.Controls.InkCanvas ink);
        //Calls made on Server to DB
        void RetrieveNotebooks(string username);
        void DistributeNotebookServer(CLPNotebook notebookVM, string author);
    }

    public class CLPServiceAgent : ICLPServiceAgent
    {
        public void AddPageAt(CLPPage page, int notebookIndex, int submissionIndex)
        {
            CLPPageViewModel pageViewModel = new CLPPageViewModel(page, App.CurrentNotebookViewModel);
            if (submissionIndex == -1)
            {
                App.CurrentNotebookViewModel.InsertPage(notebookIndex, pageViewModel);
                App.CurrentNotebookViewModel.Notebook.InsertPage(notebookIndex, page);
                //DATABASE insertion, see InsertPage method in CLPNotebook,
                //inserting new page requires generating the appropriate
                //Submissions list associated with the page.
            }
            else
            {
                //not necessary to insert student submission directly?
            }
        }

        public void RemovePageAt(int pageIndex)
        {
            App.CurrentNotebookViewModel.RemovePageAt(pageIndex);
            App.CurrentNotebookViewModel.Notebook.RemovePageAt(pageIndex);
            //DATABASE remove. make sure to add new blank page if
            //you remove last page in notebook.
        }

        public void DuplicatePageAt(int pageIndex)
        {
            CLPPage originalPage = App.CurrentNotebookViewModel.PageViewModels[pageIndex].Page;
            CLPPage copyPage = new CLPPage();
            CLPPageObjectBase pageObject = new CLPBlankStamp();
            foreach (CLPPageObjectBase obj in originalPage.PageObjects)
            {
                if (obj is CLPBlankStamp)
                {
                    CLPBlankStamp copyStamp = new CLPBlankStamp();
                    CLPBlankStamp originalStamp = obj as CLPBlankStamp;
                    copyStamp.Height = originalStamp.Height;
                    copyStamp.IsAnchored = originalStamp.IsAnchored;
                    copyStamp.Parts = originalStamp.Parts;
                    copyStamp.Position = originalStamp.Position;
                    copyStamp.Width = originalStamp.Width;
                    copyStamp.ZIndex = originalStamp.ZIndex;
                    foreach (var stroke in originalStamp.PageObjectStrokes)
                    {
                        copyStamp.PageObjectStrokes.Add(stroke);
                    }
                    pageObject = copyStamp;
                }
                else if (obj is CLPImage)
                {
                    CLPImage originalImage = obj as CLPImage;
                    CLPImage copyImage = new CLPImage(originalImage.ByteSource);
                    copyImage.Height = originalImage.Height;
                    copyImage.Position = originalImage.Position;
                    copyImage.Width = originalImage.Width;
                    copyImage.ZIndex = originalImage.ZIndex;
                    foreach (var stroke in originalImage.PageObjectStrokes)
                    {
                        copyImage.PageObjectStrokes.Add(stroke);
                    }
                    pageObject = copyImage;
                }
                else if (obj is CLPImageStamp)
                {
                    CLPImageStamp originalImage = obj as CLPImageStamp;
                    CLPImageStamp copyImage = new CLPImageStamp(originalImage.ByteSource);
                    copyImage.Height = originalImage.Height;
                    copyImage.IsAnchored = originalImage.IsAnchored;
                    copyImage.Parts = originalImage.Parts;
                    copyImage.Position = originalImage.Position;
                    copyImage.Width = copyImage.Width;
                    copyImage.ZIndex = originalImage.ZIndex;
                    foreach (var stroke in originalImage.PageObjectStrokes)
                    {
                        copyImage.PageObjectStrokes.Add(stroke);
                    }
                    pageObject = copyImage;
                }
                else if (obj is CLPSnapTile)
                {
                    CLPSnapTile originalTile = obj as CLPSnapTile;
                    CLPSnapTile copyTile = new CLPSnapTile(originalTile.Position, "SpringGreen");
                    copyTile.Height = originalTile.Height;
                    copyTile.Width = originalTile.Width;
                    copyTile.ZIndex = originalTile.ZIndex;
                    foreach (var t in originalTile.Tiles)
                    {
                        copyTile.Tiles.Add(t);
                    }
                    foreach (var stroke in originalTile.PageObjectStrokes)
                    {
                        copyTile.PageObjectStrokes.Add(stroke);
                    }
                    pageObject = copyTile;
                }
                else if (obj is CLPTextBox)
                {
                    CLPTextBox originalTextBox = obj as CLPTextBox;
                    CLPTextBox copyTextBox = new CLPTextBox();
                    copyTextBox.Height = originalTextBox.Height;
                    copyTextBox.Position = originalTextBox.Position;
                    copyTextBox.Text = originalTextBox.Text;
                    copyTextBox.Width = originalTextBox.Width;
                    copyTextBox.ZIndex = originalTextBox.ZIndex;
                    foreach (var stroke in originalTextBox.PageObjectStrokes)
                    {
                        copyTextBox.PageObjectStrokes.Add(stroke);
                    }
                    pageObject = copyTextBox;
                }
                copyPage.PageObjects.Add(pageObject);

            }
            foreach (var stroke in originalPage.Strokes)
            {
                copyPage.Strokes.Add(stroke);
            }
            AddPageAt(copyPage, App.CurrentNotebookViewModel.PageViewModels.Count, -1);
        }

        public void AddSubmission(CLPPage page)
        {
            App.CurrentNotebookViewModel.AddStudentSubmission(page.UniqueID, new CLPPageViewModel(page, App.CurrentNotebookViewModel));
        }

        public void OpenNotebook(string notebookName)
        {
            string filePath = App.NotebookDirectory + @"\" + notebookName + @".clp";
            CLPNotebookViewModel newNotebookViewModel;
            if (File.Exists(filePath))
            {
                //alternatively, pull from database and build
                CLPNotebook notebook = CLPNotebook.LoadNotebookFromFile(filePath);
                notebook.NotebookName = notebookName;
                newNotebookViewModel = new CLPNotebookViewModel(notebook);


                int count = 0;
                foreach (CLPNotebookViewModel notebookVM in App.NotebookViewModels)
                {
                    if (notebookVM.Notebook.UniqueID == newNotebookViewModel.Notebook.UniqueID)
                    {
                        App.CurrentNotebookViewModel = notebookVM;
                        count++;
                        break;
                    }
                }

                if (count == 0)
                {
                    App.NotebookViewModels.Add(newNotebookViewModel);
                    App.CurrentNotebookViewModel = newNotebookViewModel;
                }


                SetWorkspace();


                //change this to open Instructor/Student/Projector Workspace
                //App.MainWindowViewModel.Workspace = new AuthoringWorkspaceViewModel();
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
                    CLPNotebookViewModel newNotebookViewModel;
                    if (!File.Exists(filePath))
                    {
                        newNotebookViewModel = new CLPNotebookViewModel();
                        newNotebookViewModel.Notebook.NotebookName = notebookName;
                        App.NotebookViewModels.Add(newNotebookViewModel);
                        App.CurrentNotebookViewModel = newNotebookViewModel;
                        App.IsAuthoring = true;
                        App.MainWindowViewModel.Workspace = new AuthoringWorkspaceViewModel();
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

        public void SaveNotebook(CLPNotebookViewModel notebookVM)
        {
            //make async?
            //compare VM with model?
            //compare model w/ database
            string filePath = App.NotebookDirectory + @"\" + notebookVM.Notebook.NotebookName + @".clp";
            CLPNotebook.SaveNotebookToFile(filePath, notebookVM.Notebook);
            Console.WriteLine("Notebook saved locally");
            if (App.DatabaseUse == App.DatabaseMode.Using)
            {
                string s_notebook = ObjectSerializer.ToString(notebookVM.Notebook);
                Console.WriteLine("Notebook seralized");
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
                var query = Query.EQ("ID", notebook.MetaData.GetValue("UniqueID"));
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
                    CLPNotebook.SaveNotebookToFile(filePath, notebook);
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
                    { "ID", page.MetaData.GetValue("UniqueID") },
                    { "CreationDate", page.MetaData.GetValue("CreationDate") },
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
            //normal operation - take what is already avalible
            foreach (string fullFile in Directory.GetFiles(App.NotebookDirectory, "*.clp"))
            {
                string notebookName = Path.GetFileNameWithoutExtension(fullFile);
                NotebookSelectorViewModel notebookSelector = new NotebookSelectorViewModel(notebookName);
                notebookChooserVM.NotebookSelectorViewModels.Add(notebookSelector);
            }
        }


        public void ConvertNotebookToXPS(CLPNotebookViewModel notebookVM)
        {
            throw new NotImplementedException();
        }


        public void Exit()
        {
            //ask to save notebooks, large window with checks for all notebooks (possibly also converter?)
            //sync with database
            //run network disconnect

            Environment.Exit(0);

        }


        public void SubmitPage(CLPPageViewModel pageVM)
        {
            if (App.Peer.Channel != null)
            {
                string s_page = ObjectSerializer.ToString(pageVM.Page);
                App.Peer.Channel.SubmitPage(s_page, App.Peer.UserName);
            }
           
        }


        public void SendLaserPosition(Point pt)
        {
            //want to wrap this to check if Channel is null, will throw an exception if the "projector" isn't on. 
            if (App.Peer.Channel != null)
            {
                App.Peer.Channel.LaserUpdate(pt);
            }
        }

        public void TurnOffLaser()
        {
            if (App.Peer.Channel != null)
            {
                App.Peer.Channel.TurnOffLaser();
            }
        }
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
            AppMessages.RequestCurrentDisplayedPage.Send((pageViewModel) =>
            {
                CLPPageObjectBaseViewModel pageObjectViewModel;
                if (pageObject is CLPImage)
                {
                    pageObjectViewModel = new CLPImageViewModel(pageObject as CLPImage, pageViewModel);
                }
                else if (pageObject is CLPImageStamp)
                {
                    pageObjectViewModel = new CLPImageStampViewModel(pageObject as CLPImageStamp, pageViewModel);
                }
                else if (pageObject is CLPBlankStamp)
                {
                    pageObjectViewModel = new CLPBlankStampViewModel(pageObject as CLPBlankStamp, pageViewModel);
                }
                else if (pageObject is CLPTextBox)
                {
                    pageObjectViewModel = new CLPTextBoxViewModel(pageObject as CLPTextBox, pageViewModel);
                }
                else if (pageObject is CLPSnapTile)
                {
                    pageObjectViewModel = new CLPSnapTileViewModel(pageObject as CLPSnapTile, pageViewModel);
                }
                else
                {
                    pageObjectViewModel = null;
                }

                pageViewModel.PageObjectContainerViewModels.Add(new PageObjectContainerViewModel(pageObjectViewModel));
                pageViewModel.Page.PageObjects.Add(pageObjectViewModel.PageObject);
                
                if (!undoRedo)
                {
                    CLPHistoryItem item = new CLPHistoryItem("ADD");
                    pageViewModel.HistoryVM.AddHistoryItem(pageObject, item);
                }
                //DATABASE add pageobject to current page
            });
        }
        
        public void RemovePageObjectFromPage(CLPPageObjectBaseViewModel pageObject, bool undo)
        {
            undoRedo = undo;
            RemovePageObjectFromPage(pageObject);
            undoRedo = false;
        }
        public void RemovePageObjectFromPage(PageObjectContainerViewModel pageObjectContainerViewModel)
        {
            pageObjectContainerViewModel.PageObjectViewModel.PageViewModel.PageObjectContainerViewModels.Remove(pageObjectContainerViewModel);
            pageObjectContainerViewModel.PageObjectViewModel.PageViewModel.Page.PageObjects.Remove(pageObjectContainerViewModel.PageObjectViewModel.PageObject);
            //AppMessages.RequestCurrentDisplayedPage.Send((pageViewModel) =>
            //{
            //    pageViewModel.PageObjectContainerViewModels.Remove(pageObjectContainerViewModel);
            //    pageViewModel.Page.PageObjects.Remove(pageObjectContainerViewModel.PageObjectViewModel.PageObject);
            //    //DATABASE remove page object from current page
            //});
            if (!undoRedo)
            {
                CLPHistoryItem item = new CLPHistoryItem("ERASE");
                pageObjectContainerViewModel.PageObjectViewModel.PageViewModel.HistoryVM.AddHistoryItem(pageObjectContainerViewModel.PageObjectViewModel.PageObject, item);
            }
        }
        public void RemovePageObjectFromPage(CLPPageObjectBaseViewModel pageObject)
        {
            foreach (var container in pageObject.PageViewModel.PageObjectContainerViewModels)
            {
                if (container.PageObjectViewModel.PageObject.UniqueID == pageObject.PageObject.UniqueID)
                {
                    RemovePageObjectFromPage(container);
                    break;
                }
            }
        }
        
        public void RemoveStrokeFromPage(Stroke stroke, CLPPageViewModel page)
        {
            Stroke s = null;
            foreach (var v in page.Strokes)
            {
                
                if(stroke.GetPropertyData(CLPPage.StrokeIDKey).ToString().Equals(v.GetPropertyData(CLPPage.StrokeIDKey).ToString()) )
                    {
                        s = v;
                        break;
                    }
            }
            if(s != null)
                page.Strokes.Remove(s);

        }
        public void RemoveStrokeFromPage(Stroke stroke, CLPPageViewModel page, bool isUndo)
        {
            page.undoFlag = isUndo;
            RemoveStrokeFromPage(stroke, page);
            page.undoFlag = false;
        }
        public void AddStrokeToPage(Stroke stroke, CLPPageViewModel page)
        {
            page.Strokes.Add(stroke);
            
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
            pageObjectContainerViewModel.PageObjectViewModel.Position = pt; //may cause trouble?
            pageObjectContainerViewModel.PageObjectViewModel.PageObject.Position = pt;
            
            if (!undoRedo)
            {
                CLPHistoryItem item = new CLPHistoryItem("MOVE");
                item.OldValue = oldLocation.ToString();
                item.NewValue = pt.ToString();
                pageObjectContainerViewModel.PageObjectViewModel.PageViewModel.HistoryVM.AddHistoryItem(pageObjectContainerViewModel.PageObjectViewModel.PageObject, item);
            }


            //if (pageObjectContainerViewModel.PageObjectViewModel is CLPSnapTileViewModel)
            //{
            //    CLPSnapTileViewModel snapTileVM = pageObjectContainerViewModel.PageObjectViewModel as CLPSnapTileViewModel;
            //    if (snapTileVM.NextTile != null)
            //    {
            //        foreach (var container in snapTileVM.PageViewModel.PageObjectContainerViewModels)
            //        {
            //            if (container.PageObjectViewModel is CLPSnapTileViewModel)
            //            {
            //                if ((container.PageObjectViewModel as CLPSnapTileViewModel).PageObject.UniqueID == snapTileVM.NextTile.PageObject.UniqueID)
            //                {
            //                    container.Position = new Point(pageObjectContainerViewModel.Position.X, pageObjectContainerViewModel.Position.Y + CLPSnapTile.TILE_HEIGHT);
            //                    container.PageObjectViewModel.Position = new Point(pageObjectContainerViewModel.Position.X, pageObjectContainerViewModel.Position.Y + CLPSnapTile.TILE_HEIGHT);
            //                    container.PageObjectViewModel.PageObject.Position = new Point(pageObjectContainerViewModel.Position.X, pageObjectContainerViewModel.Position.Y + CLPSnapTile.TILE_HEIGHT);
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
            undoRedo = isUndo;
            foreach (var container in pageObject.PageViewModel.PageObjectContainerViewModels)
            {
                if (container.PageObjectViewModel.PageObject.UniqueID == pageObject.PageObject.UniqueID)
                {
                    ChangePageObjectPosition(container, pt);
                    break;
                }
            }
            undoRedo = false;

        }
       
        public void ChangePageObjectDimensions(PageObjectContainerViewModel pageObjectContainerViewModel, double height, double width)
        {
            double oldHeight = pageObjectContainerViewModel.Height;
            double oldWidth = pageObjectContainerViewModel.Width;
            Tuple<double, double> oldValue = new Tuple<double, double>(oldHeight, oldWidth);
            Tuple<double, double> newValue = new Tuple<double, double>(height, width);
            pageObjectContainerViewModel.Height = height;
            pageObjectContainerViewModel.Width = width;
            pageObjectContainerViewModel.PageObjectViewModel.PageObject.Height = height;
            pageObjectContainerViewModel.PageObjectViewModel.PageObject.Width = width;
            //DATABASE change page object's dimensions
            if (!undoRedo)
            {
                CLPHistoryItem item = new CLPHistoryItem("RESIZE");
                item.OldValue = oldValue.ToString();
                item.NewValue = newValue.ToString();
                pageObjectContainerViewModel.PageObjectViewModel.PageViewModel.HistoryVM.AddHistoryItem(pageObjectContainerViewModel.PageObjectViewModel.PageObject, item);
            }
        }
        public void ChangePageObjectDimensions(CLPPageObjectBaseViewModel pageObject, double height, double width, bool isUndo)
        {
            undoRedo = isUndo;
            foreach (var container in pageObject.PageViewModel.PageObjectContainerViewModels)
            {
                if (container.PageObjectViewModel.PageObject.UniqueID == pageObject.PageObject.UniqueID)
                {
                    ChangePageObjectDimensions(container, height, width);
                    break;
                }
            }
            undoRedo = false;
        }
        public void SendInkCanvas(System.Windows.Controls.InkCanvas ink)
        {
            AppMessages.RequestCurrentDisplayedPage.Send((pageViewModel) =>
            {
                pageViewModel.HistoryVM.InkCanvas = ink;
            });
        }
       
        
        public void SetWorkspace()
        {
            App.IsAuthoring = false;
            App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Hidden;

            switch (App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    App.MainWindowViewModel.Workspace = new ServerWorkspaceViewModel();
                    break;
                case App.UserMode.Instructor:
                    App.MainWindowViewModel.Ribbon.InstructorVisibility = Visibility.Visible;
                    App.MainWindowViewModel.Ribbon.StudentVisibility = Visibility.Collapsed;
                    App.MainWindowViewModel.Ribbon.RibbonVisibility = Visibility.Visible;
                    App.MainWindowViewModel.Workspace = new InstructorWorkspaceViewModel();
                    break;
                case App.UserMode.Projector:
                    App.MainWindowViewModel.Ribbon.InstructorVisibility = Visibility.Collapsed;
                    App.MainWindowViewModel.Ribbon.StudentVisibility = Visibility.Collapsed;
                    App.MainWindowViewModel.Ribbon.RibbonVisibility = Visibility.Collapsed;
                    App.MainWindowViewModel.Workspace = new ProjectorWorkspaceViewModel();
                    break;
                case App.UserMode.Student:
                    App.MainWindowViewModel.Ribbon.InstructorVisibility = Visibility.Collapsed;
                    App.MainWindowViewModel.Ribbon.StudentVisibility = Visibility.Visible;
                    App.MainWindowViewModel.Ribbon.RibbonVisibility = Visibility.Visible;
                    App.MainWindowViewModel.Workspace = new StudentWorkspaceViewModel();
                    break;
            }

            CommandManager.InvalidateRequerySuggested();
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

        public void DistributeNotebook(CLPNotebookViewModel notebookVM, string author)
        {
            if (App.DatabaseUse == App.DatabaseMode.Using)
            {
                App.Peer.Channel.DistributeNotebook(ObjectSerializer.ToString(notebookVM.Notebook), author);
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
                    { "ID", notebook.MetaData.GetValue("UniqueID") },
                    {"User", userName}, 
                    { "CreationDate", notebook.MetaData.GetValue("CreationDate") },
                    {"SaveDate", DateTime.Now.ToString()},
                    { "NotebookName", notebook.MetaData.GetValue("NotebookName") },
                    { "NotebookContent", ObjectSerializer.ToString(notebook) }
                    };
            return currentNotebook;
        }
    }
}
