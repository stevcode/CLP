using System.Windows;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPShapeViewModel : ACLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPShapeViewModel class.
        /// </summary>
        public CLPShapeViewModel(Shape shape)
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
        public ShapeType ShapeType
        {
            get { return GetValue<ShapeType>(ShapeTypeProperty); }
            set { SetValue(ShapeTypeProperty, value); }
        }

        public static readonly PropertyData ShapeTypeProperty = RegisterProperty("ShapeType", typeof(ShapeType));

        public override string Title { get { return "ShapeVM"; } }

        /// <summary> 
        /// Gets the ResizeShapeCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeShapeCommand { get; set; }

        private void OnResizeShapeCommandExecute(DragDeltaEventArgs e)
        {
            // TODO: Entities
            //var parentPage = (App.MainWindowViewModel.Workspace as NotebookWorkspaceViewModel).Notebook.GetNotebookPageByID(PageObject.ParentPageID);

            //double newHeight = PageObject.Height + e.VerticalChange;
            //double newWidth = PageObject.Width + e.HorizontalChange;
            //if((PageObject as Shape).ShapeType == ShapeType.VerticalLine)
            //{
            //    newWidth = 20;
            //    if(PageObject.YPosition + newHeight > parentPage.Height)
            //    {
            //        newHeight = PageObject.Height;
            //    }

            //}
            //if((PageObject as Shape).ShapeType == ShapeType.HorizontalLine)
            //{
            //    newHeight = 20;
            //    if(PageObject.XPosition + newWidth > parentPage.Width)
            //    {
            //        newWidth = PageObject.Width;
            //    }
            //}
            //if(newHeight < 20)
            //{
            //    newHeight = 20;
            //}
            //if(newWidth < 20)
            //{
            //    newWidth = 20;
            //}
            //if(newHeight + PageObject.YPosition > parentPage.Height)
            //{
            //    newHeight = PageObject.Height;
            //}
            //if(newWidth + PageObject.XPosition > parentPage.Width)
            //{
            //    newWidth = PageObject.Width;
            //}

            //if((PageObject as Shape).ShapeType == ShapeType.Protractor)
            //{
            //    newWidth = 2.0*newHeight;
            //}

            //ChangePageObjectDimensions(PageObject, newHeight, newWidth);
        }

    }
}
