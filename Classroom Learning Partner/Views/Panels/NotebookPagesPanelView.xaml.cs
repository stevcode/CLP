using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for NotebookPagesPanelView.xaml.
    /// </summary>
    public partial class NotebookPagesPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotebookPagesPanelView" /> class.
        /// </summary>
        public NotebookPagesPanelView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }

        protected override Type GetViewModelType() { return typeof(NotebookPagesPanelViewModel); }

        private ToggleButton _lastCheckedToggleButton;
        private void ShowSubmissionsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var toggleButton = e.Source as ToggleButton;
            if(toggleButton == null)
            {
                return;
            }
            if(_lastCheckedToggleButton != null &&
               !Equals(_lastCheckedToggleButton, toggleButton))
            {
                _lastCheckedToggleButton.IsChecked = false;
            }
            _lastCheckedToggleButton = toggleButton;
            if(toggleButton.IsChecked == false)
            {
                toggleButton.IsChecked = true;
            }
        }

        private void PagesListBox_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e) { e.Handled = true; }
    }
}