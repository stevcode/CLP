using System;
using System.Text;
using System.Windows.Ink;
using System.Windows.Input;
using Catel;

namespace CLP.Entities
{
    /// <summary>
    /// Extension methods for the <see cref="Stroke" /> class.
    /// </summary>
    public static class StrokeExtension
    {
        private static readonly Guid StrokeIDKey = new Guid("00000000-0000-0000-0000-000000000001");
        private static readonly Guid StrokePersonIDKey = new Guid("00000000-0000-0000-0000-000000000002");

        public static StrokeDTO ToStrokeDTO(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            var strokeDTO = new StrokeDTO
                            {
                                ID = stroke.GetStrokeID(),
                                PersonID = stroke.GetStrokePersonID(),
                                Height = stroke.DrawingAttributes.Height,
                                Width = stroke.DrawingAttributes.Width,
                                IsHighlighter = stroke.DrawingAttributes.IsHighlighter,
                                FitToCurve = stroke.DrawingAttributes.FitToCurve,
                                IgnorePressure = stroke.DrawingAttributes.IgnorePressure,
                                Color = stroke.DrawingAttributes.Color.ToString(),
                                StylusTip = stroke.DrawingAttributes.StylusTip == StylusTip.Ellipse ? StylusTipType.Ellipse : StylusTipType.Rectangle
                            };

            var strokePoints = new StringBuilder();
            foreach(StylusPoint strokePoint in stroke.StylusPoints)
            {
                strokePoints.Append(strokePoint.X);
                strokePoints.Append(':');
                strokePoints.Append(strokePoint.Y);
                strokePoints.Append(':');
                strokePoints.Append(strokePoint.PressureFactor);
                strokePoints.Append(",");
            }
            strokePoints.Remove(strokePoints.Length - 1, 1);
            strokeDTO.StrokePoints = strokePoints.ToString();

            return strokeDTO;
        }

        public static string GetStrokeID(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            return stroke.GetPropertyData(StrokeIDKey) as string;
        }

        public static void SetStrokeID(this Stroke stroke, string uniqueID)
        {
            Argument.IsNotNull("stroke", stroke);

            stroke.AddPropertyData(StrokeIDKey, uniqueID);
        }

        public static string GetStrokePersonID(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            return stroke.GetPropertyData(StrokePersonIDKey) as string;
        }

        public static void SetStrokePersonID(this Stroke stroke, string uniqueID)
        {
            Argument.IsNotNull("stroke", stroke);

            stroke.AddPropertyData(StrokePersonIDKey, uniqueID);
        }
    }
}