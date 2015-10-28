using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;

namespace Classroom_Learning_Partner.ViewModels
{
    public abstract class APaneBaseViewModel : ViewModelBase
    {
        protected readonly INotebookService LoadedNotebookService;
        protected readonly IDataService DataService;

        #region Constructor

        protected APaneBaseViewModel()
        {
            LoadedNotebookService = DependencyResolver.Resolve<INotebookService>();
            DataService = DependencyResolver.Resolve<IDataService>();
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public abstract string PaneTitleText { get; }

        #endregion //Bindings
    }
}