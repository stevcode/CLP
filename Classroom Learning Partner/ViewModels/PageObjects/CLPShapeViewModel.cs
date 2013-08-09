using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPShapeViewModel : ACLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPShapeViewModel class.
        /// </summary>
        public CLPShapeViewModel(CLPShape shape)
        {
            hoverTimer.Interval = 2300;
            CloseAdornerTimeOut = 0.15;
            PageObject = shape;

            ResizeShapeCommand = new Command<DragDeltaEventArgs>(OnResizeShapeCommandExecute);
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public CLPShape.CLPShapeType ShapeType
        {
            get { return GetValue<CLPShape.CLPShapeType>(ShapeTypeProperty); }
            set { SetValue(ShapeTypeProperty, value); }
        }

        /// <summary>
        /// Register the ShapeType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ShapeTypeProperty = RegisterProperty("ShapeType", typeof(CLPShape.CLPShapeType));

        public override string Title { get { return "ShapeVM"; } }

        /// <summary> 
        /// Gets the ResizeShapeCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeShapeCommand { get; set; }

        private void OnResizeShapeCommandExecute(DragDeltaEventArgs e)
        {
            var parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            double newHeight = PageObject.Height + e.VerticalChange;
            double newWidth = PageObject.Width + e.HorizontalChange;
            if((PageObject as CLPShape).ShapeType == CLP.Models.CLPShape.CLPShapeType.VerticalLine)
            {
                newWidth = 20;
                if(PageObject.YPosition + newHeight > parentPage.PageHeight)
                {
                    newHeight = PageObject.Height;
                }

            }
            if((PageObject as CLPShape).ShapeType == CLP.Models.CLPShape.CLPShapeType.HorizontalLine)
            {
                newHeight = 20;
                if(PageObject.XPosition + newWidth > parentPage.PageWidth)
                {
                    newWidth = PageObject.Width;
                }
            }
            if(newHeight < 20)
            {
                newHeight = 20;
            }
            if(newWidth < 20)
            {
                newWidth = 20;
            }
            if(newHeight + PageObject.YPosition > parentPage.PageHeight)
            {
                newHeight = PageObject.Height;
            }
            if(newWidth + PageObject.XPosition > parentPage.PageWidth)
            {
                newWidth = PageObject.Width;
            }

            if((PageObject as CLPShape).ShapeType == CLPShape.CLPShapeType.Protractor)
            {
                newWidth = 2.0*newHeight;
            }

            CLPServiceAgent.Instance.ChangePageObjectDimensions(PageObject, newHeight, newWidth);
        }

    }
}
