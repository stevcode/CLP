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

        }

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

        /// <summary>
        /// List of vertical divisions in the array - where the lines are actually placed.
        /// </summary>
        public ObservableCollection<double> VerticalDivs
        {
            get { return GetValue<ObservableCollection<double>>(VerticalDivsProperty); }
            set { SetValue(VerticalDivsProperty, value); }
        }

        /// <summary>
        /// Register the VerticalDivs property so it is known in the class.
        /// </summary>
        public static readonly PropertyData VerticalDivsProperty = RegisterProperty("VerticalDivs", typeof(ObservableCollection<double>), null);

        /// <summary>
        /// List of horizontal divisions in the array - where lines is actually placed.
        /// </summary>
        public ObservableCollection<double> HorizontalDivs
        {
            get { return GetValue<ObservableCollection<double>>(HorizontalDivsProperty); }
            set { SetValue(HorizontalDivsProperty, value); }
        }

        /// <summary>
        /// Register the HorizontalDivs property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HorizontalDivsProperty = RegisterProperty("HorizontalDivs", typeof(ObservableCollection<double>), null);

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
            // TODO: Handle command logic here
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
            // TODO: Handle command logic here
        }


        #endregion //Commands


        #region Methods


        public override bool SetInkCanvasHitTestVisibility(string hitBoxTag, string hitBoxName, bool isInkCanvasHitTestVisibile, bool isMouseDown, bool isTouchDown, bool isPenDown)
        {
            if(hitBoxName == "ArrayBodyHitBox")
            {
                if(IsBackground)
                {
                    if(App.MainWindowViewModel.IsAuthoring)
                    {
                        OpenAdornerTimeOut = 0.0;
                        IsMouseOverShowEnabled = true;
                    }
                    else
                    {
                        IsMouseOverShowEnabled = false;
                    }
                }
                else
                {
                    OpenAdornerTimeOut = 1.0; //Liz - maybe change to 1.2?
                    IsMouseOverShowEnabled = true;
                }
                return false;
            }
            if(hitBoxName == "ArrayBottomHitBox")
            {
                //TO DO Liz - create division
            }
            if(hitBoxName == "ArrayRightHitBox")
            {
                //TO DO Liz - create division
            }
            return true;
            
            
        }



        #endregion //Methods
    }
}
