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
        /// Gets or sets the HorizontalGridDivs value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<double> HorizontalGridLines
        {
            get { return GetValue<ObservableCollection<double>>(HorizontalGridLinesProperty); }
            set { SetValue(HorizontalGridLinesProperty, value); }
        }

        public static readonly PropertyData HorizontalGridLinesProperty = RegisterProperty("HorizontalGridLines", typeof(ObservableCollection<double>));

        /// <summary>
        /// Gets or sets the VerticalGridDivs value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<double> VerticalGridLines
        {
            get { return GetValue<ObservableCollection<double>>(VerticalGridLinesProperty); }
            set { SetValue(VerticalGridLinesProperty, value); }
        }

        public static readonly PropertyData VerticalGridLinesProperty = RegisterProperty("VerticalGridLines", typeof(ObservableCollection<double>));

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
            //double Position = RightArrowPosition - 5;
            //HorizontalDivs.Add(Position);
            //if(HorizontalDivLabels.Count == 0)
            //{
            //    HorizontalDivLabels.Add(Tuple.Create(Position / 2, -1));
            //    HorizontalDivLabels.Add(Tuple.Create((Position + Width)/ 2, -1));
            //}
        }

        /// <summary>
        /// Gets the CreateVerticalDivisionCommand command.
        /// </summary>
        public Command CreateVerticalDivisionCommand { get; private set; }

        private void OnCreateVerticalDivisionCommandExecute()
        {
             //VerticalDivs.Add(BottomArrowPosition - 5);
        }

        #endregion //Commands

        #region Methods

        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            //hoverTimer.Interval = 1000;
            //if(hitBoxName == "ArrayBodyHitBox" || !IsDivisionBehaviorOn)
            //{
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
            //if(hitBoxName == "ArrayBottomHitBox" && IsDivisionBehaviorOn)
            //{
            //    IsDefaultAdornerVisible = false;
            //    IsRightAdornerVisible = false;
            //    IsBottomAdornerVisible = true;

            //}
            //if(hitBoxName == "ArrayRightHitBox" && IsDivisionBehaviorOn)
            //{
            //    IsDefaultAdornerVisible = false;
            //    IsRightAdornerVisible = true;
            //    IsBottomAdornerVisible = false;
            //}

            //return !hoverTimeElapsed;       



            return false;
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

        #endregion //Methods
    }
}
