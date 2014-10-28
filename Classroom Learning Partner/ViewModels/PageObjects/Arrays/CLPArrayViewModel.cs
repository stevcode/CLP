using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Shapes;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.CustomControls;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>UserControl view model.</summary>
    public class CLPArrayViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="CLPArrayViewModel" /> class.</summary>
        public CLPArrayViewModel(CLPArray array)
        {
            PageObject = array;

            //Commands
            ResizeArrayCommand = new Command<DragDeltaEventArgs>(OnResizeArrayCommandExecute);
            DragStopAndSnapCommand = new Command<DragCompletedEventArgs>(OnDragStopAndSnapCommandExecute);
            ToggleGridCommand = new Command(OnToggleGridCommandExecute);
            ToggleDivisionAdornersCommand = new Command(OnToggleDivisionAdornersCommandExecute);
            RotateArrayCommand = new Command(OnRotateArrayCommandExecute);
            CreateVerticalDivisionCommand = new Command(OnCreateVerticalDivisionCommandExecute);
            CreateHorizontalDivisionCommand = new Command(OnCreateHorizontalDivisionCommandExecute);
            EditLabelCommand = new Command<CLPArrayDivision>(OnEditLabelCommandExecute);
            EraseDivisionCommand = new Command<MouseEventArgs>(OnEraseDivisionCommandExecute);
            ToggleMainArrayAdornersCommand = new Command<MouseButtonEventArgs>(OnToggleMainArrayAdornersCommandExecute);
            DuplicateArrayCommand = new Command(OnDuplicateArrayCommandExecute);
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

        /// <summary>Turns the grid on or off.</summary>
        [ViewModelToModel("PageObject")]
        public bool IsGridOn
        {
            get { return GetValue<bool>(IsGridOnProperty); }
            set { SetValue(IsGridOnProperty, value); }
        }

        public static readonly PropertyData IsGridOnProperty = RegisterProperty("IsGridOn", typeof (bool));

        /// <summary>Turns division behavior on or off.</summary>
        [ViewModelToModel("PageObject")]
        public bool IsDivisionBehaviorOn
        {
            get { return GetValue<bool>(IsDivisionBehaviorOnProperty); }
            set { SetValue(IsDivisionBehaviorOnProperty, value); }
        }

        public static readonly PropertyData IsDivisionBehaviorOnProperty = RegisterProperty("IsDivisionBehaviorOn", typeof (bool));

        /// <summary>Whether or not the array can snap. This property is automatically mapped to the corresponding property in PageObject.</summary>
        [ViewModelToModel("PageObject")]
        public bool IsSnappable
        {
            get { return GetValue<bool>(IsSnappableProperty); }
            set { SetValue(IsSnappableProperty, value); }
        }

        public static readonly PropertyData IsSnappableProperty = RegisterProperty("IsSnappable", typeof (bool));

        /// <summary>Visibility of the Top Labels This property is automatically mapped to the corresponding property in PageObject.</summary>
        [ViewModelToModel("PageObject")]
        public bool IsTopLabelVisible
        {
            get { return GetValue<bool>(IsTopLabelVisibleProperty); }
            set { SetValue(IsTopLabelVisibleProperty, value); }
        }

        public static readonly PropertyData IsTopLabelVisibleProperty = RegisterProperty("IsTopLabelVisible", typeof (bool));

        /// <summary>Visibility of the Side Labels This property is automatically mapped to the corresponding property in PageObject.</summary>
        [ViewModelToModel("PageObject")]
        public bool IsSideLabelVisible
        {
            get { return GetValue<bool>(IsSideLabelVisibleProperty); }
            set { SetValue(IsSideLabelVisibleProperty, value); }
        }

        public static readonly PropertyData IsSideLabelVisibleProperty = RegisterProperty("IsSideLabelVisible", typeof (bool));

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

        public static readonly PropertyData HorizontalDivisionsProperty = RegisterProperty("HorizontalDivisions",
                                                                                           typeof (ObservableCollection<CLPArrayDivision>),
                                                                                           null,
                                                                                           Divisions_Changed);

        private static void Divisions_Changed(object sender, AdvancedPropertyChangedEventArgs advancedPropertyChangedEventArgs)
        {
            //throw new NotImplementedException();
        }

        /// <summary>Gets or sets the VerticalDivisions value.</summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<CLPArrayDivision> VerticalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(VerticalDivisionsProperty); }
            set { SetValue(VerticalDivisionsProperty, value); }
        }

        public static readonly PropertyData VerticalDivisionsProperty = RegisterProperty("VerticalDivisions",
                                                                                         typeof (ObservableCollection<CLPArrayDivision>));

        #endregion //Model

        #region Bindings

        /// <summary>Gets or sets the BottomArrowPosition value.</summary>
        public double TopArrowPosition
        {
            get { return GetValue<double>(TopArrowPositionProperty); }
            set { SetValue(TopArrowPositionProperty, value); }
        }

        public static readonly PropertyData TopArrowPositionProperty = RegisterProperty("TopArrowPosition", typeof (double), 0.0);

        /// <summary>Gets or sets the RightArrowPosition value.</summary>
        public double LeftArrowPosition
        {
            get { return GetValue<double>(LeftArrowPositionProperty); }
            set { SetValue(LeftArrowPositionProperty, value); }
        }

        public static readonly PropertyData LeftArrowPositionProperty = RegisterProperty("LeftArrowPosition", typeof (double), 0.0);

        public string Transparency
        {
            get
            {
                if ((PageObject as CLPArray).ArrayType == ArrayTypes.TenByTen)
                {
                    return "White";
                }
                else
                {
                    return "Transparent";
                }
            }
        }

        /// <summary>Whether or not default adorners are on.</summary>
        public bool IsDefaultAdornerVisible
        {
            get { return GetValue<bool>(IsDefaultAdornerVisibleProperty); }
            set
            {
                SetValue(IsDefaultAdornerVisibleProperty, value);
                RaisePropertyChanged("IsToggleGridAdornerVisible");
                RaisePropertyChanged("IsToggleDivisionAdornerVisible");
                RaisePropertyChanged("IsRotateAdornerVisible");
                RaisePropertyChanged("IsDuplicateAdornerVisible");
            }
        }

        public static readonly PropertyData IsDefaultAdornerVisibleProperty = RegisterProperty("IsDefaultAdornerVisible", typeof (bool), false);

        /// <summary>Whether or not adorner to create a division on right side of array is on.</summary>
        public bool IsLeftAdornerVisible
        {
            get { return GetValue<bool>(IsLeftAdornerVisibleProperty); }
            set { SetValue(IsLeftAdornerVisibleProperty, value); }
        }

        public static readonly PropertyData IsLeftAdornerVisibleProperty = RegisterProperty("IsLeftAdornerVisible", typeof (bool), false);

        /// <summary>Whether or not adorner to create a division on bottom side of array is on.</summary>
        public bool IsTopAdornerVisible
        {
            get { return GetValue<bool>(IsTopAdornerVisibleProperty); }
            set { SetValue(IsTopAdornerVisibleProperty, value); }
        }

        public static readonly PropertyData IsTopAdornerVisibleProperty = RegisterProperty("IsTopAdornerVisible", typeof (bool), false);

        /// <summary>Whether or not to show the adorner that allows students to toggle the grid lines on and off.</summary>
        public bool IsToggleGridAdornerVisible
        {
            get { return IsDefaultAdornerVisible && Rows < 71 && Columns < 71 && (PageObject as CLPArray).ArrayType != ArrayTypes.TenByTen; }
        }

        public bool IsToggleDivisionAdornerVisible
        {
            get { return IsDefaultAdornerVisible && (PageObject as CLPArray).ArrayType == ArrayTypes.Array; }
        }

        public bool IsRotateAdornerVisible
        {
            get { return IsDefaultAdornerVisible && (PageObject as CLPArray).ArrayType != ArrayTypes.TenByTen; }
        }

        public bool IsDuplicateAdornerVisible
        {
            get { return IsDefaultAdornerVisible; } // && (PageObject as CLPArray).ArrayType == ArrayTypes.TenByTen; }
        }

        #endregion //Bindings

        #region Commands

        /// <summary>Gets the ResizePageObjectCommand command.</summary>
        public Command<DragDeltaEventArgs> ResizeArrayCommand { get; set; }

        private void OnResizeArrayCommandExecute(DragDeltaEventArgs e)
        {
            var clpArray = PageObject as CLPArray;
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

            const double MIN_ARRAY_SIZE = 16.875; //11.25;

            //Control Min Dimensions of Array.
            if (newArrayHeight < MIN_ARRAY_SIZE)
            {
                newArrayHeight = MIN_ARRAY_SIZE;
            }
            var newSquareSize = newArrayHeight / Rows;
            var newArrayWidth = newSquareSize * Columns;
            if (newArrayWidth < MIN_ARRAY_SIZE)
            {
                newArrayWidth = MIN_ARRAY_SIZE;
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
            if (newArrayWidth + 2 * clpArray.LabelLength + XPosition > clpArray.ParentPage.Width)
            {
                newArrayWidth = clpArray.ParentPage.Width - XPosition - 2 * clpArray.LabelLength;
                newSquareSize = newArrayWidth / Columns;
                //newArrayHeight = newSquareSize * Rows;
            }

            clpArray.SizeArrayToGridLevel(newSquareSize);

            ////Resize History
            var heightDiff = Math.Abs(oldHeight - Height);
            var widthDiff = Math.Abs(oldWidth - Width);
            var diff = heightDiff + widthDiff;
            if (!(diff > PageHistory.SAMPLE_RATE))
            {
                return;
            }

            var batch = PageObject.ParentPage.History.CurrentHistoryBatch;
            if (batch is PageObjectResizeBatchHistoryItem)
            {
                (batch as PageObjectResizeBatchHistoryItem).AddResizePointToBatch(PageObject.ID, new Point(Width, Height));
            }
            else
            {
                var batchHistoryItem = PageObject.ParentPage.History.EndBatch();
                ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
                //TODO: log this error
            }
        }

        public Command<DragCompletedEventArgs> DragStopAndSnapCommand { get; private set; }

        private enum SnapType
        {
            Top,
            Bottom,
            Left,
            Right
        }

        private void OnDragStopAndSnapCommandExecute(DragCompletedEventArgs e)
        {
            var initialX = XPosition;
            var initialY = YPosition;

            var movementBatch = PageObject.ParentPage.History.CurrentHistoryBatch as PageObjectMoveBatchHistoryItem;
            if (movementBatch != null)
            {
                movementBatch.AddPositionPointToBatch(PageObject.ID, new Point(PageObject.XPosition, PageObject.YPosition));
            }
            var batchHistoryItem = PageObject.ParentPage.History.EndBatch();
            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            PageObject.OnMoved(initialX, initialY);

            var snappingArray = PageObject as CLPArray;
            if (snappingArray == null)
            {
                return;
            }

            ACLPArrayBase closestPersistingArray = null;
            var closestSnappingDistance = Double.MaxValue;
            var snapType = SnapType.Top;
            foreach (var pageObject in PageObject.ParentPage.PageObjects)
            {
                var persistingArray = pageObject as ACLPArrayBase;
                if (persistingArray == null ||
                    persistingArray.ID == snappingArray.ID ||
                    !persistingArray.IsSnappable)
                {
                    continue;
                }

                var top = Math.Max(snappingArray.YPosition + snappingArray.LabelLength, persistingArray.YPosition + persistingArray.LabelLength);
                var bottom = Math.Min(snappingArray.YPosition + snappingArray.LabelLength + snappingArray.ArrayHeight,
                                      persistingArray.YPosition + persistingArray.LabelLength + persistingArray.ArrayHeight);
                var verticalIntersectionLength = bottom - top;
                var isVerticalIntersection = verticalIntersectionLength > persistingArray.ArrayHeight / 2 ||
                                             verticalIntersectionLength > snappingArray.ArrayHeight / 2;

                var left = Math.Max(snappingArray.XPosition + snappingArray.LabelLength, persistingArray.XPosition + persistingArray.LabelLength);
                var right = Math.Min(snappingArray.XPosition + snappingArray.LabelLength + snappingArray.ArrayWidth,
                                     persistingArray.XPosition + persistingArray.LabelLength + persistingArray.ArrayWidth);
                var horizontalIntersectionLength = right - left;
                var isHorizontalIntersection = horizontalIntersectionLength > persistingArray.ArrayWidth / 2 ||
                                               horizontalIntersectionLength > snappingArray.ArrayWidth / 2;

                //   Update Remainder Region
                //if(factorCard.IsRemainderRegionDisplayed && factorCard.CurrentRemainder == snappingArray.Columns * snappingArray.Rows)
                //{
                //    CLPFuzzyFactorCardRemainder remainderRegion = PageObject.ParentPage.GetPageObjectByUniqueID(factorCard.RemainderRegionUniqueID) as CLPFuzzyFactorCardRemainder;
                //    var currentIndex = PageObject.ParentPage.PageObjects.IndexOf(remainderRegion);
                //    ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryPageObjectRemove(PageObject.ParentPage, remainderRegion, currentIndex));
                //}

                //Fuzzy Factor Card array snapping in - HACK: for now this will override array snapping even if an array might be closer

                #region Snap to FFC

                if (pageObject is FuzzyFactorCard)
                {
                    var divisionTemplate = pageObject as FuzzyFactorCard;
                    var divisionTemplateIDsInHistory = DivisionTemplateAnalysis.GetListOfDivisionTemplateIDsInHistory(PageObject.ParentPage);
                    if (isVerticalIntersection)
                    {
                        var diff =
                            Math.Abs(snappingArray.XPosition + snappingArray.LabelLength -
                                     (persistingArray.XPosition + persistingArray.LabelLength + divisionTemplate.LastDivisionPosition));
                        if (diff < 50)
                        {
                            var snapFailed = false;

                            if (divisionTemplate.CurrentRemainder != divisionTemplate.Dividend % divisionTemplate.Rows)
                            {
                                var existingFactorPairErrorsTag = divisionTemplate.ParentPage.Tags.OfType<DivisionTemplateFactorPairErrorsTag>().FirstOrDefault(x => x.DivisionTemplateID == divisionTemplate.ID);
                                var isArrayDimensionErrorsTagOnPage = true;

                                if (existingFactorPairErrorsTag == null)
                                {
                                    existingFactorPairErrorsTag = new DivisionTemplateFactorPairErrorsTag(divisionTemplate.ParentPage,
                                                                                                        Origin.StudentPageGenerated,
                                                                                                        divisionTemplate.ID,
                                                                                                        divisionTemplate.Dividend,
                                                                                                        divisionTemplate.Rows,
                                                              divisionTemplateIDsInHistory.IndexOf(divisionTemplate.ID));
                                    isArrayDimensionErrorsTagOnPage = false;
                                }

                                if (snappingArray.Rows != divisionTemplate.Rows)
                                {
                                    if (snappingArray.Columns == divisionTemplate.Rows)
                                    {
                                        existingFactorPairErrorsTag.SnapWrongOrientationDimensions.Add(string.Format("{0}x{1}", snappingArray.Rows, snappingArray.Columns));
                                    }
                                    else
                                    {
                                        existingFactorPairErrorsTag.SnapIncorrectDimensionDimensions.Add(string.Format("{0}x{1}", snappingArray.Rows, snappingArray.Columns));
                                    }

                                    snapFailed = true;
                                }
                                else if (divisionTemplate.CurrentRemainder < snappingArray.Rows * snappingArray.Columns)
                                {
                                    existingFactorPairErrorsTag.SnapArrayTooLargeDimensions.Add(string.Format("{0}x{1}", snappingArray.Rows, snappingArray.Columns));

                                    snapFailed = true;
                                }

                                if (!isArrayDimensionErrorsTagOnPage &&
                                    existingFactorPairErrorsTag.ErrorAtemptsSum > 0)
                                {
                                    divisionTemplate.ParentPage.AddTag(existingFactorPairErrorsTag);
                                }
                            }
                            else
                            {
                                var existingRemainderErrorsTag = divisionTemplate.ParentPage.Tags.OfType<DivisionTemplateRemainderErrorsTag>().FirstOrDefault(x => x.DivisionTemplateID == divisionTemplate.ID);
                                var isRemainderErrorsTagOnPage = true;

                                if (existingRemainderErrorsTag == null)
                                {
                                    existingRemainderErrorsTag = new DivisionTemplateRemainderErrorsTag(divisionTemplate.ParentPage,
                                                                                                        Origin.StudentPageGenerated,
                                                                                                        divisionTemplate.ID,
                                                                                                        divisionTemplate.Dividend,
                                                                                                        divisionTemplate.Rows,
                                                              divisionTemplateIDsInHistory.IndexOf(divisionTemplate.ID));
                                    isRemainderErrorsTagOnPage = false;
                                }

                                if (snappingArray.Rows != divisionTemplate.Rows)
                                {
                                    if (snappingArray.Columns == divisionTemplate.Rows)
                                    {
                                        existingRemainderErrorsTag.SnapWrongOrientationDimensions.Add(string.Format("{0}x{1}", snappingArray.Rows, snappingArray.Columns));
                                    }
                                    else
                                    {
                                        existingRemainderErrorsTag.SnapIncorrectDimensionDimensions.Add(string.Format("{0}x{1}", snappingArray.Rows, snappingArray.Columns));
                                    }

                                    snapFailed = true;
                                }
                                else if (divisionTemplate.CurrentRemainder < snappingArray.Rows * snappingArray.Columns)
                                {
                                    existingRemainderErrorsTag.SnapArrayTooLargeDimensions.Add(string.Format("{0}x{1}", snappingArray.Rows, snappingArray.Columns));

                                    snapFailed = true;
                                }

                                if (!isRemainderErrorsTagOnPage &&
                                    existingRemainderErrorsTag.ErrorAtemptsSum > 0)
                                {
                                    divisionTemplate.ParentPage.AddTag(existingRemainderErrorsTag);
                                }
                            }

                            if (snapFailed)
                            {
                                var factorCardViewModels = CLPServiceAgent.Instance.GetViewModelsFromModel(divisionTemplate);
                                foreach (var viewModel in factorCardViewModels)
                                {
                                    (viewModel as FuzzyFactorCardViewModel).RejectSnappedArray();
                                }
                                continue;
                            }

                            //If first division - update IsGridOn to match new array
                            if (divisionTemplate.LastDivisionPosition == 0)
                            {
                                divisionTemplate.IsGridOn = snappingArray.IsGridOn;
                            }

                            //Add a new division and remove snapping array
                            PageObject.ParentPage.PageObjects.Remove(PageObject);
                            divisionTemplate.SnapInArray(snappingArray.Columns);

                            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                                                                       new FFCArraySnappedInHistoryItem(PageObject.ParentPage,
                                                                                                        App.MainWindowViewModel.CurrentUser,
                                                                                                        pageObject.ID,
                                                                                                        snappingArray));
                            return;
                        }
                    }
                    continue;
                }

                #endregion //Snap to FFC

                if (isVerticalIntersection && snappingArray.Rows == persistingArray.Rows)
                {
                    var rightDiff =
                        Math.Abs(snappingArray.XPosition + snappingArray.LabelLength -
                                 (persistingArray.XPosition + persistingArray.LabelLength + persistingArray.ArrayWidth));
                    if (rightDiff < 50)
                    {
                        if (closestPersistingArray == null ||
                            rightDiff < closestSnappingDistance)
                        {
                            closestPersistingArray = persistingArray;
                            closestSnappingDistance = rightDiff;
                            snapType = SnapType.Right;
                        }
                    }

                    var leftDiff =
                        Math.Abs(snappingArray.XPosition + snappingArray.LabelLength + snappingArray.ArrayWidth -
                                 (persistingArray.XPosition + persistingArray.LabelLength));
                    if (leftDiff < 50)
                    {
                        if (closestPersistingArray == null ||
                            leftDiff < closestSnappingDistance)
                        {
                            closestPersistingArray = persistingArray;
                            closestSnappingDistance = leftDiff;
                            snapType = SnapType.Left;
                        }
                    }
                }

                if (isHorizontalIntersection && snappingArray.Columns == persistingArray.Columns)
                {
                    var bottomDiff =
                        Math.Abs(snappingArray.YPosition + snappingArray.LabelLength -
                                 (persistingArray.YPosition + persistingArray.LabelLength + persistingArray.ArrayHeight));
                    if (bottomDiff < 50)
                    {
                        if (closestPersistingArray == null ||
                            bottomDiff < closestSnappingDistance)
                        {
                            closestPersistingArray = persistingArray;
                            closestSnappingDistance = bottomDiff;
                            snapType = SnapType.Bottom;
                        }
                    }

                    var topDiff =
                        Math.Abs(snappingArray.YPosition + snappingArray.LabelLength + snappingArray.ArrayHeight -
                                 (persistingArray.YPosition + persistingArray.LabelLength));
                    if (topDiff < 50)
                    {
                        if (closestPersistingArray == null ||
                            topDiff < closestSnappingDistance)
                        {
                            closestPersistingArray = persistingArray;
                            closestSnappingDistance = topDiff;
                            snapType = SnapType.Top;
                        }
                    }
                }
            }

            if (closestPersistingArray == null)
            {
                return;
            }

            var squareSize = closestPersistingArray.GridSquareSize;
            ObservableCollection<CLPArrayDivision> tempDivisions;

            CLPArraySnapHistoryItem arraySnapHistoryItem;
            switch (snapType)
            {
                case SnapType.Top:
                    arraySnapHistoryItem = new CLPArraySnapHistoryItem(PageObject.ParentPage,
                                                                       App.MainWindowViewModel.CurrentUser,
                                                                       closestPersistingArray,
                                                                       snappingArray,
                                                                       true);

                    closestPersistingArray.VerticalDivisions.Clear();

                    snappingArray.SizeArrayToGridLevel(squareSize);

                    if (closestPersistingArray.HorizontalDivisions.Any())
                    {
                        tempDivisions = new ObservableCollection<CLPArrayDivision>(closestPersistingArray.HorizontalDivisions);
                        closestPersistingArray.HorizontalDivisions.Clear();
                    }
                    else
                    {
                        tempDivisions = new ObservableCollection<CLPArrayDivision>
                                        {
                                            new CLPArrayDivision(ArrayDivisionOrientation.Horizontal,
                                                                 0,
                                                                 closestPersistingArray.ArrayHeight,
                                                                 closestPersistingArray.Rows)
                                        };
                    }

                    if (!snappingArray.HorizontalDivisions.Any())
                    {
                        closestPersistingArray.HorizontalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Horizontal,
                                                                                            0,
                                                                                            snappingArray.ArrayHeight,
                                                                                            snappingArray.Rows));
                    }

                    foreach (var horizontalDivision in snappingArray.HorizontalDivisions)
                    {
                        closestPersistingArray.HorizontalDivisions.Add(horizontalDivision);
                    }
                    foreach (var horizontalDivision in tempDivisions)
                    {
                        closestPersistingArray.HorizontalDivisions.Add(new CLPArrayDivision(horizontalDivision.Orientation,
                                                                                            horizontalDivision.Position + snappingArray.ArrayHeight,
                                                                                            horizontalDivision.Length,
                                                                                            horizontalDivision.Value));
                    }

                    closestPersistingArray.Rows += snappingArray.Rows;
                    closestPersistingArray.YPosition -= snappingArray.ArrayHeight;
                    break;
                case SnapType.Bottom:
                    arraySnapHistoryItem = new CLPArraySnapHistoryItem(PageObject.ParentPage,
                                                                       App.MainWindowViewModel.CurrentUser,
                                                                       closestPersistingArray,
                                                                       snappingArray,
                                                                       true);

                    closestPersistingArray.VerticalDivisions.Clear();

                    snappingArray.SizeArrayToGridLevel(squareSize);

                    if (!closestPersistingArray.HorizontalDivisions.Any())
                    {
                        closestPersistingArray.HorizontalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Horizontal,
                                                                                            0,
                                                                                            closestPersistingArray.ArrayHeight,
                                                                                            closestPersistingArray.Rows));
                    }

                    if (!snappingArray.HorizontalDivisions.Any())
                    {
                        closestPersistingArray.HorizontalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Horizontal,
                                                                                            closestPersistingArray.ArrayHeight,
                                                                                            snappingArray.ArrayHeight,
                                                                                            snappingArray.Rows));
                    }
                    else
                    {
                        foreach (var horizontalDivision in snappingArray.HorizontalDivisions)
                        {
                            closestPersistingArray.HorizontalDivisions.Add(new CLPArrayDivision(horizontalDivision.Orientation,
                                                                                                horizontalDivision.Position +
                                                                                                closestPersistingArray.ArrayHeight,
                                                                                                horizontalDivision.Length,
                                                                                                horizontalDivision.Value));
                        }
                    }

                    closestPersistingArray.Rows += snappingArray.Rows;
                    break;
                case SnapType.Left:
                    arraySnapHistoryItem = new CLPArraySnapHistoryItem(PageObject.ParentPage,
                                                                       App.MainWindowViewModel.CurrentUser,
                                                                       closestPersistingArray,
                                                                       snappingArray,
                                                                       false);

                    closestPersistingArray.HorizontalDivisions.Clear();

                    snappingArray.SizeArrayToGridLevel(squareSize);

                    if (closestPersistingArray.VerticalDivisions.Any())
                    {
                        tempDivisions = new ObservableCollection<CLPArrayDivision>(closestPersistingArray.VerticalDivisions);
                        closestPersistingArray.VerticalDivisions.Clear();
                    }
                    else
                    {
                        tempDivisions = new ObservableCollection<CLPArrayDivision>
                                        {
                                            new CLPArrayDivision(ArrayDivisionOrientation.Vertical,
                                                                 0,
                                                                 closestPersistingArray.ArrayWidth,
                                                                 closestPersistingArray.Columns)
                                        };
                    }

                    if (!snappingArray.VerticalDivisions.Any())
                    {
                        closestPersistingArray.VerticalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Vertical,
                                                                                          0,
                                                                                          snappingArray.ArrayWidth,
                                                                                          snappingArray.Columns));
                    }

                    foreach (var verticalDivision in snappingArray.VerticalDivisions)
                    {
                        closestPersistingArray.VerticalDivisions.Add(verticalDivision);
                    }
                    foreach (var verticalDivision in tempDivisions)
                    {
                        closestPersistingArray.VerticalDivisions.Add(new CLPArrayDivision(verticalDivision.Orientation,
                                                                                          verticalDivision.Position + snappingArray.ArrayWidth,
                                                                                          verticalDivision.Length,
                                                                                          verticalDivision.Value));
                    }

                    closestPersistingArray.Columns += snappingArray.Columns;
                    closestPersistingArray.XPosition -= snappingArray.ArrayWidth;
                    break;
                case SnapType.Right:
                    arraySnapHistoryItem = new CLPArraySnapHistoryItem(PageObject.ParentPage,
                                                                       App.MainWindowViewModel.CurrentUser,
                                                                       closestPersistingArray,
                                                                       snappingArray,
                                                                       false);

                    closestPersistingArray.HorizontalDivisions.Clear();

                    snappingArray.SizeArrayToGridLevel(squareSize);

                    if (!closestPersistingArray.VerticalDivisions.Any())
                    {
                        closestPersistingArray.VerticalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Vertical,
                                                                                          0,
                                                                                          closestPersistingArray.ArrayWidth,
                                                                                          closestPersistingArray.Columns));
                    }

                    if (!snappingArray.VerticalDivisions.Any())
                    {
                        closestPersistingArray.VerticalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Vertical,
                                                                                          closestPersistingArray.ArrayWidth,
                                                                                          snappingArray.ArrayWidth,
                                                                                          snappingArray.Columns));
                    }
                    else
                    {
                        foreach (var verticalDivision in snappingArray.VerticalDivisions)
                        {
                            closestPersistingArray.VerticalDivisions.Add(new CLPArrayDivision(verticalDivision.Orientation,
                                                                                              verticalDivision.Position +
                                                                                              closestPersistingArray.ArrayWidth,
                                                                                              verticalDivision.Length,
                                                                                              verticalDivision.Value));
                        }
                    }

                    closestPersistingArray.Columns += snappingArray.Columns;
                    break;
                default:
                    return;
            }

            closestPersistingArray.SizeArrayToGridLevel(squareSize, false);
            closestPersistingArray.IsDivisionBehaviorOn = true;

            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, arraySnapHistoryItem);

            //var extraPageObjects = PageObject.GetPageObjectsOverPageObject();
            PageObject.ParentPage.PageObjects.Remove(PageObject);
            PageObject.OnDeleted();
            //closestPersistingArray.RefreshStrokeParentIDs();
            //closestPersistingArray.RefreshPageObjectIDs();

            //var addObjects = new ObservableCollection<ICLPPageObject>();
            //var removeObjects = new ObservableCollection<ICLPPageObject>();
            //foreach(ICLPPageObject obj in extraPageObjects)
            //{
            //    if(!(closestPersistingArray.GetPageObjectsOverPageObject().Contains(obj)))
            //    {
            //        if(obj.XPosition + obj.Width < closestPersistingArray.XPosition + closestPersistingArray.LabelLength ||
            //           obj.XPosition > closestPersistingArray.XPosition + closestPersistingArray.LabelLength + closestPersistingArray.ArrayWidth ||
            //           obj.YPosition + obj.Height < closestPersistingArray.YPosition + closestPersistingArray.LabelLength ||
            //           obj.YPosition > closestPersistingArray.YPosition + closestPersistingArray.LabelLength + closestPersistingArray.ArrayHeight)
            //        {
            //            obj.XPosition = closestPersistingArray.XPosition + closestPersistingArray.LabelLength + 10 * addObjects.Count + 5;
            //            obj.YPosition = closestPersistingArray.YPosition + closestPersistingArray.LabelLength + 10 * addObjects.Count + 5;
            //        }
            //        addObjects.Add(obj);
            //    }
            //}
            //closestPersistingArray.AcceptObjects(addObjects, removeObjects);
        }

        /// <summary>Toggle the visibility of GridLines on the array.</summary>
        public Command ToggleGridCommand { get; private set; }

        private void OnToggleGridCommandExecute()
        {
            var clpArray = PageObject as CLPArray;
            if (clpArray != null)
            {
                clpArray.IsGridOn = !clpArray.IsGridOn;
                ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                                                           new CLPArrayGridToggleHistoryItem(PageObject.ParentPage,
                                                                                             App.MainWindowViewModel.CurrentUser,
                                                                                             PageObject.ID));
            }
        }

        /// <summary>Toggles the Division Behavior on and off.</summary>
        public Command ToggleDivisionAdornersCommand { get; private set; }

        private void OnToggleDivisionAdornersCommandExecute() { IsDivisionBehaviorOn = !IsDivisionBehaviorOn; }

        /// <summary>Rotates the array 90 degrees</summary>
        public Command RotateArrayCommand { get; private set; }

        protected void OnRotateArrayCommandExecute()
        {
            if ((PageObject as CLPArray).ArrayHeight > PageObject.ParentPage.Width ||
                (PageObject as CLPArray).ArrayWidth > PageObject.ParentPage.Height)
            {
                return;
            }

            var initXPos = PageObject.XPosition;
            var initYPos = PageObject.YPosition;
            (PageObject as CLPArray).RotateArray();
            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                                                       new CLPArrayRotateHistoryItem(PageObject.ParentPage,
                                                                                     App.MainWindowViewModel.CurrentUser,
                                                                                     PageObject.ID,
                                                                                     initXPos,
                                                                                     initYPos));

            var divisionTemplateIDsInHistory = DivisionTemplateAnalysis.GetListOfDivisionTemplateIDsInHistory(PageObject.ParentPage);
            foreach (var divisionTemplate in PageObject.ParentPage.PageObjects.OfType<FuzzyFactorCard>())
            {
                // Only increase OrientationChanged attempt if Division Template already full.
                if (divisionTemplate.CurrentRemainder != divisionTemplate.Dividend % divisionTemplate.Rows)
                {
                    var existingFactorPairErrorsTag =
                    PageObject.ParentPage.Tags.OfType<DivisionTemplateRemainderErrorsTag>()
                              .FirstOrDefault(x => x.DivisionTemplateID == divisionTemplate.ID);

                    if (existingFactorPairErrorsTag == null)
                    {
                        existingFactorPairErrorsTag = new DivisionTemplateRemainderErrorsTag(PageObject.ParentPage,
                                                                                                        Origin.StudentPageGenerated,
                                                                                                        divisionTemplate.ID,
                                                                                                        divisionTemplate.Dividend,
                                                                                                        divisionTemplate.Rows,
                                                              divisionTemplateIDsInHistory.IndexOf(divisionTemplate.ID));
                        PageObject.ParentPage.AddTag(existingFactorPairErrorsTag);
                    }
                    existingFactorPairErrorsTag.OrientationChangedDimensions.Add(string.Format("{0}x{1}", Rows, Columns));
                }
                else
                {
                    var existingRemainderErrorsTag =
                        PageObject.ParentPage.Tags.OfType<DivisionTemplateRemainderErrorsTag>()
                                  .FirstOrDefault(x => x.DivisionTemplateID == divisionTemplate.ID);

                    if (existingRemainderErrorsTag == null)
                    {
                        existingRemainderErrorsTag = new DivisionTemplateRemainderErrorsTag(PageObject.ParentPage,
                                                                                            Origin.StudentPageGenerated,
                                                                                            divisionTemplate.ID,
                                                                                            divisionTemplate.Dividend,
                                                                                            divisionTemplate.Rows,
                                                              divisionTemplateIDsInHistory.IndexOf(divisionTemplate.ID));
                        PageObject.ParentPage.AddTag(existingRemainderErrorsTag);
                    }
                    existingRemainderErrorsTag.OrientationChangedDimensions.Add(string.Format("{0}x{1}", Rows, Columns));
                }
            }
        }

        /// <summary>Gets the CreateHorizontalDivisionCommand command.</summary>
        public Command CreateHorizontalDivisionCommand { get; private set; }

        private void OnCreateHorizontalDivisionCommandExecute()
        {
            var position = LeftArrowPosition - 5;
            if (IsGridOn)
            {
                position = (PageObject as CLPArray).GetClosestGridLine(position);
            }

            if (HorizontalDivisions.Any(horizontalDivision => Math.Abs(horizontalDivision.Position - position) < 30.0))
            {
                return;
            }
            if (HorizontalDivisions.Count >= (PageObject as CLPArray).Rows)
            {
                MessageBox.Show("The number of divisions cannot be larger than the number of Rows.");
                return;
            }

            var divAbove = (PageObject as CLPArray).FindDivisionAbove(position, HorizontalDivisions);
            var divBelow = (PageObject as CLPArray).FindDivisionBelow(position, HorizontalDivisions);

            var addedDivisions = new List<CLPArrayDivision>();
            var removedDivisions = new List<CLPArrayDivision>();

            CLPArrayDivision topDiv;
            if (divAbove == null)
            {
                topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0, position, 0);
            }
            else
            {
                topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, divAbove.Position, position - divAbove.Position, 0);
                HorizontalDivisions.Remove(divAbove);
                removedDivisions.Add(divAbove);
            }
            HorizontalDivisions.Add(topDiv);
            addedDivisions.Add(topDiv);

            var bottomDiv = divBelow == null
                                ? new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, position, ArrayHeight - position, 0)
                                : new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, position, divBelow.Position - position, 0);

            HorizontalDivisions.Add(bottomDiv);
            addedDivisions.Add(bottomDiv);

            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                                                       new CLPArrayDivisionsChangedHistoryItem(PageObject.ParentPage,
                                                                                               App.MainWindowViewModel.CurrentUser,
                                                                                               PageObject.ID,
                                                                                               addedDivisions,
                                                                                               removedDivisions));
        }

        /// <summary>Gets the CreateVerticalDivisionCommand command.</summary>
        public Command CreateVerticalDivisionCommand { get; private set; }

        private void OnCreateVerticalDivisionCommandExecute()
        {
            var position = TopArrowPosition - 5;
            if (IsGridOn)
            {
                position = (PageObject as CLPArray).GetClosestGridLine(position);
            }

            if (VerticalDivisions.Any(verticalDivision => Math.Abs(verticalDivision.Position - position) < 30.0))
            {
                return;
            }
            if (VerticalDivisions.Count >= (PageObject as CLPArray).Columns)
            {
                MessageBox.Show("The number of divisions cannot be larger than the number of Columns.");
                return;
            }

            var divAbove = (PageObject as CLPArray).FindDivisionAbove(position, VerticalDivisions);
            var divBelow = (PageObject as CLPArray).FindDivisionBelow(position, VerticalDivisions);

            var addedDivisions = new List<CLPArrayDivision>();
            var removedDivisions = new List<CLPArrayDivision>();

            CLPArrayDivision topDiv;
            if (divAbove == null)
            {
                topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, position, 0);
            }
            else
            {
                topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, position - divAbove.Position, 0);
                VerticalDivisions.Remove(divAbove);
                removedDivisions.Add(divAbove);
            }
            VerticalDivisions.Add(topDiv);
            addedDivisions.Add(topDiv);

            var bottomDiv = divBelow == null
                                ? new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, ArrayWidth - position, 0)
                                : new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, divBelow.Position - position, 0);

            VerticalDivisions.Add(bottomDiv);
            addedDivisions.Add(bottomDiv);

            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                                                       new CLPArrayDivisionsChangedHistoryItem(PageObject.ParentPage,
                                                                                               App.MainWindowViewModel.CurrentUser,
                                                                                               PageObject.ID,
                                                                                               addedDivisions,
                                                                                               removedDivisions));
        }

        /// <summary>Gets the EditLabelCommand command.</summary>
        public Command<CLPArrayDivision> EditLabelCommand { get; private set; }

        private void OnEditLabelCommandExecute(CLPArrayDivision division)
        {
            // Pop up numberpad and save result as value of division
            var keyPad = new KeypadWindowView
                         {
                             Owner = Application.Current.MainWindow,
                             WindowStartupLocation = WindowStartupLocation.Manual,
                             Top = 100,
                             Left = 100
                         };
            keyPad.ShowDialog();
            if (keyPad.DialogResult != true ||
                keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }

            var previousValue = division.Value;
            var isHorizontalDivision = division.Orientation == ArrayDivisionOrientation.Horizontal;

            division.Value = Int32.Parse(keyPad.NumbersEntered.Text);

            var array = PageObject as CLPArray;
            if (array == null)
            {
                return;
            }
            var divisionIndex = isHorizontalDivision ? array.HorizontalDivisions.IndexOf(division) : array.VerticalDivisions.IndexOf(division);

            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                                                       new CLPArrayDivisionValueChangedHistoryItem(PageObject.ParentPage,
                                                                                                   App.MainWindowViewModel.CurrentUser,
                                                                                                   PageObject.ID,
                                                                                                   isHorizontalDivision,
                                                                                                   divisionIndex,
                                                                                                   previousValue));

            // Check if array labels add up to larger array dimension
            var dividerValues = new List<int>();
            if (division.Orientation == ArrayDivisionOrientation.Horizontal)
            {
                var total = 0;
                foreach (var div in HorizontalDivisions)
                {
                    if (div.Value == 0)
                    {
                        total = Rows;
                        break;
                    }
                    dividerValues.Add(div.Value);
                    total += div.Value;
                }

                if (total == Rows)
                {
                    return;
                }
                var labelsString = "";
                if (HorizontalDivisions.Count == 2)
                {
                    labelsString = HorizontalDivisions.First().Value + " and " + HorizontalDivisions.Last().Value;
                }
                else
                {
                    for (var i = 0; i < HorizontalDivisions.Count; i++)
                    {
                        labelsString += HorizontalDivisions.ElementAt(i).Value.ToString();
                        if (i < HorizontalDivisions.Count - 1)
                        {
                            labelsString += ", ";
                        }
                        if (i == HorizontalDivisions.Count - 2)
                        {
                            labelsString += "and ";
                        }
                    }
                }

                PageObject.ParentPage.AddTag(new ArrayTriedWrongDividerValuesTag(PageObject.ParentPage,
                                                                                 Origin.StudentPageObjectGenerated,
                                                                                 PageObject.ID,
                                                                                 Rows,
                                                                                 Columns,
                                                                                 DividerValuesOrientation.Vertical,
                                                                                 dividerValues));
                MessageBox.Show(
                                "The side of the array is " + Rows + ". You broke the side into " + labelsString + ", which don’t add up to " + Rows +
                                ".",
                                "Oops");
            }
            else
            {
                var total = 0;
                foreach (var div in VerticalDivisions)
                {
                    if (div.Value == 0)
                    {
                        total = Columns;
                        break;
                    }
                    dividerValues.Add(div.Value);
                    total += div.Value;
                }

                if (total == Columns)
                {
                    return;
                }
                var labelsString = "";
                if (VerticalDivisions.Count == 2)
                {
                    labelsString = VerticalDivisions.First().Value + " and " + VerticalDivisions.Last().Value;
                }
                else
                {
                    for (var i = 0; i < VerticalDivisions.Count; i++)
                    {
                        labelsString += VerticalDivisions.ElementAt(i).Value.ToString();
                        if (i < VerticalDivisions.Count - 1)
                        {
                            labelsString += ", ";
                        }
                        if (i == VerticalDivisions.Count - 2)
                        {
                            labelsString += "and ";
                        }
                    }
                }

                PageObject.ParentPage.AddTag(new ArrayTriedWrongDividerValuesTag(PageObject.ParentPage,
                                                                                 Origin.StudentPageObjectGenerated,
                                                                                 PageObject.ID,
                                                                                 Rows,
                                                                                 Columns,
                                                                                 DividerValuesOrientation.Horizontal,
                                                                                 dividerValues));
                MessageBox.Show(
                                "The side of the array is " + Columns + ". You broke the side into " + labelsString + ", which don’t add up to " +
                                Columns + ".",
                                "Oops");
            }
        }

        /// <summary>Gets the EraseDivisionCommand command.</summary>
        public Command<MouseEventArgs> EraseDivisionCommand { get; private set; }

        private void OnEraseDivisionCommandExecute(MouseEventArgs e)
        {
            var rectangle = e.Source as Rectangle;
            if ((e.StylusDevice == null || !e.StylusDevice.Inverted || e.LeftButton != MouseButtonState.Pressed) &&
                e.MiddleButton != MouseButtonState.Pressed ||
                rectangle == null)
            {
                return;
            }

            var division = rectangle.DataContext as CLPArrayDivision;

            if (division == null ||
                division.Position == 0.0)
            {
                return;
            }

            var addedDivisions = new List<CLPArrayDivision>();
            var removedDivisions = new List<CLPArrayDivision>();
            if (division.Orientation == ArrayDivisionOrientation.Horizontal)
            {
                var divAbove = (PageObject as CLPArray).FindDivisionAbove(division.Position, (PageObject as CLPArray).HorizontalDivisions);
                (PageObject as CLPArray).HorizontalDivisions.Remove(divAbove);
                (PageObject as CLPArray).HorizontalDivisions.Remove(division);
                removedDivisions.Add(divAbove);
                removedDivisions.Add(division);

                //Add new division unless we removed the only division line
                if ((PageObject as CLPArray).HorizontalDivisions.Count > 0)
                {
                    var newLength = divAbove.Length + division.Length;
                    var newDivision = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, divAbove.Position, newLength, 0);
                    (PageObject as CLPArray).HorizontalDivisions.Add(newDivision);
                    addedDivisions.Add(newDivision);
                }
            }
            if (division.Orientation == ArrayDivisionOrientation.Vertical)
            {
                var divAbove = (PageObject as CLPArray).FindDivisionAbove(division.Position, (PageObject as CLPArray).VerticalDivisions);
                (PageObject as CLPArray).VerticalDivisions.Remove(divAbove);
                (PageObject as CLPArray).VerticalDivisions.Remove(division);
                removedDivisions.Add(divAbove);
                removedDivisions.Add(division);

                //Add new division unless we removed the only division line
                if ((PageObject as CLPArray).VerticalDivisions.Count > 0)
                {
                    var newLength = divAbove.Length + division.Length;
                    var newDivision = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, newLength, 0);
                    (PageObject as CLPArray).VerticalDivisions.Add(newDivision);
                    addedDivisions.Add(newDivision);
                }
            }

            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                                                       new CLPArrayDivisionsChangedHistoryItem(PageObject.ParentPage,
                                                                                               App.MainWindowViewModel.CurrentUser,
                                                                                               PageObject.ID,
                                                                                               addedDivisions,
                                                                                               removedDivisions));
        }

        /// <summary>Toggles the main adorners for the array.</summary>
        public Command<MouseButtonEventArgs> ToggleMainArrayAdornersCommand { get; private set; }

        private void OnToggleMainArrayAdornersCommandExecute(MouseButtonEventArgs e)
        {
            if (App.MainWindowViewModel.CurrentUser.ID != PageObject.CreatorID &&
                !PageObject.IsManipulatableByNonCreator)
            {
                return;
            }

            if (e.ChangedButton != MouseButton.Left ||
                e.StylusDevice != null && e.StylusDevice.Inverted)
            {
                return;
            }
            var tempAdornerState = IsDefaultAdornerVisible;
            ACLPPageBaseViewModel.ClearAdorners(PageObject.ParentPage);
            IsAdornerVisible = !tempAdornerState;
            IsDefaultAdornerVisible = !tempAdornerState;
            IsTopAdornerVisible = tempAdornerState;
            IsLeftAdornerVisible = tempAdornerState;

            var contextRibbon = NotebookWorkspaceViewModel.GetContextRibbon();
            if (contextRibbon == null)
            {
                return;
            }

            if (IsAdornerVisible)
            {
                PopulateContextRibbon(contextRibbon);
            }
            else
            {
                contextRibbon.Buttons.Clear();
            }
        }

        private ObservableCollection<UIElement> _contextButtons = new ObservableCollection<UIElement>();
        public void PopulateContextRibbon(ContextRibbonViewModel contextRibbon)
        {
            if (!_contextButtons.Any())
            {
                _contextButtons.Add(new RibbonButton("Delete", "pack://application:,,,/Images/Delete.png", RemovePageObjectCommand, null, true));
                _contextButtons.Add(new RibbonButton("Duplicate", "pack://application:,,,/Images/AddToDisplay.png", DuplicateArrayCommand, null, true));

                _contextButtons.Add(MajorRibbonViewModel.Separater);

                _contextButtons.Add(new RibbonButton("Rotate", "pack://application:,,,/Images/Protractor64.png", RotateArrayCommand, null, true));
                _contextButtons.Add(new RibbonButton("Toggle Grid Lines", "pack://application:,,,/Resources/Images/Array32.png", ToggleGridCommand, null, true));
                _contextButtons.Add(new RibbonButton("Snap", "pack://application:,,,/Resources/Images/AdornerImages/ArraySnap64.png", null, null, true));
                _contextButtons.Add(new RibbonButton("Size to Other Arrays", "pack://application:,,,/Resources/Images/AdornerImages/ArraySnap64.png", null, null, true));
            }

            contextRibbon.Buttons = _contextButtons;
        }

        /// <summary>Brings up a menu to make multiple copies of an array</summary>
        public Command DuplicateArrayCommand { get; private set; }

        private void OnDuplicateArrayCommandExecute()
        {
            var keyPad = new KeypadWindowView("How many copies?", 21)
                         {
                             Owner = Application.Current.MainWindow,
                             WindowStartupLocation = WindowStartupLocation.Manual,
                             Top = 100,
                             Left = 100
                         };
            keyPad.ShowDialog();
            if (keyPad.DialogResult != true ||
                keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }
            var numberOfArrays = Int32.Parse(keyPad.NumbersEntered.Text);

            var xPosition = 10.0;
            var yPosition = 160.0;
            if (YPosition + 2 * Height + 10.0 < PageObject.ParentPage.Height)
            {
                yPosition = YPosition + Height + 10.0;
            }
            else if (XPosition + 2 * Width + 10.0 < PageObject.ParentPage.Width)
            {
                yPosition = YPosition;
                xPosition = XPosition + Width + 10.0;
            }
            const double LABEL_LENGTH = 22.0;

            var arraysToAdd = new List<CLPArray>();
            foreach (var index in Enumerable.Range(1, numberOfArrays))
            {
                var array = PageObject.Duplicate() as CLPArray;
                array.XPosition = xPosition;
                array.YPosition = yPosition;

                if (xPosition + 2 * (ArrayWidth + LABEL_LENGTH) <= PageObject.ParentPage.Width)
                {
                    xPosition += ArrayWidth + LABEL_LENGTH;
                }
                    //If there isn't room, diagonally pile the rest
                else if ((xPosition + ArrayWidth + LABEL_LENGTH + 20.0 <= PageObject.ParentPage.Width) &&
                         (yPosition + ArrayHeight + LABEL_LENGTH + 20.0 <= PageObject.ParentPage.Height))
                {
                    xPosition += 20.0;
                    yPosition += 20.0;
                }
                arraysToAdd.Add(array);
            }

            if (arraysToAdd.Count == 1)
            {
                ACLPPageBaseViewModel.AddPageObjectToPage(arraysToAdd.First());
            }
            else
            {
                ACLPPageBaseViewModel.AddPageObjectsToPage(PageObject.ParentPage, arraysToAdd);
            }
        }

        #endregion //Commands

        #region Methods

        public override void ClearAdorners()
        {
            IsAdornerVisible = false;
            IsDefaultAdornerVisible = false;
            IsTopAdornerVisible = false;
            IsLeftAdornerVisible = false;
        }

        #endregion //Methods

        #region Static Methods

        public static bool CreateDivision(CLPArray array, Stroke cuttingStroke)
        {
            var strokeTop = cuttingStroke.GetBounds().Top;
            var strokeBottom = cuttingStroke.GetBounds().Bottom;
            var strokeLeft = cuttingStroke.GetBounds().Left;
            var strokeRight = cuttingStroke.GetBounds().Right;

            var cuttableTop = array.YPosition + array.LabelLength;
            var cuttableBottom = cuttableTop + array.ArrayHeight;
            var cuttableLeft = array.XPosition + array.LabelLength;
            var cuttableRight = cuttableLeft + array.ArrayWidth;

            if (array.ArrayType != ArrayTypes.Array)
            {
                return false;
            }

            const double MIN_THRESHHOLD = 30.0;

            if (Math.Abs(strokeLeft - strokeRight) < Math.Abs(strokeTop - strokeBottom) &&
                strokeRight <= cuttableRight &&
                strokeLeft >= cuttableLeft &&
                strokeTop - cuttableTop <= MIN_THRESHHOLD &&
                cuttableBottom - strokeBottom <= MIN_THRESHHOLD &&
                array.Columns > 1) //Vertical Stroke. Stroke must be within the bounds of the pageObject
            {
                var average = (strokeRight + strokeLeft) / 2;
                var relativeAverage = average - array.LabelLength - array.XPosition;
                var position = relativeAverage;
                if (array.IsGridOn)
                {
                    position = array.GetClosestGridLine(position);
                }

                if (array.VerticalDivisions.Any(verticalDivision => Math.Abs(verticalDivision.Position - position) < 30.0))
                {
                    return false;
                }
                if (array.VerticalDivisions.Count >= array.Columns)
                {
                    //MessageBox.Show("The number of divisions cannot be larger than the number of Columns.");
                    return false;
                }

                var divAbove = array.FindDivisionAbove(position, array.VerticalDivisions);
                var divBelow = array.FindDivisionBelow(position, array.VerticalDivisions);

                var addedDivisions = new List<CLPArrayDivision>();
                var removedDivisions = new List<CLPArrayDivision>();

                CLPArrayDivision topDiv;
                if (divAbove == null)
                {
                    topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, position, 0);
                }
                else
                {
                    topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, position - divAbove.Position, 0);
                    array.VerticalDivisions.Remove(divAbove);
                    removedDivisions.Add(divAbove);
                }
                array.VerticalDivisions.Add(topDiv);
                addedDivisions.Add(topDiv);

                var bottomDiv = divBelow == null
                                    ? new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, array.ArrayWidth - position, 0)
                                    : new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, divBelow.Position - position, 0);

                array.VerticalDivisions.Add(bottomDiv);
                addedDivisions.Add(bottomDiv);

                ACLPPageBaseViewModel.AddHistoryItemToPage(array.ParentPage,
                                                           new CLPArrayDivisionsChangedHistoryItem(array.ParentPage,
                                                                                                   App.MainWindowViewModel.CurrentUser,
                                                                                                   array.ID,
                                                                                                   addedDivisions,
                                                                                                   removedDivisions));
                return true;
            }
            else if (Math.Abs(strokeLeft - strokeRight) > Math.Abs(strokeTop - strokeBottom) &&
                     strokeBottom <= cuttableBottom &&
                     strokeTop >= cuttableTop &&
                     cuttableRight - strokeRight <= MIN_THRESHHOLD &&
                     strokeLeft - cuttableLeft <= MIN_THRESHHOLD &&
                     array.Rows > 1) //Horizontal Stroke. Stroke must be within the bounds of the pageObject
            {
                var average = (strokeTop + strokeBottom) / 2;
                var relativeAverage = average - array.LabelLength - array.YPosition;
                var position = relativeAverage;
                if (array.IsGridOn)
                {
                    position = array.GetClosestGridLine(position);
                }

                if (array.HorizontalDivisions.Any(horizontalDivision => Math.Abs(horizontalDivision.Position - position) < 30.0))
                {
                    return false;
                }
                if (array.HorizontalDivisions.Count >= array.Rows)
                {
                    //MessageBox.Show("The number of divisions cannot be larger than the number of Rows.");
                    return false;
                }

                var divAbove = array.FindDivisionAbove(position, array.HorizontalDivisions);
                var divBelow = array.FindDivisionBelow(position, array.HorizontalDivisions);

                var addedDivisions = new List<CLPArrayDivision>();
                var removedDivisions = new List<CLPArrayDivision>();

                CLPArrayDivision topDiv;
                if (divAbove == null)
                {
                    topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0, position, 0);
                }
                else
                {
                    topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, divAbove.Position, position - divAbove.Position, 0);
                    array.HorizontalDivisions.Remove(divAbove);
                    removedDivisions.Add(divAbove);
                }
                array.HorizontalDivisions.Add(topDiv);
                addedDivisions.Add(topDiv);

                var bottomDiv = divBelow == null
                                    ? new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, position, array.ArrayHeight - position, 0)
                                    : new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, position, divBelow.Position - position, 0);

                array.HorizontalDivisions.Add(bottomDiv);
                addedDivisions.Add(bottomDiv);

                ACLPPageBaseViewModel.AddHistoryItemToPage(array.ParentPage,
                                                           new CLPArrayDivisionsChangedHistoryItem(array.ParentPage,
                                                                                                   App.MainWindowViewModel.CurrentUser,
                                                                                                   array.ID,
                                                                                                   addedDivisions,
                                                                                                   removedDivisions));
                return true;
            }

            return false;
        }

        public static void AddArrayToPage(CLPPage page)
        {
            if (page == null)
            {
                return;
            }

            var arrayType = "ARRAY";
            int rows, columns, dividend = 1, numberOfArrays = 1;
            switch (arrayType)
            {
                case "TENBYTEN":
                    rows = 10;
                    columns = 10;
                    numberOfArrays = 1;
                    break;
                case "ARRAY":
                    var arrayCreationView = new ArrayCreationView { Owner = Application.Current.MainWindow };
                    arrayCreationView.ShowDialog();

                    if (arrayCreationView.DialogResult != true)
                    {
                        return;
                    }

                    try
                    {
                        rows = Convert.ToInt32(arrayCreationView.Rows.Text);
                    }
                    catch (FormatException)
                    {
                        rows = 1;
                    }

                    try
                    {
                        columns = Convert.ToInt32(arrayCreationView.Columns.Text);
                    }
                    catch (FormatException)
                    {
                        columns = 1;
                    }

                    try
                    {
                        numberOfArrays = Convert.ToInt32(arrayCreationView.NumberOfArrays.Text);
                    }
                    catch (FormatException)
                    {
                        numberOfArrays = 1;
                    }
                    break;
                case "FUZZYFACTORCARD":
                    var factorCreationView = new FuzzyFactorCardCreationView { Owner = Application.Current.MainWindow };
                    factorCreationView.ShowDialog();

                    if (factorCreationView.DialogResult != true)
                    {
                        return;
                    }

                    try
                    {
                        dividend = Convert.ToInt32(factorCreationView.Product.Text);
                    }
                    catch (FormatException)
                    {
                        return;
                    }

                    try
                    {
                        rows = Convert.ToInt32(factorCreationView.Factor.Text);
                    }
                    catch (FormatException)
                    {
                        return;
                    }

                    columns = dividend / rows;
                    numberOfArrays = 1;
                    break;
                case "FFCREMAINDER":
                    var factorRemainderCreationView = new FuzzyFactorCardWithTilesCreationView
                    {
                        Owner = Application.Current.MainWindow
                    };
                    factorRemainderCreationView.ShowDialog();

                    if (factorRemainderCreationView.DialogResult != true)
                    {
                        return;
                    }

                    try
                    {
                        dividend = Convert.ToInt32(factorRemainderCreationView.Product.Text);
                    }
                    catch (FormatException)
                    {
                        return;
                    }

                    try
                    {
                        rows = Convert.ToInt32(factorRemainderCreationView.Factor.Text);
                    }
                    catch (FormatException)
                    {
                        return;
                    }

                    columns = dividend / rows;
                    numberOfArrays = 1;
                    break;
                case "ARRAYCARD":
                case "FACTORCARD":
                //    var factorCreationView = new FactorCardCreationView{ Owner = Application.Current.MainWindow};
                //    factorCreationView.ShowDialog();
                //    if(factorCreationView.DialogResult != true)
                //    {
                //        return;
                //    }

                //    try
                //    {
                //        dividend = Convert.ToInt32(factorCreationView.Product.Text);
                //    }
                //    catch(FormatException)
                //    {
                //        return;
                //    }

                //    try
                //    {
                //        rows = Convert.ToInt32(factorCreationView.Factor.Text);
                //    }
                //    catch(FormatException)
                //    {
                //        return;
                //    }

                //    columns = dividend / rows;
                //    numberOfArrays = 1;
                default:
                    return;
            }

            var arraysToAdd = new List<ACLPArrayBase>();
            foreach (var index in Enumerable.Range(1, numberOfArrays))
            {
                ACLPArrayBase array;
                switch (arrayType)
                {
                    case "TENBYTEN":
                        array = new CLPArray(page, columns, rows, ArrayTypes.TenByTen);
                        array.IsSnappable = false;
                        array.IsTopLabelVisible = false;
                        array.IsSideLabelVisible = false;
                        break;
                    case "ARRAY":
                        array = new CLPArray(page, columns, rows, ArrayTypes.Array);
                        break;
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
                    case "ARRAYCARD":
                        array = new CLPArray(page, columns, rows, ArrayTypes.ArrayCard);
                        break;
                    case "FACTORCARD":
                        array = new CLPArray(page, columns, rows, ArrayTypes.FactorCard);
                        break;
                    default:
                        return;
                }

                arraysToAdd.Add(array);
            }

            int arrayStacks = MatchArrayGridSize(arraysToAdd, page);

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
            ACLPArrayBase.ApplyDistinctPosition(firstArray, App.MainWindowViewModel.CurrentUser.ID);

            PlaceArrayNextToExistingArray(arraysToAdd, page);
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

        public static int MatchArrayGridSize(List<ACLPArrayBase> arraysToAdd, CLPPage CurrentPage)
        {
            int numberOfArrays = arraysToAdd.Count;
            var firstArray = arraysToAdd.First();
            int rows = firstArray.Rows;
            int columns = firstArray.Columns;

            const double MIN_SIDE = 20.0;
            const double MIN_FFC_SIDE = 185.0;
            const double LABEL_LENGTH = 22.0;

            var arrayStacks = 1;
            var isHorizontallyAligned = CurrentPage.Width / columns > CurrentPage.Height / 4 * 3 / rows;
            firstArray.SizeArrayToGridLevel();
            var initialGridsquareSize = firstArray.GridSquareSize;
            var xPosition = 0.0;
            var yPosition = 150.0;

            //attempt to size newArray to lastArray
            //if fail, resize all other arrays to newArray
            //squareSize will be the grid size of the most recently placed array, or 0 if there are no non-background arrays
            double squareSize = 0.0;
            if (!(firstArray is FuzzyFactorCard))
            {
                foreach (var pageObject in CurrentPage.PageObjects)
                {
                    if ((pageObject is CLPArray || pageObject is FuzzyFactorCard) && pageObject.CreatorID != Person.Author.ID)
                    {
                        squareSize = (pageObject as ACLPArrayBase).ArrayHeight / (pageObject as ACLPArrayBase).Rows;
                    }
                }
            }

            var minSide = (firstArray is FuzzyFactorCard)
                ? MIN_FFC_SIDE :
                MIN_SIDE;
            var defaultSquareSize = (firstArray is FuzzyFactorCard) ?
                Math.Max(45.0, (MIN_FFC_SIDE / (Math.Min(rows, columns)))) :
                45.0;
            var initializedSquareSize = (squareSize > 0) ? Math.Max(squareSize, (minSide / (Math.Min(rows, columns)))) : defaultSquareSize;
            if ((firstArray is FuzzyFactorCard) && xPosition + initializedSquareSize * columns + LABEL_LENGTH * 3.0 + 12.0 > CurrentPage.Width)
            {
                initializedSquareSize = minSide / (Math.Min(rows, columns));
            }

            while (xPosition + 2 * LABEL_LENGTH + initializedSquareSize * columns >= CurrentPage.Width || yPosition + 2 * LABEL_LENGTH + initializedSquareSize * rows >= CurrentPage.Height)
            {
                initializedSquareSize = Math.Abs(initializedSquareSize - 45.0) < .0001 ? 22.5 : initializedSquareSize / 4 * 3;
            }
            if (numberOfArrays > 1)
            {
                if (isHorizontallyAligned)
                {
                    while (xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * numberOfArrays + LABEL_LENGTH >= CurrentPage.Width)
                    {
                        initializedSquareSize = Math.Abs(initializedSquareSize - 45.0) < .0001 ? 22.5 : initializedSquareSize / 4 * 3;

                        if (numberOfArrays < 5 || xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * numberOfArrays + LABEL_LENGTH < CurrentPage.Width)
                        {
                            continue;
                        }

                        if (xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * Math.Ceiling((double)numberOfArrays / 2) + LABEL_LENGTH < CurrentPage.Width &&
                           yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * 2 + LABEL_LENGTH < CurrentPage.Height)
                        {
                            arrayStacks = 2;
                            break;
                        }

                        if (xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * Math.Ceiling((double)numberOfArrays / 3) + LABEL_LENGTH < CurrentPage.Width &&
                           yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * 3 + LABEL_LENGTH < CurrentPage.Height)
                        {
                            arrayStacks = 3;
                            break;
                        }
                    }
                }
                else
                {
                    yPosition = 100;
                    while (yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * numberOfArrays + LABEL_LENGTH >= CurrentPage.Height)
                    {
                        initializedSquareSize = Math.Abs(initializedSquareSize - 45.0) < .0001 ? 22.5 : initializedSquareSize / 4 * 3;

                        if (numberOfArrays < 5 || yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * numberOfArrays + LABEL_LENGTH < CurrentPage.Height)
                        {
                            continue;
                        }

                        if (yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * Math.Ceiling((double)numberOfArrays / 2) + LABEL_LENGTH < CurrentPage.Height &&
                           xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * 2 + LABEL_LENGTH < CurrentPage.Width)
                        {
                            arrayStacks = 2;
                            break;
                        }

                        if (yPosition + (LABEL_LENGTH + rows * initializedSquareSize) * Math.Ceiling((double)numberOfArrays / 3) + LABEL_LENGTH < CurrentPage.Height &&
                           xPosition + (LABEL_LENGTH + columns * initializedSquareSize) * 3 + LABEL_LENGTH < CurrentPage.Width)
                        {
                            arrayStacks = 3;
                            break;
                        }
                    }
                }
            }

            // If it doesn't fit, resize all other non-background arrays on page to match new array grid size
            if (squareSize > 0.0 && initializedSquareSize != squareSize)
            {
                Dictionary<string, Point> oldDimensions = new Dictionary<string, Point>();
                foreach (var pageObject in CurrentPage.PageObjects)
                {
                    if (pageObject is CLPArray && pageObject.CreatorID != Person.Author.ID)
                    {
                        oldDimensions.Add(pageObject.ID, new Point(pageObject.Width, pageObject.Height));
                        if ((pageObject as ACLPArrayBase).Rows * initializedSquareSize > MIN_SIDE && (pageObject as ACLPArrayBase).Columns * initializedSquareSize > MIN_SIDE)
                        {
                            if (pageObject.XPosition + (pageObject as ACLPArrayBase).Columns * initializedSquareSize + 2 * LABEL_LENGTH <= CurrentPage.Width && pageObject.YPosition + (pageObject as ACLPArrayBase).Rows * initializedSquareSize + 2 * LABEL_LENGTH <= CurrentPage.Height)
                            {
                                (pageObject as ACLPArrayBase).SizeArrayToGridLevel(initializedSquareSize);
                            }
                        }
                        else
                        {
                            (pageObject as ACLPArrayBase).SizeArrayToGridLevel(MIN_SIDE / Math.Min((pageObject as ACLPArrayBase).Rows, (pageObject as ACLPArrayBase).Columns));
                        }
                    }
                    initialGridsquareSize = initializedSquareSize;
                }
            }


            double MAX_HEIGHT = CurrentPage.Height - 400.0;
            if (squareSize == 0.0)
            {
                initializedSquareSize = Math.Min(initialGridsquareSize, MAX_HEIGHT / rows);
            }

            firstArray.SizeArrayToGridLevel(initializedSquareSize);

            return arrayStacks;
        }

        public static void PlaceArrayNextToExistingArray(List<ACLPArrayBase> arraysToAdd, CLPPage CurrentPage)
        {
            const double LABEL_LENGTH = 22.0;
            int numberOfArrays = arraysToAdd.Count;
            var firstArray = arraysToAdd.First();
            int rows = firstArray.Rows;
            int columns = firstArray.Columns;
            var isHorizontallyAligned = CurrentPage.Width / columns > CurrentPage.Height / 4 * 3 / rows;
            double initializedSquareSize = firstArray.ArrayHeight / rows;
            var xPosition = firstArray.XPosition;
            var yPosition = firstArray.YPosition;

            //if there is exactly one other array on the page, keep track of it for placement
            ACLPArrayBase onlyArray = null;
            foreach (var pageObject in CurrentPage.PageObjects)
            {
                if (pageObject is CLPArray)
                {
                    onlyArray = (onlyArray == null) ? pageObject as CLPArray : null;
                }
                else if (pageObject is FuzzyFactorCard)
                {
                    onlyArray = (onlyArray == null) ? pageObject as FuzzyFactorCard : null;
                }
            }

            //Position to not overlap with first array on page if possible
            if (onlyArray != null)
            {
                if (isHorizontallyAligned)
                {
                    const double GAP = 35.0;
                    if (!(onlyArray is FuzzyFactorCard && (onlyArray as FuzzyFactorCard).RemainderTiles != null) && onlyArray.XPosition + onlyArray.Width + (LABEL_LENGTH + columns * initializedSquareSize) * numberOfArrays + LABEL_LENGTH + GAP <= CurrentPage.Width
                        && rows * initializedSquareSize + LABEL_LENGTH < CurrentPage.Height)
                    {
                        xPosition = onlyArray.XPosition + onlyArray.Width + GAP;
                        yPosition = onlyArray.YPosition;
                    }
                    else if (onlyArray.XPosition + (LABEL_LENGTH + columns * initializedSquareSize) * numberOfArrays + LABEL_LENGTH <= CurrentPage.Width
                        && onlyArray.YPosition + onlyArray.Height + rows * initializedSquareSize + LABEL_LENGTH + GAP < CurrentPage.Height)
                    {
                        yPosition = onlyArray.YPosition + onlyArray.Height + GAP;
                        xPosition = onlyArray.XPosition;
                    }
                    else
                    {
                        yPosition = CurrentPage.Height - rows * initializedSquareSize - 2 * LABEL_LENGTH;
                        xPosition = onlyArray.XPosition;
                    }
                }
                else
                {
                    const double GAP = 35.0;
                    if (!(onlyArray is FuzzyFactorCard && (onlyArray as FuzzyFactorCard).RemainderTiles != null) && onlyArray.YPosition + (LABEL_LENGTH + rows * initializedSquareSize) * numberOfArrays + LABEL_LENGTH <= CurrentPage.Height
                        && onlyArray.XPosition + onlyArray.Width + columns * initializedSquareSize + LABEL_LENGTH + GAP < CurrentPage.Width)
                    {
                        xPosition = onlyArray.XPosition + onlyArray.Width + GAP;
                        yPosition = onlyArray.YPosition;
                    }
                    else if (onlyArray.YPosition + onlyArray.Height + (LABEL_LENGTH + rows * initializedSquareSize) * numberOfArrays + LABEL_LENGTH + GAP <= CurrentPage.Width
                        && onlyArray.XPosition + rows * initializedSquareSize + LABEL_LENGTH < CurrentPage.Height)
                    {
                        yPosition = onlyArray.YPosition + onlyArray.Height + GAP;
                        xPosition = onlyArray.XPosition;
                    }
                }
            }

            firstArray.XPosition = xPosition;
            firstArray.YPosition = yPosition;
        }

        #endregion //Static Methods
    }
}