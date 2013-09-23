﻿using System;
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
using CLP.Models;


namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPArrayViewModel : ACLPPageObjectBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CLPArrayViewModel"/> class.
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
            RotateArrayCommand = new Command(OnRotateArrayCommandExecute);
            CreateVerticalDivisionCommand = new Command(OnCreateVerticalDivisionCommandExecute);
            CreateHorizontalDivisionCommand = new Command(OnCreateHorizontalDivisionCommandExecute);
            EditLabelCommand = new Command<CLPArrayDivision>(OnEditLabelCommandExecute);
            EraseDivisionCommand = new Command<MouseEventArgs>(OnEraseDivisionCommandExecute);
            ToggleMainArrayAdornersCommand = new Command<MouseButtonEventArgs>(OnToggleMainArrayAdornersCommandExecute);
        }

        #endregion //Constructor

        #region Model

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
            set { SetValue(ArrayHeightProperty, value); }
        }

        public static readonly PropertyData ArrayHeightProperty = RegisterProperty("ArrayHeight", typeof(double));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public double ArrayWidth
        {
            get { return GetValue<double>(ArrayWidthProperty); }
            set { SetValue(ArrayWidthProperty, value); }
        }

        public static readonly PropertyData ArrayWidthProperty = RegisterProperty("ArrayWidth", typeof(double));

        /// <summary>
        /// Gets or sets the HorizontalGridLines value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<double> HorizontalGridLines
        {
            get { return GetValue<ObservableCollection<double>>(HorizontalGridLinesProperty); }
            set { SetValue(HorizontalGridLinesProperty, value); }
        }

        public static readonly PropertyData HorizontalGridLinesProperty = RegisterProperty("HorizontalGridLines", typeof(ObservableCollection<double>));

        /// <summary>
        /// Gets or sets the VerticalGridLines value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<double> VerticalGridLines
        {
            get { return GetValue<ObservableCollection<double>>(VerticalGridLinesProperty); }
            set { SetValue(VerticalGridLinesProperty, value); }
        }

        public static readonly PropertyData VerticalGridLinesProperty = RegisterProperty("VerticalGridLines", typeof(ObservableCollection<double>));

        /// <summary>
        /// Gets or sets the HorizontalDivisions value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<CLPArrayDivision> HorizontalDivisions
        {
            get { return GetValue<ObservableCollection<CLPArrayDivision>>(HorizontalDivisionsProperty); }
            set { SetValue(HorizontalDivisionsProperty, value); }
        }

        /// <summary>
        /// Register the HorizontalDivisions property so it is known in the class.
        /// </summary>
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

        /// <summary>
        /// Register the VerticalDivisions property so it is known in the class.
        /// </summary>
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
            set { SetValue(IsDefaultAdornerVisibleProperty, value); }
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
                newArrayHeight = (ArrayWidth + e.HorizontalChange)/Columns*Rows;
            }

            //Control Min Dimensions of Array.
            if(newArrayHeight < 45.0)
            {
                newArrayHeight = 45.0;
            }
            var newSquareSize = newArrayHeight / Rows;
            var newArrayWidth = newSquareSize * Columns;
            if(newArrayWidth < 45.0)
            {
                newArrayWidth = 45.0;
                newSquareSize = newArrayWidth/Columns;
                newArrayHeight = newSquareSize*Rows;
            }

            //Control Max Dimensions of Array.
            if(newArrayHeight + 2*clpArray.LabelLength + YPosition > clpArray.ParentPage.PageHeight)
            {
                newArrayHeight = clpArray.ParentPage.PageHeight - YPosition - 2*clpArray.LabelLength;
                newSquareSize = newArrayHeight / Rows;
                newArrayWidth = newSquareSize * Columns;
            }
            if(newArrayWidth + 2*clpArray.LabelLength + XPosition > clpArray.ParentPage.PageWidth)
            {
                newArrayWidth = clpArray.ParentPage.PageWidth - XPosition - 2*clpArray.LabelLength;
                newSquareSize = newArrayWidth / Columns;
                //newArrayHeight = newSquareSize * Rows;
            }

            clpArray.SizeArrayToGridLevel(newSquareSize);

            //Resize History
            var heightDiff = Math.Abs(oldHeight - Height);
            var widthDiff = Math.Abs(oldWidth - Width);
            var diff = heightDiff + widthDiff;
            if(!(diff > CLPHistory.SAMPLE_RATE))
            {
                return;
            }

            var batch = PageObject.ParentPage.PageHistory.CurrentHistoryBatch;
            if(batch is CLPHistoryPageObjectResizeBatch)
            {
                (batch as CLPHistoryPageObjectResizeBatch).AddResizePointToBatch(PageObject.UniqueID,
                                                                                 new Point(Width, Height));
            }
            else
            {
                PageObject.ParentPage.PageHistory.EndBatch();
                //TODO: log this error
            }
        }

        /// <summary>
        /// Gets the SnapCommand command.
        /// </summary>
        public Command<DragCompletedEventArgs> DragStopAndSnapCommand { get; private set; }

        private void OnDragStopAndSnapCommandExecute(DragCompletedEventArgs e)
        {
            var movementBatch = PageObject.ParentPage.PageHistory.CurrentHistoryBatch as CLPHistoryPageObjectMoveBatch;
            if(movementBatch != null)
            {
                movementBatch.AddPositionPointToBatch(PageObject.UniqueID,
                                                      new Point(PageObject.XPosition, PageObject.YPosition));
            }
            PageObject.ParentPage.PageHistory.EndBatch();

            var snappingArray = PageObject as CLPArray;
            if(snappingArray == null)
            {
                return;
            }

            foreach(var pageObject in PageObject.ParentPage.PageObjects)
            {
                var persistingArray = pageObject as CLPArray;
                if(persistingArray == null || persistingArray.UniqueID == snappingArray.UniqueID)
                {
                    continue;
                }

                var deltaX = Math.Abs(snappingArray.XPosition + snappingArray.LabelLength - (persistingArray.XPosition + persistingArray.LabelLength));
                var deltaY = Math.Abs(snappingArray.YPosition + snappingArray.LabelLength - (persistingArray.YPosition + persistingArray.LabelLength));

                var bottomDiff = Math.Abs(snappingArray.YPosition + snappingArray.LabelLength - (persistingArray.YPosition + persistingArray.Height));
                if(bottomDiff < 50 && deltaX < 50 && snappingArray.Columns == persistingArray.Columns) //Snapping from below
                {
                    PageObject.ParentPage.PageHistory.AddHistoryItem(new CLPHistoryArraySnap(
                        PageObject.ParentPage, 
                        persistingArray, 
                        snappingArray, 
                        true));

                    persistingArray.VerticalDivisions.Clear();
                    
                    var squareSize = persistingArray.ArrayWidth/persistingArray.Columns;
                    snappingArray.SizeArrayToGridLevel(squareSize);

                    if(!persistingArray.HorizontalDivisions.Any())
                    {
                        persistingArray.HorizontalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0,
                                                                                persistingArray.ArrayHeight, persistingArray.Rows));
                    }

                    if(!snappingArray.HorizontalDivisions.Any())
                    {
                        persistingArray.HorizontalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, persistingArray.ArrayHeight,
                                                                                snappingArray.ArrayHeight, snappingArray.Rows));
                    }
                    else
                    {
                        foreach(var horizontalDivision in snappingArray.HorizontalDivisions)
                        {
                            persistingArray.HorizontalDivisions.Add(new CLPArrayDivision(
                                horizontalDivision.Orientation, 
                                horizontalDivision.Position + persistingArray.ArrayHeight, 
                                horizontalDivision.Length, 
                                horizontalDivision.Value));
                        }
                    }

                    persistingArray.Rows += snappingArray.Rows;
                    persistingArray.SizeArrayToGridLevel(squareSize, false);
                    persistingArray.IsDivisionBehaviorOn = true;

                    CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject, false);
                    break;
                }

                var topDiff = Math.Abs(PageObject.YPosition + PageObject.Height - (persistingArray.YPosition + persistingArray.LabelLength));
                if(topDiff < 50 && deltaX < 50 && snappingArray.Columns == persistingArray.Columns) //Snapping from above
                {
                    PageObject.ParentPage.PageHistory.AddHistoryItem(new CLPHistoryArraySnap(
                        PageObject.ParentPage,
                        persistingArray,
                        snappingArray,
                        true));

                    persistingArray.VerticalDivisions.Clear();

                    var squareSize = persistingArray.ArrayWidth / persistingArray.Columns;
                    snappingArray.SizeArrayToGridLevel(squareSize);

                    ObservableCollection<CLPArrayDivision> tempDivisions;
                    if(persistingArray.HorizontalDivisions.Any())
                    {
                        tempDivisions = new ObservableCollection<CLPArrayDivision>(persistingArray.HorizontalDivisions);
                        persistingArray.HorizontalDivisions.Clear();
                    }
                    else
                    {
                        tempDivisions = new ObservableCollection<CLPArrayDivision>
                                        {
                                            new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0,
                                                                 persistingArray.ArrayHeight, persistingArray.Rows)
                                        };
                    }

                    if(!snappingArray.HorizontalDivisions.Any())
                    {
                        persistingArray.HorizontalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0,
                                                                               snappingArray.ArrayHeight, snappingArray.Rows));
                    }

                    foreach(var horizontalDivision in snappingArray.HorizontalDivisions)
                    {
                        persistingArray.HorizontalDivisions.Add(horizontalDivision);
                    }
                    foreach(var horizontalDivision in tempDivisions)
                    {
                        persistingArray.HorizontalDivisions.Add(new CLPArrayDivision(
                                horizontalDivision.Orientation,
                                horizontalDivision.Position + snappingArray.ArrayHeight,
                                horizontalDivision.Length,
                                horizontalDivision.Value));
                    }

                    persistingArray.Rows += snappingArray.Rows;
                    persistingArray.YPosition -= snappingArray.ArrayHeight;
                    persistingArray.SizeArrayToGridLevel(squareSize, false);
                    persistingArray.IsDivisionBehaviorOn = true;

                    CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject, false);
                    break;
                }

                var leftDiff = Math.Abs(snappingArray.XPosition + snappingArray.Width - (persistingArray.XPosition + persistingArray.LabelLength));
                if(leftDiff < 50 && deltaY < 50 && snappingArray.Rows == persistingArray.Rows) //Snapping from left
                {
                    PageObject.ParentPage.PageHistory.AddHistoryItem(new CLPHistoryArraySnap(
                        PageObject.ParentPage,
                        persistingArray,
                        snappingArray,
                        false));

                    persistingArray.HorizontalDivisions.Clear();

                    var squareSize = persistingArray.ArrayWidth / persistingArray.Columns;
                    snappingArray.SizeArrayToGridLevel(squareSize);

                    ObservableCollection<CLPArrayDivision> tempDivisions;
                    if(persistingArray.VerticalDivisions.Any())
                    {
                        tempDivisions = new ObservableCollection<CLPArrayDivision>(persistingArray.VerticalDivisions);
                        persistingArray.VerticalDivisions.Clear();
                    }
                    else
                    {
                        tempDivisions = new ObservableCollection<CLPArrayDivision>
                                        {
                                            new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0,
                                                                 persistingArray.ArrayWidth, persistingArray.Columns)
                                        };
                    }

                    if(!snappingArray.VerticalDivisions.Any())
                    {
                        persistingArray.VerticalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0,
                                                                             snappingArray.ArrayWidth, snappingArray.Columns));
                    }

                    foreach(var verticalDivision in snappingArray.VerticalDivisions)
                    {
                        persistingArray.VerticalDivisions.Add(verticalDivision);
                    }
                    foreach(var verticalDivision in tempDivisions)
                    {
                        persistingArray.VerticalDivisions.Add(new CLPArrayDivision(
                                verticalDivision.Orientation,
                                verticalDivision.Position + snappingArray.ArrayWidth,
                                verticalDivision.Length,
                                verticalDivision.Value));
                    }

                    persistingArray.Columns += snappingArray.Columns;
                    persistingArray.XPosition -= snappingArray.ArrayWidth;
                    persistingArray.SizeArrayToGridLevel(squareSize, false);
                    persistingArray.IsDivisionBehaviorOn = true;

                    CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject, false);
                    break;
                }

                var rightDiff = Math.Abs(snappingArray.XPosition + snappingArray.LabelLength - (persistingArray.XPosition + persistingArray.Width));
                if(rightDiff < 50 && deltaY < 50 && snappingArray.Rows == persistingArray.Rows) //Snapping from right
                {
                    PageObject.ParentPage.PageHistory.AddHistoryItem(new CLPHistoryArraySnap(
                        PageObject.ParentPage,
                        persistingArray,
                        snappingArray,
                        false));

                    persistingArray.HorizontalDivisions.Clear();

                    var squareSize = persistingArray.ArrayWidth / persistingArray.Columns;
                    snappingArray.SizeArrayToGridLevel(squareSize);

                    if(!persistingArray.VerticalDivisions.Any())
                    {
                        persistingArray.VerticalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0,
                                                                                persistingArray.ArrayWidth, persistingArray.Columns));
                    }

                    if(!snappingArray.VerticalDivisions.Any())
                    {
                        persistingArray.VerticalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Vertical, persistingArray.ArrayWidth,
                                                                                snappingArray.ArrayWidth, snappingArray.Columns));
                    }
                    else
                    {
                        foreach(var verticalDivision in snappingArray.VerticalDivisions)
                        {
                            persistingArray.VerticalDivisions.Add(new CLPArrayDivision(
                                verticalDivision.Orientation,
                                verticalDivision.Position + persistingArray.ArrayWidth,
                                verticalDivision.Length,
                                verticalDivision.Value));
                        }
                    }

                    persistingArray.Columns += snappingArray.Columns;
                    persistingArray.SizeArrayToGridLevel(squareSize, false);
                    persistingArray.IsDivisionBehaviorOn = true;

                    CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject, false);
                    break;
                }
            }

            PageObject.OnMoved();
        }

        /// <summary>
        /// Toggle the visibility of GridLines on the array.
        /// </summary>
        public Command ToggleGridCommand { get; private set; }

        private void OnToggleGridCommandExecute()
        {
            (PageObject as CLPArray).IsGridOn = !(PageObject as CLPArray).IsGridOn;
            PageObject.ParentPage.PageHistory.AddHistoryItem(new CLPHistoryArrayGridToggle(PageObject.ParentPage,
                                                                                           PageObject.UniqueID));
        }

        /// <summary>
        /// Rotates the array 90 degrees
        /// </summary>
        public Command RotateArrayCommand { get; private set; }

        private void OnRotateArrayCommandExecute()
        {
            (PageObject as CLPArray).RotateArray();
            PageObject.ParentPage.PageHistory.AddHistoryItem(new CLPHistoryArrayRotate(PageObject.ParentPage,
                                                                                           PageObject.UniqueID));
        }

        /// <summary>
        /// Gets the CreateHorizontalDivisionCommand command.
        /// </summary>
        public Command CreateHorizontalDivisionCommand { get; private set; }

        private void OnCreateHorizontalDivisionCommandExecute()
        {
            var position = LeftArrowPosition - 5;
            if (IsGridOn)
            {
                position = (PageObject as CLPArray).GetClosestGridLine(ArrayDivisionOrientation.Horizontal, position);
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
           
            CLPArrayDivision bottomDiv;
            if(divBelow == null)
            {
                bottomDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, position, ArrayHeight - position, 0);
            }
            else
            {
                bottomDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, position, divBelow.Position - position, 0);
            }

            HorizontalDivisions.Add(bottomDiv);
            addedDivisions.Add(bottomDiv);

            PageObject.ParentPage.PageHistory.AddHistoryItem(new CLPHistoryArrayDivisionsChanged(PageObject.ParentPage,
                                                                                                 PageObject.UniqueID,
                                                                                                 addedDivisions,
                                                                                                 removedDivisions));
        }

        /// <summary>
        /// Gets the CreateVerticalDivisionCommand command.
        /// </summary>
        public Command CreateVerticalDivisionCommand { get; private set; }

        private void OnCreateVerticalDivisionCommandExecute()
        {
            var position = TopArrowPosition - 5;
            if (IsGridOn)
            {
                position = (PageObject as CLPArray).GetClosestGridLine(ArrayDivisionOrientation.Vertical, position);
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

            CLPArrayDivision divAbove = (PageObject as CLPArray).FindDivisionAbove(position, VerticalDivisions);
            CLPArrayDivision divBelow = (PageObject as CLPArray).FindDivisionBelow(position, VerticalDivisions);

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

            CLPArrayDivision bottomDiv;
            if(divBelow == null)
            {
                bottomDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, ArrayWidth - position, 0);
            }
            else
            {
                bottomDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, divBelow.Position - position, 0);
            }

            VerticalDivisions.Add(bottomDiv);
            addedDivisions.Add(bottomDiv);

            PageObject.ParentPage.PageHistory.AddHistoryItem(new CLPHistoryArrayDivisionsChanged(PageObject.ParentPage,
                                                                                                 PageObject.UniqueID,
                                                                                                 addedDivisions,
                                                                                                 removedDivisions));
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
            if(keyPad.DialogResult == true && keyPad.NumbersEntered.Text.Length > 0)
            {
                division.Value = Int32.Parse(keyPad.NumbersEntered.Text);
            }
        }

        /// <summary>
        /// Gets the EraseDivisionCommand command.
        /// </summary>
        public Command<MouseEventArgs> EraseDivisionCommand { get; private set; }

        private void OnEraseDivisionCommandExecute(MouseEventArgs e)
        {
            if((e.StylusDevice != null && e.StylusDevice.Inverted && e.LeftButton == MouseButtonState.Pressed) || e.MiddleButton == MouseButtonState.Pressed)
            {
                var rectangle = e.Source as Rectangle;
                if(rectangle != null) 
                {
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
                        CLPArrayDivision divAbove = (PageObject as CLPArray).FindDivisionAbove(division.Position, (PageObject as CLPArray).HorizontalDivisions);
                        (PageObject as CLPArray).HorizontalDivisions.Remove(divAbove);
                        (PageObject as CLPArray).HorizontalDivisions.Remove(division);
                        removedDivisions.Add(divAbove);
                        removedDivisions.Add(division);

                        //Add new division unless we removed the only division line
                        if((PageObject as CLPArray).HorizontalDivisions.Count > 0)
                        {
                            double newLength = divAbove.Length + division.Length;
                            CLPArrayDivision newDivision = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, divAbove.Position, newLength, 0);
                            (PageObject as CLPArray).HorizontalDivisions.Add(newDivision);
                            addedDivisions.Add(newDivision);
                        }
                    }
                    if(division.Orientation == ArrayDivisionOrientation.Vertical)
                    {
                        CLPArrayDivision divAbove = (PageObject as CLPArray).FindDivisionAbove(division.Position, (PageObject as CLPArray).VerticalDivisions);
                        (PageObject as CLPArray).VerticalDivisions.Remove(divAbove);
                        (PageObject as CLPArray).VerticalDivisions.Remove(division);
                        removedDivisions.Add(divAbove);
                        removedDivisions.Add(division);

                        //Add new division unless we removed the only division line
                        if((PageObject as CLPArray).VerticalDivisions.Count > 0)
                        {
                            double newLength = divAbove.Length + division.Length;
                            CLPArrayDivision newDivision = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, newLength, 0);
                            (PageObject as CLPArray).VerticalDivisions.Add(newDivision);
                            addedDivisions.Add(newDivision);
                        }
                    }

                    PageObject.ParentPage.PageHistory.AddHistoryItem(new CLPHistoryArrayDivisionsChanged(PageObject.ParentPage,
                                                                                                 PageObject.UniqueID,
                                                                                                 addedDivisions,
                                                                                                 removedDivisions));
                }
            }
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

            if(e.ChangedButton == MouseButton.Left && !(e.StylusDevice != null && e.StylusDevice.Inverted))
            {
                var tempAdornerState = IsDefaultAdornerVisible;
                CLPPageViewModel.ClearAdorners(PageObject.ParentPage);
                IsAdornerVisible = !tempAdornerState;
                IsDefaultAdornerVisible = !tempAdornerState;
                IsTopAdornerVisible = tempAdornerState;
                IsLeftAdornerVisible = tempAdornerState;
            }
        }

        #endregion //Commands

        #region Methods

        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            return false;
            //if(hitBoxName == "ArrayBodyHitBox" || !IsDivisionBehaviorOn)
            //{
            //    if (isMouseDown)
            //    {
            //        hoverTimer.Stop();
            //        timerRunning = false;
            //        hoverTimeElapsed = false;
            //        return true;
            //    }
            //    if (IsRightAdornerVisible || IsBottomAdornerVisible)
            //    {
            //        IsAdornerVisible = false;
            //    }

            //    OpenAdornerTimeOut = 0.0;
            //    IsDefaultAdornerVisible = true;
            //    IsRightAdornerVisible = false;
            //    IsBottomAdornerVisible = false;
            //    if(IsBackground)
            //    {
            //        if(App.MainWindowViewModel.IsAuthoring)
            //        {
            //            IsMouseOverShowEnabled = true;
            //            if(!timerRunning)
            //            {
            //                timerRunning = true;
            //                hoverTimer.Start();
            //            }
            //        }
            //        else
            //        {
            //            IsMouseOverShowEnabled = false;
            //            hoverTimer.Stop();
            //            timerRunning = false;
            //            hoverTimeElapsed = false;
            //        }
            //    }
            //    else
            //    {
            //        IsMouseOverShowEnabled = true;
            //        if(!timerRunning)
            //        {
            //            timerRunning = true;
            //            hoverTimer.Start();
            //        }
            //    }
            //}
            //if(hitBoxName == "ArrayBottomHitBox" && IsDivisionBehaviorOn && !isMouseDown)
            //{
            //    hoverTimer.Stop();
            //    timerRunning = false;
            //    hoverTimeElapsed = false;
            //    OpenAdornerTimeOut = 0.0;
            //    IsDefaultAdornerVisible = false;
            //    IsRightAdornerVisible = false;
            //    IsBottomAdornerVisible = true;
            //    IsMouseOverShowEnabled = true;
            //    IsAdornerVisible = true;
            //    return false;
            //}
            //if(hitBoxName == "ArrayRightHitBox" && IsDivisionBehaviorOn && !isMouseDown)
            //{
            //    hoverTimer.Stop();
            //    timerRunning = false;
            //    hoverTimeElapsed = false;
            //    OpenAdornerTimeOut = 0.0;
            //    IsDefaultAdornerVisible = false;
            //    IsRightAdornerVisible = true;
            //    IsBottomAdornerVisible = false;
            //    IsMouseOverShowEnabled = true;
            //    IsAdornerVisible = true;
            //    return false;
            //}
            //if(hitBoxName == "RightLabelHitBox" && IsDivisionBehaviorOn && !isMouseDown)
            //{
            //    IsMouseOverShowEnabled = false;
            //    return false;
            //}
            //if(hitBoxName == "BottomLabelHitBox" && IsDivisionBehaviorOn && !isMouseDown)
            //{
            //    IsMouseOverShowEnabled = false;
            //    return false;
            //}

            //return !hoverTimeElapsed;       
        }

        public override void EraserHitTest(string hitBoxName, object tag)
        {
            if(IsBackground && !App.MainWindowViewModel.IsAuthoring)
            {
                //don't erase
            }
            else if(hitBoxName == "DivisionHitBox")
            {
                CLPArrayDivision division = tag as CLPArrayDivision;
                if(division.Position != 0.0) //don't delete first division
                {
                    if(division.Orientation == ArrayDivisionOrientation.Horizontal)
                    {
                        CLPArrayDivision divAbove = (PageObject as CLPArray).FindDivisionAbove(division.Position, (PageObject as CLPArray).HorizontalDivisions);
                        (PageObject as CLPArray).HorizontalDivisions.Remove(divAbove);
                        (PageObject as CLPArray).HorizontalDivisions.Remove(division);

                        //Add new division unless we removed the only division line
                        if((PageObject as CLPArray).HorizontalDivisions.Count > 0)
                        {
                            double newLength = divAbove.Length + division.Length;
                            CLPArrayDivision newDivision = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, divAbove.Position, newLength, 0);
                            (PageObject as CLPArray).HorizontalDivisions.Add(newDivision);
                        }
                    }
                    if(division.Orientation == ArrayDivisionOrientation.Vertical)
                    {
                        CLPArrayDivision divAbove = (PageObject as CLPArray).FindDivisionAbove(division.Position, (PageObject as CLPArray).VerticalDivisions);
                        (PageObject as CLPArray).VerticalDivisions.Remove(divAbove);
                        (PageObject as CLPArray).VerticalDivisions.Remove(division);

                        //Add new division unless we removed the only division line
                        if((PageObject as CLPArray).VerticalDivisions.Count > 0)
                        {
                            double newLength = divAbove.Length + division.Length;
                            CLPArrayDivision newDivision = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, newLength, 0);
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
