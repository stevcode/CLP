using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Shapes;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.Views;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.CustomControls;
using CLP.Entities;
using Org.BouncyCastle.Cms;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>UserControl view model.</summary>
    public class CLPArrayViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        protected IPageInteractionService PageInteractionService;

        /// <summary>Initializes a new instance of the <see cref="CLPArrayViewModel" /> class.</summary>
        public CLPArrayViewModel(CLPArray array)
        {
            PageObject = array;
            PageInteractionService = DependencyResolver.Resolve<IPageInteractionService>();

            //Commands
            ResizeArrayCommand = new Command<DragDeltaEventArgs>(OnResizeArrayCommandExecute);
            SnapArrayCommand = new Command(OnSnapArrayCommandExecute);
            RotateArrayCommand = new Command(OnRotateArrayCommandExecute);
            EditLabelCommand = new Command<CLPArrayDivision>(OnEditLabelCommandExecute);
            EraseDivisionCommand = new Command<MouseEventArgs>(OnEraseDivisionCommandExecute);
            DuplicateArrayCommand = new Command(OnDuplicateArrayCommandExecute);
            InitializeButtons();
        }

        #endregion //Constructor

        #region Buttons

        private ToggleRibbonButton _toggleLabelsButton;
        private ToggleRibbonButton _toggleObscureColumnsButton;
        private ToggleRibbonButton _toggleObscureRowsButton;
        private ToggleRibbonButton _toggleGridLinesButton;

        private void InitializeButtons()
        {
            var array = PageObject as CLPArray;
            if (array == null)
            {
                return;
            }

            _contextButtons.Add(MajorRibbonViewModel.Separater);

            _contextButtons.Add(new RibbonButton("Make Copies", "pack://application:,,,/Images/AddToDisplay.png", DuplicateArrayCommand, null, true));

            _contextButtons.Add(MajorRibbonViewModel.Separater);

            _contextButtons.Add(new RibbonButton("Rotate", "pack://application:,,,/Resources/Images/AdornerImages/ArrayRotate64.png", RotateArrayCommand, null, true));

            if (array.ArrayType == ArrayTypes.Array)
            {
                _toggleLabelsButton = new ToggleRibbonButton("Show Labels", "Hide Labels", "pack://application:,,,/Resources/Images/ArrayCard32.png", true)
                {
                    IsChecked = IsTopLabelVisible && IsSideLabelVisible
                };
                _toggleLabelsButton.Checked += toggleLabelsButton_Checked;
                _toggleLabelsButton.Unchecked += toggleLabelsButton_Checked;
                _contextButtons.Add(_toggleLabelsButton);
            }

            if (array.ArrayType == ArrayTypes.ObscurableArray)
            {
                _toggleObscureColumnsButton = new ToggleRibbonButton("Show Columns", "Hide Columns", "pack://application:,,,/Resources/Images/ArrayCard32.png", true)
                {
                    IsChecked = !IsColumnsObscured
                };

                _toggleObscureColumnsButton.Checked += toggleObscureColumnsButton_Checked;
                _toggleObscureColumnsButton.Unchecked += toggleObscureColumnsButton_Checked;
                _toggleObscureColumnsButton.IsEnabled = !IsRowsObscured;
                _contextButtons.Add(_toggleObscureColumnsButton);

                _toggleObscureRowsButton = new ToggleRibbonButton("Show Rows", "Hide Rows", "pack://application:,,,/Resources/Images/ArrayCard32.png", true)
                {
                    IsChecked = !IsRowsObscured
                };
                _toggleObscureRowsButton.Checked += toggleObscureRowsButton_Checked;
                _toggleObscureRowsButton.Unchecked += toggleObscureRowsButton_Checked;
                _toggleObscureRowsButton.IsEnabled = !IsColumnsObscured;
                _contextButtons.Add(_toggleObscureRowsButton);
            }

            _toggleGridLinesButton = new ToggleRibbonButton("Show Grid Lines", "Hide Grid Lines", "pack://application:,,,/Resources/Images/ArrayCard32.png", true)
                                     {
                                         IsChecked = IsGridOn
                                     };
            _toggleGridLinesButton.Checked += toggleGridLinesButton_Checked;
            _toggleGridLinesButton.Unchecked += toggleGridLinesButton_Checked;
            _contextButtons.Add(_toggleGridLinesButton);

            _contextButtons.Add(new RibbonButton("Snap", "pack://application:,,,/Resources/Images/AdornerImages/ArraySnap64.png", SnapArrayCommand, null, true));
            //    _contextButtons.Add(new RibbonButton("Size to Other Arrays", "pack://application:,,,/Resources/Images/AdornerImages/ArraySnap64.png", null, null, true));
        }

        private void toggleLabelsButton_Checked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleRibbonButton;
            if (toggleButton == null ||
                toggleButton.IsChecked == null)
            {
                return;
            }

            var array = PageObject as CLPArray;
            if (array == null)
            {
                return;
            }

            array.IsTopLabelVisible = (bool)toggleButton.IsChecked && !array.IsColumnsObscured;
            array.IsSideLabelVisible = (bool)toggleButton.IsChecked && !array.IsRowsObscured;
        }

        private void toggleObscureColumnsButton_Checked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleRibbonButton;
            if (toggleButton == null ||
                toggleButton.IsChecked == null)
            {
                return;
            }

            var array = PageObject as CLPArray;
            if (array == null)
            {
                return;
            }

            var oldRegions = array.VerticalDivisions.ToList();

            if (array.IsColumnsObscured)
            {
                array.Unobscure(true);
            }
            else
            {
                array.Obscure(true);
            }

            var newRegions = array.VerticalDivisions.ToList();
            ACLPPageBaseViewModel.AddHistoryItemToPage(array.ParentPage,
                                                       new CLPArrayDivisionsChangedHistoryItem(array.ParentPage,
                                                                                               App.MainWindowViewModel.CurrentUser,
                                                                                               array.ID,
                                                                                               oldRegions,
                                                                                               newRegions));

            IsTopLabelVisible = !array.IsColumnsObscured;
            _toggleObscureRowsButton.IsEnabled = !array.IsColumnsObscured;
        }

        private void toggleObscureRowsButton_Checked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleRibbonButton;
            if (toggleButton == null ||
                toggleButton.IsChecked == null)
            {
                return;
            }

            var array = PageObject as CLPArray;
            if (array == null)
            {
                return;
            }

            var oldRegions = array.HorizontalDivisions.ToList();

            if (array.IsRowsObscured)
            {
                array.Unobscure(false);
            }
            else
            {
                array.Obscure(false);
            }

            var newRegions = array.HorizontalDivisions.ToList();
            ACLPPageBaseViewModel.AddHistoryItemToPage(array.ParentPage,
                                                       new CLPArrayDivisionsChangedHistoryItem(array.ParentPage,
                                                                                               App.MainWindowViewModel.CurrentUser,
                                                                                               array.ID,
                                                                                               oldRegions,
                                                                                               newRegions));

            IsSideLabelVisible = !array.IsRowsObscured;
            _toggleObscureColumnsButton.IsEnabled = !array.IsRowsObscured;
        }

        private void toggleGridLinesButton_Checked(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleRibbonButton;
            if (toggleButton == null ||
                toggleButton.IsChecked == null)
            {
                return;
            }

            var clpArray = PageObject as CLPArray;
            if (clpArray == null)
            {
                return;
            }

            clpArray.IsGridOn = (bool)toggleButton.IsChecked;
            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                                                       new CLPArrayGridToggleHistoryItem(PageObject.ParentPage,
                                                                                         App.MainWindowViewModel.CurrentUser,
                                                                                         PageObject.ID,
                                                                                         clpArray.IsGridOn));
        }

        #endregion //Buttons

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

        public static readonly PropertyData HorizontalDivisionsProperty = RegisterProperty("HorizontalDivisions", typeof (ObservableCollection<CLPArrayDivision>));

        /// <summary>Gets or sets the VerticalDivisions value.</summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<CLPArrayDivision> VerticalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(VerticalDivisionsProperty); }
            set { SetValue(VerticalDivisionsProperty, value); }
        }

        public static readonly PropertyData VerticalDivisionsProperty = RegisterProperty("VerticalDivisions", typeof (ObservableCollection<CLPArrayDivision>));

        /// <summary>Toggles visibility of Columns obscurer.</summary>
        [ViewModelToModel("PageObject")]
        public bool IsColumnsObscured
        {
            get { return GetValue<bool>(IsColumnsObscuredProperty); }
            set { SetValue(IsColumnsObscuredProperty, value); }
        }

        public static readonly PropertyData IsColumnsObscuredProperty = RegisterProperty("IsColumnsObscured", typeof (bool));

        /// <summary>Toggles visibity of Row obscurer.</summary>
        [ViewModelToModel("PageObject")]
        public bool IsRowsObscured
        {
            get { return GetValue<bool>(IsRowsObscuredProperty); }
            set { SetValue(IsRowsObscuredProperty, value); }
        }

        public static readonly PropertyData IsRowsObscuredProperty = RegisterProperty("IsRowsObscured", typeof (bool));

        /// <summary>The type of array.</summary>
        [ViewModelToModel("PageObject")]
        public ArrayTypes ArrayType
        {
            get { return GetValue<ArrayTypes>(ArrayTypeProperty); }
            set { SetValue(ArrayTypeProperty, value); }
        }

        public static readonly PropertyData ArrayTypeProperty = RegisterProperty("ArrayType", typeof (ArrayTypes));


        #endregion //Model

        #region Bindings

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

        /// <summary>Snaps the array to an adjacent array.</summary>
        public Command SnapArrayCommand { get; private set; }

        private enum SnapType
        {
            Top,
            Bottom,
            Left,
            Right
        }

        private void OnSnapArrayCommandExecute()
        {
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
                                var existingFactorPairErrorsTag =
                                    divisionTemplate.ParentPage.Tags.OfType<DivisionTemplateFactorPairErrorsTag>().FirstOrDefault(x => x.DivisionTemplateID == divisionTemplate.ID);
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
                                var existingRemainderErrorsTag =
                                    divisionTemplate.ParentPage.Tags.OfType<DivisionTemplateRemainderErrorsTag>().FirstOrDefault(x => x.DivisionTemplateID == divisionTemplate.ID);
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
                                var factorCardViewModels = divisionTemplate.GetAllViewModels();
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
                            ContextRibbon.Buttons.Clear();
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

                if (isVerticalIntersection && snappingArray.Rows == persistingArray.Rows &&
                    snappingArray.IsRowsObscured == persistingArray.IsRowsObscured)
                {
                    var rightDiff =
                        Math.Abs(snappingArray.XPosition + snappingArray.LabelLength - (persistingArray.XPosition + persistingArray.LabelLength + persistingArray.ArrayWidth));
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
                        Math.Abs(snappingArray.XPosition + snappingArray.LabelLength + snappingArray.ArrayWidth - (persistingArray.XPosition + persistingArray.LabelLength));
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

                if (isHorizontalIntersection && snappingArray.Columns == persistingArray.Columns &&
                    snappingArray.IsColumnsObscured == persistingArray.IsColumnsObscured)
                {
                    var bottomDiff =
                        Math.Abs(snappingArray.YPosition + snappingArray.LabelLength - (persistingArray.YPosition + persistingArray.LabelLength + persistingArray.ArrayHeight));
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
                        Math.Abs(snappingArray.YPosition + snappingArray.LabelLength + snappingArray.ArrayHeight - (persistingArray.YPosition + persistingArray.LabelLength));
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
                    arraySnapHistoryItem = new CLPArraySnapHistoryItem(PageObject.ParentPage, App.MainWindowViewModel.CurrentUser, closestPersistingArray, snappingArray, true);

                    if (!closestPersistingArray.IsColumnsObscured)
                    {
                        closestPersistingArray.VerticalDivisions.Clear();
                    }

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
                                            new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0, closestPersistingArray.ArrayHeight, closestPersistingArray.Rows)
                                        };
                    }

                    if (!snappingArray.HorizontalDivisions.Any())
                    {
                        closestPersistingArray.HorizontalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0, snappingArray.ArrayHeight, snappingArray.Rows));
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
                                                                                            horizontalDivision.Value,
                                                                                            horizontalDivision.IsObscured));
                    }

                    closestPersistingArray.Rows += snappingArray.Rows;
                    closestPersistingArray.YPosition -= snappingArray.ArrayHeight;
                    break;
                case SnapType.Bottom:
                    arraySnapHistoryItem = new CLPArraySnapHistoryItem(PageObject.ParentPage, App.MainWindowViewModel.CurrentUser, closestPersistingArray, snappingArray, true);

                    if (!closestPersistingArray.IsColumnsObscured)
                    {
                        closestPersistingArray.VerticalDivisions.Clear();
                    }

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
                                                                                                horizontalDivision.Position + closestPersistingArray.ArrayHeight,
                                                                                                horizontalDivision.Length,
                                                                                                horizontalDivision.Value,
                                                                                                horizontalDivision.IsObscured));
                        }
                    }

                    closestPersistingArray.Rows += snappingArray.Rows;
                    break;
                case SnapType.Left:
                    arraySnapHistoryItem = new CLPArraySnapHistoryItem(PageObject.ParentPage, App.MainWindowViewModel.CurrentUser, closestPersistingArray, snappingArray, false);

                    if (!closestPersistingArray.IsRowsObscured)
                    {
                        closestPersistingArray.HorizontalDivisions.Clear();
                    }

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
                                            new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, closestPersistingArray.ArrayWidth, closestPersistingArray.Columns)
                                        };
                    }

                    if (!snappingArray.VerticalDivisions.Any())
                    {
                        closestPersistingArray.VerticalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, snappingArray.ArrayWidth, snappingArray.Columns));
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
                                                                                          verticalDivision.Value,
                                                                                          verticalDivision.IsObscured));
                    }

                    closestPersistingArray.Columns += snappingArray.Columns;
                    closestPersistingArray.XPosition -= snappingArray.ArrayWidth;
                    break;
                case SnapType.Right:
                    arraySnapHistoryItem = new CLPArraySnapHistoryItem(PageObject.ParentPage, App.MainWindowViewModel.CurrentUser, closestPersistingArray, snappingArray, false);

                    if (!closestPersistingArray.IsRowsObscured)
                    {
                        closestPersistingArray.HorizontalDivisions.Clear();
                    }

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
                                                                                              verticalDivision.Position + closestPersistingArray.ArrayWidth,
                                                                                              verticalDivision.Length,
                                                                                              verticalDivision.Value,
                                                                                              verticalDivision.IsObscured));
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
            ContextRibbon.Buttons.Clear();
            var snappedArray = PageObject as CLPArray;
            var array = closestPersistingArray as CLPArray;
            if (snappedArray != null &&
                array != null)
            {
                var strokesToRestore = new StrokeCollection();

                foreach (var stroke in snappedArray.AcceptedStrokes.Where(stroke => PageObject.ParentPage.History.TrashedInkStrokes.Contains(stroke)))
                {
                    strokesToRestore.Add(stroke);
                }

                PageObject.ParentPage.InkStrokes.Add(strokesToRestore);
                PageObject.ParentPage.History.TrashedInkStrokes.Remove(strokesToRestore);
                array.ChangeAcceptedStrokes(strokesToRestore, new List<Stroke>());
            }

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

        /// <summary>Rotates the array 90 degrees</summary>
        public Command RotateArrayCommand { get; private set; }

        protected void OnRotateArrayCommandExecute()
        {
            var array = PageObject as CLPArray;
            if (array == null)
            {
                return;
            }
            if (array.ArrayHeight > array.ParentPage.Width ||
                array.ArrayWidth > array.ParentPage.Height)
            {
                return;
            }

            var initXPos = array.XPosition;
            var initYPos = array.YPosition;
            var oldWidth = array.Width;
            var oldHeight = array.Height;
            array.RotateArray();

            if (array.ArrayType == ArrayTypes.ObscurableArray)
            {
                _toggleObscureColumnsButton.Checked -= toggleObscureColumnsButton_Checked;
                _toggleObscureColumnsButton.Unchecked -= toggleObscureColumnsButton_Checked;
                _toggleObscureRowsButton.Checked -= toggleObscureRowsButton_Checked;
                _toggleObscureRowsButton.Unchecked -= toggleObscureRowsButton_Checked;
                _toggleObscureColumnsButton.IsEnabled = !array.IsRowsObscured;
                _toggleObscureColumnsButton.IsChecked = !array.IsColumnsObscured;

                _toggleObscureRowsButton.IsEnabled = !array.IsColumnsObscured;
                _toggleObscureRowsButton.IsChecked = !array.IsRowsObscured;
                _toggleObscureColumnsButton.Checked += toggleObscureColumnsButton_Checked;
                _toggleObscureColumnsButton.Unchecked += toggleObscureColumnsButton_Checked;
                _toggleObscureRowsButton.Checked += toggleObscureRowsButton_Checked;
                _toggleObscureRowsButton.Unchecked += toggleObscureRowsButton_Checked;
            }

            ACLPPageBaseViewModel.AddHistoryItemToPage(array.ParentPage,
                                                       new CLPArrayRotateHistoryItem(array.ParentPage,
                                                                                     App.MainWindowViewModel.CurrentUser,
                                                                                     array.ID,
                                                                                     initXPos,
                                                                                     initYPos,
                                                                                     array.XPosition,
                                                                                     array.YPosition,
                                                                                     oldWidth,
                                                                                     oldHeight,
                                                                                     array.Columns,
                                                                                     array.Rows));

            if (array.CanAcceptStrokes)
            {
                array.AcceptedStrokes.RotateAll(90, XPosition, YPosition, Width, 0);
            }

            var divisionTemplateIDsInHistory = DivisionTemplateAnalysis.GetListOfDivisionTemplateIDsInHistory(array.ParentPage);
            foreach (var divisionTemplate in array.ParentPage.PageObjects.OfType<FuzzyFactorCard>())
            {
                // Only increase OrientationChanged attempt if Division Template already full.
                if (divisionTemplate.CurrentRemainder != divisionTemplate.Dividend % divisionTemplate.Rows)
                {
                    var existingFactorPairErrorsTag =
                        array.ParentPage.Tags.OfType<DivisionTemplateRemainderErrorsTag>().FirstOrDefault(x => x.DivisionTemplateID == divisionTemplate.ID);

                    if (existingFactorPairErrorsTag == null)
                    {
                        existingFactorPairErrorsTag = new DivisionTemplateRemainderErrorsTag(array.ParentPage,
                                                                                             Origin.StudentPageGenerated,
                                                                                             divisionTemplate.ID,
                                                                                             divisionTemplate.Dividend,
                                                                                             divisionTemplate.Rows,
                                                                                             divisionTemplateIDsInHistory.IndexOf(divisionTemplate.ID));
                        array.ParentPage.AddTag(existingFactorPairErrorsTag);
                    }
                    existingFactorPairErrorsTag.OrientationChangedDimensions.Add(string.Format("{0}x{1}", Rows, Columns));
                }
                else
                {
                    var existingRemainderErrorsTag =
                        array.ParentPage.Tags.OfType<DivisionTemplateRemainderErrorsTag>().FirstOrDefault(x => x.DivisionTemplateID == divisionTemplate.ID);

                    if (existingRemainderErrorsTag == null)
                    {
                        existingRemainderErrorsTag = new DivisionTemplateRemainderErrorsTag(array.ParentPage,
                                                                                            Origin.StudentPageGenerated,
                                                                                            divisionTemplate.ID,
                                                                                            divisionTemplate.Dividend,
                                                                                            divisionTemplate.Rows,
                                                                                            divisionTemplateIDsInHistory.IndexOf(divisionTemplate.ID));
                        array.ParentPage.AddTag(existingRemainderErrorsTag);
                    }
                    existingRemainderErrorsTag.OrientationChangedDimensions.Add(string.Format("{0}x{1}", Rows, Columns));
                }
            }
        }

        /// <summary>Gets the EditLabelCommand command.</summary>
        public Command<CLPArrayDivision> EditLabelCommand { get; private set; }

        private void OnEditLabelCommandExecute(CLPArrayDivision division)
        {
            // Pop up numberpad and save result as value of division
            var keyPad = new KeypadWindowView
                         {
                             Owner = Application.Current.MainWindow,
                             WindowStartupLocation = WindowStartupLocation.Manual
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
                                                                                                   previousValue,
                                                                                                   division.Value));

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

                //PageObject.ParentPage.AddTag(new ArrayTriedWrongDividerValuesTag(PageObject.ParentPage,
                //                                                                 Origin.StudentPageObjectGenerated,
                //                                                                 PageObject.ID,
                //                                                                 Rows,
                //                                                                 Columns,
                //                                                                 DividerValuesOrientation.Vertical,
                //                                                                 dividerValues));
                MessageBox.Show("The side of the array is " + Rows + ". You broke the side into " + labelsString + ", which don’t add up to " + Rows + ".", "Oops");
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

                //PageObject.ParentPage.AddTag(new ArrayTriedWrongDividerValuesTag(PageObject.ParentPage,
                //                                                                 Origin.StudentPageObjectGenerated,
                //                                                                 PageObject.ID,
                //                                                                 Rows,
                //                                                                 Columns,
                //                                                                 DividerValuesOrientation.Horizontal,
                //                                                                 dividerValues));
                MessageBox.Show("The side of the array is " + Columns + ". You broke the side into " + labelsString + ", which don’t add up to " + Columns + ".", "Oops");
            }
        }

        /// <summary>Gets the EraseDivisionCommand command.</summary>
        public Command<MouseEventArgs> EraseDivisionCommand { get; private set; }

        private void OnEraseDivisionCommandExecute(MouseEventArgs e)
        {
            var rectangle = e.Source as Rectangle;
            var array = PageObject as ACLPArrayBase;
            if (rectangle == null ||
                array == null ||
                PageInteractionService == null ||
                PageInteractionService.CurrentPageInteractionMode != PageInteractionModes.Erase ||
                PageInteractionService.CurrentErasingMode != ErasingModes.Dividers)
            {
                return;
            }

            var division = rectangle.DataContext as CLPArrayDivision;

            if (division == null ||
                division.Position == 0.0)
            {
                return;
            }

            var oldRegions = new List<CLPArrayDivision>();
            var newRegions = new List<CLPArrayDivision>();

            if (division.Orientation == ArrayDivisionOrientation.Horizontal)
            {
                oldRegions = array.HorizontalDivisions.ToList();
                var divAbove = array.FindDivisionAbove(division.Position, array.HorizontalDivisions);
                array.HorizontalDivisions.Remove(divAbove);
                array.HorizontalDivisions.Remove(division);

                var isOsbscuredDividerRegionRemoved = divAbove.IsObscured || division.IsObscured;

                //Add new division unless we removed the only division line
                if (array.HorizontalDivisions.Count > 0 ||
                    isOsbscuredDividerRegionRemoved)
                {
                    var newLength = divAbove.Length + division.Length;
                    var newDivision = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, divAbove.Position, newLength, 0);
                    if (isOsbscuredDividerRegionRemoved)
                    {
                        newDivision.IsObscured = true;
                    }
                    array.HorizontalDivisions.Add(newDivision);
                }

                newRegions = array.HorizontalDivisions.ToList();
            }
            if (division.Orientation == ArrayDivisionOrientation.Vertical)
            {
                oldRegions = array.VerticalDivisions.ToList();
                var divAbove = array.FindDivisionAbove(division.Position, array.VerticalDivisions);
                array.VerticalDivisions.Remove(divAbove);
                array.VerticalDivisions.Remove(division);

                var isOsbscuredDividerRegionRemoved = divAbove.IsObscured || division.IsObscured;

                //Add new division unless we removed the only division line
                if (array.VerticalDivisions.Count > 0 ||
                    isOsbscuredDividerRegionRemoved)
                {
                    var newLength = divAbove.Length + division.Length;
                    var newDivision = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, newLength, 0);
                    if (isOsbscuredDividerRegionRemoved)
                    {
                        newDivision.IsObscured = true;
                    }
                    array.VerticalDivisions.Add(newDivision);
                }

                newRegions = array.VerticalDivisions.ToList();
            }

            ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage,
                                                       new CLPArrayDivisionsChangedHistoryItem(PageObject.ParentPage,
                                                                                               App.MainWindowViewModel.CurrentUser,
                                                                                               PageObject.ID,
                                                                                               oldRegions,
                                                                                               newRegions));
        }

        /// <summary>Brings up a menu to make multiple copies of an array</summary>
        public Command DuplicateArrayCommand { get; private set; }

        private void OnDuplicateArrayCommandExecute()
        {
            var keyPad = new KeypadWindowView("How many copies?", 21)
                         {
                             Owner = Application.Current.MainWindow,
                             WindowStartupLocation = WindowStartupLocation.Manual
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

        #region Static Methods

        public static bool InteractWithAcceptedStrokes(CLPArray array, IEnumerable<Stroke> addedStrokes, IEnumerable<Stroke> removedStrokes, bool canInteract)
        {
            //return false; //HACK: skip this new implementation for history conversions. Remove after generating new cache.

            if (array == null ||
                !canInteract)
            {
                return false;
            }

            var removedStrokesList = removedStrokes as IList<Stroke> ?? removedStrokes.ToList();
            var addedStrokesList = addedStrokes as IList<Stroke> ?? addedStrokes.ToList();

            if (removedStrokesList.Any() ||
                addedStrokesList.Count() != 1)
            {
                return false;
            }

            var potentialDividingStroke = addedStrokesList.FirstOrDefault();
            return CreateDivision(array, potentialDividingStroke);
        }

        public static bool CreateDivision(CLPArray array, Stroke dividingStroke)
        {
            var pageInteractionService = Catel.IoC.ServiceLocator.Default.ResolveType<IPageInteractionService>();
            if (array == null ||
                dividingStroke == null ||
                array.ArrayType != ArrayTypes.Array ||
                pageInteractionService == null ||
                pageInteractionService.CurrentPageInteractionMode != PageInteractionModes.DividerCreation)
            {
                return false;
            }

            var strokeTop = dividingStroke.GetBounds().Top;
            var strokeBottom = dividingStroke.GetBounds().Bottom;
            var strokeLeft = dividingStroke.GetBounds().Left;
            var strokeRight = dividingStroke.GetBounds().Right;

            var cuttableTop = array.YPosition + array.LabelLength;
            var cuttableBottom = cuttableTop + array.ArrayHeight;
            var cuttableLeft = array.XPosition + array.LabelLength;
            var cuttableRight = cuttableLeft + array.ArrayWidth;

            const double MIN_THRESHHOLD = 30.0;

            List<CLPArrayDivision> oldRegions;
            List<CLPArrayDivision> newRegions;

            if (Math.Abs(strokeLeft - strokeRight) < Math.Abs(strokeTop - strokeBottom) &&
                strokeRight <= cuttableRight &&
                strokeLeft >= cuttableLeft &&
                strokeTop - cuttableTop <= MIN_THRESHHOLD &&
                cuttableBottom - strokeBottom <= MIN_THRESHHOLD &&
                array.Columns > 1 &&
                !array.IsColumnsObscured) //Vertical Stroke. Stroke must be within the bounds of the pageObject
            {
                oldRegions = array.VerticalDivisions.ToList();
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

                CLPArrayDivision topDiv;
                if (divAbove == null)
                {
                    topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, position, 0);
                }
                else
                {
                    topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, position - divAbove.Position, 0);
                    array.VerticalDivisions.Remove(divAbove);
                }
                array.VerticalDivisions.Add(topDiv);

                var bottomDiv = divBelow == null
                                    ? new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, array.ArrayWidth - position, 0)
                                    : new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, divBelow.Position - position, 0);

                array.VerticalDivisions.Add(bottomDiv);

                newRegions = array.VerticalDivisions.ToList();
                ACLPPageBaseViewModel.AddHistoryItemToPage(array.ParentPage,
                                                           new CLPArrayDivisionsChangedHistoryItem(array.ParentPage,
                                                                                                   App.MainWindowViewModel.CurrentUser,
                                                                                                   array.ID,
                                                                                                   oldRegions,
                                                                                                   newRegions));
                return true;
            }

            if (Math.Abs(strokeLeft - strokeRight) > Math.Abs(strokeTop - strokeBottom) &&
                strokeBottom <= cuttableBottom &&
                strokeTop >= cuttableTop &&
                cuttableRight - strokeRight <= MIN_THRESHHOLD &&
                strokeLeft - cuttableLeft <= MIN_THRESHHOLD &&
                array.Rows > 1 &&
                !array.IsRowsObscured) //Horizontal Stroke. Stroke must be within the bounds of the pageObject
            {
                oldRegions = array.HorizontalDivisions.ToList();
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

                CLPArrayDivision topDiv;
                if (divAbove == null)
                {
                    topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0, position, 0);
                }
                else
                {
                    topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, divAbove.Position, position - divAbove.Position, 0);
                    array.HorizontalDivisions.Remove(divAbove);
                }
                array.HorizontalDivisions.Add(topDiv);

                var bottomDiv = divBelow == null
                                    ? new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, position, array.ArrayHeight - position, 0)
                                    : new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, position, divBelow.Position - position, 0);

                array.HorizontalDivisions.Add(bottomDiv);

                newRegions = array.HorizontalDivisions.ToList();
                ACLPPageBaseViewModel.AddHistoryItemToPage(array.ParentPage,
                                                           new CLPArrayDivisionsChangedHistoryItem(array.ParentPage,
                                                                                                   App.MainWindowViewModel.CurrentUser,
                                                                                                   array.ID,
                                                                                                   oldRegions,
                                                                                                   newRegions));
                return true;
            }

            return false;
        }

        public static void AddArrayToPage(CLPPage page, ArrayTypes arrayType)
        {
            if (page == null)
            {
                return;
            }

            //Initial Values.
            int rows;
            int columns;
            int numberOfArrays;
            var initialGridSize = ACLPArrayBase.DefaultGridSquareSize;
            var isMatchingOtherGridSquareSize = false;

            //Launch Array Creation Window.
            var arrayCreationView = new ArrayCreationView
                                    {
                                        Owner = Application.Current.MainWindow
                                    };
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

            //Match GridSquareSize if any Division Templates or Arrays are already on the page.
            //Attempts to match first against a GridSquareSize shared by the most DTs, then by the DT that has been most recently added to the page.
            //Ignores any Division Templates that are full, unless all DTs on the page are full.
            //If no DTs are on the page, match against other Arrays on the page.
            var divisionTemplatesOnPage = page.PageObjects.OfType<FuzzyFactorCard>().Where(d => d.CurrentRemainder < d.Rows).ToList();
            if (!divisionTemplatesOnPage.Any())
            {
                divisionTemplatesOnPage = page.PageObjects.OfType<FuzzyFactorCard>().ToList();
            }
            if (divisionTemplatesOnPage.Any())
            {
                var groupSize = divisionTemplatesOnPage.GroupBy(d => d.GridSquareSize).OrderByDescending(g => g.Count()).First().Count();
                var relevantDivisionTemplateIDs =
                    divisionTemplatesOnPage.GroupBy(d => d.GridSquareSize).Where(g => g.Count() == groupSize).SelectMany(g => g).Select(d => d.ID).ToList();
                initialGridSize = divisionTemplatesOnPage.Last(d => relevantDivisionTemplateIDs.Contains(d.ID)).GridSquareSize;
                isMatchingOtherGridSquareSize = true;
            }
            else
            {
                var arraysOnPage = page.PageObjects.OfType<CLPArray>().ToList();
                if (arraysOnPage.Any())
                {
                    var groupSize = arraysOnPage.GroupBy(a => a.GridSquareSize).OrderByDescending(g => g.Count()).First().Count();
                    var relevantarrayIDs = arraysOnPage.GroupBy(a => a.GridSquareSize).Where(g => g.Count() == groupSize).SelectMany(g => g).Select(a => a.ID).ToList();
                    initialGridSize = arraysOnPage.Last(a => relevantarrayIDs.Contains(a.ID)).GridSquareSize;
                    isMatchingOtherGridSquareSize = true;
                }
            }

            //Generate a GridSquareSize that accommodates all the arrays being created.
            initialGridSize = AdjustGridSquareSize(page, rows, columns, numberOfArrays, initialGridSize, isMatchingOtherGridSquareSize);

            //Create arrays.
            var arraysToAdd =
                Enumerable.Range(1, numberOfArrays).Select(index => new CLPArray(page, initialGridSize, columns, rows, arrayType)).Cast<ACLPArrayBase>().ToList();
            var firstArray = arraysToAdd.First();
            arraysToAdd.Remove(firstArray);

            //Reposition first array.
            ACLPArrayBase.ApplyDistinctPosition(firstArray);

            //Reposition other arrays.
            var newXPosition = firstArray.XPosition;
            var newYPosition = firstArray.YPosition;
            var isVerticalArray = firstArray.Rows >= firstArray.Columns;
            foreach (var array in arraysToAdd)
            {
                if (isVerticalArray) //Move array right.
                {
                    array.XPosition = newXPosition + array.Width;
                    newXPosition = array.XPosition;
                    array.YPosition = newYPosition;
                    if (array.XPosition + array.Width >= page.Width)
                    {
                        array.XPosition = 0.0;
                        newXPosition = array.XPosition;
                        array.YPosition = newYPosition + array.Height;
                        newYPosition = array.YPosition;
                    }
                }
                else //Move array down.
                {
                    array.XPosition = newXPosition;
                    array.YPosition = newYPosition + array.Height;
                    newYPosition = array.YPosition;
                    if (array.YPosition + array.Height >= page.Height)
                    {
                        array.XPosition = newXPosition + array.Width;
                        newXPosition = array.XPosition;
                        array.YPosition = ACLPArrayBase.ARRAY_STARING_Y_POSITION;
                        newYPosition = array.YPosition;
                    }
                }
            }

            //Verify all arrays are on page.
            var rnd = new Random();
            foreach (var array in arraysToAdd)
            {
                if (array.YPosition + array.Height >= page.Height)
                {
                    array.YPosition = page.Height - array.Height - rnd.Next(30);
                }
                if (array.XPosition + array.Width >= page.Width)
                {
                    array.XPosition = page.Width - array.Width - rnd.Next(30);
                }
            }

            //Add to page.
            arraysToAdd.Insert(0, firstArray);
            ACLPPageBaseViewModel.AddPageObjectsToPage(page, arraysToAdd);

            App.MainWindowViewModel.MajorRibbon.PageInteractionMode = PageInteractionModes.Select;
        }

        public static double AdjustGridSquareSize(CLPPage page, int rows, int columns, int numberOfArrays, double initialGridSquareSize, bool isMatchingOtherGridSquareSize)
        {
            // HACK: set default gridsquaresize for all arrays
            //return (page.Width - (2 * ACLPArrayBase.ARRAY_LABEL_LENGTH) - 2.0) / 36;

            var availablePageArea = page.Width * page.Height;

            while (true)
            {
                var arrayWidth = (initialGridSquareSize * columns) + (2 * ACLPArrayBase.ARRAY_LABEL_LENGTH);
                var arrayHeight = (initialGridSquareSize * rows) + (2 * ACLPArrayBase.ARRAY_LABEL_LENGTH);
                var totalArrayArea = arrayWidth * arrayHeight * numberOfArrays;

                if (arrayWidth < page.Width &&
                    arrayHeight < page.Height &&
                    (isMatchingOtherGridSquareSize || totalArrayArea < availablePageArea))
                {
                    return initialGridSquareSize;
                }

                initialGridSquareSize = Math.Abs(initialGridSquareSize - ACLPArrayBase.DefaultGridSquareSize) < .0001 ? 22.5 : initialGridSquareSize * 0.75;
            }
        }

        #endregion //Static Methods
    }
}