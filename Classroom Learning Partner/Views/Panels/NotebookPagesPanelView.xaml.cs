using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Classroom_Learning_Partner.Views
{
    public partial class NotebookPagesPanelView
    {
        public NotebookPagesPanelView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }

        private ToggleButton _lastCheckedToggleButton;

        private void ShowSubmissionsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var toggleButton = e.Source as ToggleButton;
            if (toggleButton == null)
            {
                return;
            }
            if (_lastCheckedToggleButton != null &&
                !Equals(_lastCheckedToggleButton, toggleButton))
            {
                _lastCheckedToggleButton.IsChecked = false;
            }
            _lastCheckedToggleButton = toggleButton;
            if (toggleButton.IsChecked == false)
            {
                toggleButton.IsChecked = true;
            }
        }

        private void PagesListBox_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}