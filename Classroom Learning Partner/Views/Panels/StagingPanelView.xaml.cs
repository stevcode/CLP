using System;
using System.Windows;
using System.Windows.Controls.Primitives;
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

        private void CorrectnessTagButtons_OnClick(object sender, RoutedEventArgs e)
        {
            var toggleButton = e.Source as ToggleButton;
            if(toggleButton == null)
            {
                return;
            }
            toggleButton.IsChecked = true;
        }

        protected override Type GetViewModelType() { return typeof(StagingPanelViewModel); }
        private void PagesListBox_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e) { e.Handled = true; }
    }
}