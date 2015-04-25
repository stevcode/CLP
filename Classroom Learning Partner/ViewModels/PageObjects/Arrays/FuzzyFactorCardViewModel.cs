using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using CLP.CustomControls;
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
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            _contextButtons.Clear();

            _contextButtons.Add(new RibbonButton("Delete", "pack://application:,,,/Images/Delete.png", RemoveFuzzyFactorCardCommand, null, true));

            _contextButtons.Add(MajorRibbonViewModel.Separater);

            _contextButtons.Add(new RibbonButton("Remove Last Snapped Array", "pack://application:,,,/Images/HorizontalLineIcon.png", RemoveLastArrayCommand, null, true));
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
        /// Value of the Dividend.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int Dividend
        {
            get { return GetValue<int>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof(int));

        /// <summary>
        /// Whether the grid is turned on.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsGridOn
        {
            get { return GetValue<bool>(IsGridOnProperty); }
            set { SetValue(IsGridOnProperty, value); }
        }

        public static readonly PropertyData IsGridOnProperty = RegisterProperty("IsGridOn", typeof(bool));

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

            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new FFCArrayRemovedHistoryItem(PageObject.ParentPage, App.MainWindowViewModel.CurrentUser, PageObject.ID, divisionValue));
        }

        #endregion //Commands

        #region Static Methods

        public static void AddDivisionTemplateToPage(CLPPage page)
        {
            if (page == null)
            {
                return;
            }


            int rows, dividend = 1, numberOfArrays = 1;
            var isShowingTiles = false;
            var factorCreationView = new FuzzyFactorCardCreationView { Owner = Application.Current.MainWindow };
            factorCreationView.ShowDialog();

            if (factorCreationView.DialogResult != true)
            {
                return;
            }

            try
            {
                dividend = Convert.ToInt32(factorCreationView.Product.Text);
                rows = Convert.ToInt32(factorCreationView.Factor.Text);
                if (factorCreationView.TileCheckBox.IsChecked != null)
                {
                    isShowingTiles = (bool)factorCreationView.TileCheckBox.IsChecked && dividend < 51;
                }
            }
            catch (FormatException)
            {
                return;
            }
            
            var columns = dividend / rows;
            var arrayType = isShowingTiles ? "FFCREMAINDER" : "FUZZYFACTORCARD";
            
            var arraysToAdd = new List<ACLPArrayBase>();
            foreach (var index in Enumerable.Range(1, numberOfArrays))
            {
                ACLPArrayBase array;
                switch (arrayType)
                {
                    case "FUZZYFACTORCARD":
                        array = new FuzzyFactorCard(page, columns, rows, dividend);
                        // HACK: Find better way to set this
                        array.CreatorID = App.MainWindowViewModel.CurrentUser.ID;
                        array.OwnerID = App.MainWindowViewModel.CurrentUser.ID;
                        break;
                    case "FFCREMAINDER":
                        bool isRemainderRegionDisplayed = (dividend <= 50);
                        array = new FuzzyFactorCard(page, columns, rows, dividend, isRemainderRegionDisplayed);
                        // HACK: Find better way to set this
                        array.CreatorID = App.MainWindowViewModel.CurrentUser.ID;
                        if (isRemainderRegionDisplayed)
                        {
                            (array as FuzzyFactorCard).RemainderTiles.CreatorID = array.CreatorID;
                            (array as FuzzyFactorCard).RemainderTiles.OwnerID = array.OwnerID;
                        }
                        break;
                    default:
                        return;
                }

                arraysToAdd.Add(array);
            }

            int arrayStacks = CLPArrayViewModel.MatchArrayGridSize(arraysToAdd, page);

            var isHorizontallyAligned = page.Width / columns > page.Height / 4 * 3 / rows;
            var firstArray = arraysToAdd.First();
            double initializedSquareSize = firstArray.ArrayHeight / firstArray.Rows;

            firstArray.XPosition = 0.0;
            if (295.0 + firstArray.Height < page.Height)
            {
                firstArray.YPosition = 295.0;
            }
            else
            {
                firstArray.YPosition = page.Height - firstArray.Height;
            }
            ACLPArrayBase.ApplyDistinctPosition(firstArray);

            CLPArrayViewModel.PlaceArrayNextToExistingArray(arraysToAdd, page);
            double xPosition = firstArray.XPosition;
            double yPosition = firstArray.YPosition;

            //Place arrays on the page
            if (arraysToAdd.Count == 1)
            {
                firstArray.SizeArrayToGridLevel(initializedSquareSize);

                if (firstArray.XPosition + firstArray.Width >= firstArray.ParentPage.Width)
                {
                    firstArray.XPosition = firstArray.ParentPage.Width - firstArray.Width;
                }
                if (firstArray.YPosition + firstArray.Height >= firstArray.ParentPage.Height)
                {
                    firstArray.YPosition = firstArray.ParentPage.Height - firstArray.Height;
                }

                ACLPPageBaseViewModel.AddPageObjectToPage(firstArray);

                if (arrayType == "FFCREMAINDER" && dividend <= 50)
                {
                    if (xPosition + firstArray.Width + 20.0 + (firstArray as FuzzyFactorCard).RemainderTiles.Width <= page.Width)
                    {
                        (firstArray as FuzzyFactorCard).RemainderTiles.XPosition = xPosition + firstArray.Width + 20.0;
                    }
                    else
                    {
                        (firstArray as FuzzyFactorCard).RemainderTiles.XPosition = page.Width - (firstArray as FuzzyFactorCard).RemainderTiles.Width;
                    }
                    if (yPosition + (firstArray as FuzzyFactorCard).LabelLength + (firstArray as FuzzyFactorCard).RemainderTiles.Height <= page.Height)
                    {
                        (firstArray as FuzzyFactorCard).RemainderTiles.YPosition = yPosition + (firstArray as FuzzyFactorCard).LabelLength;
                    }
                    else
                    {
                        (firstArray as FuzzyFactorCard).RemainderTiles.YPosition = page.Height - (firstArray as FuzzyFactorCard).RemainderTiles.Height;
                    }
                }
            }
            else
            {
                double initialGridsquareSize = initializedSquareSize;
                if (isHorizontallyAligned)
                {
                    while (xPosition + (firstArray.LabelLength + columns * initialGridsquareSize) * numberOfArrays + firstArray.LabelLength >= page.Width)
                    {
                        initialGridsquareSize = Math.Abs(initialGridsquareSize - 45.0) < .0001 ? 22.5 : initialGridsquareSize / 4 * 3;

                        if (numberOfArrays < 5 ||
                            xPosition + (firstArray.LabelLength + columns * initialGridsquareSize) * numberOfArrays + firstArray.LabelLength < page.Width)
                        {
                            continue;
                        }

                        if (xPosition + (firstArray.LabelLength + columns * initialGridsquareSize) * Math.Ceiling((double)numberOfArrays / 2) + firstArray.LabelLength < page.Width &&
                            yPosition + (firstArray.LabelLength + rows * initialGridsquareSize) * 2 + firstArray.LabelLength < page.Height)
                        {
                            arrayStacks = 2;
                            break;
                        }

                        if (xPosition + (firstArray.LabelLength + columns * initialGridsquareSize) * Math.Ceiling((double)numberOfArrays / 3) + firstArray.LabelLength < page.Width &&
                            yPosition + (firstArray.LabelLength + rows * initialGridsquareSize) * 3 + firstArray.LabelLength < page.Height)
                        {
                            arrayStacks = 3;
                            break;
                        }
                    }
                }
                else
                {
                    yPosition = 100;
                    while (yPosition + (firstArray.LabelLength + rows * initialGridsquareSize) * numberOfArrays + firstArray.LabelLength >= page.Height)
                    {
                        initialGridsquareSize = Math.Abs(initialGridsquareSize - 45.0) < .0001 ? 22.5 : initialGridsquareSize / 4 * 3;

                        if (numberOfArrays < 5 ||
                            yPosition + (firstArray.LabelLength + rows * initialGridsquareSize) * numberOfArrays + firstArray.LabelLength < page.Height)
                        {
                            continue;
                        }

                        if (yPosition + (firstArray.LabelLength + rows * initialGridsquareSize) * Math.Ceiling((double)numberOfArrays / 2) + firstArray.LabelLength < page.Height &&
                            xPosition + (firstArray.LabelLength + columns * initialGridsquareSize) * 2 + firstArray.LabelLength < page.Width)
                        {
                            arrayStacks = 2;
                            break;
                        }

                        if (yPosition + (firstArray.LabelLength + rows * initialGridsquareSize) * Math.Ceiling((double)numberOfArrays / 3) + firstArray.LabelLength < page.Height &&
                            xPosition + (firstArray.LabelLength + columns * initialGridsquareSize) * 3 + firstArray.LabelLength < page.Width)
                        {
                            arrayStacks = 3;
                            break;
                        }
                    }
                }

                foreach (var array in arraysToAdd)
                {
                    var index = arraysToAdd.IndexOf(array) + 1;
                    if (isHorizontallyAligned)
                    {
                        if (arrayStacks == 2 &&
                            index == (int)Math.Ceiling((double)numberOfArrays / 2) + 1)
                        {
                            xPosition = firstArray.XPosition;
                            yPosition += firstArray.LabelLength + rows * initialGridsquareSize;
                        }
                        if (arrayStacks == 3 &&
                            (index == (int)Math.Ceiling((double)numberOfArrays / 3) + 1 || index == (int)Math.Ceiling((double)numberOfArrays / 3) * 2 + 1))
                        {
                            xPosition = firstArray.XPosition;
                            yPosition += firstArray.LabelLength + rows * initialGridsquareSize;
                        }
                        array.XPosition = xPosition;
                        array.YPosition = yPosition;
                        xPosition += firstArray.LabelLength + columns * initialGridsquareSize;
                        array.SizeArrayToGridLevel(initialGridsquareSize);
                    }
                    else
                    {
                        if (arrayStacks == 2 &&
                            index == (int)Math.Ceiling((double)numberOfArrays / 2) + 1)
                        {
                            xPosition += firstArray.LabelLength + columns * initialGridsquareSize;
                            yPosition = firstArray.YPosition;
                        }
                        if (arrayStacks == 3 &&
                            (index == (int)Math.Ceiling((double)numberOfArrays / 3) + 1 || index == (int)Math.Ceiling((double)numberOfArrays / 3) * 2 + 1))
                        {
                            xPosition += firstArray.LabelLength + columns * initialGridsquareSize;
                            yPosition = firstArray.YPosition;
                        }
                        array.XPosition = xPosition;
                        array.YPosition = yPosition;
                        yPosition += firstArray.LabelLength + rows * initialGridsquareSize;
                        array.SizeArrayToGridLevel(initialGridsquareSize);
                    }
                }

                ACLPPageBaseViewModel.AddPageObjectsToPage(page, arraysToAdd);
            }

            App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Select;
        }

        #endregion //Static Methods
    }
}