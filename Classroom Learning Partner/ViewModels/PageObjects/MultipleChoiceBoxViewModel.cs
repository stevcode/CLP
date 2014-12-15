using System.Collections.Generic;
using System.Windows.Documents;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class MultipleChoiceBoxViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        public MultipleChoiceBoxViewModel(MultipleChoiceBox multipleChoiceBox)
        {
            PageObject = multipleChoiceBox;
        } 

        #endregion //Constructor

        #region Bindings

        public List<string> ChoiceLabels
        {
            get
            {
                return new List<string>();
            }
        }

        #endregion //Bindings
    }
}