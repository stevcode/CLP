using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System;
using Classroom_Learning_Partner.ViewModels;
using System.Collections.ObjectModel;
using Classroom_Learning_Partner.Model;
using System.IO;

namespace Classroom_Learning_Partner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {

            base.OnStartup(e);
            MainWindow window = new MainWindow();
            _mainWindowViewModel = new MainViewModel();
            window.DataContext = MainWindowViewModel;
            window.Show();

            _notebookDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks";
            AppMessages.SelectNotebookMessage.Register(this, OnSelectNotebookMessage);
            //Workspace = new NotebookChooserWorkspaceViewModel();


            DispatcherHelper.Initialize();
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

        private MainViewModel _mainWindowViewModel;
        public MainViewModel MainWindowViewModel
        {
            get
            {
                return _mainWindowViewModel;
            }
        }

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
    }
}
