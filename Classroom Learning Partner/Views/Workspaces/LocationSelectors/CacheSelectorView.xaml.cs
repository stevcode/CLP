using System;
using System.Windows.Input;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for CacheSelectorView.xaml</summary>
    public partial class CacheSelectorView
    {
        public CacheSelectorView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(CacheSelectorViewModel); }

        private void UIElement_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e) { e.Handled = true; }
    }
}