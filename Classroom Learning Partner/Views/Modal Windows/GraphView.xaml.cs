using System;
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
        }

        private void ChartCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender is Canvas canvas))
            {
                return;
            }

            var max = Math.Max(PlotPoint.XMax, PlotPoint.YMax);
            var min = Math.Min(PlotPoint.XMin, PlotPoint.YMin);
            var diff = max - min;
            var scale = PlotPoint.GRAPH_LENGTH / diff;

            var mousePos = e.GetPosition(canvas);
            var xMousePos = max - (mousePos.X / scale);
            var yMousePos = max - ((PlotPoint.GRAPH_LENGTH - mousePos.Y) / scale);

            //var xDiff = PlotPoint.XMax - PlotPoint.XMin;
            //var xScale = PlotPoint.GRAPH_LENGTH / xDiff;
            //var xMousePos = PlotPoint.XMax - (mousePos.X / xScale);

            //var yDiff = PlotPoint.YMax - PlotPoint.YMin;
            //var yScale = PlotPoint.GRAPH_LENGTH / yDiff;
            //var yMousePos = PlotPoint.YMax - ((PlotPoint.GRAPH_LENGTH - mousePos.Y) / yScale);

            Title = $"X: {xMousePos}, Y: {yMousePos}";
        }
    }
}