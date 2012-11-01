using System.Windows;
using Catel.Windows.Controls;
using Classroom_Learning_Partner.ViewModels;
using Classroom_Learning_Partner.Views.Modal_Windows;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPGroupingRegionView.xaml.
    /// </summary>
    public partial class CLPGroupingRegionView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPGroupingRegionView"/> class.
        /// </summary>
        public CLPGroupingRegionView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPGroupingRegionViewModel);
        }
    }
}
