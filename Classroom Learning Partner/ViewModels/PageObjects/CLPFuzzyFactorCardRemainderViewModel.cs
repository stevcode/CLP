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
    class CLPFuzzyFactorCardRemainderViewModel : ACLPPageObjectBaseViewModel
    {

        //#region Constructor

        ///// <summary>
        ///// Initializes a new instance of the <see cref="CLPFuzzyFactorCardRemainderViewModel"/> class.
        ///// </summary>
        //public CLPFuzzyFactorCardRemainderViewModel(CLPFuzzyFactorCardRemainder ffcRemainder)
        //{
        //    PageObject = ffcRemainder;
        //    hoverTimer.Interval = 2300;
        //    CloseAdornerTimeOut = 0.15;
        //}

        //#endregion //Constructor

        //#region Model

        ///// <summary>
        ///// Gets or sets the property value.
        ///// </summary>
        //[ViewModelToModel("PageObject")]
        //public ObservableCollection<string> TileOffsets
        //{
        //    get
        //    {
        //        return GetValue<ObservableCollection<string>>(TileOffsetsProperty);
        //    }
        //    set
        //    {
        //        SetValue(TileOffsetsProperty, value);
        //    }
        //}

        ///// <summary>
        ///// Register the TileOffsets property so it is known in the class.
        ///// </summary>
        //public static readonly PropertyData TileOffsetsProperty = RegisterProperty("TileOffsets", typeof(ObservableCollection<string>));

        //#endregion //Model

        //#region Methods

        //public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        //{
        //    return false;
        //}

        //#endregion //Methods
    }
}
