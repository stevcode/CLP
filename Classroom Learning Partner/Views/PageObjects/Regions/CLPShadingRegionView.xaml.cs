using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Catel.Windows.Controls;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for CLPSquareShapeView.xaml.</summary>
    public partial class CLPShadingRegionView : UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="CLPGridView" /> class.</summary>
        public CLPShadingRegionView()
        {
            InitializeComponent();
        }

        public void ShowContentsCommand(object sender, RoutedEventArgs e)
        {
            //string result = (DataContext as CLPShadingRegionViewModel).GetStringRepresentation();
            //MessageBox.Show(result);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.Grid.Rows * this.Grid.Columns; i++)
            {
                Rectangle rect = new Rectangle();
                rect.Stroke = Brushes.Black;
                rect.StrokeThickness = 0.5;
                this.Grid.Children.Add(rect);
            }
        }
    }
}