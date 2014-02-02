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
    public class CLPFuzzyFactorCardViewModel : CLPArrayViewModel
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
        /// Whether or not the answer is displayed.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsAnswerVisible
        {
            get
            {
                return GetValue<bool>(IsAnswerVisibleProperty);
            }
            set
            {
                SetValue(IsAnswerVisibleProperty, value);
            }
        }

        /// <summary>
        /// True if division labels are on top and answer (if shown) is on bottom.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsArrayDivisionLabelOnTop
        {
            get
            {
                return GetValue<bool>(IsArrayDivisionLabelOnTopProperty);
            }
            set
            {
                SetValue(IsArrayDivisionLabelOnTopProperty, value);
            }
        }

        public static readonly PropertyData IsArrayDivisionLabelOnTopProperty = RegisterProperty("IsArrayDivisionLabelOnTop", typeof(bool));


        /// <summary>
        /// Register the IsAnswerVisible property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsAnswerVisibleProperty = RegisterProperty("IsAnswerVisible", typeof(bool));


        /// <summary>
        /// True if FFC is aligned so that fuzzy edge is on the right
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsHorizontallyAligned
        {
            get
            {
                return GetValue<bool>(IsHorizontallyAlignedProperty);
            }
            set
            {
                SetValue(IsHorizontallyAlignedProperty, value);
            }
        }

        public static readonly PropertyData IsHorizontallyAlignedProperty = RegisterProperty("IsHorizontallyAligned", typeof(bool));


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
