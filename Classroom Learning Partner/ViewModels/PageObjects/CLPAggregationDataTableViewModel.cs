using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPAggregationDataTableViewModel : ACLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPImageViewModel class.
        /// </summary>
        public CLPAggregationDataTableViewModel(CLPAggregationDataTable dataTable)
            : base()
        {
            PageObject = dataTable;
            IsMouseOverShowEnabled = true;


            AddRowCommand = new Command(OnAddRowCommandExecute);
            AddColumnCommand = new Command(OnAddColumnCommandExecute);
        }

        public override string Title { get { return "AggregationDataTableVM"; } }

        #region Bindings

        /// <summary>
        /// Height of the Header Section of each Column.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double ColumnHeaderHeight
        {
            get { return GetValue<double>(ColumnHeaderHeightProperty); }
            set { SetValue(ColumnHeaderHeightProperty, value); }
        }

        public static readonly PropertyData ColumnHeaderHeightProperty = RegisterProperty("ColumnHeaderHeight", typeof(double));

        /// <summary>
        /// All the Columns of the DataTable.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<CLPGridPart> Columns
        {
            get { return GetValue<ObservableCollection<CLPGridPart>>(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly PropertyData ColumnsProperty = RegisterProperty("Columns", typeof(ObservableCollection<CLPGridPart>));

        /// <summary>
        /// Width of Header section of each Row.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double RowHeaderWidth
        {
            get { return GetValue<double>(RowHeaderWidthProperty); }
            set { SetValue(RowHeaderWidthProperty, value); }
        }

        public static readonly PropertyData RowHeaderWidthProperty = RegisterProperty("RowHeaderWidth", typeof(double));

        /// <summary>
        /// All the Rows of the DataTable.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<CLPGridPart> Rows
        {
            get { return GetValue<ObservableCollection<CLPGridPart>>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof(ObservableCollection<CLPGridPart>));

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Add a row to the DataTable.
        /// </summary>
        public Command AddRowCommand { get; private set; }

        private void OnAddRowCommandExecute()
        {
            CLPGridPart newRow = new CLPGridPart(GridPartOrientation.Row, 75, Width);
            (PageObject as CLPAggregationDataTable).AddGridPart(newRow);
        }

        /// <summary>
        /// Add a column to the DataTable.
        /// </summary>
        public Command AddColumnCommand { get; private set; }

        private void OnAddColumnCommandExecute()
        {
            CLPGridPart newRow = new CLPGridPart(GridPartOrientation.Column, Height, 150);
            (PageObject as CLPAggregationDataTable).AddGridPart(newRow);
        }

        #endregion //Commands

        #region Methods

        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            if(App.MainWindowViewModel.IsAuthoring)
            {
                return false;
            }

            return true;
        }

        public override void EraserHitTest(string hitBoxName)
        {
            if(App.MainWindowViewModel.IsAuthoring && hitBoxName == "TopLeftHitBox")
            {
                //TODO: Steve - remove pageObject
            }
        }

        #endregion //Methods
    }
}
