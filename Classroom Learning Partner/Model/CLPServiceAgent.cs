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
using Classroom_Learning_Partner.Views.Modal_Windows;

namespace Classroom_Learning_Partner.Model
{
    public interface ICLPServiceAgent
    {
        void SetWorkspace();

        void AddPageAt(CLPPage page, int notebookIndex, int submissionIndex);
        void RemovePageAt(int pageIndex);

        void OpenNotebook(string notebookName);
        void OpenNewNotebook();
        void SaveNotebook(CLPNotebookViewModel notebookVM);
        void ChooseNotebook(NotebookChooserWorkspaceViewModel notebookChooserVM);
        void ConvertNotebookToXPS(CLPNotebookViewModel notebookVM);

        void SubmitPage(CLPPageViewModel pageVM);
        void Exit();

        void SendLaserPosition(Point pt);


        void AddPageObjectToPage(CLPPageObjectBase pageObject);
        void RemovePageObjectFromPage(PageObjectContainerViewModel pageObjectContainerViewModel);
        void ChangePageObjectPosition(PageObjectContainerViewModel pageObjectContainerViewModel, Point pt);
        void ChangePageObjectDimensions(PageObjectContainerViewModel pageObjectContainerViewModel, double height, double width);
    }

    public class CLPServiceAgent : ICLPServiceAgent
    {
        public void AddPageAt(CLPPage page, int notebookIndex, int submissionIndex)
        {
            CLPPageViewModel pageViewModel = new CLPPageViewModel(page);
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

        public void OpenNotebook(string notebookName)
        {
            string filePath = App.NotebookDirectory + @"\" + notebookName + @".clp";
            CLPNotebookViewModel newNotebookViewModel;
            if (File.Exists(filePath))
            {
                //alternatively, pull from database and build
                CLPNotebook notebook = CLPNotebook.LoadNotebookFromFile(filePath);
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
            //else doesn't exist, error checking
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

            //save to database?
        }


        public void ChooseNotebook(NotebookChooserWorkspaceViewModel notebookChooserVM)
        {
            if (!Directory.Exists(App.NotebookDirectory))
            {
                Directory.CreateDirectory(App.NotebookDirectory);
            }
            foreach (string fullFile in Directory.GetFiles(App.NotebookDirectory, "*.clp"))
            {
                string notebookName = Path.GetFileNameWithoutExtension(fullFile);
                NotebookSelectorViewModel notebookSelector = new NotebookSelectorViewModel(notebookName);
                notebookChooserVM.NotebookSelectorViewModels.Add(notebookSelector);
            }

            //grab list of available notebooks from database

            //compare?
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
            string s_page = ObjectSerializer.ToString(pageVM.Page);
            App.Peer.Channel.SubmitPage(s_page);
        }


        public void SendLaserPosition(Point pt)
        {
            //call SendLaserPosition for network service agent? which will call updatePoint() in ...CLPPageViewModel.cs?
            //want to wrap this to check if Channel is null, will throw an exception if the "projector" isn't on. 
            App.Peer.Channel.LaserUpdate(pt);

        }

        public void AddPageObjectToPage(CLPPageObjectBase pageObject)
        {
            AppMessages.RequestCurrentDisplayedPage.Send((pageViewModel) =>
            {
                CLPPageObjectBaseViewModel pageObjectViewModel;
                if (pageObject is CLPImage)
                {
                    pageObjectViewModel = new CLPImageViewModel(pageObject as CLPImage);
                }
                else if (pageObject is CLPImageStamp)
                {
                    pageObjectViewModel = new CLPImageStampViewModel(pageObject as CLPImageStamp);
                }
                else if (pageObject is CLPTextBox)
                {
                    pageObjectViewModel = new CLPTextBoxViewModel(pageObject as CLPTextBox);
                }
                else
                {
                    pageObjectViewModel = null;
                }
                
                pageViewModel.PageObjectContainerViewModels.Add(new PageObjectContainerViewModel(pageObjectViewModel));
                pageViewModel.Page.PageObjects.Add(pageObjectViewModel.PageObject);
                //DATABASE add pageobject to current page
            });
            //CLPHistoryItem item = new CLPHistoryItem(pageObject, "ADD");
            //AppMessages.UpdateCLPHistory.Send(item);
        }


        public void RemovePageObjectFromPage(PageObjectContainerViewModel pageObjectContainerViewModel)
        {
            AppMessages.RequestCurrentDisplayedPage.Send((pageViewModel) =>
            {
                pageViewModel.PageObjectContainerViewModels.Remove(pageObjectContainerViewModel);
                pageViewModel.Page.PageObjects.Remove(pageObjectContainerViewModel.PageObjectViewModel.PageObject);
                //DATABASE remove page object from current page
            });
        }


        public void ChangePageObjectPosition(PageObjectContainerViewModel pageObjectContainerViewModel, Point pt)
        {
            pageObjectContainerViewModel.Position = pt;
            pageObjectContainerViewModel.PageObjectViewModel.PageObject.Position = pt;
            //DATABASE change page object's position
        }


        public void ChangePageObjectDimensions(PageObjectContainerViewModel pageObjectContainerViewModel, double height, double width)
        {
            pageObjectContainerViewModel.Height = height;
            pageObjectContainerViewModel.Width = width;
            pageObjectContainerViewModel.PageObjectViewModel.PageObject.Height = height;
            pageObjectContainerViewModel.PageObjectViewModel.PageObject.Width = width;
            //DATABASE change page object's dimensions
        }

        public void SetWorkspace()
        {
            App.MainWindowViewModel.Ribbon.AuthoringTabVisibility = Visibility.Hidden;

            switch (App.CurrentUserMode)
            {
                case App.UserMode.Server:
                    App.MainWindowViewModel.Workspace = new ServerWorkspaceViewModel();
                    break;
                case App.UserMode.Instructor:
                    App.MainWindowViewModel.Workspace = new InstructorWorkspaceViewModel();
                    break;
                case App.UserMode.Projector:
                    App.MainWindowViewModel.Workspace = new ProjectorWorkspaceViewModel();
                    break;
                case App.UserMode.Student:
                    App.MainWindowViewModel.Workspace = new StudentWorkspaceViewModel();
                    break;
            }
        }
    }
}
