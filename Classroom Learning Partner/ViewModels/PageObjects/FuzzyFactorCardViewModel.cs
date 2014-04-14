using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class FuzzyFactorCardViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FuzzyFactorCardViewModel" /> class.
        /// </summary>
        public FuzzyFactorCardViewModel(FuzzyFactorCard fuzzyFactorCard)
        {
            PageObject = fuzzyFactorCard;
            
            RemoveFuzzyFactorCardCommand = new Command(OnRemoveFuzzyFactorCardCommandExecute);
            ResizeFuzzyFactorCardCommand = new Command<DragDeltaEventArgs>(OnResizeFuzzyFactorCardCommandExecute);
            RemoveLastArrayCommand = new Command(OnRemoveLastArrayCommandExecute);
            ToggleMainArrayAdornersCommand = new Command<MouseButtonEventArgs>(OnToggleMainArrayAdornersCommandExecute);
        }

        #endregion //Constructor    

        #region Model

        /// <summary>
        /// Gets or sets the Rows value
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int Rows
        {
            get { return GetValue<int>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof(int));

        /// <summary>
        /// Gets or sets the Columns value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int Columns
        {
            get { return GetValue<int>(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly PropertyData ColumnsProperty = RegisterProperty("Columns", typeof(int));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double ArrayHeight
        {
            get { return GetValue<double>(ArrayHeightProperty); }
        }

        public static readonly PropertyData ArrayHeightProperty = RegisterProperty("ArrayHeight", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double ArrayWidth
        {
            get { return GetValue<double>(ArrayWidthProperty); }
        }

        public static readonly PropertyData ArrayWidthProperty = RegisterProperty("ArrayWidth", typeof(double));

        /// <summary>
        /// Length of the grid square.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double GridSquareSize
        {
            get { return GetValue<double>(GridSquareSizeProperty); }
        }

        public static readonly PropertyData GridSquareSizeProperty = RegisterProperty("GridSquareSize", typeof(double));

        /// <summary>
        /// Gets or sets the HorizontalDivisions value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<CLPArrayDivision> HorizontalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(HorizontalDivisionsProperty); }
            set { SetValue(HorizontalDivisionsProperty, value); }
        }

        public static readonly PropertyData HorizontalDivisionsProperty = RegisterProperty("HorizontalDivisions", typeof(ObservableCollection<CLPArrayDivision>));

        /// <summary>
        /// Gets or sets the VerticalDivisions value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<CLPArrayDivision> VerticalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(VerticalDivisionsProperty); }
            set { SetValue(VerticalDivisionsProperty, value); }
        }

        public static readonly PropertyData VerticalDivisionsProperty = RegisterProperty("VerticalDivisions", typeof(ObservableCollection<CLPArrayDivision>));

        /// <summary>
        /// True if FFC is aligned so that fuzzy edge is on the right
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsHorizontallyAligned
        {
            get { return GetValue<bool>(IsHorizontallyAlignedProperty); }
            set { SetValue(IsHorizontallyAlignedProperty, value); }
        }

        public static readonly PropertyData IsHorizontallyAlignedProperty = RegisterProperty("IsHorizontallyAligned", typeof(bool));

        /// <summary>
        /// Value of the Dividend.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int Dividend
        {
            get { return GetValue<int>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof(int));

        #endregion //Model

        #region Bindings

        /// <summary>
        /// Color of the border - usually black but flashes red when extra arrays are snapped to it.
        /// </summary>
        public string BorderColor
        {
            get { return GetValue<string>(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public static readonly PropertyData BorderColorProperty = RegisterProperty("BorderColor", typeof(string), "Black");

        /// <summary>
        /// Color of the fuzzy edge - usually gray but flashes red when extra arrays are snapped to it.
        /// </summary>
        public string FuzzyEdgeColor
        {
            get { return GetValue<string>(FuzzyEdgeColorProperty); }
            set { SetValue(FuzzyEdgeColorProperty, value); }
        }

        public static readonly PropertyData FuzzyEdgeColorProperty = RegisterProperty("FuzzyEdgeColor", typeof(string), "DarkGray");

        #endregion //Bindings

        #region Methods

        public void RejectSnappedArray()
        {
            BorderColor = "Red";
            FuzzyEdgeColor = "Red";
            Task.Run(async delegate
                           {
                               await Task.Delay(400);
                               BorderColor = "Black";
                               FuzzyEdgeColor = "Gray";
                           });
        }

        #endregion //Methods

        #region Commands

        /// <summary>
        /// Removes pageObject from page when Delete button is pressed.
        /// </summary>
        public Command RemoveFuzzyFactorCardCommand { get; private set; }

        private void OnRemoveFuzzyFactorCardCommandExecute()
        {
            if((PageObject as FuzzyFactorCard).RemainderTiles != null)
            {
                //var currentIndex = PageObject.ParentPage.PageObjects.IndexOf(remainderRegion);
                //ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryPageObjectRemove(PageObject.ParentPage, remainderRegion, currentIndex));
                PageObject.ParentPage.PageObjects.Remove((PageObject as FuzzyFactorCard).RemainderTiles);
            }
            ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject);
        }

        /// <summary>
        /// Gets the ResizePageObjectCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeFuzzyFactorCardCommand { get; private set; }

        private void OnResizeFuzzyFactorCardCommandExecute(DragDeltaEventArgs e)
        {
            var clpArray = PageObject as FuzzyFactorCard;
            if(clpArray == null)
            {
                return;
            }
            var oldHeight = Height;
            var oldWidth = Width;

            double newArrayHeight;
            var isVerticalChange = e.VerticalChange > e.HorizontalChange;
            if(isVerticalChange)
            {
                newArrayHeight = ArrayHeight + e.VerticalChange;
            }
            else
            {
                newArrayHeight = (ArrayWidth + e.HorizontalChange) / Columns * Rows;
            }

            //TODO Liz - make min dimension depend on horizontal vs vertical alignment
            const double MIN_HEIGHT = 150.0; 
            const double MIN_WIDTH = 50.0;

            //Control Min Dimensions of Array.
            if(newArrayHeight < MIN_HEIGHT)
            {
                newArrayHeight = MIN_HEIGHT;
            }
            var newSquareSize = newArrayHeight / Rows;
            var newArrayWidth = newSquareSize * Columns;
            if(newArrayWidth < MIN_WIDTH)
            {
                newArrayWidth = MIN_WIDTH;
                newSquareSize = newArrayWidth / Columns;
                newArrayHeight = newSquareSize * Rows;
            }

            //Control Max Dimensions of Array.
            if(newArrayHeight + 2 * clpArray.LabelLength + YPosition > clpArray.ParentPage.Height)
            {
                newArrayHeight = clpArray.ParentPage.Height - YPosition - 2 * clpArray.LabelLength;
                newSquareSize = newArrayHeight / Rows;
                newArrayWidth = newSquareSize * Columns;
            }
            //TODO Liz - update this when rotating is enabled
            if(newArrayWidth + clpArray.LargeLabelLength + clpArray.LabelLength + XPosition > clpArray.ParentPage.Width)
            {
                newArrayWidth = clpArray.ParentPage.Width - XPosition - clpArray.LargeLabelLength - clpArray.LabelLength;
                newSquareSize = newArrayWidth / Columns;
            }

            clpArray.SizeArrayToGridLevel(newSquareSize);

            //Resize History
            //var heightDiff = Math.Abs(oldHeight - Height);
            //var widthDiff = Math.Abs(oldWidth - Width);
            //var diff = heightDiff + widthDiff;
            //if(!(diff > CLPHistory.SAMPLE_RATE))
            //{
            //    return;
            //}

            //var batch = PageObject.ParentPage.PageHistory.CurrentHistoryBatch;
            //if(batch is CLPHistoryPageObjectResizeBatch)
            //{
            //    (batch as CLPHistoryPageObjectResizeBatch).AddResizePointToBatch(PageObject.UniqueID,
            //                                                                     new Point(Width, Height));
            //}
            //else
            //{
            //    var batchHistoryItem = PageObject.ParentPage.PageHistory.EndBatch();
            //    ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            //    //TODO: log this error
            //}
        }

        /// <summary>
        /// Gets the RemoveLastArrayCommand command.
        /// </summary>
        public Command RemoveLastArrayCommand { get; private set; }

        private void OnRemoveLastArrayCommandExecute()
        {
            if(VerticalDivisions.Count <= 1)
            {
                return;
            }
            var divisionValue = (VerticalDivisions[VerticalDivisions.Count - 2]).Value;
            (PageObject as FuzzyFactorCard).RemoveLastDivision();

          //  ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryFFCDivisionRemoved(PageObject.ParentPage, PageObject.UniqueID, divisionValue));
        }

        /// <summary>
        /// Toggles the main adorners for the array.
        /// </summary>
        public Command<MouseButtonEventArgs> ToggleMainArrayAdornersCommand { get; private set; }

        private void OnToggleMainArrayAdornersCommandExecute(MouseButtonEventArgs e)
        {
            if(!App.MainWindowViewModel.IsAuthoring && IsBackground)
            {
                return;
            }

            if(e.ChangedButton != MouseButton.Left ||
               e.StylusDevice != null && e.StylusDevice.Inverted)
            {
                return;
            }

            ACLPPageBaseViewModel.ClearAdorners(PageObject.ParentPage);
            IsAdornerVisible = !IsAdornerVisible;
        }

        #endregion //Commands
    }
}