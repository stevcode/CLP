using System.Windows;
using Catel.Windows.Controls;
using Classroom_Learning_Partner.Views.Modal_Windows;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for CLPHandwritingRegionView.xaml.</summary>
    public partial class CLPHandwritingRegionView : UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="CLPHandwritingRegionView" /> class.</summary>
        public CLPHandwritingRegionView()
        {
            InitializeComponent();
        }

        public void EditInkRegion(object sender, RoutedEventArgs e)
        {
            CustomizeInkRegionView optionChooser = new CustomizeInkRegionView();
            optionChooser.Owner = Application.Current.MainWindow;
            optionChooser.ShowDialog();
            if (optionChooser.DialogResult == true)
            {
                int selected_type = optionChooser.ExpectedType.SelectedIndex;
                this.AnalysisType.Text = selected_type.ToString();
            }
        }
    }
}