using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Views
{
    public partial class GraphView
    {
        public GraphView(GraphViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
            Top = 0;
            Left = 0;
        }

        private void ChartCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender is Canvas canvas))
            {
                return;
            }

            var mousePos = e.GetPosition(canvas);
            var yMousePos = ((300 - mousePos.Y) / 5) - 10;
            var xMousePos = (mousePos.X / 5) - 10;
            //var denormalizedY = ((chartCanvas.Height - yMousePos) * (yMax - yMin) / chartCanvas.Height) + yMin;
            Title = $"X: {xMousePos}, Y: {yMousePos}";
        }
    }
}