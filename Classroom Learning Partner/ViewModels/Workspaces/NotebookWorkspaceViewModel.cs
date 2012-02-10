namespace Classroom_Learning_Partner.ViewModels.Workspaces
{
    using Catel.MVVM;

    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class NotebookWorkspaceViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookWorkspaceViewModel"/> class.
        /// </summary>
        public NotebookWorkspaceViewModel()
        {
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "View model title"; } }

        // TODO: Register models with the vmpropmodel codesnippet
        // TODO: Register view model properties with the vmprop or vmpropviewmodeltomodel codesnippets
        // TODO: Register commands with the vmcommand or vmcommandwithcanexecute codesnippets
    }
}
