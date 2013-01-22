using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Model;
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
            ResizeDataTableCommand = new Command<DragDeltaEventArgs>(OnResizeDataTableCommandExecute);
            ResizeColumnHeightCommand = new Command<DragDeltaEventArgs>(OnResizeColumnHeightCommandExecute);
            ResizeRowWidthCommand = new Command<DragDeltaEventArgs>(OnResizeRowWidthCommandExecute);
            CreateLinkedAggregationDataTableCommand = new Command(OnCreateLinkedAggregationDataTableCommandExecute);
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

        /// <summary>
        /// Resizes DataTable, keeping rows and columns equal length.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeDataTableCommand { get; private set; }

        private void OnResizeDataTableCommandExecute(DragDeltaEventArgs e)
        {
            CLPPage parentPage = PageObject.ParentPage;

            double newHeight = PageObject.Height + e.VerticalChange;
            double newWidth = PageObject.Width + e.HorizontalChange;
            double minHeight = (PageObject as CLPAggregationDataTable).ColumnHeaderHeight + (20 * (PageObject as CLPAggregationDataTable).Columns.Count);
            double minWidth = (PageObject as CLPAggregationDataTable).RowHeaderWidth + (20 * (PageObject as CLPAggregationDataTable).Rows.Count);
            if(newHeight < minHeight)
            {
                newHeight = minHeight;
            }
            if(newWidth < minWidth)
            {
                newWidth = minWidth;
            }
            if(newHeight + PageObject.YPosition > parentPage.PageHeight)
            {
                newHeight = PageObject.Height;
            }
            if(newWidth + PageObject.XPosition > parentPage.PageWidth)
            {
                newWidth = PageObject.Width;
            }

            CLPServiceAgent.Instance.ChangePageObjectDimensions(PageObject, newHeight, newWidth);

            ResizeGridPartsEvenly();
        }

        /// <summary>
        /// Resize the ColumnHeaderHeight.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeColumnHeightCommand { get; private set; }

        private void OnResizeColumnHeightCommandExecute(DragDeltaEventArgs e)
        {
            CLPPage parentPage = PageObject.ParentPage;

            double newHeight = PageObject.Height + e.VerticalChange;
            double newHeaderHeight = (PageObject as CLPAggregationDataTable).ColumnHeaderHeight + e.VerticalChange;
            if(newHeight + PageObject.YPosition < parentPage.PageHeight && newHeaderHeight > 20)
            {
                (PageObject as CLPAggregationDataTable).ColumnHeaderHeight = newHeaderHeight;
                CLPServiceAgent.Instance.ChangePageObjectDimensions(PageObject, newHeight, PageObject.Width);
                ResizeGridPartsEvenly();
            }
        }

        /// <summary>
        /// Resize the RowHeaderWidth.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeRowWidthCommand { get; private set; }

        private void OnResizeRowWidthCommandExecute(DragDeltaEventArgs e)
        {
            CLPPage parentPage = PageObject.ParentPage;

            double newWidth = PageObject.Width + e.HorizontalChange;
            double newHeaderWidth = (PageObject as CLPAggregationDataTable).RowHeaderWidth + e.HorizontalChange;
            if(newWidth + PageObject.XPosition < parentPage.PageWidth && newHeaderWidth > 20)
            {
                (PageObject as CLPAggregationDataTable).RowHeaderWidth = newHeaderWidth;
                CLPServiceAgent.Instance.ChangePageObjectDimensions(PageObject, PageObject.Height, newWidth);
                ResizeGridPartsEvenly();
            }
        }

        /// <summary>
        /// Gets the CreateLinkedAggregationDataTableCommand command.
        /// </summary>
        public Command CreateLinkedAggregationDataTableCommand { get; private set; }

        private void OnCreateLinkedAggregationDataTableCommandExecute()
        {
            //CLPAggregationDataTable linkedTable = (PageObject as CLPAggregationDataTable).CreateAggregatedTable(gridPart);
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

        private void ResizeGridPartsEvenly()
        {
            double newRowHeight = (PageObject.Height - (PageObject as CLPAggregationDataTable).ColumnHeaderHeight) / (PageObject as CLPAggregationDataTable).Rows.Count;
            double yPos = 0;
            foreach(CLPGridPart row in (PageObject as CLPAggregationDataTable).Rows)
            {
                row.Height = newRowHeight;
                row.YPosition = yPos;
                yPos += row.Height;
                row.Width = PageObject.Width;
            }

            double newColWidth = (PageObject.Width - (PageObject as CLPAggregationDataTable).RowHeaderWidth) / (PageObject as CLPAggregationDataTable).Columns.Count;
            double xPos = 0;
            foreach(CLPGridPart col in (PageObject as CLPAggregationDataTable).Columns)
            {
                col.Width = newColWidth;
                col.XPosition = xPos;
                xPos += col.Width;
                col.Height = PageObject.Height;
            }
        }

        #endregion //Methods
    }
}
