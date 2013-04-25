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
            OpenAdornerTimeOut = 1.2;

            //Commands
            ResizeArrayCommand = new Command<DragDeltaEventArgs>(OnResizeArrayCommandExecute);
            CreateVerticalDivisionCommand = new Command(OnCreateVerticalDivisionCommandExecute);
            CreateHorizontalDivisionCommand = new Command(OnCreateHorizontalDivisionCommandExecute);
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
            set { SetValue(RightArrowPositionProperty, value); Console.WriteLine("RightArrowPosition: " + RightArrowPosition); }
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
        /// hether or not adorner to create a division on bottom side of array is on.
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
            // TO DO Liz - 7x77 won't resize?
            CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            double newHeight = PageObject.Height + e.VerticalChange;
            double newWidth = newHeight * ((double)Columns) / ((double)Rows);
            if(newHeight < 100)
            {
                newHeight = 100;
                newWidth = newHeight * ((double)Columns) / ((double)Rows);
            }
            if(newHeight + PageObject.YPosition > parentPage.PageHeight)
            {
                newHeight = PageObject.Height;
                newWidth = newHeight * ((double)Columns) / ((double)Rows);
            }
            if(newWidth + PageObject.XPosition > parentPage.PageWidth)
            {
                newWidth = PageObject.Width;
                newHeight = newWidth * ((double)Rows) / ((double)Columns);
            }

            (PageObject as CLPArray).CalculateGridLines();

            //TO DO Liz - make it so resizing preserves divisions

            CLPServiceAgent.Instance.ChangePageObjectDimensions(PageObject, newHeight, newWidth);
        }

        /// <summary>
        /// Gets the CreateHorizontalDivisionCommand command.
        /// </summary>
        public Command CreateHorizontalDivisionCommand { get; private set; }

        private void OnCreateHorizontalDivisionCommandExecute()
        {
            double position = RightArrowPosition - 5;

            CLPArrayDivision divAbove = FindDivisionAbove(position, HorizontalDivisions);
            CLPArrayDivision divBelow = FindDivisionBelow(position, HorizontalDivisions);

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
                bottomDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, position, Height - position, 0);
            }
            else
            {
                bottomDiv = new CLPArrayDivision(ArrayDivisionOrientation.Horizontal, position, divBelow.Position - position, 0);
            }

            HorizontalDivisions.Add(bottomDiv);
        }

        /// <summary>
        /// Gets the CreateVerticalDivisionCommand command.
        /// </summary>
        public Command CreateVerticalDivisionCommand { get; private set; }

        private void OnCreateVerticalDivisionCommandExecute()
        {
            double position = BottomArrowPosition - 5;

            CLPArrayDivision divAbove = FindDivisionAbove(position, VerticalDivisions);
            CLPArrayDivision divBelow = FindDivisionBelow(position, VerticalDivisions);

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
                bottomDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, Width - position, 0);
            }
            else
            {
                bottomDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, divBelow.Position - position, 0);
            }

            VerticalDivisions.Add(bottomDiv);
        }

        #endregion //Commands

        #region Methods

        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            hoverTimer.Interval = 1000;
            if(hitBoxName == "ArrayBodyHitBox" || !IsDivisionBehaviorOn)
            {
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
                IsDefaultAdornerVisible = false;
                IsRightAdornerVisible = false;
                IsBottomAdornerVisible = true;

            }
            if(hitBoxName == "ArrayRightHitBox" && IsDivisionBehaviorOn)
            {
                IsDefaultAdornerVisible = false;
                IsRightAdornerVisible = true;
                IsBottomAdornerVisible = false;
            }

            return !hoverTimeElapsed;       

        }

        public override void EraserHitTest(string hitBoxName)
        {
            //if(IsBackground && !App.MainWindowViewModel.IsAuthoring)
            //{
            //    //don't erase
            //}
            //else if(hitBoxName == "ArrayBodyHitBox")
            //{
            //    var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            //    if(notebookWorkspaceViewModel != null)
            //    {
            //        CLPPage parentPage = notebookWorkspaceViewModel.Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            //        if(parentPage != null)
            //        {
            //            foreach(CLPPageViewModel pageVM in ViewModelManager.GetViewModelsOfModel(parentPage))
            //            {
            //                pageVM.IsInkCanvasHitTestVisible = true;
            //            }
            //        }
            //        CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject);
            //    }
            //}
            //else if(hitBoxName == "HorizontalDivisionHitBox")
            //{
            //    var notebookWorkspaceViewModel = App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel;
            //    if(notebookWorkspaceViewModel != null)
            //    {
            //        CLPPage parentPage = notebookWorkspaceViewModel.Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            //        if(parentPage != null)
            //        {
            //            foreach(CLPPageViewModel pageVM in ViewModelManager.GetViewModelsOfModel(parentPage))
            //            {
            //                pageVM.IsInkCanvasHitTestVisible = true;
            //            }

            //            //CLPServiceAgent.Instance.RemovePageObjectFromPage(PageObject);

            //            foreach(Tuple<double, int> Label in HorizontalDivLabels)
            //            {
            //                //To Do Liz - figure out which division was erased
            //                if(Label.Item1 == YPosition)
            //                {
            //                    HorizontalDivLabels.Remove(Label);
            //                }
            //            }
            //        }
            //    }
            //}
        }

        public CLPArrayDivision FindDivisionAbove(double position, ObservableCollection<CLPArrayDivision> divisionList)
        {
            CLPArrayDivision divAbove = null;
            foreach(CLPArrayDivision div in divisionList)
            {
                if(divAbove == null)
                {
                    if(div.Position < position)
                    {
                        divAbove = div;
                    }
                }
                else if(divAbove.Position < div.Position && div.Position < position)
                {
                    divAbove = div;
                }
            }
            return divAbove;
         }

        public CLPArrayDivision FindDivisionBelow(double position, ObservableCollection<CLPArrayDivision> divisionList)
        {
            CLPArrayDivision divBelow = null;
            foreach(CLPArrayDivision div in divisionList)
            {
                if(divBelow == null)
                {
                    if(div.Position > position)
                    {
                        divBelow = div;
                    }
                }
                else if(divBelow.Position > div.Position && div.Position > position)
                {
                    divBelow = div;
                }
            }
            return divBelow;
        }

        #endregion //Methods
    }
}
