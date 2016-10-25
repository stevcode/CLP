using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ColumnDisplayViewModel : AMultiDisplayViewModelBase
    {
        #region Constructor

        /// <summary>Initializes a new instance of the ColumnDisplayViewModel class.</summary>
        public ColumnDisplayViewModel(ColumnDisplay columnDisplay, IDataService dataService)
            : base(columnDisplay, dataService) { }

        #endregion //Constructor
    }
}