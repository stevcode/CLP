using System.Windows;

namespace Classroom_Learning_Partner.Views.Modal_Windows
{
    /// <summary>
    /// Interaction logic for CustomizeInkRegionView.xaml
    /// </summary>
    public partial class CustomizeInkRegionView : Window
    {
        public CustomizeInkRegionView()
        {
            InitializeComponent();
        }

        public CustomizeInkRegionView(int AnalysisType)
        {
            this.ExpectedType.SelectedIndex = AnalysisType;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
