using Classroom_Learning_Partner.Model.CLPPageObjects;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPCircleViewModel : CLPPageObjectBaseViewModel
    {
        public CLPCircleViewModel(CLPCircle circle, CLPPageViewModel pageViewModel)
            : base(pageViewModel)
        {
            PageObject = circle;
        }
    }
}
