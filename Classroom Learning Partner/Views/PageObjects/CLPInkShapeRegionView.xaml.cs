using Catel.Windows.Controls;
using System.Windows;
using System;
using Classroom_Learning_Partner.Views.Modal_Windows;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPHandwritingRegionView.xaml.
    /// </summary>
    public partial class CLPInkShapeRegionView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPHandwritingRegionView"/> class.
        /// </summary>
        public CLPInkShapeRegionView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPInkShapeRegionViewModel);
        }
    }
}
