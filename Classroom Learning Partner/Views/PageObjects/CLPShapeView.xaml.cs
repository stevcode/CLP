using System.Windows;
using System.Windows.Controls.Primitives;
using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
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
