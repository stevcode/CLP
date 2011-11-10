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



        void AddPageObjectToPage(CLPPageObjectBase pageObject);
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



                

                //change this to open Instructor/Student/Projector Workspace
                App.MainWindowViewModel.Workspace = new AuthoringWorkspaceViewModel();
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
                        newNotebookViewModel.Notebook.Name = notebookName;
                        App.NotebookViewModels.Add(newNotebookViewModel);
                        App.CurrentNotebookViewModel = newNotebookViewModel;
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
            string filePath = App.NotebookDirectory + @"\" + notebookVM.Notebook.Name + @".clp";
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
            throw new NotImplementedException();
        }

        public void AddPageObjectToPage(CLPPageObjectBase pageObject)
        {
            AppMessages.RequestCurrentDisplayedPage.Send((callbackMessage) =>
            {
                callbackMessage.Page.PageObjects.Add(pageObject);
                CLPPageObjectBaseViewModel pageObjectViewModel;
                if (pageObject is CLPImage)
                {
                    pageObjectViewModel = new CLPImageViewModel(pageObject as CLPImage);
                }
                else if (pageObject is CLPImageStamp)
                {
                    pageObjectViewModel = new CLPImageStampViewModel(pageObject as CLPImageStamp);
                }
                else
                {
                    pageObjectViewModel = null;
                }
                callbackMessage.PageObjectContainerViewModels.Add(new PageObjectContainerViewModel(pageObjectViewModel));
            });
            CLPHistoryItem item = new CLPHistoryItem(pageObject, "ADD");
            AppMessages.UpdateCLPHistory.Send(item);
            Console.WriteLine("Add Object send to History.");
        }
    }
}
