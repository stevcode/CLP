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
                                              string filePath = App.NotebookDirectory + @"\" + "blah1" + @".clp2";
                                              if (!File.Exists(filePath))
                                              {
                                                  CLPNotebookViewModel newNotebookViewModel = new CLPNotebookViewModel();
                                                  newNotebookViewModel.Notebook.Name = "blah1";
                                                  App.NotebookViewModels.Add(newNotebookViewModel);
                                                  App.CurrentNotebookViewModel = newNotebookViewModel;
                                                  App.MainWindowViewModel.Workspace = new AuthoringWorkspaceViewModel();
                                              }
                                          }));
            }
        }
    }
}