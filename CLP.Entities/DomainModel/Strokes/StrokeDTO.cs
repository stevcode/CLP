using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace CLP.Entities
{
    public enum StylusTipType
    {
        Ellipse,
        Rectangle
    }

    [Serializable]
    public class StrokeDTO
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="StrokeDTO" /> from scratch.
        /// </summary>
        public StrokeDTO() { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Unique Identifier of the <see cref="StrokeDTO" />.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Unique Identifier of the <see cref="Person" /> that made the <see cref="StrokeDTO" />.
        /// </summary>
        /// <remarks>
        /// Foreign Key.
        /// </remarks>
        public string PersonID { get; set; }

        public int VersionIndex { get; set; }

        /// <summary>
        /// List of all the points that make up a <see cref="StrokeDTO" />. Entire list stored as a string because
        /// individual points do not need to be inserted into the database. Points are delimited by a ','. Each
        /// individual point is a group whose parts are delimited by a ':', in the form of X:Y:PressureFactor. The
        /// Types of those parts are double:double:float, respectively.
        /// </summary>
        public string StrokePoints { get; set; }

        /// <summary>
        /// Height of the <see cref="StrokeDTO" /> tip.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Width of the <see cref="StrokeDTO" /> tip.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Signifies <see cref="StrokeDTO" /> is translucent.
        /// </summary>
        public bool IsHighlighter { get; set; }

        /// <summary>
        /// Signifies the <see cref="StrokeDTO" /> is using Beizer Curves for smoothing.
        /// </summary>
        public bool FitToCurve { get; set; }

        /// <summary>
        /// Signifies the <see cref="StrokeDTO" /> is not using pressure information.
        /// </summary>
        public bool IgnorePressure { get; set; }

        /// <summary>
        /// Color of the <see cref="StrokeDTO" />.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// <see cref="StylusTipType" /> of the <see cref="StrokeDTO" />.
        /// </summary>
        public StylusTipType StylusTip { get; set; }

        #endregion //Properties

        #region Methods

        public Stroke ToStroke()
        {
            var strokePoints = new StylusPointCollection();
            var pointGroups = StrokePoints.Split(',');
            foreach(var stylusPoint in pointGroups.Select(pointGroup => pointGroup.Split(':')).Select(pointValues => new StylusPoint
                                                                                                                             {
                                                                                                                                 X = Convert.ToDouble(pointValues[0]),
                                                                                                                                 Y = Convert.ToDouble(pointValues[1]),
                                                                                                                                 PressureFactor = Convert.ToSingle(pointValues[2])
                                                                                                                             }))
            {
                strokePoints.Add(stylusPoint);
            }

            var drawingAttributes = new DrawingAttributes
                                    {
                                        Height = Height,
                                        Width = Width,
                                        IsHighlighter = IsHighlighter,
                                        FitToCurve = FitToCurve,
                                        IgnorePressure = IgnorePressure,
                                        StylusTip = StylusTip == StylusTipType.Ellipse ? System.Windows.Ink.StylusTip.Ellipse : System.Windows.Ink.StylusTip.Rectangle
                                    };
            var convertFromString = ColorConverter.ConvertFromString(Color);
            drawingAttributes.Color = convertFromString != null ? (Color)convertFromString : Colors.Black;

            var stroke = new Stroke(strokePoints)
                         {
                             DrawingAttributes = drawingAttributes
                         };
            stroke.SetStrokeID(ID);
            stroke.SetStrokeOwnerID(PersonID);
            stroke.SetStrokeVersionIndex(VersionIndex);

            return stroke;
        }

        public static List<StrokeDTO> SaveInkStrokes(IEnumerable<Stroke> strokes)
        {
            var serializedStrokes = new List<StrokeDTO>();
            foreach(var stroke in strokes)
            {
                serializedStrokes.Add(stroke.ToStrokeDTO());
            }

            return serializedStrokes;
        }

        public static StrokeCollection LoadInkStrokes(IEnumerable<StrokeDTO> serializedStrokes)
        {
            var strokes = new StrokeCollection();
            foreach(var stroke in serializedStrokes.Where(strokeDTO => strokeDTO.StrokePoints.Any()).Select(strokeDTO => strokeDTO.ToStroke()).Where(stroke => stroke != null))
            {
                strokes.Add(stroke);
            }

            return strokes;
        }

        #endregion //Methods
    }
}