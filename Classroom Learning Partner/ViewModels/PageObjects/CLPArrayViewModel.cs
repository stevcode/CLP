using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
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
            hoverTimer.Interval = 1500;

            //Commands
            ResizeArrayCommand = new Command<DragDeltaEventArgs>(OnResizeArrayCommandExecute);
            CreateVerticalDivisionCommand = new Command(OnCreateVerticalDivisionCommandExecute);
            CreateHorizontalDivisionCommand = new Command(OnCreateHorizontalDivisionCommandExecute);
            EditLabelCommand = new Command<CLPArrayDivision>(OnEditLabelCommandExecute);
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
        public double BottomArrowPosition
        {
            get { return GetValue<double>(BottomArrowPositionProperty); }
            set { SetValue(BottomArrowPositionProperty, value); }
        }

        public static readonly PropertyData BottomArrowPositionProperty = RegisterProperty("BottomArrowPosition", typeof(double), 0.0);

        /// <summary>
        /// Gets or sets the RightArrowPosition value.
        /// </summary>
        public double RightArrowPosition
        {
            get { return GetValue<double>(RightArrowPositionProperty); }
            set { SetValue(RightArrowPositionProperty, value); }
        }

        public static readonly PropertyData RightArrowPositionProperty = RegisterProperty("RightArrowPosition", typeof(double), 0.0);

        /// <summary>
        /// Whether or not default adorners are on.
        /// </summary>
        public bool IsDefaultAdornerVisible
        {
            get { return GetValue<bool>(IsDefaultAdornerVisibleProperty); }
            set { SetValue(IsDefaultAdornerVisibleProperty, value); }
        }

        public static readonly PropertyData IsDefaultAdornerVisibleProperty = RegisterProperty("IsDefaultAdornerVisible", typeof(bool), true);

        /// <summary>
        /// Whether or not adorner to create a division on right side of array is on.
        /// </summary>
        public bool IsRightAdornerVisible
        {
            get { return GetValue<bool>(IsRightAdornerVisibleProperty); }
            set { SetValue(IsRightAdornerVisibleProperty, value); }
        }

        public static readonly PropertyData IsRightAdornerVisibleProperty = RegisterProperty("IsRightAdornerVisible", typeof(bool), false);

        /// <summary>
        /// Whether or not adorner to create a division on bottom side of array is on.
        /// </summary>
        public bool IsBottomAdornerVisible
        {
            get { return GetValue<bool>(IsBottomAdornerVisibleProperty); }
            set { SetValue(IsBottomAdornerVisibleProperty, value); }
        }

        public static readonly PropertyData IsBottomAdornerVisibleProperty = RegisterProperty("IsBottomAdornerVisible", typeof(bool), false);

        #endregion //Bindings

        #region Commands

        /// <summary>
        /// Gets the ResizePageObjectCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeArrayCommand { get; set; }

        private void OnResizeArrayCommandExecute(DragDeltaEventArgs e)
        {
            CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            Height = PageObject.Height + e.VerticalChange;
            if(Height < 200)
            {
                Height = 200;
            }
            (PageObject as CLPArray).RefreshArrayDimensions();
            (PageObject as CLPArray).EnforceAspectRatio(Columns * 1.0 / Rows);
            if(Height + PageObject.YPosition > parentPage.PageHeight)
            {
                Height = parentPage.PageHeight - PageObject.YPosition;
                (PageObject as CLPArray).EnforceAspectRatio(Columns * 1.0 / Rows);
            }
            if(Width + PageObject.XPosition > parentPage.PageWidth)
            {
                Width = parentPage.PageWidth - PageObject.XPosition;
                (PageObject as CLPArray).EnforceAspectRatio(Columns * 1.0 / Rows);
            }


            //CLPServiceAgent.Instance.ChangePageObjectDimensions(PageObject, newHeight, newWidth);
            //TODO: Steve - Make work with History.

            (PageObject as CLPArray).ResizeDivisions();
            (PageObject as CLPArray).CalculateGridLines();
        }

        /// <summary>
        /// Gets the CreateHorizontalDivisionCommand command.
        /// </summary>
        public Command CreateHorizontalDivisionCommand { get; private set; }

        private void OnCreateHorizontalDivisionCommandExecute()
        {
            double position = RightArrowPosition - 5;

            CLPArrayDivision divAbove = (PageObject as CLPArray).FindDivisionAbove(position, HorizontalDivisions);
            CLPArrayDivision divBelow = (PageObject as CLPArray).FindDivisionBelow(position, HorizontalDivisions);

            CLPArrayDivision topDiv;
            if(divAbove == null)
            {
                topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, 0, position, 0);
            }
            else
            {
                topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, divAbove.Position, position - divAbove.Position, 0);
                HorizontalDivisions.Remove(divAbove);
            }
            HorizontalDivisions.Add(topDiv);
           
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

            PageObject.ParentPage.PageHistory.Push(new CLPHistoryAddArrayLine(PageObject.ParentPage,
                        (PageObject as CLPArray), divAbove, topDiv, bottomDiv));
        }

        /// <summary>
        /// Gets the CreateVerticalDivisionCommand command.
        /// </summary>
        public Command CreateVerticalDivisionCommand { get; private set; }

        private void OnCreateVerticalDivisionCommandExecute()
        {
            double position = BottomArrowPosition - 5;

            CLPArrayDivision divAbove = (PageObject as CLPArray).FindDivisionAbove(position, VerticalDivisions);
            CLPArrayDivision divBelow = (PageObject as CLPArray).FindDivisionBelow(position, VerticalDivisions);

            CLPArrayDivision topDiv;
            if(divAbove == null)
            {
                topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, position, 0);
            }
            else
            {
                topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, position - divAbove.Position, 0);
                VerticalDivisions.Remove(divAbove);
            }
            VerticalDivisions.Add(topDiv);

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

            PageObject.ParentPage.PageHistory.Push(new CLPHistoryAddArrayLine(PageObject.ParentPage,
                        (PageObject as CLPArray), divAbove, topDiv, bottomDiv));
        }

        /// <summary>
        /// Gets the EditLabelCommand command.
        /// </summary>
        public Command<CLPArrayDivision> EditLabelCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the EditLabelCommand command is executed.
        /// </summary>
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

        #endregion //Commands

        #region Methods

        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            
            if(hitBoxName == "ArrayBodyHitBox" || !IsDivisionBehaviorOn)
            {
                if (isMouseDown)
                {
                    hoverTimer.Stop();
                    timerRunning = false;
                    hoverTimeElapsed = false;
                    return true;
                }
                if (IsRightAdornerVisible || IsBottomAdornerVisible)
                {
                    IsAdornerVisible = false;
                }

                OpenAdornerTimeOut = 0.0;
                IsDefaultAdornerVisible = true;
                IsRightAdornerVisible = false;
                IsBottomAdornerVisible = false;
                if(IsBackground)
                {
                    if(App.MainWindowViewModel.IsAuthoring)
                    {
                        IsMouseOverShowEnabled = true;
                        if(!timerRunning)
                        {
                            timerRunning = true;
                            hoverTimer.Start();
                        }
                    }
                    else
                    {
                        IsMouseOverShowEnabled = false;
                        hoverTimer.Stop();
                        timerRunning = false;
                        hoverTimeElapsed = false;
                    }
                }
                else
                {
                    IsMouseOverShowEnabled = true;
                    if(!timerRunning)
                    {
                        timerRunning = true;
                        hoverTimer.Start();
                    }
                }
            }
            if(hitBoxName == "ArrayBottomHitBox" && IsDivisionBehaviorOn)
            {
                hoverTimer.Stop();
                timerRunning = false;
                hoverTimeElapsed = false;
                OpenAdornerTimeOut = 0.0;
                IsDefaultAdornerVisible = false;
                IsRightAdornerVisible = false;
                IsBottomAdornerVisible = true;
                IsMouseOverShowEnabled = true;
                IsAdornerVisible = true;
                return false;
            }
            if(hitBoxName == "ArrayRightHitBox" && IsDivisionBehaviorOn)
            {
                hoverTimer.Stop();
                timerRunning = false;
                hoverTimeElapsed = false;
                OpenAdornerTimeOut = 0.0;
                IsDefaultAdornerVisible = false;
                IsRightAdornerVisible = true;
                IsBottomAdornerVisible = false;
                IsMouseOverShowEnabled = true;
                IsAdornerVisible = true;
                return false;
            }
            if(hitBoxName == "RightLabelHitBox" && IsDivisionBehaviorOn)
            {
                IsMouseOverShowEnabled = false;
                return false;
            }
            if(hitBoxName == "BottomLabelHitBox" && IsDivisionBehaviorOn)
            {
                IsMouseOverShowEnabled = false;
                return false;
            }

            return !hoverTimeElapsed;       
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

        #endregion //Methods
    }
}
