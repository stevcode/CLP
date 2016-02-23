using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>Interaction logic for OPTICSReachabilityPlotView.xaml</summary>
    public partial class OPTICSReachabilityPlotView : Window
    {
        public List<Point> Reachability = new List<Point>(); 
        private Polyline p1;
        private double xMax = 0.0;
        private double yMin = 0.0;
        private double yMax = 0.0;

        public OPTICSReachabilityPlotView()
        {
            InitializeComponent();
        }

        private void AddChart()
        {
            var reachabilityDistances = Reachability.Select(r => r.Y).Where(d => !double.IsInfinity(d) && !double.IsNaN(d)).ToList();
            xMax = Reachability.Count;
            yMin = reachabilityDistances.Min() - 5;
            yMax = reachabilityDistances.Max() + 5;

            p1 = new Polyline();
            p1.Stroke = Brushes.Black;
            p1.StrokeThickness = 2;
            //add points example
            //for (var i = 0; i < 70; i++)
            //{
            //    var x = i / 5.0;
            //    var y = Math.Sin(x);
            //    p1.Points.Add(NormalizePoint(new Point(x, y)));
            //}
            for (var i = 0; i < reachabilityDistances.Count; i++)
            {
                var d = reachabilityDistances[i];
                var x = i;
                var y = d;
                p1.Points.Add(NormalizePoint(new Point(x, y)));
            }

            chartCanvas.Children.Add(p1);
        }

        private Point NormalizePoint(Point pt)
        {
            var result = new Point();
            result.X = pt.X * chartCanvas.Width / xMax;
            result.Y = chartCanvas.Height - (pt.Y - yMin) * chartCanvas.Height / (yMax - yMin);
            return result;
        }

        private void ChartGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            chartCanvas.Width = chartGrid.ActualWidth;
            chartCanvas.Height = chartGrid.ActualHeight;
            chartCanvas.Children.Clear();
            AddChart();
        }
    }
}