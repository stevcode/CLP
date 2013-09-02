using System;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for DisplayListPanelView.xaml.
    /// </summary>
    public partial class DisplayListPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayListPanelView"/> class.
        /// </summary>
        public DisplayListPanelView()
        {
            InitializeComponent();
        }

        protected override Type GetViewModelType()
        {
            return typeof(DisplayListPanelViewModel);
        }
    }
}
