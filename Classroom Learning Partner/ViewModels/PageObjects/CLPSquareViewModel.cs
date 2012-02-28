using Classroom_Learning_Partner.Model.CLPPageObjects;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPSquareViewModel : CLPPageObjectBaseViewModel
    {
        public CLPSquareViewModel(CLPSquare square, CLPPageViewModel pageViewModel) : base(pageViewModel)
        {
            PageObject = square;
        }
    }
}
