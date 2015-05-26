using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ColumnDisplayViewModel : AMultiDisplayViewModelBase
    {
        private readonly INotebookService _notebookService;

        #region Constructor

        /// <summary>Initializes a new instance of the GridDisplayViewModel class.</summary>
        public ColumnDisplayViewModel(ColumnDisplay columnDisplay)
            : base(columnDisplay) { }

        public override string Title
        {
            get { return "GridDisplayVM"; }
        }

        #endregion //Constructor
    }
}