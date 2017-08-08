using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPPageViewModel : ACLPPageBaseViewModel
    {
        #region Constructor

        public CLPPageViewModel(CLPPage page, IDataService dataService, IPageInteractionService pageInteractionService)
            : base(page, dataService, pageInteractionService) { }

        #endregion //Constructor
    }
}