using System;
using System.Windows.Input;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for StagingPanelView.xaml
    /// </summary>
    public partial class StagingPanelView
    {
        public StagingPanelView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }

        protected override Type GetViewModelType() { return typeof(StagingPanelViewModel); }
        private void PagesListBox_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e) { e.Handled = true; }
    }
}