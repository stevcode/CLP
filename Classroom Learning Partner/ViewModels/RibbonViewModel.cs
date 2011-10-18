using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;
using GalaSoft.MvvmLight.Command;
using System.IO;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using System.Windows;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class RibbonViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the RibbonViewModel class.
        /// </summary>
        public RibbonViewModel()
        {
            CLPService = new CLPServiceAgent();
            //CLPService.AddPage(new CLPPage());
        }

        private ICLPServiceAgent CLPService { get; set; }

        #region Commands

        #region Notebook Commands

        private RelayCommand _newNotebookCommand;

        /// <summary>
        /// Gets the NewNotebookCommand.
        /// </summary>
        public RelayCommand NewNotebookCommand
        {
            get
            {
                return _newNotebookCommand
                    ?? (_newNotebookCommand = new RelayCommand(
                                          () =>
                                          {
                                              CLPService.OpenNewNotebook();
                                          }));
            }
        }

        private RelayCommand _openNotebookCommand;

        /// <summary>
        /// Gets the OpenNotebookCommand.
        /// </summary>
        public RelayCommand OpenNotebookCommand
        {
            get
            {
                return _openNotebookCommand
                    ?? (_openNotebookCommand = new RelayCommand(
                                          () =>
                                          {
                                              App.MainWindowViewModel.Workspace = new NotebookChooserWorkspaceViewModel();
                                          }));
            }
        }

        private RelayCommand _saveNotebookCommand;

        /// <summary>
        /// Gets the SaveNotebookCommand.
        /// </summary>
        public RelayCommand SaveNotebookCommand
        {
            get
            {
                return _saveNotebookCommand
                    ?? (_saveNotebookCommand = new RelayCommand(
                                          () =>
                                          {
                                              CLPService.SaveNotebook(App.CurrentNotebookViewModel);
                                          }));
            }
        }

        private RelayCommand _saveAllNotebooksCommand;

        /// <summary>
        /// Gets the SaveAllNotebooksCommand.
        /// </summary>
        public RelayCommand SaveAllNotebooksCommand
        {
            get
            {
                return _saveAllNotebooksCommand
                    ?? (_saveAllNotebooksCommand = new RelayCommand(
                                          () =>
                                          {
                                              foreach (CLPNotebookViewModel notebookVM in App.NotebookViewModels)
                                              {
                                                  CLPService.SaveNotebook(notebookVM);
                                              }
                                          }));
            }
        }

        private RelayCommand _convertToXPSCommand;

        /// <summary>
        /// Gets the ConvertToXPSCommand.
        /// </summary>
        public RelayCommand ConvertToXPSCommand
        {
            get
            {
                return _convertToXPSCommand
                    ?? (_convertToXPSCommand = new RelayCommand(
                                          () =>
                                          {

                                          }));
            }
        }

        #endregion //Notebook Commands

        private RelayCommand _exitCommand;

        /// <summary>
        /// Gets the ExitCommand.
        /// </summary>
        public RelayCommand ExitCommand
        {
            get
            {
                return _exitCommand
                    ?? (_exitCommand = new RelayCommand(
                                          () =>
                                          {
                                              if (MessageBox.Show("Are you sure you want to exit?",
                                "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                              {
                                                  CLPService.Exit();
                                              }
                                          }));
            }
        }

        #endregion //Commands
    }
}