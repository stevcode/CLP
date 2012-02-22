using Classroom_Learning_Partner.Model;
using Catel.MVVM;
using Catel.Data;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NotebookSelectorViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the NotebookSelectorViewModel class.
        /// </summary>
        public NotebookSelectorViewModel(string notebookName) : base()
        {
            NotebookName = notebookName;
            SelectNotebookCommand = new Command(OnSelectNotebookCommandExecute);
        }

        public override string Title { get { return "NotebookSelectorVM"; } }

        #region Bindings

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string NotebookName
        {
            get { return GetValue<string>(NotebookNameProperty); }
            set { SetValue(NotebookNameProperty, value); }
        }

        /// <summary>
        /// Register the NotebookName property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NotebookNameProperty = RegisterProperty("NotebookName", typeof(string));

        #endregion //Bindings

        /// <summary>
        /// Gets the SelectNotebookCommand command.
        /// </summary>
        public Command SelectNotebookCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the SelectNotebookCommand command is executed.
        /// </summary>
        private void OnSelectNotebookCommandExecute()
        {
            CLPServiceAgent.Instance.OpenNotebook(NotebookName);
        }
    }
}