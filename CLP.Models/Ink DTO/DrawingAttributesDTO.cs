using System;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Windows.Media;

namespace CLP.Models
{
    [DataContract]
    [Serializable]
    public class DrawingAttributesDTO
    {

        #region Constructors

        public DrawingAttributesDTO(DrawingAttributes source)
        {
            Height = source.Height;
            Width = source.Width;
            IsHighlighter = source.IsHighlighter;
            FitToCurve = source.FitToCurve;
            IgnorePressure = source.IgnorePressure;
            StrokeColor = source.Color.ToString();
            StylusTip = source.StylusTip;
            StylusTripTransform = source.StylusTipTransform;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Height of StylusTip
        /// </summary>
        [DataMember]
        public double Height { get; set; }

        /// <summary>
        /// Width of StylusTip
        /// </summary>
        [DataMember]
        public double Width { get; set; }

        /// <summary>
        /// Signifies Stroke is translucent.
        /// </summary>
        [DataMember]
        public bool IsHighlighter { get; set; }

        /// <summary>
        /// Signifies Stroke is using Beizer Curves for smoothing.
        /// </summary>
        [DataMember]
        public bool FitToCurve { get; set; }

        /// <summary>
        /// Signifies Stroke is using pressure information.
        /// </summary>
        [DataMember]
        public bool IgnorePressure { get; set; }

        /// <summary>
        /// Color of the Stroke.
        /// </summary>
        [DataMember]
        public string StrokeColor { get; set; }

        /// <summary>
        /// StylusTip of Stroke.
        /// </summary>
        [DataMember]
        public StylusTip StylusTip { get; set; }

        /// <summary>
        /// Transformation Matrix of StylusTip of Stroke.
        /// </summary>
        [DataMember]
        public Matrix StylusTripTransform { get; set; }

        #endregion //Properties

        #region Methods

        public DrawingAttributes ToDrawingAttributes()
        {
            var drawingAttributes = new DrawingAttributes
                                    {
                                        Height = Height,
                                        Width = Width,
                                        IsHighlighter = IsHighlighter,
                                        FitToCurve = FitToCurve,
                                        IgnorePressure = IgnorePressure,
                                        StylusTip = StylusTip,
                                        StylusTipTransform = StylusTripTransform
                                    };

            var convertFromString = ColorConverter.ConvertFromString(StrokeColor);
            if(convertFromString != null)
            {
                drawingAttributes.Color = (Color)convertFromString;
            }
            else
            {
                drawingAttributes.Color = Colors.Black;
            }

            return drawingAttributes;
        }

        #endregion //Methods
    }
}
