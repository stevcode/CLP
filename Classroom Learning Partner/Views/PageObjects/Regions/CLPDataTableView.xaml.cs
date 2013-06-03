using System.Windows;
using System.Windows.Shapes;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for CLPSquareShapeView.xaml.
    /// </summary>
    public partial class CLPDataTableView : Catel.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPGridView"/> class.
        /// </summary>
        public CLPDataTableView()
        {
            InitializeComponent();
        }

        public void ShowContentsCommand(object sender, RoutedEventArgs e)
        {
            string result = (DataContext as CLPDataTableViewModel).GetStringRepresentation();
            MessageBox.Show(result);
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(CLPDataTableViewModel);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("DIMENSIONS: " + this.Grid.Columns + " , " + this.Grid.Rows + " , " + this.Grid.Width);
            for (int i = 0; i < this.Grid.Rows * this.Grid.Columns; i++)
            {
                //Console.WriteLine("DOING THIS");
                Rectangle rect = new Rectangle();
                rect.Stroke = System.Windows.Media.Brushes.Black;
                rect.StrokeThickness = 0.5;
                this.Grid.Children.Add(rect);
            }
        }
    }
}
