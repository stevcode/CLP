using System;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace CLP.Entities.Demo
{
    [Serializable]
    public class StrokePathDTO
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="StrokePathDTO" /> from scratch.
        /// </summary>
        public StrokePathDTO() { }

        public StrokePathDTO(Stroke stroke)
        {
            var firstPoint = stroke.StylusPoints[0];

            var geometry = new StreamGeometry();
            using(var geometryContext = geometry.Open())
            {
                geometryContext.BeginFigure(new Point(firstPoint.X, firstPoint.Y), true, false);
                foreach(var point in stroke.StylusPoints)
                {
                    geometryContext.LineTo(new Point(point.X, point.Y), true, true);
                }
            }
            geometry.Freeze();

            PathData = geometry;
            PathColor = (SolidColorBrush)new BrushConverter().ConvertFromString(stroke.DrawingAttributes.Color.ToString());
            PathWidth = stroke.DrawingAttributes.Width;
            IsHighlighter = stroke.DrawingAttributes.IsHighlighter;
        }

        #endregion //Constructors

        #region Properties

        public Geometry PathData { get; set; }
 
        public SolidColorBrush PathColor { get; set; }

        public double PathWidth { get; set; }

        public bool IsHighlighter { get; set; }

        #endregion //Properties
    }
}
