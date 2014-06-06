using System;
using System.Windows.Input;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for DisplaysPanelView.xaml.
    /// </summary>
    public partial class DisplaysPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplaysPanelView" /> class.
        /// </summary>
        public DisplaysPanelView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }

        protected override Type GetViewModelType() { return typeof(DisplaysPanelViewModel); }
        private void UIElement_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e) { e.Handled = true; }
    }
}