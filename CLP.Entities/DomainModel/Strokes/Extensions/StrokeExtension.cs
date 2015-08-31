using System;
using System.Text;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Catel;

namespace CLP.Entities
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

        #region ExtendedProperties

        public static bool HasStrokeID(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            return stroke.ContainsPropertyData(StrokeIDKey);
        }

        public static string GetStrokeID(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            if (stroke.ContainsPropertyData(StrokeIDKey))
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

            if (stroke.ContainsPropertyData(StrokeOwnerIDKey))
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

            if (stroke.ContainsPropertyData(StrokeDifferentiationGroupKey))
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


            if (stroke.ContainsPropertyData(StrokeVersionIndexKey))
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

        #endregion //ExtendedProperties

        #region Transformation

        /// <summary>
        /// Scales a <see cref="Stroke" /> with respect to a center point.
        /// </summary>
        public static void Stretch(this Stroke stroke, double scaleX, double scaleY, double centerX, double centerY)
        {
            Argument.IsNotNull("stroke", stroke);

            var transform = new Matrix();
            transform.ScaleAt(scaleX, scaleY, centerX, centerY);
            stroke.Transform(transform, false);
        }

        /// <summary>
        /// Moves every <see cref="StylusPoint" /> in a <see cref="Stroke" /> by an offset.
        /// </summary>
        public static void Move(this Stroke stroke, double deltaX, double deltaY)
        {
            Argument.IsNotNull("stroke", stroke);

            var transform = new Matrix();
            transform.Translate(deltaX, deltaY);
            stroke.Transform(transform, true);
        }

        public static void Rotate(this Stroke stroke, double angle, double centerX, double centerY, double offsetX, double offsetY)
        {
            Argument.IsNotNull("stroke", stroke);

            var transform = new Matrix();
            transform.RotateAt(90, centerX, centerY);
            transform.Translate(offsetX, offsetY);
            stroke.Transform(transform, false);
        }

        #endregion //Transformation

        #region HitTesting

        public static double PercentContainedByBounds(this Stroke stroke, Rect bounds)
        {
            Argument.IsNotNull("stroke", stroke);
            Argument.IsNotNull("bounds", bounds);

            var da = stroke.DrawingAttributes;
            var stylusPoints = stroke.StylusPoints;
            var weightContained = 0.0;
            var weightNotContained = 0.0;
            for (var i = 0; i < stylusPoints.Count; i++)
            {
                var pointWeight = 0.0;
                if (i == 0)
                {
                    pointWeight += Math.Sqrt(da.Width * da.Width + da.Height * da.Height) / 2.0;
                }
                else
                {
                    var spine = (Point)stylusPoints[i] - (Point)stylusPoints[i - 1];
                    pointWeight += Math.Sqrt(spine.LengthSquared) / 2.0;
                }

                if (i == stylusPoints.Count - 1)
                {
                    pointWeight += Math.Sqrt(da.Width * da.Width + da.Height * da.Height) / 2.0;
                }
                else
                {
                    var spine = (Point)stylusPoints[i + 1] - (Point)stylusPoints[i];
                    pointWeight += Math.Sqrt(spine.LengthSquared) / 2.0;
                    
                }

                if (bounds.Contains((Point)stylusPoints[i]))
                {
                    weightContained += pointWeight;
                }
                else
                {
                    weightNotContained += pointWeight;
                }
            }

            var totalWeight = weightContained + weightNotContained;
            return weightContained / totalWeight;
        }

        public static double StrokeWeight(this Stroke stroke)
        {
            Argument.IsNotNull("stroke", stroke);

            var da = stroke.DrawingAttributes;
            var stylusPoints = stroke.StylusPoints;
            var weight = 0.0;
            for (var i = 0; i < stylusPoints.Count; i++)
            {
                var pointWeight = 0.0;
                if (i == 0)
                {
                    pointWeight += Math.Sqrt(da.Width * da.Width + da.Height * da.Height) / 2.0;
                }
                else
                {
                    var spine = (Point)stylusPoints[i] - (Point)stylusPoints[i - 1];
                    pointWeight += Math.Sqrt(spine.LengthSquared) / 2.0;
                }

                if (i == stylusPoints.Count - 1)
                {
                    pointWeight += Math.Sqrt(da.Width * da.Width + da.Height * da.Height) / 2.0;
                }
                else
                {
                    var spine = (Point)stylusPoints[i + 1] - (Point)stylusPoints[i];
                    pointWeight += Math.Sqrt(spine.LengthSquared) / 2.0;

                }

                weight += pointWeight;
            }

            return weight;
        }

        #endregion //HitTesting

        #region History

        public static Rect GetBoundsAtHistoryIndex(this Stroke stroke, CLPPage page, int historyIndex)
        {
            var strokeID = stroke.GetStrokeID();
            return stroke.GetBounds();
        }

        #endregion // History
    }
}