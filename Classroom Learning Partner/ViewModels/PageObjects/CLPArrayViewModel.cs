using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPArrayViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CLPArrayViewModel" /> class.
        /// </summary>
        public CLPArrayViewModel(CLPArray array)
        {
            PageObject = array;
            hoverTimer.Interval = 2300;
            CloseAdornerTimeOut = 0.15;

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
        /// Turns the grid on or off.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsGridOn
        {
            get { return GetValue<bool>(IsGridOnProperty); }
            set { SetValue(IsGridOnProperty, value); }
        }

        public static readonly PropertyData IsGridOnProperty = RegisterProperty("IsGridOn", typeof(bool));

        /// <summary>
        /// Turns division behavior on or off.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsDivisionBehaviorOn
        {
            get { return GetValue<bool>(IsDivisionBehaviorOnProperty); }
            set { SetValue(IsDivisionBehaviorOnProperty, value); }
        }

        public static readonly PropertyData IsDivisionBehaviorOnProperty = RegisterProperty("IsDivisionBehaviorOn", typeof(bool));

        /// <summary>
        /// Whether or not the array can snap.
        /// This property is automatically mapped to the corresponding property in PageObject.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsSnappable
        {
            get { return GetValue<bool>(IsSnappableProperty); }
            set { SetValue(IsSnappableProperty, value); }
        }

        public static readonly PropertyData IsSnappableProperty = RegisterProperty("IsSnappable", typeof(bool));

        /// <summary>
        /// Visibility of the Top Labels
        /// This property is automatically mapped to the corresponding property in PageObject.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsTopLabelVisible
        {
            get { return GetValue<bool>(IsTopLabelVisibleProperty); }
            set { SetValue(IsTopLabelVisibleProperty, value); }
        }

        public static readonly PropertyData IsTopLabelVisibleProperty = RegisterProperty("IsTopLabelVisible", typeof(bool));

        /// <summary>
        /// Visibility of the Side Labels
        /// This property is automatically mapped to the corresponding property in PageObject.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsSideLabelVisible
        {
            get { return GetValue<bool>(IsSideLabelVisibleProperty); }
            set { SetValue(IsSideLabelVisibleProperty, value); }
        }

        public static readonly PropertyData IsSideLabelVisibleProperty = RegisterProperty("IsSideLabelVisible", typeof(bool));

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

        #endregion //Model

        #region Bindings

        /// <summary>
        /// Gets or sets the BottomArrowPosition value.
        /// </summary>
        public double TopArrowPosition
        {
            get { return GetValue<double>(TopArrowPositionProperty); }
            set { SetValue(TopArrowPositionProperty, value); }
        }

        public static readonly PropertyData TopArrowPositionProperty = RegisterProperty("TopArrowPosition", typeof(double), 0.0);

        /// <summary>
        /// Gets or sets the RightArrowPosition value.
        /// </summary>
        public double LeftArrowPosition
        {
            get { return GetValue<double>(LeftArrowPositionProperty); }
            set { SetValue(LeftArrowPositionProperty, value); }
        }

        public static readonly PropertyData LeftArrowPositionProperty = RegisterProperty("LeftArrowPosition", typeof(double), 0.0);

        /// <summary>
        /// Whether or not default adorners are on.
        /// </summary>
        public bool IsDefaultAdornerVisible
        {
            get { return GetValue<bool>(IsDefaultAdornerVisibleProperty); }
            set
            {
                SetValue(IsDefaultAdornerVisibleProperty, value);
                RaisePropertyChanged("IsToggleGridAdornerVisible");
                RaisePropertyChanged("IsToggleDivisionAdornerVisible");
                RaisePropertyChanged("IsRotateAdornerVisible");
            }
        }

        public static readonly PropertyData IsDefaultAdornerVisibleProperty = RegisterProperty("IsDefaultAdornerVisible", typeof(bool), false);

        /// <summary>
        /// Whether or not adorner to create a division on right side of array is on.
        /// </summary>
        public bool IsLeftAdornerVisible
        {
            get { return GetValue<bool>(IsLeftAdornerVisibleProperty); }
            set { SetValue(IsLeftAdornerVisibleProperty, value); }
        }

        public static readonly PropertyData IsLeftAdornerVisibleProperty = RegisterProperty("IsLeftAdornerVisible", typeof(bool), false);

        /// <summary>
        /// Whether or not adorner to create a division on bottom side of array is on.
        /// </summary>
        public bool IsTopAdornerVisible
        {
            get { return GetValue<bool>(IsTopAdornerVisibleProperty); }
            set { SetValue(IsTopAdornerVisibleProperty, value); }
        }

        public static readonly PropertyData IsTopAdornerVisibleProperty = RegisterProperty("IsTopAdornerVisible", typeof(bool), false);

        /// <summary>
        /// Whether or not to show the adorner that allows students to toggle the grid lines on and off.
        /// </summary>
        public bool IsToggleGridAdornerVisible
        {
            get { return IsDefaultAdornerVisible && Rows < 51 && Columns < 51; }
        }

        public bool IsToggleDivisionAdornerVisible
        {
            get { return IsDefaultAdornerVisible && (PageObject as CLPArray).ArrayType == ArrayTypes.Array; }
        }

        public bool IsRotateAdornerVisible
        {
            get { return IsDefaultAdornerVisible; }
        }

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Gets the ResizePageObjectCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeArrayCommand { get; set; }

        private void OnResizeArrayCommandExecute(DragDeltaEventArgs e)
        {
            var clpArray = PageObject as CLPArray;
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

            const double MIN_ARRAY_SIZE = 16.875; //11.25;

            //Control Min Dimensions of Array.
            if(newArrayHeight < MIN_ARRAY_SIZE)
            {
                newArrayHeight = MIN_ARRAY_SIZE;
            }
            var newSquareSize = newArrayHeight / Rows;
            var newArrayWidth = newSquareSize * Columns;
            if(newArrayWidth < MIN_ARRAY_SIZE)
            {
                newArrayWidth = MIN_ARRAY_SIZE;
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
            if(newArrayWidth + 2 * clpArray.LabelLength + XPosition > clpArray.ParentPage.Width)
            {
                newArrayWidth = clpArray.ParentPage.Width - XPosition - 2 * clpArray.LabelLength;
                newSquareSize = newArrayWidth / Columns;
                //newArrayHeight = newSquareSize * Rows;
            }

            clpArray.SizeArrayToGridLevel(newSquareSize);

            ////Resize History
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
            //    (batch as CLPHistoryPageObjectResizeBatch).AddResizePointToBatch(PageObject.UniqueID, new Point(Width, Height));
            //}
            //else
            //{
            //    var batchHistoryItem = PageObject.ParentPage.PageHistory.EndBatch();
            //    ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            //    //TODO: log this error
            //}
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
            //var movementBatch = PageObject.ParentPage.PageHistory.CurrentHistoryBatch as CLPHistoryPageObjectMoveBatch;
            //if(movementBatch != null)
            //{
            //    movementBatch.AddPositionPointToBatch(PageObject.UniqueID, new Point(PageObject.XPosition, PageObject.YPosition));
            //}
            //var batchHistoryItem = PageObject.ParentPage.PageHistory.EndBatch();
            //ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
            PageObject.OnMoved();

            var snappingArray = PageObject as CLPArray;
            if(snappingArray == null)
            {
                return;
            }

            ACLPArrayBase closestPersistingArray = null;
            var closestSnappingDistance = Double.MaxValue;
            var snapType = SnapType.Top;
            foreach(IPageObject pageObject in PageObject.ParentPage.PageObjects)
            {
                var persistingArray = pageObject as ACLPArrayBase;
                if(persistingArray == null ||
                   persistingArray.ID == snappingArray.ID ||
                   !persistingArray.IsSnappable)
                {
                    continue;
                }

                var top = Math.Max(snappingArray.YPosition + snappingArray.LabelLength, persistingArray.YPosition + persistingArray.LabelLength);
                var bottom = Math.Min(snappingArray.YPosition + snappingArray.LabelLength + snappingArray.ArrayHeight,
                                      persistingArray.YPosition + persistingArray.LabelLength + persistingArray.ArrayHeight);
                var verticalIntersectionLength = bottom - top;
                var isVerticalIntersection = verticalIntersectionLength > persistingArray.ArrayHeight / 2 || verticalIntersectionLength > snappingArray.ArrayHeight / 2;

                var left = Math.Max(snappingArray.XPosition + snappingArray.LabelLength, persistingArray.XPosition + persistingArray.LabelLength);
                var right = Math.Min(snappingArray.XPosition + snappingArray.LabelLength + snappingArray.ArrayWidth,
                                     persistingArray.XPosition + persistingArray.LabelLength + persistingArray.ArrayWidth);
                var horizontalIntersectionLength = right - left;
                var isHorizontalIntersection = horizontalIntersectionLength > persistingArray.ArrayWidth / 2 || horizontalIntersectionLength > snappingArray.ArrayWidth / 2;

                //   Update Remainder Region
                //if(factorCard.IsRemainderRegionDisplayed && factorCard.CurrentRemainder == snappingArray.Columns * snappingArray.Rows)
                //{
                //    CLPFuzzyFactorCardRemainder remainderRegion = PageObject.ParentPage.GetPageObjectByUniqueID(factorCard.RemainderRegionUniqueID) as CLPFuzzyFactorCardRemainder;
                //    var currentIndex = PageObject.ParentPage.PageObjects.IndexOf(remainderRegion);
                //    ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryPageObjectRemove(PageObject.ParentPage, remainderRegion, currentIndex));
                //}

                //Fuzzy Factor Card array snapping in - for now this will override array snapping even if an array might be closer

                #region Snap to FFC

                if(pageObject is FuzzyFactorCard)
                {
                    var factorCard = pageObject as FuzzyFactorCard;
                    if(isVerticalIntersection)
                    {
                        var diff = Math.Abs(snappingArray.XPosition + snappingArray.LabelLength - (persistingArray.XPosition + persistingArray.LabelLength + factorCard.LastDivisionPosition));
                        if(diff < 50)
                        {
                            if(snappingArray.Rows != factorCard.Rows)
                            {
                                var hasTag = false;
                                if(snappingArray.Columns == factorCard.Rows)
                                {
                                    foreach(ATagBase tag in PageObject.ParentPage.Tags.ToList())
                                    {
                                        if(tag is FuzzyFactorCardFailedSnapTag && tag.Value == FuzzyFactorCardFailedSnapTag.AcceptedValues.SnappedWrongOrientationMultipleTimes.ToString())
                                        {
                                            hasTag = true;
                                            break;
                                        }
                                        if(tag is FuzzyFactorCardFailedSnapTag && tag.Value == FuzzyFactorCardFailedSnapTag.AcceptedValues.SnappedWrongOrientation.ToString())
                                        {
                                            tag.Value = FuzzyFactorCardFailedSnapTag.AcceptedValues.SnappedWrongOrientationMultipleTimes.ToString();
                                            hasTag = true;
                                            break;
                                        }
                                    }
                                    //Apply tag to note that the student tried to snap array in wrong orientation
                                    if(!hasTag)
                                    {
                                        var tag = new FuzzyFactorCardFailedSnapTag(PageObject.ParentPage, FuzzyFactorCardFailedSnapTag.AcceptedValues.SnappedWrongOrientation);
                                        PageObject.ParentPage.Tags.Add(tag);
                                    }
                                }
                                else
                                {
                                    foreach(ATagBase tag in PageObject.ParentPage.Tags.ToList())
                                    {
                                        if(tag is FuzzyFactorCardFailedSnapTag && tag.Value == FuzzyFactorCardFailedSnapTag.AcceptedValues.SnappedIncorrectDimensionMultipleTimes.ToString())
                                        {
                                            hasTag = true;
                                            break;
                                        }
                                        if(tag is FuzzyFactorCardFailedSnapTag && tag.Value == FuzzyFactorCardFailedSnapTag.AcceptedValues.SnappedIncorrectDimension.ToString())
                                        {
                                            tag.Value = FuzzyFactorCardFailedSnapTag.AcceptedValues.SnappedIncorrectDimensionMultipleTimes.ToString();
                                            hasTag = true;
                                            break;
                                        }
                                    }
                                    //Apply tag to note that the student tried to snap array in wrong orientation
                                    if(!hasTag)
                                    {
                                        var tag = new FuzzyFactorCardFailedSnapTag(PageObject.ParentPage, FuzzyFactorCardFailedSnapTag.AcceptedValues.SnappedIncorrectDimension);
                                        PageObject.ParentPage.Tags.Add(tag);
                                    }
                                }

                                var factorCardViewModels = CLPServiceAgent.Instance.GetViewModelsFromModel(factorCard);
                                foreach(IViewModel viewModel in factorCardViewModels)
                                {
                                    (viewModel as FuzzyFactorCardViewModel).RejectSnappedArray();
                                }
                                continue;
                            }
                            if(factorCard.CurrentRemainder < factorCard.Rows * snappingArray.Columns)
                            {
                                //TODO Liz - get old position - maybe from move batch? (Steve will email about this)
                                //var oldX = 10.0;
                                //var oldY = 10.0;
                                //APageObjectBaseViewModel.ChangePageObjectPosition(snappingArray, oldX, oldY, false);

                                var hasTag = false;
                                foreach(ATagBase tag in PageObject.ParentPage.Tags.ToList())
                                {
                                    if(tag is FuzzyFactorCardFailedSnapTag && tag.Value == FuzzyFactorCardFailedSnapTag.AcceptedValues.TooManyMultipleTimes.ToString())
                                    {
                                        hasTag = true;
                                        break;
                                    }
                                    if(tag is FuzzyFactorCardFailedSnapTag && tag.Value == FuzzyFactorCardFailedSnapTag.AcceptedValues.TooMany.ToString())
                                    {
                                        tag.Value = FuzzyFactorCardFailedSnapTag.AcceptedValues.TooManyMultipleTimes.ToString();
                                        hasTag = true;
                                        break;
                                    }
                                }
                                //Apply tag to note that the student tried to snap array in wrong orientation
                                if(!hasTag)
                                {
                                    var tag = new FuzzyFactorCardFailedSnapTag(PageObject.ParentPage, FuzzyFactorCardFailedSnapTag.AcceptedValues.TooMany);
                                    PageObject.ParentPage.Tags.Add(tag);
                                }

                                ////Apply tag to note that the student tried to snap too many
                                //if(!hasTag)
                                //{
                                //    var tag = new Tag(Tag.Origins.Generated, FuzzyFactorCardFailedSnapTagType.Instance);
                                //    tag.AddTagOptionValue(new TagOptionValue("too many"));
                                //    PageObject.ParentPage.PageTags.Add(tag);
                                //}

                                var factorCardViewModels = CLPServiceAgent.Instance.GetViewModelsFromModel(factorCard);
                                foreach(IViewModel viewModel in factorCardViewModels)
                                {
                                    (viewModel as FuzzyFactorCardViewModel).RejectSnappedArray();
                                }
                                continue;
                            }

                            //If first division - update IsGridOn to match new array
                            if(factorCard.LastDivisionPosition == 0)
                            {
                                factorCard.IsGridOn = snappingArray.IsGridOn;
                            }

                            //Add a new division and remove snapping array
                            PageObject.ParentPage.PageObjects.Remove(PageObject);
                            if(factorCard.IsHorizontallyAligned)
                            {
                                factorCard.SnapInArray(snappingArray.Columns);
                            }
                            else
                            {
                                factorCard.SnapInArray(snappingArray.Rows);
                            }

                            //ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryFFCArraySnappedIn(PageObject.ParentPage, pageObject.ID, snappingArray));
                            return;
                        }
                    }
                    continue;
                }

                #endregion //Snap to FFC

                if(isVerticalIntersection && snappingArray.Rows == persistingArray.Rows)
                {
                    var rightDiff = Math.Abs(snappingArray.XPosition + snappingArray.LabelLength - (persistingArray.XPosition + persistingArray.LabelLength + persistingArray.ArrayWidth));
                    if(rightDiff < 50)
                    {
                        if(closestPersistingArray == null ||
                           rightDiff < closestSnappingDistance)
                        {
                            closestPersistingArray = persistingArray;
                            closestSnappingDistance = rightDiff;
                            snapType = SnapType.Right;
                        }
                    }

                    var leftDiff = Math.Abs(snappingArray.XPosition + snappingArray.LabelLength + snappingArray.ArrayWidth - (persistingArray.XPosition + persistingArray.LabelLength));
                    if(leftDiff < 50)
                    {
                        if(closestPersistingArray == null ||
                           leftDiff < closestSnappingDistance)
                        {
                            closestPersistingArray = persistingArray;
                            closestSnappingDistance = leftDiff;
                            snapType = SnapType.Left;
                        }
                    }
                }

                if(isHorizontalIntersection && snappingArray.Columns == persistingArray.Columns)
                {
                    var bottomDiff = Math.Abs(snappingArray.YPosition + snappingArray.LabelLength - (persistingArray.YPosition + persistingArray.LabelLength + persistingArray.ArrayHeight));
                    if(bottomDiff < 50)
                    {
                        if(closestPersistingArray == null ||
                           bottomDiff < closestSnappingDistance)
                        {
                            closestPersistingArray = persistingArray;
                            closestSnappingDistance = bottomDiff;
                            snapType = SnapType.Bottom;
                        }
                    }

                    var topDiff = Math.Abs(snappingArray.YPosition + snappingArray.LabelLength + snappingArray.ArrayHeight - (persistingArray.YPosition + persistingArray.LabelLength));
                    if(topDiff < 50)
                    {
                        if(closestPersistingArray == null ||
                           topDiff < closestSnappingDistance)
                        {
                            closestPersistingArray = persistingArray;
                            closestSnappingDistance = topDiff;
                            snapType = SnapType.Top;
                        }
                    }
                }
            }

            if(closestPersistingArray == null)
            {
                return;
            }

            var squareSize = closestPersistingArray.GridSquareSize;
            ObservableCollection<CLPArrayDivision> tempDivisions;
            switch(snapType)
            {
                case SnapType.Top:
                    // ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryArraySnap(PageObject.ParentPage, closestPersistingArray, snappingArray, true));

                    closestPersistingArray.VerticalDivisions.Clear();

                    snappingArray.SizeArrayToGridLevel(squareSize);

                    if(closestPersistingArray.HorizontalDivisions.Any())
                    {
                        tempDivisions = new ObservableCollection<CLPArrayDivision>(closestPersistingArray.HorizontalDivisions);
                        closestPersistingArray.HorizontalDivisions.Clear();
                    }
                    else
                    {
                        tempDivisions = new ObservableCollection<CLPArrayDivision>
                                        {
                                            new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0, closestPersistingArray.ArrayHeight, closestPersistingArray.Rows)
                                        };
                    }

                    if(!snappingArray.HorizontalDivisions.Any())
                    {
                        closestPersistingArray.HorizontalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0, snappingArray.ArrayHeight, snappingArray.Rows));
                    }

                    foreach(CLPArrayDivision horizontalDivision in snappingArray.HorizontalDivisions)
                    {
                        closestPersistingArray.HorizontalDivisions.Add(horizontalDivision);
                    }
                    foreach(CLPArrayDivision horizontalDivision in tempDivisions)
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
                    //   ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryArraySnap(PageObject.ParentPage, closestPersistingArray, snappingArray, true));

                    closestPersistingArray.VerticalDivisions.Clear();

                    snappingArray.SizeArrayToGridLevel(squareSize);

                    if(!closestPersistingArray.HorizontalDivisions.Any())
                    {
                        closestPersistingArray.HorizontalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0, closestPersistingArray.ArrayHeight, closestPersistingArray.Rows));
                    }

                    if(!snappingArray.HorizontalDivisions.Any())
                    {
                        closestPersistingArray.HorizontalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Horizontal,
                                                                                            closestPersistingArray.ArrayHeight,
                                                                                            snappingArray.ArrayHeight,
                                                                                            snappingArray.Rows));
                    }
                    else
                    {
                        foreach(CLPArrayDivision horizontalDivision in snappingArray.HorizontalDivisions)
                        {
                            closestPersistingArray.HorizontalDivisions.Add(new CLPArrayDivision(horizontalDivision.Orientation,
                                                                                                horizontalDivision.Position + closestPersistingArray.ArrayHeight,
                                                                                                horizontalDivision.Length,
                                                                                                horizontalDivision.Value));
                        }
                    }

                    closestPersistingArray.Rows += snappingArray.Rows;
                    break;
                case SnapType.Left:
                    //   ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryArraySnap(PageObject.ParentPage, closestPersistingArray, snappingArray, false));

                    closestPersistingArray.HorizontalDivisions.Clear();

                    snappingArray.SizeArrayToGridLevel(squareSize);

                    if(closestPersistingArray.VerticalDivisions.Any())
                    {
                        tempDivisions = new ObservableCollection<CLPArrayDivision>(closestPersistingArray.VerticalDivisions);
                        closestPersistingArray.VerticalDivisions.Clear();
                    }
                    else
                    {
                        tempDivisions = new ObservableCollection<CLPArrayDivision>
                                        {
                                            new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, closestPersistingArray.ArrayWidth, closestPersistingArray.Columns)
                                        };
                    }

                    if(!snappingArray.VerticalDivisions.Any())
                    {
                        closestPersistingArray.VerticalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, snappingArray.ArrayWidth, snappingArray.Columns));
                    }

                    foreach(CLPArrayDivision verticalDivision in snappingArray.VerticalDivisions)
                    {
                        closestPersistingArray.VerticalDivisions.Add(verticalDivision);
                    }
                    foreach(CLPArrayDivision verticalDivision in tempDivisions)
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
                    //   ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryArraySnap(PageObject.ParentPage, closestPersistingArray, snappingArray, false));

                    closestPersistingArray.HorizontalDivisions.Clear();

                    snappingArray.SizeArrayToGridLevel(squareSize);

                    if(!closestPersistingArray.VerticalDivisions.Any())
                    {
                        closestPersistingArray.VerticalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, closestPersistingArray.ArrayWidth, closestPersistingArray.Columns));
                    }

                    if(!snappingArray.VerticalDivisions.Any())
                    {
                        closestPersistingArray.VerticalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Vertical,
                                                                                          closestPersistingArray.ArrayWidth,
                                                                                          snappingArray.ArrayWidth,
                                                                                          snappingArray.Columns));
                    }
                    else
                    {
                        foreach(CLPArrayDivision verticalDivision in snappingArray.VerticalDivisions)
                        {
                            closestPersistingArray.VerticalDivisions.Add(new CLPArrayDivision(verticalDivision.Orientation,
                                                                                              verticalDivision.Position + closestPersistingArray.ArrayWidth,
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

        /// <summary>
        /// Toggle the visibility of GridLines on the array.
        /// </summary>
        public Command ToggleGridCommand { get; private set; }

        private void OnToggleGridCommandExecute()
        {
            var clpArray = PageObject as CLPArray;
            if(clpArray != null)
            {
                clpArray.IsGridOn = !clpArray.IsGridOn;
                //ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryArrayGridToggle(PageObject.ParentPage, PageObject.UniqueID));
            }
        }

        /// <summary>
        /// Toggles the Division Behavior on and off.
        /// </summary>
        public Command ToggleDivisionAdornersCommand { get; private set; }

        private void OnToggleDivisionAdornersCommandExecute() { IsDivisionBehaviorOn = !IsDivisionBehaviorOn; }

        /// <summary>
        /// Rotates the array 90 degrees
        /// </summary>
        public Command RotateArrayCommand { get; private set; }

        protected void OnRotateArrayCommandExecute()
        {
            if((PageObject as CLPArray).ArrayHeight > PageObject.ParentPage.Width ||
               (PageObject as CLPArray).ArrayWidth > PageObject.ParentPage.Height)
            {
                return;
            }

            var initXPos = PageObject.XPosition;
            var initYPos = PageObject.YPosition;
            (PageObject as CLPArray).RotateArray();
            //ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryArrayRotate(PageObject.ParentPage, PageObject.UniqueID, initXPos, initYPos));
        }

        /// <summary>
        /// Gets the CreateHorizontalDivisionCommand command.
        /// </summary>
        public Command CreateHorizontalDivisionCommand { get; private set; }

        private void OnCreateHorizontalDivisionCommandExecute()
        {
            var position = LeftArrowPosition - 5;
            if(IsGridOn)
            {
                position = (PageObject as CLPArray).GetClosestGridLine(position);
            }

            if(HorizontalDivisions.Any(horizontalDivision => Math.Abs(horizontalDivision.Position - position) < 30.0))
            {
                return;
            }
            if(HorizontalDivisions.Count >= (PageObject as CLPArray).Rows)
            {
                MessageBox.Show("The number of divisions cannot be larger than the number of Rows.");
                return;
            }

            var divAbove = (PageObject as CLPArray).FindDivisionAbove(position, HorizontalDivisions);
            var divBelow = (PageObject as CLPArray).FindDivisionBelow(position, HorizontalDivisions);

            var addedDivisions = new List<CLPArrayDivision>();
            var removedDivisions = new List<CLPArrayDivision>();

            CLPArrayDivision topDiv;
            if(divAbove == null)
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

            //ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryArrayDivisionsChanged(PageObject.ParentPage, PageObject.UniqueID, addedDivisions, removedDivisions));
        }

        /// <summary>
        /// Gets the CreateVerticalDivisionCommand command.
        /// </summary>
        public Command CreateVerticalDivisionCommand { get; private set; }

        private void OnCreateVerticalDivisionCommandExecute()
        {
            var position = TopArrowPosition - 5;
            if(IsGridOn)
            {
                position = (PageObject as CLPArray).GetClosestGridLine(position);
            }

            if(VerticalDivisions.Any(verticalDivision => Math.Abs(verticalDivision.Position - position) < 30.0))
            {
                return;
            }
            if(VerticalDivisions.Count >= (PageObject as CLPArray).Columns)
            {
                MessageBox.Show("The number of divisions cannot be larger than the number of Columns.");
                return;
            }

            var divAbove = (PageObject as CLPArray).FindDivisionAbove(position, VerticalDivisions);
            var divBelow = (PageObject as CLPArray).FindDivisionBelow(position, VerticalDivisions);

            var addedDivisions = new List<CLPArrayDivision>();
            var removedDivisions = new List<CLPArrayDivision>();

            CLPArrayDivision topDiv;
            if(divAbove == null)
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

            //ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryArrayDivisionsChanged(PageObject.ParentPage, PageObject.UniqueID, addedDivisions, removedDivisions));
        }

        /// <summary>
        /// Gets the EditLabelCommand command.
        /// </summary>
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
            if(keyPad.DialogResult != true ||
               keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }

            var previousValue = division.Value;
            var isHorizontalDivision = division.Orientation == ArrayDivisionOrientation.Horizontal;

            division.Value = Int32.Parse(keyPad.NumbersEntered.Text);

            var array = PageObject as CLPArray;
            if(array == null)
            {
                return;
            }
            var divisionIndex = isHorizontalDivision ? array.HorizontalDivisions.IndexOf(division) : array.VerticalDivisions.IndexOf(division);

            //ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
            //                                           new CLPHistoryArrayDivisionValueChanged(PageObject.ParentPage, PageObject.UniqueID, isHorizontalDivision, divisionIndex, previousValue));

            // Check if array labels add up to larger array dimension
            if(division.Orientation == ArrayDivisionOrientation.Horizontal)
            {
                var total = 0;
                foreach(CLPArrayDivision div in HorizontalDivisions)
                {
                    if(div.Value == 0)
                    {
                        total = Rows;
                        break;
                    }
                    total += div.Value;
                }

                if(total == Rows)
                {
                    return;
                }
                var labelsString = "";
                if(HorizontalDivisions.Count == 2)
                {
                    labelsString = HorizontalDivisions.First().Value + " and " + HorizontalDivisions.Last().Value;
                }
                else
                {
                    for(var i = 0; i < HorizontalDivisions.Count; i++)
                    {
                        labelsString += HorizontalDivisions.ElementAt(i).Value.ToString();
                        if(i < HorizontalDivisions.Count - 1)
                        {
                            labelsString += ", ";
                        }
                        if(i == HorizontalDivisions.Count - 2)
                        {
                            labelsString += "and ";
                        }
                    }
                }
                MessageBox.Show(
                                "The side of the array is " + Rows + ". You broke the side into " + labelsString + " , which don’t add up to " + Rows + ".",
                                "Oops");
            }
            else
            {
                var total = 0;
                foreach(CLPArrayDivision div in VerticalDivisions)
                {
                    if(div.Value == 0)
                    {
                        total = Columns;
                        break;
                    }
                    total += div.Value;
                }

                if(total == Columns)
                {
                    return;
                }
                var labelsString = "";
                if(VerticalDivisions.Count == 2)
                {
                    labelsString = VerticalDivisions.First().Value + " and " + VerticalDivisions.Last().Value;
                }
                else
                {
                    for(var i = 0; i < VerticalDivisions.Count; i++)
                    {
                        labelsString += VerticalDivisions.ElementAt(i).Value.ToString();
                        if(i < VerticalDivisions.Count - 1)
                        {
                            labelsString += ", ";
                        }
                        if(i == VerticalDivisions.Count - 2)
                        {
                            labelsString += "and ";
                        }
                    }
                }
                MessageBox.Show(
                                "The side of the array is " + Columns + ". You broke the side into " + labelsString + " , which don’t add up to " + Columns + ".",
                                "Oops");
            }
        }

        /// <summary>
        /// Gets the EraseDivisionCommand command.
        /// </summary>
        public Command<MouseEventArgs> EraseDivisionCommand { get; private set; }

        private void OnEraseDivisionCommandExecute(MouseEventArgs e)
        {
            var rectangle = e.Source as Rectangle;
            if((e.StylusDevice == null || !e.StylusDevice.Inverted || e.LeftButton != MouseButtonState.Pressed) && e.MiddleButton != MouseButtonState.Pressed ||
               rectangle == null)
            {
                return;
            }

            var division = rectangle.DataContext as CLPArrayDivision;

            if(division == null ||
               division.Position == 0.0)
            {
                return;
            }

            var addedDivisions = new List<CLPArrayDivision>();
            var removedDivisions = new List<CLPArrayDivision>();
            if(division.Orientation == ArrayDivisionOrientation.Horizontal)
            {
                var divAbove = (PageObject as CLPArray).FindDivisionAbove(division.Position, (PageObject as CLPArray).HorizontalDivisions);
                (PageObject as CLPArray).HorizontalDivisions.Remove(divAbove);
                (PageObject as CLPArray).HorizontalDivisions.Remove(division);
                removedDivisions.Add(divAbove);
                removedDivisions.Add(division);

                //Add new division unless we removed the only division line
                if((PageObject as CLPArray).HorizontalDivisions.Count > 0)
                {
                    var newLength = divAbove.Length + division.Length;
                    var newDivision = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, divAbove.Position, newLength, 0);
                    (PageObject as CLPArray).HorizontalDivisions.Add(newDivision);
                    addedDivisions.Add(newDivision);
                }
            }
            if(division.Orientation == ArrayDivisionOrientation.Vertical)
            {
                var divAbove = (PageObject as CLPArray).FindDivisionAbove(division.Position, (PageObject as CLPArray).VerticalDivisions);
                (PageObject as CLPArray).VerticalDivisions.Remove(divAbove);
                (PageObject as CLPArray).VerticalDivisions.Remove(division);
                removedDivisions.Add(divAbove);
                removedDivisions.Add(division);

                //Add new division unless we removed the only division line
                if((PageObject as CLPArray).VerticalDivisions.Count > 0)
                {
                    var newLength = divAbove.Length + division.Length;
                    var newDivision = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, newLength, 0);
                    (PageObject as CLPArray).VerticalDivisions.Add(newDivision);
                    addedDivisions.Add(newDivision);
                }
            }

            //ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryArrayDivisionsChanged(PageObject.ParentPage, PageObject.UniqueID, addedDivisions, removedDivisions));
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
            var tempAdornerState = IsDefaultAdornerVisible;
            ACLPPageBaseViewModel.ClearAdorners(PageObject.ParentPage);
            IsAdornerVisible = !tempAdornerState;
            IsDefaultAdornerVisible = !tempAdornerState;
            IsTopAdornerVisible = tempAdornerState;
            IsLeftAdornerVisible = tempAdornerState;
        }

        /// <summary>
        /// Brings up a menu to make multiple copies of an array
        /// </summary>
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
            if(keyPad.DialogResult != true ||
               keyPad.NumbersEntered.Text.Length <= 0)
            {
                return;
            }
            var numberOfArrays = Int32.Parse(keyPad.NumbersEntered.Text);

            var xPosition = 10.0;
            var yPosition = 160.0;
            if(YPosition + 2 * Height + 10.0 < PageObject.ParentPage.Height)
            {
                yPosition = YPosition + Height + 10.0;
            }
            else if(XPosition + 2 * Width + 10.0 < PageObject.ParentPage.Width)
            {
                yPosition = YPosition;
                xPosition = XPosition + Width + 10.0;
            }
            const double LABEL_LENGTH = 22.0;

            var arraysToAdd = new List<CLPArray>();
            foreach(int index in Enumerable.Range(1, numberOfArrays))
            {
                var array = PageObject.Duplicate() as CLPArray;
                array.XPosition = xPosition;
                array.YPosition = yPosition;

                if(xPosition + 2 * (ArrayWidth + LABEL_LENGTH) <= PageObject.ParentPage.Width)
                {
                    xPosition += ArrayWidth + LABEL_LENGTH;
                }
                    //If there isn't room, diagonally pile the rest
                else if((xPosition + ArrayWidth + LABEL_LENGTH + 20.0 <= PageObject.ParentPage.Width) &&
                        (yPosition + ArrayHeight + LABEL_LENGTH + 20.0 <= PageObject.ParentPage.Height))
                {
                    xPosition += 20.0;
                    yPosition += 20.0;
                }
                arraysToAdd.Add(array);
            }

            if(arraysToAdd.Count == 1)
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

        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown) { return false; }

        public override void EraserHitTest(string hitBoxName, object tag)
        {
            if(IsBackground && !App.MainWindowViewModel.IsAuthoring)
            {
                //don't erase
            }
            else if(hitBoxName == "DivisionHitBox")
            {
                var division = tag as CLPArrayDivision;
                if(division.Position != 0.0) //don't delete first division
                {
                    if(division.Orientation == ArrayDivisionOrientation.Horizontal)
                    {
                        var divAbove = (PageObject as CLPArray).FindDivisionAbove(division.Position, (PageObject as CLPArray).HorizontalDivisions);
                        (PageObject as CLPArray).HorizontalDivisions.Remove(divAbove);
                        (PageObject as CLPArray).HorizontalDivisions.Remove(division);

                        //Add new division unless we removed the only division line
                        if((PageObject as CLPArray).HorizontalDivisions.Count > 0)
                        {
                            var newLength = divAbove.Length + division.Length;
                            var newDivision = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, divAbove.Position, newLength, 0);
                            (PageObject as CLPArray).HorizontalDivisions.Add(newDivision);
                        }
                    }
                    if(division.Orientation == ArrayDivisionOrientation.Vertical)
                    {
                        var divAbove = (PageObject as CLPArray).FindDivisionAbove(division.Position, (PageObject as CLPArray).VerticalDivisions);
                        (PageObject as CLPArray).VerticalDivisions.Remove(divAbove);
                        (PageObject as CLPArray).VerticalDivisions.Remove(division);

                        //Add new division unless we removed the only division line
                        if((PageObject as CLPArray).VerticalDivisions.Count > 0)
                        {
                            var newLength = divAbove.Length + division.Length;
                            var newDivision = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, newLength, 0);
                            (PageObject as CLPArray).VerticalDivisions.Add(newDivision);
                        }
                    }
                }
            }
        }

        public override void ClearAdorners()
        {
            IsAdornerVisible = false;
            IsDefaultAdornerVisible = false;
            IsTopAdornerVisible = false;
            IsLeftAdornerVisible = false;
        }

        #endregion //Methods
    }
}