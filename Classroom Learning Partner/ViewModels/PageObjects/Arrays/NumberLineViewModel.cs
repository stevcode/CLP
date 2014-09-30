using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class NumberLineViewModel : APageObjectBaseViewModel
    {
        #region

        public NumberLineViewModel(NumberLine numberLine) { PageObject = numberLine; }

        #endregion //Constructor

        #region Model

        [ViewModelToModel("PageObject")]
        public int NumberLineLength
        {
            get { return GetValue<int>(NumberLineLengthProperty); }
            set { SetValue(NumberLineLengthProperty, value); }
        }

        public static readonly PropertyData NumberLineLengthProperty = RegisterProperty("NumberLineLength", typeof (int));

        #endregion //Model

        #region Commands

        #endregion //Commands
    }
}