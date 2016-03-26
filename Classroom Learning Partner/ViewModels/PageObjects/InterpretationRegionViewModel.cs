using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class InterpretationRegionViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        public InterpretationRegionViewModel(InterpretationRegion interpretationRegion)
        {
            PageObject = interpretationRegion;
        }

        #endregion // Constructor

        #region Static Methods

        public static void AddInterpretationRegionToPage(CLPPage page)
        {
            var interpretationRegion = new InterpretationRegion(page);
            ACLPPageBaseViewModel.AddPageObjectToPage(interpretationRegion);
        }

        #endregion //Static Methods
    }
}