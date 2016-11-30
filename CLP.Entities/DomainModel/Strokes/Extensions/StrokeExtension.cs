using System;
using System.Text;
using System.Windows.Ink;
using System.Windows.Input;
using Catel;

namespace CLP.Entities.Ann
{
    /// <summary>
    /// Extension methods for the <see cref="Stroke" /> class.
    /// </summary>
    public static class StrokeExtension
    {
        private static readonly Guid StrokeIDKey = new Guid("00000000-0000-0000-0000-000000000001");
        private static readonly Guid StrokeOwnerIDKey = new Guid("00000000-0000-0000-0000-000000000002");
        private static readonly Guid StrokeVersionIndexKey = new Guid("00000000-0000-0000-0000-000000000003");
        private static readonly Guid StrokeDifferentiationGroupKey = new Guid("00000000-0000-0000-0000-000000000004");

        public static StrokeDTO ToStrokeDTO(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            var strokeDTO = new StrokeDTO
                            {
                                ID = stroke.GetStrokeID(),
                                PersonID = stroke.GetStrokeOwnerID(),
                                //DifferentiationGroup = stroke.GetStrokeDifferentiationGroup(),
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

            if(stroke.ContainsPropertyData(StrokeIDKey))
            {
                return stroke.GetPropertyData(StrokeIDKey) as string;
            }

            return "noStrokeID";
        }

        public static void SetStrokeID(this Stroke stroke, string uniqueID)
        {
            Argument.IsNotNull("stroke", stroke);

            stroke.AddPropertyData(StrokeIDKey, uniqueID);
        }

        public static string GetStrokeOwnerID(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);
            
            if(stroke.ContainsPropertyData(StrokeOwnerIDKey))
            {
                return stroke.GetPropertyData(StrokeOwnerIDKey) as string;
            }

            return "noStrokeOwnerID";
        }

        public static void SetStrokeOwnerID(this Stroke stroke, string uniqueID)
        {
            Argument.IsNotNull("stroke", stroke);

            stroke.AddPropertyData(StrokeOwnerIDKey, uniqueID);
        }

        public static string GetStrokeDifferentiationGroup(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            if(stroke.ContainsPropertyData(StrokeDifferentiationGroupKey))
            {
                return stroke.GetPropertyData(StrokeDifferentiationGroupKey) as string;
            }

            return "noStrokeDifferentiationGroup";
        }

        public static void SetStrokeDifferentiationGroup(this Stroke stroke, string group)
        {
            Argument.IsNotNull("stroke", stroke);

            stroke.AddPropertyData(StrokeDifferentiationGroupKey, group);
        }

        public static string GetStrokeVersionIndex(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            
            if(stroke.ContainsPropertyData(StrokeVersionIndexKey))
            {
                return stroke.GetPropertyData(StrokeVersionIndexKey) as string;
            }

            return "noStrokeVersionIndex";
        }

        public static void SetStrokeVersionIndex(this Stroke stroke, int index)
        {
            Argument.IsNotNull("stroke", stroke);

            stroke.AddPropertyData(StrokeVersionIndexKey, index);
        }

        public static bool HasStrokeID(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            return stroke.ContainsPropertyData(StrokeIDKey);
        }
    }
}