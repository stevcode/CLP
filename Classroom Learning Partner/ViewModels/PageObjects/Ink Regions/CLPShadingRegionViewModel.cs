﻿using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPShadingRegionViewModel : APageObjectBaseViewModel
    {
        ///// <summary>
        ///// Initializes a new instance of the <see cref="CLPHandwritingRegionViewModel"/> class.
        ///// </summary>
        //public CLPShadingRegionViewModel(CLPShadingRegion inkRegion)
        //    : base()
        //{
        //    PageObject = inkRegion;
        //    //CLogger.AppendToLog(DataTableCols + " *** " + DataTableRows);
        //}

        ///// <summary>
        ///// Gets the title of the view model.
        ///// </summary>
        ///// <value>The title.</value>
        //public override string Title { get { return "ShadingRegionVM"; } }

        //#region Model

        ///// <summary>
        ///// Percent Shaded
        ///// </summary>
        //[ViewModelToModel("PageObject")]
        //public double PercentFilled
        //{
        //    get { return GetValue<double>(PercentFilledProperty); }
        //    set { SetValue(PercentFilledProperty, value); }
        //}

        ///// <summary>
        ///// Register the PercentFilled property so it is known in the class.
        ///// </summary>
        //public static readonly PropertyData PercentFilledProperty = RegisterProperty("PercentFilled", typeof(double));

        ///// <summary>
        ///// Number of rows
        ///// </summary>
        //[ViewModelToModel("PageObject")]
        //public int Rows
        //{
        //    get { return GetValue<int>(RowsProperty); }
        //    set { SetValue(RowsProperty, value); }
        //}

        ///// <summary>
        ///// Register the DataTableRows property so it is known in the class.
        ///// </summary>
        //public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof(int));

        ///// <summary>
        ///// Number of cols
        ///// </summary>
        //[ViewModelToModel("PageObject")]
        //public int Cols
        //{
        //    get { return GetValue<int>(ColsProperty); }
        //    set { SetValue(ColsProperty, value); }
        //}

        ///// <summary>
        ///// Register the DataTableCols property so it is known in the class.
        ///// </summary>
        //public static readonly PropertyData ColsProperty = RegisterProperty("Cols", typeof(int));

        //#endregion //Model

        //public string GetStringRepresentation()
        //{
        //    return PercentFilled.ToString();
        //}
    }
}
