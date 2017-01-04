using System;
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
using CLP.Entities.Demo;

namespace Classroom_Learning_Partner.ViewModels
{
    public class DivisionTemplateViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="DivisionTemplateViewModel" /> class.</summary>
        public DivisionTemplateViewModel(DivisionTemplate divisionTemplate)
        {
            PageObject = divisionTemplate;

            RemoveDivisionTemplateCommand = new Command(OnRemoveDivisionTemplateCommandExecute);
            ResizeDivisionTemplateCommand = new Command<DragDeltaEventArgs>(OnResizeDivisionTemplateCommandExecute);
            RemoveLastArrayCommand = new Command(OnRemoveLastArrayCommandExecute);
            
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            _contextButtons.Clear();

            _contextButtons.Add(new RibbonButton("Delete", "pack://application:,,,/Resources/Images/Delete.png", RemoveDivisionTemplateCommand, null, true));

            _contextButtons.Add(MajorRibbonViewModel.Separater);

            _contextButtons.Add(new RibbonButton("Remove Last Snapped Array", "pack://application:,,,/Resources/Images/HorizontalLineIcon.png", RemoveLastArrayCommand, null, true));

            if (Dividend > DivisionTemplate.MAX_NUMBER_OF_REMAINDER_TILES)
            {
                return;
            }

            var toggleRemainderTilesButton = new ToggleRibbonButton("Show Remainder Tiles", "Hide RemainderTiles", "pack://application:,,,/Resources/Images/ArrayCard32.png", true)
                                             {
                                                 IsChecked = PageObject is DivisionTemplate && ((DivisionTemplate)PageObject).CanShowRemainderTiles
                                             };
            toggleRemainderTilesButton.Checked += toggleRemainderTilesButton_Checked;
            toggleRemainderTilesButton.Unchecked += toggleRemainderTilesButton_Checked;
           // _contextButtons.Add(toggleRemainderTilesButton);
        }

        private void toggleRemainderTilesButton_Checked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleRibbonButton;
            if (toggleButton == null ||
                toggleButton.IsChecked == null)
            {
                return;
            }

            var divisionTemplate = PageObject as DivisionTemplate;
            if (divisionTemplate == null)
            {
                return;
            }

            var page = divisionTemplate.ParentPage;
            if (page == null)
            {
                return;
            }

            divisionTemplate.IsRemainderTilesVisible = (bool)toggleButton.IsChecked;
            if (divisionTemplate.CanShowRemainderTiles)
            {
                if (divisionTemplate.RemainderTiles == null)
                {
                    divisionTemplate.InitializeRemainderTiles();
                    divisionTemplate.RemainderTiles.CreatorID = divisionTemplate.CreatorID;
                }

                if (!page.PageObjects.Contains(divisionTemplate.RemainderTiles))
                {
                    page.PageObjects.Add(divisionTemplate.RemainderTiles);
                    ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                                                               new RemainderTilesVisibilityToggledHistoryItem(PageObject.ParentPage,
                                                                                                              App.MainWindowViewModel.CurrentUser,
                                                                                                              divisionTemplate.ID,
                                                                                                              true));
                }
            }
            else if (divisionTemplate.RemainderTiles != null &&
                     page.PageObjects.Contains(divisionTemplate.RemainderTiles))
            {
                page.PageObjects.Remove(divisionTemplate.RemainderTiles);
                ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                                                           new RemainderTilesVisibilityToggledHistoryItem(PageObject.ParentPage,
                                                                                                          App.MainWindowViewModel.CurrentUser,
                                                                                                          divisionTemplate.ID,
                                                                                                          false));
            }

            divisionTemplate.UpdateReport();
        }

        #endregion //Constructor  

        #region Model

        /// <summary>Gets or sets the Rows value</summary>
        [ViewModelToModel("PageObject")]
        public int Rows
        {
            get { return GetValue<int>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof (int));

        /// <summary>Gets or sets the Columns value.</summary>
        [ViewModelToModel("PageObject")]
        public int Columns
        {
            get { return GetValue<int>(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly PropertyData ColumnsProperty = RegisterProperty("Columns", typeof (int));

        /// <summary>Gets or sets the property value.</summary>
        [ViewModelToModel("PageObject")]
        public double ArrayHeight
        {
            get { return GetValue<double>(ArrayHeightProperty); }
        }

        public static readonly PropertyData ArrayHeightProperty = RegisterProperty("ArrayHeight", typeof (double));

        /// <summary>Gets or sets the property value.</summary>
        [ViewModelToModel("PageObject")]
        public double ArrayWidth
        {
            get { return GetValue<double>(ArrayWidthProperty); }
        }

        public static readonly PropertyData ArrayWidthProperty = RegisterProperty("ArrayWidth", typeof (double));

        /// <summary>Length of the grid square.</summary>
        [ViewModelToModel("PageObject")]
        public double GridSquareSize
        {
            get { return GetValue<double>(GridSquareSizeProperty); }
        }

        public static readonly PropertyData GridSquareSizeProperty = RegisterProperty("GridSquareSize", typeof (double));

        /// <summary>Gets or sets the HorizontalDivisions value.</summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<CLPArrayDivision> HorizontalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(HorizontalDivisionsProperty); }
            set { SetValue(HorizontalDivisionsProperty, value); }
        }

        public static readonly PropertyData HorizontalDivisionsProperty = RegisterProperty("HorizontalDivisions", typeof (ObservableCollection<CLPArrayDivision>));

        /// <summary>Gets or sets the VerticalDivisions value.</summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<CLPArrayDivision> VerticalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(VerticalDivisionsProperty); }
            set { SetValue(VerticalDivisionsProperty, value); }
        }

        public static readonly PropertyData VerticalDivisionsProperty = RegisterProperty("VerticalDivisions", typeof (ObservableCollection<CLPArrayDivision>));

        /// <summary>Value of the Dividend.</summary>
        [ViewModelToModel("PageObject")]
        public int Dividend
        {
            get { return GetValue<int>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof (int));

        /// <summary>Whether the grid is turned on.</summary>
        [ViewModelToModel("PageObject")]
        public bool IsGridOn
        {
            get { return GetValue<bool>(IsGridOnProperty); }
            set { SetValue(IsGridOnProperty, value); }
        }

        public static readonly PropertyData IsGridOnProperty = RegisterProperty("IsGridOn", typeof (bool));

        #endregion //Model

        #region Bindings

        /// <summary>Color of the border - usually black but flashes red when extra arrays are snapped to it.</summary>
        public string BorderColor
        {
            get { return GetValue<string>(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public static readonly PropertyData BorderColorProperty = RegisterProperty("BorderColor", typeof (string), "Black");

        /// <summary>Color of the fuzzy edge - usually gray but flashes red when extra arrays are snapped to it.</summary>
        public string FuzzyEdgeColor
        {
            get { return GetValue<string>(FuzzyEdgeColorProperty); }
            set { SetValue(FuzzyEdgeColorProperty, value); }
        }

        public static readonly PropertyData FuzzyEdgeColorProperty = RegisterProperty("FuzzyEdgeColor", typeof (string), "DarkGray");

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

        /// <summary>Removes pageObject from page when Delete button is pressed.</summary>
        public Command RemoveDivisionTemplateCommand { get; private set; }

        private void OnRemoveDivisionTemplateCommandExecute()
        {
            var contextRibbon = NotebookWorkspaceViewModel.GetContextRibbon();
            if (contextRibbon != null)
            {
                contextRibbon.Buttons.Clear();
            }

            var divisionTemplate = PageObject as DivisionTemplate;
            if (divisionTemplate == null)
            {
                return;
            }
            if (divisionTemplate.RemainderTiles != null)
            {
                PageObject.ParentPage.PageObjects.Remove(divisionTemplate.RemainderTiles);
            }

            ACLPPageBaseViewModel.RemovePageObjectFromPage(divisionTemplate);
        }

        /// <summary>Gets the ResizePageObjectCommand command.</summary>
        public Command<DragDeltaEventArgs> ResizeDivisionTemplateCommand { get; private set; }

        private void OnResizeDivisionTemplateCommandExecute(DragDeltaEventArgs e)
        {
            var clpArray = PageObject as DivisionTemplate;
            if (clpArray == null)
            {
                return;
            }
            var oldHeight = Height;
            var oldWidth = Width;

            double newArrayHeight;
            var isVerticalChange = e.VerticalChange > e.HorizontalChange;
            if (isVerticalChange)
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
            if (newArrayHeight < MIN_HEIGHT)
            {
                newArrayHeight = MIN_HEIGHT;
            }
            var newSquareSize = newArrayHeight / Rows;
            var newArrayWidth = newSquareSize * Columns;
            if (newArrayWidth < MIN_WIDTH)
            {
                newArrayWidth = MIN_WIDTH;
                newSquareSize = newArrayWidth / Columns;
                newArrayHeight = newSquareSize * Rows;
            }

            //Control Max Dimensions of Array.
            if (newArrayHeight + 2 * clpArray.LabelLength + YPosition > clpArray.ParentPage.Height)
            {
                newArrayHeight = clpArray.ParentPage.Height - YPosition - 2 * clpArray.LabelLength;
                newSquareSize = newArrayHeight / Rows;
                newArrayWidth = newSquareSize * Columns;
            }

            if (newArrayWidth + clpArray.LargeLabelLength + clpArray.LabelLength + XPosition > clpArray.ParentPage.Width)
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

        /// <summary>Gets the RemoveLastArrayCommand command.</summary>
        public Command RemoveLastArrayCommand { get; private set; }

        private void OnRemoveLastArrayCommandExecute()
        {
            if (VerticalDivisions.Count <= 1)
            {
                return;
            }
            var divisionValue = (VerticalDivisions[VerticalDivisions.Count - 2]).Value;
            (PageObject as DivisionTemplate).RemoveLastDivision();

            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                                                       new DivisionTemplateArrayRemovedHistoryItem(PageObject.ParentPage, App.MainWindowViewModel.CurrentUser, PageObject.ID, divisionValue));
        }

        #endregion //Commands

        #region Static Methods

        public static void AddDivisionTemplateToPage(CLPPage page)
        {
            if (page == null)
            {
                return;
            }

            //Initial Values.
            int rows;
            int dividend;
            var isRemainderTilesVisible = false;
            var initialGridSize = ACLPArrayBase.DefaultGridSquareSize;

            //Launch Division Template Creation Window.
            var divisionTemplateCreationView = new DivisionTemplateCreationView
                                     {
                                         Owner = Application.Current.MainWindow
                                     };
            divisionTemplateCreationView.ShowDialog();

            if (divisionTemplateCreationView.DialogResult != true)
            {
                return;
            }

            try
            {
                dividend = Convert.ToInt32(divisionTemplateCreationView.Product.Text);
                rows = Convert.ToInt32(divisionTemplateCreationView.Factor.Text);
                if (divisionTemplateCreationView.TileCheckBox.IsChecked != null)
                {
                    isRemainderTilesVisible = (bool)divisionTemplateCreationView.TileCheckBox.IsChecked && dividend <= DivisionTemplate.MAX_NUMBER_OF_REMAINDER_TILES;
                }
            }
            catch (FormatException)
            {
                return;
            }

            var columns = dividend / rows;

            //Match GridSquareSize if any Division Templates are already on the page.
            //Attempts to match first against a GridSquareSize shared by the most DTs, then by the DT that has been most recently added to the page.
            var divisionTemplatesOnPage = page.PageObjects.OfType<DivisionTemplate>().ToList();
            if (divisionTemplatesOnPage.Any())
            {
                var groupSize = divisionTemplatesOnPage.GroupBy(d => d.GridSquareSize).OrderByDescending(g => g.Count()).First().Count();
                var relevantDivisionTemplateIDs =
                    divisionTemplatesOnPage.GroupBy(d => d.GridSquareSize).Where(g => g.Count() == groupSize).SelectMany(g => g).Select(d => d.ID).ToList();
                initialGridSize = divisionTemplatesOnPage.Last(d => relevantDivisionTemplateIDs.Contains(d.ID)).GridSquareSize;
            }
            else //If no Division Templates or other Arrays are on the page, generate a GridSquareSize that accommodates all the arrays being created.
            {
                initialGridSize = AdjustGridSquareSize(page, rows, columns, initialGridSize);
            }

            //Create Division Template.
            var divisionTemplate = new DivisionTemplate(page, initialGridSize, columns, rows, dividend, isRemainderTilesVisible);

            //Reposition Division Template.
            ACLPArrayBase.ApplyDistinctPosition(divisionTemplate);
            if (isRemainderTilesVisible)
            {
                divisionTemplate.InitializeRemainderTiles();
            }

            //Add Division Template to page.
            ACLPPageBaseViewModel.AddPageObjectToPage(divisionTemplate);

            App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Select;
        }

        public static double AdjustGridSquareSize(CLPPage page, int rows, int columns, double initialGridSquareSize)
        {
            var availablePageHeight = page.Height - ACLPArrayBase.ARRAY_STARING_Y_POSITION;
            var availablePageArea = page.Width * availablePageHeight;

            while (true)
            {
                var arrayWidth = (initialGridSquareSize * columns) + ACLPArrayBase.DT_LABEL_LENGTH + ACLPArrayBase.DT_LARGE_LABEL_LENGTH;
                var arrayHeight = (initialGridSquareSize * rows) + (2 * ACLPArrayBase.DT_LABEL_LENGTH);
                var totalArrayArea = arrayWidth * arrayHeight;

                if (arrayWidth < page.Width &&
                    arrayHeight < availablePageHeight &&
                    totalArrayArea < availablePageArea)
                {
                    return initialGridSquareSize;
                }

                initialGridSquareSize = Math.Abs(initialGridSquareSize - ACLPArrayBase.DefaultGridSquareSize) < .0001 ? 22.5 : initialGridSquareSize * 0.75;
            }
        }

        #endregion //Static Methods
    }
}