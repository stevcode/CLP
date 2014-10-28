using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;

namespace Classroom_Learning_Partner.ViewModels
{
    public abstract class APaneBaseViewModel : ViewModelBase
    {
        protected readonly INotebookService LoadedNotebookService;

        #region Constructor

        protected APaneBaseViewModel() { LoadedNotebookService = DependencyResolver.Resolve<INotebookService>(); }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public abstract string PaneTitleText { get; }

        #endregion //Bindings
    }
}