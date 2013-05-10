using System;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Windows.Media;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class DrawingAttributesDTO : DataObjectBase<DrawingAttributesDTO>
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

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected DrawingAttributesDTO(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Height of StylusTip
        /// </summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double), 0.0);

        /// <summary>
        /// Width of StylusTip
        /// </summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double), 0.0);

        /// <summary>
        /// Signifies Stroke is translucent.
        /// </summary>
        public bool IsHighlighter
        {
            get { return GetValue<bool>(IsHighlighterProperty); }
            set { SetValue(IsHighlighterProperty, value); }
        }

        public static readonly PropertyData IsHighlighterProperty = RegisterProperty("IsHighlighter", typeof(bool), false);

        /// <summary>
        /// Signifies Stroke is using Beizer Curves for smoothing.
        /// </summary>
        public bool FitToCurve
        {
            get { return GetValue<bool>(FitToCurveProperty); }
            set { SetValue(FitToCurveProperty, value); }
        }

        public static readonly PropertyData FitToCurveProperty = RegisterProperty("FitToCurve", typeof(bool), false);

        /// <summary>
        /// Signifies Stroke is using pressure information.
        /// </summary>
        public bool IgnorePressure
        {
            get { return GetValue<bool>(IgnorePressureProperty); }
            set { SetValue(IgnorePressureProperty, value); }
        }

        public static readonly PropertyData IgnorePressureProperty = RegisterProperty("IgnorePressure", typeof(bool), false);

        /// <summary>
        /// Color of the Stroke.
        /// </summary>
        public string StrokeColor
        {
            get { return GetValue<string>(StrokeColorProperty); }
            set { SetValue(StrokeColorProperty, value); }
        }

        public static readonly PropertyData StrokeColorProperty = RegisterProperty("StrokeColor", typeof(string), @"#FF000000");

        /// <summary>
        /// StylusTip of Stroke.
        /// </summary>
        public StylusTip StylusTip
        {
            get { return GetValue<StylusTip>(StylusTipProperty); }
            set { SetValue(StylusTipProperty, value); }
        }

        public static readonly PropertyData StylusTipProperty = RegisterProperty("StylusTip", typeof(StylusTip), () => new StylusTip());

        /// <summary>
        /// Transformation Matrix of StylusTip of Stroke.
        /// </summary>
        public Matrix StylusTripTransform
        {
            get { return GetValue<Matrix>(StylusTripTransformProperty); }
            set { SetValue(StylusTripTransformProperty, value); }
        }

        public static readonly PropertyData StylusTripTransformProperty = RegisterProperty("StylusTripTransform", typeof(Matrix), Matrix.Identity);

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
