using Classroom_Learning_Partner.Model.CLPPageObjects;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{

    public class CLPBlankStampViewModel : CLPStampBaseViewModel
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CLPBlankStampViewModel class.
        /// </summary>
        public CLPBlankStampViewModel(CLPBlankStamp stamp, CLPPageViewModel pageViewModel)
            : base(stamp, pageViewModel)
        {
        }

        #endregion //Constructors
    }
}