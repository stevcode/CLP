using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;
using System.Collections.ObjectModel;
using Classroom_Learning_Partner.Views.Modal_Windows;
using Classroom_Learning_Partner.Views;
using System.Windows.Input;


namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPArrayViewModel : ACLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPArrayViewModel"/> class.
        /// </summary>
        public CLPArrayViewModel(CLPArray array)
            : base()
        {
            PageObject = array;

            //Commands
            ResizeArrayCommand = new Command<DragDeltaEventArgs>(OnResizeArrayCommandExecute);
            CreateVerticalDivisionCommand = new Command(OnCreateVerticalDivisionCommandExecute);
            CreateHorizontalDivisionCommand = new Command(OnCreateHorizontalDivisionCommandExecute);
            EnterRowsCommand = new Command(OnEnterRowsCommandExecute);;
        }

        /// <summary>
        /// Gets or sets the IsDefaultAdornerVisible value.
        /// </summary>
        public Boolean IsDefaultAdornerVisible
        {
            get { return GetValue<Boolean>(IsDefaultAdornerVisibleProperty); }
            set { SetValue(IsDefaultAdornerVisibleProperty, value); }
        }

        /// <summary>
        /// Register the IsDefaultAdornerVisible property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsDefaultAdornerVisibleProperty = RegisterProperty("IsDefaultAdornerVisible", typeof(Boolean), true);

        /// <summary>
        /// Gets or sets the IsRightAdornerVisible value.
        /// </summary>
        public Boolean IsRightAdornerVisible
        {
            get { return GetValue<Boolean>(IsRightAdornerVisibleProperty); }
            set { SetValue(IsRightAdornerVisibleProperty, value); }
        }

        /// <summary>
        /// Register the IsRightAdornerVisible property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsRightAdornerVisibleProperty = RegisterProperty("IsRightAdornerVisible", typeof(Boolean), false);

        /// <summary>
        /// Gets or sets the IsBottomAdornerVisible value.
        /// </summary>
        public Boolean IsBottomAdornerVisible
        {
            get { return GetValue<Boolean>(IsBottomAdornerVisibleProperty); }
            set { SetValue(IsBottomAdornerVisibleProperty, value); }
        }

        /// <summary>
        /// Register the IsBottomAdornerVisible property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsBottomAdornerVisibleProperty = RegisterProperty("IsBottomAdornerVisible", typeof(Boolean), false);

        /// <summary>
        /// Gets or sets the BottomArrowPosition value.
        /// </summary>
        public double BottomArrowPosition
        {
            get { return GetValue<double>(BottomArrowPositionProperty); }
            set { SetValue(BottomArrowPositionProperty, value); }
        }

        /// <summary>
        /// Register the BottomArrowPosition property so it is known in the class.
        /// </summary>
        public static readonly PropertyData BottomArrowPositionProperty = RegisterProperty("BottomArrowPosition", typeof(double), 0.0);

        /// <summary>
        /// Gets or sets the RightArrowPosition value.
        /// </summary>
        public double RightArrowPosition
        {
            get { return GetValue<double>(RightArrowPositionProperty); }
            set { SetValue(RightArrowPositionProperty, value); Console.WriteLine("RightArrowPosition: " + RightArrowPosition); }
        }

        /// <summary>
        /// Register the RightArrowPosition property so it is known in the class.
        /// </summary>
        public static readonly PropertyData RightArrowPositionProperty = RegisterProperty("RightArrowPosition", typeof(double), 0.0);

        /// <summary>
        /// Gets or sets the Rows value
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int Rows
        {
            get { return GetValue<int>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        /// <summary>
        /// Register the Rows property so it is known in the class.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the HorizontalDivs value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<double> HorizontalDivs
        {
            get { return GetValue<ObservableCollection<double>>(HorizontalDivsProperty); }
            set { SetValue(HorizontalDivsProperty, value); }
        }

        /// <summary>
        /// Register the HorizontalDivs property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HorizontalDivsProperty = RegisterProperty("HorizontalDivs", typeof(ObservableCollection<double>));

        /// <summary>
        /// Gets or sets the VerticalDivs value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<double> VerticalDivs
        {
            get { return GetValue<ObservableCollection<double>>(VerticalDivsProperty); }
            set { SetValue(VerticalDivsProperty, value); }
        }

        /// <summary>
        /// Register the VerticalDivs property so it is known in the class.
        /// </summary>
        public static readonly PropertyData VerticalDivsProperty = RegisterProperty("VerticalDivs", typeof(ObservableCollection<double>));

        /// <summary>
        /// Gets or sets the RowDivs value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<int> RowDivs
        {
            get { return GetValue<ObservableCollection<int>>(RowDivsProperty); }
            set { SetValue(RowDivsProperty, value); }
        }

        /// <summary>
        /// Register the RowDivs property so it is known in the class.
        /// </summary>
        public static readonly PropertyData RowDivsProperty = RegisterProperty("RowDivs", typeof(ObservableCollection<int>));

        /// <summary>
        /// Gets or sets the ColumnDivs value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<int> ColumnDivs
        {
            get { return GetValue<ObservableCollection<int>>(ColumnDivsProperty); }
            set { SetValue(ColumnDivsProperty, value); }
        }

        /// <summary>
        /// Register the ColumnDivs property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ColumnDivsProperty = RegisterProperty("ColumnDivs", typeof(ObservableCollection<int>));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public CLPHandwritingRegion HandwritingRegionParts
        {
            get { return GetValue<CLPHandwritingRegion>(HandwritingRegionPartsProperty); }
            set { SetValue(HandwritingRegionPartsProperty, value); }
        }

        /// <summary>
        /// Register the HandwritingRegionParts property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HandwritingRegionPartsProperty = RegisterProperty("HandwritingRegionParts", typeof(CLPHandwritingRegion));


        /// <summary>
        /// Register the Columns property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ColumnsProperty = RegisterProperty("Columns", typeof(int));

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
            double newWidth = newHeight * ((double)this.Columns) / ((double)this.Rows);
            if(newHeight < 100)
            {
                newHeight = 100;
                newWidth = newHeight * ((double)this.Columns) / ((double)this.Rows);
            }
            if(newHeight + PageObject.YPosition > parentPage.PageHeight)
            {
                newHeight = PageObject.Height;
                newWidth = newHeight * ((double)this.Columns) / ((double)this.Rows);
            }
            if(newWidth + PageObject.XPosition > parentPage.PageWidth)
            {
                newWidth = PageObject.Width;
                newHeight = newWidth * ((double)this.Rows) / ((double)this.Columns);
            }
            //TO DO Liz - make sure resizing doesn't change ratio
            //TO DO Liz - make it so resizing preserves divisions

            CLPServiceAgent.Instance.ChangePageObjectDimensions(PageObject, newHeight, newWidth);
        }


        /// <summary>
        /// Gets the CreateHorizontalDivisionCommand command.
        /// </summary>
        public Command CreateHorizontalDivisionCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the CreateHorizontalDivisionCommand command is executed.
        /// </summary>
        private void OnCreateHorizontalDivisionCommandExecute()
        {
            HorizontalDivs.Add(RightArrowPosition);
        }

        /// <summary>
        /// Gets the CreateVerticalDivisionCommand command.
        /// </summary>
        public Command CreateVerticalDivisionCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the CreateVerticalDivisionCommand command is executed.
        /// </summary>
        private void OnCreateVerticalDivisionCommandExecute()
        {
             VerticalDivs.Add(BottomArrowPosition);
        }

        /// <summary>
        /// Shows Modal Window Keypad to input Parts manually.
        /// </summary>
        public Command EnterRowsCommand { get; private set; }

        private void OnEnterRowsCommandExecute()
        {
            if(App.MainWindowViewModel.IsAuthoring || !(PageObject as CLPStamp).PartsAuthorGenerated)
            {
                KeypadWindowView keyPad = new KeypadWindowView();
                keyPad.Owner = Application.Current.MainWindow;
                keyPad.WindowStartupLocation = WindowStartupLocation.Manual;
                keyPad.Top = 100;
                keyPad.Left = 100;
                keyPad.ShowDialog();
                if(keyPad.DialogResult == true && keyPad.NumbersEntered.Text.Length > 0)
                {
                    (PageObject as CLPArray).Rows = Int32.Parse(keyPad.NumbersEntered.Text);
                }
            }
        }

        #endregion //Commands


        #region Methods


        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            hoverTimer.Interval = 1000;
            if(hitBoxName == "ArrayBodyHitBox")
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
            if(hitBoxName == "ArrayBottomHitBox")
            {
                IsDefaultAdornerVisible = false;
                IsRightAdornerVisible = false;
                IsBottomAdornerVisible = true;
                //TO DO Liz - create division
                //use isMouseDown
                if(isMouseDown)
                {
                    //CREATE DIVISION
                }

            }
            if(hitBoxName == "ArrayRightHitBox")
            {
                IsDefaultAdornerVisible = false;
                IsRightAdornerVisible = true;
                IsBottomAdornerVisible = false;
                //TO DO Liz - create division
            }

            return !hoverTimeElapsed;       
        }



        #endregion //Methods
    }
}
