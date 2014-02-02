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
        /// Register the IsAnswerVisible property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsAnswerVisibleProperty = RegisterProperty("IsAnswerVisible", typeof(bool));

        /// <summary>
        /// Whether the top label is shown or displayed as a "?".
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsTopLabelVisible
        {
            get
            {
                return GetValue<bool>(IsTopLabelVisibleProperty);
            }
            set
            {
                SetValue(IsTopLabelVisibleProperty, value);
            }
        }

        /// <summary>
        /// Register the IsTopLabelVisible property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsTopLabelVisibleProperty = RegisterProperty("IsTopLabelVisible", typeof(bool));

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

        /// <summary>
        /// The total number of groups (columns or rows) that have been subtracted so far.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int GroupsSubtracted
        {
            get
            {
                return GetValue<int>(GroupsSubtractedProperty);
            }
            set
            {
                SetValue(GroupsSubtractedProperty, value);
            }
        }

        /// <summary>
        /// Register the GroupsSubtracted property so it is known in the class.
        /// </summary>
        public static readonly PropertyData GroupsSubtractedProperty = RegisterProperty("GroupsSubtracted", typeof(int));

        /// <summary>
        /// The area remaining in the array after subtracting the area of the snapped in arrays.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int CurrentRemainder
        {
            get
            {
                return GetValue<int>(CurrentRemainderProperty);
            }
            set
            {
                SetValue(CurrentRemainderProperty, value);
            }
        }

        /// <summary>
        /// Register the CurrentRemainder property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CurrentRemainderProperty = RegisterProperty("CurrentRemainder", typeof(int));

        /// <summary>
        /// Position of the last division in the FFC.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double LastDivisionPosition
        {
            get
            {
                return GetValue<double>(LastDivisionPositionProperty);
            }
            set
            {
                SetValue(LastDivisionPositionProperty, value);
            }
        }

        /// <summary>
        /// Register the LastDivisionPosition property so it is known in the class.
        /// </summary>
        public static readonly PropertyData LastDivisionPositionProperty = RegisterProperty("LastDivisionPosition", typeof(double));

        #endregion //Properties
    }
}
