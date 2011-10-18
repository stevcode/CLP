using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.ViewModels.Workspaces;

namespace Classroom_Learning_Partner.Model
{
    public interface ICLPServiceAgent
    {
        void AddPage(CLPPage page);

        void OpenNotebook();

        void OpenNewNotebook();

    }

    public class CLPServiceAgent : ICLPServiceAgent
    {
        public void AddPage(CLPPage page)
        {
            throw new NotImplementedException();
        }


        public void OpenNotebook()
        {
            string notebookName = "blah1"; // get this from list of available notebooks, query database and/or local
            string filePath = App.NotebookDirectory + @"\" + notebookName + @".clp2";
            CLPNotebookViewModel newNotebookViewModel;
            if (File.Exists(filePath))
            {
                CLPNotebook notebook = CLPNotebook.LoadNotebookFromFile(filePath);
                newNotebookViewModel = new CLPNotebookViewModel(notebook);
                App.NotebookViewModels.Add(newNotebookViewModel);
                App.CurrentNotebookViewModel = newNotebookViewModel;

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
            //error checking, file already exists, try different name
            
        }
    }
}
