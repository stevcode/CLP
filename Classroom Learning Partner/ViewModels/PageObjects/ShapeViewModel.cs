using System;
using System.Windows.Controls.Primitives;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities.Old;

namespace Classroom_Learning_Partner.ViewModels
{
    public class ShapeViewModel : APageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ShapeViewModel class.
        /// </summary>
        public ShapeViewModel(Shape shape)
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
            var parentPage = PageObject.ParentPage;
            var shape = PageObject as Shape;
            if(shape == null)
            {
                return;
            }

            const int MIN_WIDTH = 20;
            const int MIN_HEIGHT = 20;

            var newWidth = Math.Max(MIN_WIDTH, PageObject.Width + e.HorizontalChange);
            newWidth = shape.ShapeType == ShapeType.VerticalLine ? shape.Width : Math.Min(newWidth, parentPage.Width - PageObject.XPosition);
            var newHeight = Math.Max(MIN_HEIGHT, PageObject.Height + e.VerticalChange);
            newHeight = shape.ShapeType == ShapeType.HorizontalLine ? shape.Height :  Math.Min(newHeight, parentPage.Height - PageObject.YPosition);

            // BUG: Steve - Protractor can be resized passed the Width of the page.
            if(shape.ShapeType == ShapeType.Protractor)
            {
                newWidth = 2.0 * newHeight;
            }
            
            ChangePageObjectDimensions(PageObject, newHeight, newWidth);
        }
    }
}
