using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPFuzzyFactorCardViewModel : CLPFactorCardViewModel
    {
                
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CLPFuzzyFactorCardViewModel"/> class.
        /// </summary>
        public CLPFuzzyFactorCardViewModel(CLPFuzzyFactorCard factorCard) : base(factorCard)
        {
        }

        #endregion //Constructor    

        #region Properties

        /// <summary>
        /// Value of the Dividend.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int Dividend
        {
            get
            {
                return GetValue<int>(DividendProperty);
            }
            set
            {
                SetValue(DividendProperty, value);
            }
        }

        /// <summary>
        /// Register the Dividend property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof(int));

        #endregion //Properties
    }
}
