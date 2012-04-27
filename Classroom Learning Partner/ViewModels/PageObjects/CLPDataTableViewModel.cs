namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    using Catel.MVVM;
    using Classroom_Learning_Partner.Model.CLPPageObjects;
    using System.Windows.Ink;
    using Catel.Data;
    using Classroom_Learning_Partner.Resources;
    using System;
    using System.Collections.ObjectModel;
    using Classroom_Learning_Partner.Model;

    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPDataTableViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPHandwritingRegionViewModel"/> class.
        /// </summary>
        public CLPDataTableViewModel(CLPDataTable inkRegion)
            : base()
        {
            PageObject = inkRegion;
            //Console.WriteLine(DataTableCols + " *** " + DataTableRows);
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "DataTableVM"; } }

        #region Model

        /// <summary>
        /// Stored values in the data table
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<CLPNamedInkSet> DataValues
        {
            get { return GetValue<ObservableCollection<CLPNamedInkSet>>(DataValuesProperty); }
            set { SetValue(DataValuesProperty, value); }
        }

        /// <summary>
        /// Register the DataValues property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DataValuesProperty = RegisterProperty("DataValues", typeof(ObservableCollection<CLPNamedInkSet>));

        /// <summary>
        /// Number of rows
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int Rows
        {
            get { return GetValue<int>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        /// <summary>
        /// Register the DataTableRows property so it is known in the class.
        /// </summary>
        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof(int));

        /// <summary>
        /// Number of cols
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int Cols
        {
            get { return GetValue<int>(ColsProperty); }
            set { SetValue(ColsProperty, value); }
        }

        /// <summary>
        /// Register the DataTableCols property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ColsProperty = RegisterProperty("Cols", typeof(int));

        #endregion //Model

    }
}
