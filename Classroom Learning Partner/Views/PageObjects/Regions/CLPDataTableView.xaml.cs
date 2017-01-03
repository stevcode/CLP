using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Catel.Windows.Controls;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for CLPSquareShapeView.xaml.</summary>
    public partial class CLPDataTableView : UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="CLPGridView" /> class.</summary>
        public CLPDataTableView()
        {
            InitializeComponent();
        }

        public void ShowContentsCommand(object sender, RoutedEventArgs e)
        {
            //string result = (DataContext as CLPDataTableViewModel).GetStringRepresentation();
            //MessageBox.Show(result);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("DIMENSIONS: " + this.Grid.Columns + " , " + this.Grid.Rows + " , " + this.Grid.Width);
            for (int i = 0; i < this.Grid.Rows * this.Grid.Columns; i++)
            {
                //Debug.WriteLine("DOING THIS");
                Rectangle rect = new Rectangle();
                rect.Stroke = Brushes.Black;
                rect.StrokeThickness = 0.5;
                this.Grid.Children.Add(rect);
            }
        }
    }
}