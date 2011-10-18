using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Classroom_Learning_Partner.Model;

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
    public class NotebookSelectorViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the NotebookSelectorViewModel class.
        /// </summary>
        public NotebookSelectorViewModel(string notebookName)
        {
            CLPService = new CLPServiceAgent();
            _notebookName = notebookName;
        }

        private ICLPServiceAgent CLPService { get; set; }

        #region Bindings

        private string _notebookName = "";
        public string NotebookName
        {
            get
            {
                return _notebookName;
            }
        }

        #endregion //Bindings

        private RelayCommand _selectNotebook;

        /// <summary>
        /// Gets the SelectNotebookCommand.
        /// </summary>
        public RelayCommand SelectNotebookCommand
        {
            get
            {
                return _selectNotebook
                    ?? (_selectNotebook = new RelayCommand(
                                          () =>
                                          {
                                              CLPService.OpenNotebook(NotebookName);
                                          }));
            }
        }

    }
}