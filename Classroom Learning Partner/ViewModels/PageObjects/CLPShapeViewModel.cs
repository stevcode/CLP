using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Model.CLPPageObjects;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPShapeViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPImageViewModel class.
        /// </summary>
        public CLPShapeViewModel(CLPShape shape)
            : base()
        {
            PageObject = shape;
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
    }
}
