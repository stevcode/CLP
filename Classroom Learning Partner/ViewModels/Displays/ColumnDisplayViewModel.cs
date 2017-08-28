using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ColumnDisplayViewModel : AMultiDisplayViewModelBase
    {
        #region Constructor

        public ColumnDisplayViewModel(ColumnDisplay columnDisplay, IDataService dataService, IRoleService roleService)
            : base(columnDisplay, dataService, roleService) { }

        #endregion //Constructor
    }
}