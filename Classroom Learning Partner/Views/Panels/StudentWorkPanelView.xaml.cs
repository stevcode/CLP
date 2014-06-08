using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for StudentWorkPanelView.xaml
    /// </summary>
    public partial class StudentWorkPanelView
    {
        public StudentWorkPanelView()
        {
            InitializeComponent();
            CloseViewModelOnUnloaded = false;
        }

        public void recalculateSubmissionCounts()
        {
            Run submissionsCount = (Run)FindName("SubmissionsCount");
            BindingOperations.GetBindingExpression(submissionsCount, Run.TextProperty).UpdateTarget();
        }

        protected override Type GetViewModelType() { return typeof(StudentWorkPanelViewModel); }

        private ToggleButton _lastCheckedToggleButton;
        private void ShowSubmissionsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var toggleButton = e.Source as ToggleButton;
            if(toggleButton == null)
            {
                return;
            }
            //TODO check toggles according to some appropriate condition (currently never)
            toggleButton.IsChecked = false;

            /*if(_lastCheckedToggleButton != null &&
               !Equals(_lastCheckedToggleButton, toggleButton))
            {
                _lastCheckedToggleButton.IsChecked = false;
            }
            _lastCheckedToggleButton = toggleButton;
            if(toggleButton.IsChecked == false)
            {
                toggleButton.IsChecked = true;
            }*/
        }
    }
}