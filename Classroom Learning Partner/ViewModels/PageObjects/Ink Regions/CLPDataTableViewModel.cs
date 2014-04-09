using System.Collections.Generic;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPDataTableViewModel : APageObjectBaseViewModel
    {
        ///// <summary>
        ///// Initializes a new instance of the <see cref="CLPHandwritingRegionViewModel"/> class.
        ///// </summary>
        //public CLPDataTableViewModel(CLPDataTable inkRegion)
        //    : base()
        //{
        //    PageObject = inkRegion;
        //    //Console.WriteLine(DataTableCols + " *** " + DataTableRows);
        //}

        ///// <summary>
        ///// Gets the title of the view model.
        ///// </summary>
        ///// <value>The title.</value>
        //public override string Title { get { return "DataTableVM"; } }

        //#region Model

        ///// <summary>
        ///// Stored values in the data table
        ///// </summary>
        //[ViewModelToModel("PageObject")]
        //public List<CLPNamedInkSet> DataValues
        //{
        //    get { return GetValue<List<CLPNamedInkSet>>(DataValuesProperty); }
        //    set { SetValue(DataValuesProperty, value); }
        //}

        ///// <summary>
        ///// Register the DataValues property so it is known in the class.
        ///// </summary>
        //public static readonly PropertyData DataValuesProperty = RegisterProperty("DataValues", typeof(List<CLPNamedInkSet>));

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

        ///// <summary>
        ///// Handwriting analysis type
        ///// </summary>
        //[ViewModelToModel("PageObject")]
        //public CLPHandwritingAnalysisType AnalysisType
        //{
        //    get { return GetValue<CLPHandwritingAnalysisType>(AnalysisTypeProperty); }
        //    set { SetValue(AnalysisTypeProperty, value); }
        //}

        ///// <summary>
        ///// Register the DataTableCols property so it is known in the class.
        ///// </summary>
        //public static readonly PropertyData AnalysisTypeProperty = RegisterProperty("AnalysisType", typeof(CLPHandwritingAnalysisType));

        //#endregion //Model

        //public string GetStringRepresentation()
        //{
        //    string result = AnalysisType.ToString()+"\n";
        //    for (int i = 0; i < Rows * Cols; i++)
        //    {
        //        if (i % Cols == Cols - 1)
        //        {
        //            result += DataValues[i].InkShapeType + ",\n";
        //        }
        //        else
        //        {

        //            result += DataValues[i].InkShapeType + ",\t";
        //        }

        //    }
        //    return result;
        //}
    }
}
