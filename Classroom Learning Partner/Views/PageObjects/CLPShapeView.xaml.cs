using Classroom_Learning_Partner.ViewModels.PageObjects;

namespace Classroom_Learning_Partner.Views.PageObjects
{
    /// <summary>
    /// Interaction logic for CLPSquareShapeView.xaml.
    /// </summary>
    public partial class CLPShapeView : Catel.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPSquareShapeView"/> class.
        /// </summary>
        public CLPShapeView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPShapeViewModel);
        }
    }
}
