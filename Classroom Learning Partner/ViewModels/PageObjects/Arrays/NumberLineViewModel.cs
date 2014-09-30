using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities.DomainModel.PageObjects.Arrays;

namespace Classroom_Learning_Partner.ViewModels.PageObjects.Arrays
{
    public class NumberLineViewModel: APageObjectBaseViewModel
    {
        #region

        public NumberLineViewModel(NumberLine numberLine)
        {
            PageObject = numberLine;
            
            //Commands
        }

        #endregion //Constructor

        #region Model

        [ViewModelToModel("Pagebject")]
        public int NumberLength
        {
            get { return GetValue<int>(NumberLengthProperty); }
            set { SetValue(NumberLengthProperty, value);}
        }

        public static readonly PropertyData NumberLengthProperty = RegisterProperty("NumberLength", typeof (int));

        #endregion //Model
    }


}
