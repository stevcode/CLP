using System.Windows;
using System.Windows.Input;
using Catel.Windows;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for ClassRosterView.xaml</summary>
    public partial class ClassRosterView
    {
        public const double WINDOW_HEIGHT = 600;

        public ClassRosterView()
            : base(DataWindowMode.Custom)
        {
            InitializeComponent();
            Top = SystemParameters.FullPrimaryScreenHeight / 2 - (ActualHeight / 2);
            Left = SystemParameters.FullPrimaryScreenWidth / 2 - (ActualWidth / 2);
        }

        private void ListBox_OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}
