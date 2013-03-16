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



        #endregion //Commands


        #region Methods

        /// <summary>
        /// Gets the ResizePageObjectCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeArrayCommand { get; set; }

        private void OnResizeArrayCommandExecute(DragDeltaEventArgs e)
        {
            CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            double newHeight = PageObject.Height + e.VerticalChange;
            double newWidth = PageObject.Width + e.HorizontalChange;
            if(newHeight < 10)
            {
                newHeight = 10;
            }
            if(newWidth < 10)
            {
                newWidth = 10;
            }
            if(newHeight + PageObject.YPosition > parentPage.PageHeight)
            {
                newHeight = PageObject.Height;
            }
            if(newWidth + PageObject.XPosition > parentPage.PageWidth)
            {
                newWidth = PageObject.Width;
            }

            //TO DO Liz - make it so resizing preserves divisions

            CLPServiceAgent.Instance.ChangePageObjectDimensions(PageObject, newHeight, newWidth);
        }

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
                    OpenAdornerTimeOut = 0.8;
                    IsMouseOverShowEnabled = true;
                }
                return false;
            }
            return true;
            
            //TO DO Liz - Bottom and side hit box check here
        }


        #endregion //Methods
    }
}
