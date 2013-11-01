using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPRegionViewModel : ACLPPageObjectBaseViewModel
    {
        public CLPRegionViewModel(CLPRegion region)
        {
            PageObject = region;

            //Commands
            RemoveCommand = new Command<DragDeltaEventArgs>(OnResizeArrayCommandExecute);
        }

        #region Commands

        #endregion

    }
}