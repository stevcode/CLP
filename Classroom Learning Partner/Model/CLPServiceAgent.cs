using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using System.Windows;

namespace Classroom_Learning_Partner.Model
{
    public interface ICLPServiceAgent
    {
        void AddPage(CLPPage page);
        void RemovePage(string UniqueID);

        void OpenNotebook(string notebookName);
        void OpenNewNotebook();
        void SaveNotebook(CLPNotebookViewModel notebookVM);
        void ChooseNotebook(NotebookChooserWorkspaceViewModel notebookChooserVM);
        void ConvertNotebookToXPS(CLPNotebookViewModel notebookVM);

        void SubmitPage(CLPPageViewModel pageVM);
        void Exit();

        void SendLaserPosition(Point pt);


    }

    public class CLPServiceAgent : ICLPServiceAgent
    {
        public void AddPage(CLPPage page)
        {
            //re-write constructor to take in location for abstraction
            int currentPageIndex = -1;
            AppMessages.RequestCurrentDisplayedPage.Send((callbackMessage) =>
            {
                currentPageIndex = App.CurrentNotebookViewModel.PageViewModels.IndexOf(callbackMessage);
            });

            CLPPageViewModel viewModel = new CLPPageViewModel(page);
            App.CurrentNotebookViewModel.InsertPage(currentPageIndex, viewModel);
            App.CurrentNotebookViewModel.Notebook.Pages.Insert(currentPageIndex, page);
        }

        public void RemovePage(string UniqueID)
        {
            //re-write and overload to accept UniqueID or location: RemovePageAt(loc)
            int currentPageIndex = -1;
            AppMessages.RequestCurrentDisplayedPage.Send((callbackMessage) =>
            {
                currentPageIndex = App.CurrentNotebookViewModel.PageViewModels.IndexOf(callbackMessage);
            });

            App.CurrentNotebookViewModel.RemovePageAt(currentPageIndex);
            App.CurrentNotebookViewModel.Notebook.Pages.RemoveAt(currentPageIndex);
        }

        public void OpenNotebook(string notebookName)
        {
            string filePath = App.NotebookDirectory + @"\" + notebookName + @".clp2";
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



                

                //change this to open Instructor/Student/Projector Workspace
                App.MainWindowViewModel.Workspace = new AuthoringWorkspaceViewModel();
            }
            //else doesn't exist, error checking
        }


        public void OpenNewNotebook()
        {
            string notebookName = "blah1"; // get this from prompt askign new name of notebook
            string filePath = App.NotebookDirectory + @"\" + notebookName + @".clp2";
            CLPNotebookViewModel newNotebookViewModel;
            if (!File.Exists(filePath))
            {
                newNotebookViewModel = new CLPNotebookViewModel();
                newNotebookViewModel.Notebook.Name = notebookName;
                App.NotebookViewModels.Add(newNotebookViewModel);
                App.CurrentNotebookViewModel = newNotebookViewModel;
                App.MainWindowViewModel.Workspace = new AuthoringWorkspaceViewModel();
            }
            //else error checking, file already exists, try different name
      


        }

        public void SaveNotebook(CLPNotebookViewModel notebookVM)
        {
            //make async?
            //compare VM with model?
            //compare model w/ database
            string filePath = App.NotebookDirectory + @"\" + notebookVM.Notebook.Name + @".clp2";
            CLPNotebook.SaveNotebookToFile(filePath, notebookVM.Notebook);

            //save to database?
        }


        public void ChooseNotebook(NotebookChooserWorkspaceViewModel notebookChooserVM)
        {
            if (!Directory.Exists(App.NotebookDirectory))
            {
                Directory.CreateDirectory(App.NotebookDirectory);
            }
            foreach (string fullFile in Directory.GetFiles(App.NotebookDirectory, "*.clp2"))
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
            throw new NotImplementedException();
        }





        public void SendLaserPosition(Point pt)
        {
            throw new NotImplementedException();
        }
    }
}
