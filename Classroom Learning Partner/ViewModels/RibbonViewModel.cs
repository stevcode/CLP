using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;
using GalaSoft.MvvmLight.Command;
using System.IO;
using Classroom_Learning_Partner.ViewModels.Workspaces;

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

        #endregion //Notebook Commands

        #endregion //Commands
    }
}