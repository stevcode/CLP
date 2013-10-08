using System;
using System.Windows;
using System.Windows.Controls.Primitives;
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

        private void MirrorDisplayToggle_Click(object sender, RoutedEventArgs e)
        {
            var toggleButton = e.Source as ToggleButton;
            var displayPanel = DisplayListPanelViewModel.GetDisplayListPanelViewModel();
            if(toggleButton == null || displayPanel == null)
            {
                return;
            }
            if((bool)!toggleButton.IsChecked)
            {
                displayPanel.ProjectedDisplayString = string.Empty;
            }
        }

        private void GridDisplayToggle_Click(object sender, RoutedEventArgs e)
        {
            var toggleButton = e.Source as ToggleButton;
            var displayPanel = DisplayListPanelViewModel.GetDisplayListPanelViewModel();
            if(toggleButton == null || displayPanel == null)
            {
                return;
            }
            if((bool)!toggleButton.IsChecked)
            {
                displayPanel.ProjectedDisplayString = string.Empty;
            }
        }
    }
}
