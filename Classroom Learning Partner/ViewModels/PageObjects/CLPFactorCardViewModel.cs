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

    public class CLPFactorCardViewModel : CLPArrayViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CLPFactorCardViewModel"/> class.
        /// </summary>
        public CLPFactorCardViewModel(CLPFactorCard factorCard) : base(factorCard)
        {
        }

        #endregion //Constructor    

    }
}
