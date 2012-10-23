using System.Windows;
using Catel.Data;
using Catel.MVVM;
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
            if(App.MainWindowViewModel.IsAuthoring)
            {
                AllowAdorner = Visibility.Visible;
            }
            else
            {
                AllowAdorner = Visibility.Hidden;
            }
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

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if(propertyName == "IsAuthoring")
            {
                if((viewModel as MainWindowViewModel).IsAuthoring)
                {
                    AllowAdorner = Visibility.Visible;
                }
                else
                {
                    AllowAdorner = Visibility.Hidden;
                }
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }
    }
}
