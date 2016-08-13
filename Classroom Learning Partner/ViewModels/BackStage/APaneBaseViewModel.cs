using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;

namespace Classroom_Learning_Partner.ViewModels
{
    public abstract class APaneBaseViewModel : ViewModelBase
    {
        protected readonly IDataService _dataService;

        #region Constructor

        protected APaneBaseViewModel()
        {
            _dataService = DependencyResolver.Resolve<IDataService>();
        }

        #endregion //Constructor

        #region Bindings

        /// <summary>Title Text for the Pane.</summary>
        public abstract string PaneTitleText { get; }

        #endregion //Bindings
    }
}