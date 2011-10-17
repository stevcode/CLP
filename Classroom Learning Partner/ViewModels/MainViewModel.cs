using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public const string clpText = "Classroom Learning Partner - ";

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _notebookDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks";
            AppMessages.SelectNotebookMessage.Register(this, OnSelectNotebookMessage);
            Workspace = new NotebookChooserWorkspaceViewModel();
        }

        #region Messages

        private void OnSelectNotebookMessage(string notebookName)
        {
            if (!Directory.Exists(NotebookDirectory))
            {
                Directory.CreateDirectory(NotebookDirectory);
            }
            string filePath = NotebookDirectory + @"\" + notebookName + ".clp2";
            CLPNotebook notebook = CLPNotebook.LoadNotebookFromFile(filePath);
            CLPNotebookViewModel notebookViewModel = new CLPNotebookViewModel(notebook);

            int count = 0;
            foreach (CLPNotebookViewModel notebookVM in NotebookViewModels)
            {
                if (notebookVM.Notebook.UniqueID == notebookViewModel.Notebook.UniqueID)
                {
                    CurrentNotebookViewModel = notebookVM;
                    count++;
                    break;
                }
            }

            if (count == 0)
            {
                NotebookViewModels.Add(notebookViewModel);
                CurrentNotebookViewModel = notebookViewModel;
                
            }

            
        }



        #endregion //Messages

        #region Properties

        private string _notebookDirectory;
        public string NotebookDirectory
        {
            get
            {
                return _notebookDirectory;
            }
        }

        private readonly ObservableCollection<CLPNotebookViewModel> _notebookViewModels = new ObservableCollection<CLPNotebookViewModel>();
        public ObservableCollection<CLPNotebookViewModel> NotebookViewModels
        {
            get
            {
                return _notebookViewModels;
            }
        }

        //make this send message?
        private CLPNotebookViewModel _currentNotebookViewModel;
        public CLPNotebookViewModel CurrentNotebookViewModel
        {
            get
            {
                return _currentNotebookViewModel;
            }
            set
            {
                _currentNotebookViewModel = value;
            }
        }

        #endregion //Properties

        #region Bindings

        public string TitleBarText
        {
            //get { return clpText + UserName + " (" + ConnectionStatus + ")"; }
            get { return clpText; }
        }

        /// <summary>
        /// The <see cref="Workspace" /> property's name.
        /// </summary>
        public const string WorkspacePropertyName = "Workspace";

        private ViewModelBase _workspace = new BlankWorkspaceViewModel();

        /// <summary>
        /// Sets and gets the Workspace property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ViewModelBase Workspace
        {
            get
            {
                return _workspace;
            }

            set
            {
                if (_workspace == value)
                {
                    return;
                }

                _workspace = value;
                RaisePropertyChanged(WorkspacePropertyName);
            }
        }


        private RibbonViewModel _ribbon = new RibbonViewModel();
        public RibbonViewModel Ribbon
        {
            get
            {
                return _ribbon;
            }
        }

        #endregion //Bindings

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}