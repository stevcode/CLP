using Catel.MVVM;
using System.Collections.ObjectModel;
using Classroom_Learning_Partner.Model;
using Catel.Data;

namespace Classroom_Learning_Partner.ViewModels.Workspaces
{
    public class NotebookChooserWorkspaceViewModel : ViewModelBase, IWorkspaceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the NotebookChooserWorkspaceViewModel class.
        /// </summary>
        public NotebookChooserWorkspaceViewModel() : base()
        {
            CLPService = new CLPServiceAgent();
            NotebookSelectorViewModels = new ObservableCollection<NotebookSelectorViewModel>();
            CLPService.ChooseNotebook(this);
        }

        private ICLPServiceAgent CLPService { get; set; }


        //Steve - No need for NotebookSelecterViewModel, convert to something cleaner
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<NotebookSelectorViewModel> NotebookSelectorViewModels
        {
            get { return GetValue<ObservableCollection<NotebookSelectorViewModel>>(NotebookSelectorViewModelsProperty); }
            set { SetValue(NotebookSelectorViewModelsProperty, value); }
        }

        /// <summary>
        /// Register the NotebookSelectorViewModels property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NotebookSelectorViewModelsProperty = RegisterProperty("NotebookSelectorViewModels", typeof(ObservableCollection<NotebookSelectorViewModel>));

        public string WorkspaceName
        {
            get { return "NotebookChooserWorkspace"; }
        }
    }
}