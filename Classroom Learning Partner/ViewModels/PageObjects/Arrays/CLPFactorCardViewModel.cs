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
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{

    public class CLPFactorCardViewModel : CLPArrayViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CLPFactorCardViewModel"/> class.
        /// </summary>
        public CLPFactorCardViewModel(CLPArray factorCard) : base(factorCard)
        {
            RotateArrayCommand = new Command(OnRotateArrayCommandExecute);
        }

        #endregion //Constructor    

        #region Properties

        /// <summary>
        /// Whether the top label is displayed or shown as a "?".
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

        #endregion //Properties

        #region Commands

        /// <summary>
        /// Rotates the array 90 degrees
        /// </summary>
        new public Command RotateArrayCommand
        {
            get;
            private set;
        }

        new protected void OnRotateArrayCommandExecute()
        {
            IsTopLabelVisible = !IsTopLabelVisible; 
            base.OnRotateArrayCommandExecute();
        }

        #endregion

        #region Static Methods

        #endregion //Static Methods
    }
}
