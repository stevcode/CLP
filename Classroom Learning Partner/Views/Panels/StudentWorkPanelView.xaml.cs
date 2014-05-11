using System;
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
    }
}