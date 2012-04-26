namespace Classroom_Learning_Partner.Views.PageObjects
{
    using Catel.Windows.Controls;
    using System.Windows;
    using System;
    using Classroom_Learning_Partner.Views.Modal_Windows;
    using Classroom_Learning_Partner.ViewModels.PageObjects;

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
            SkipSearchingForInfoBarMessageControl = true;
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPInkShapeRegionViewModel);
        }
    }
}
