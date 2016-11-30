using CLP.Entities.Ann;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPPageViewModel : ACLPPageBaseViewModel
    {
        #region Constructor

        public CLPPageViewModel(CLPPage page)
            : base(page) { }

        public override string Title
        {
            get { return "PageVM"; }
        }

        #endregion //Constructor
    }
}