using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Model;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPShapeViewModel : ACLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPImageViewModel class.
        /// </summary>
        public CLPShapeViewModel(CLPShape shape)
            : base()
        {
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
            CLPPage parentPage = (App.MainWindowViewModel.SelectedWorkspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            double newHeight = PageObject.Height + e.VerticalChange;
            double newWidth = PageObject.Width + e.HorizontalChange;
            if((PageObject as CLPShape).ShapeType == CLP.Models.CLPShape.CLPShapeType.VerticalLine)
            {
                newWidth = 10;
            }
            if((PageObject as CLPShape).ShapeType == CLP.Models.CLPShape.CLPShapeType.HorizontalLine)
            {
                newHeight = 10;
            }
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

            CLPServiceAgent.Instance.ChangePageObjectDimensions(PageObject, newHeight, newWidth);
        }

    }
}
