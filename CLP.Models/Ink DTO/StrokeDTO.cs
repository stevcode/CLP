using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Windows.Input;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class StrokeDTO : DataObjectBase<StrokeDTO>
    {
        #region Constructors

        public StrokeDTO(Stroke source)
        {
            foreach(var point in source.StylusPoints)
            {
                StrokePoints.Add(new StylusPointDTO(point));
            }

            StrokeDrawingAttributes = new DrawingAttributesDTO(source.DrawingAttributes);
            StrokeID = source.GetStrokeUniqueID();
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected StrokeDTO(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Individual Points of a stroke.
        /// </summary>
        public ObservableCollection<StylusPointDTO> StrokePoints
        {
            get { return GetValue<ObservableCollection<StylusPointDTO>>(StrokePointsProperty); }
            set { SetValue(StrokePointsProperty, value); }
        }

        public static readonly PropertyData StrokePointsProperty = RegisterProperty("StrokePoints", typeof(ObservableCollection<StylusPointDTO>), () => new ObservableCollection<StylusPointDTO>());

        /// <summary>
        /// Drawing Attributes for the Stroke (Height/Width/Color/etc).
        /// </summary>
        public DrawingAttributesDTO StrokeDrawingAttributes
        {
            get { return GetValue<DrawingAttributesDTO>(StrokeDrawingAttributesProperty); }
            set { SetValue(StrokeDrawingAttributesProperty, value); }
        }

        public static readonly PropertyData StrokeDrawingAttributesProperty = RegisterProperty("StrokeDrawingAttributes", typeof(DrawingAttributesDTO));

        /// <summary>
        /// Unique ID of the Stroke.
        /// </summary>
        public string StrokeID
        {
            get { return GetValue<string>(StrokeIDProperty); }
            set { SetValue(StrokeIDProperty, value); }
        }

        public static readonly PropertyData StrokeIDProperty = RegisterProperty("StrokeID", typeof(string), "");

        /// <summary>
        /// Unique ID of the Person who made the stroke.
        /// </summary>
        public string StrokeOwnerID
        {
            get { return GetValue<string>(StrokeOwnerIDProperty); }
            set { SetValue(StrokeOwnerIDProperty, value); }
        }

        public static readonly PropertyData StrokeOwnerIDProperty = RegisterProperty("StrokeOwnerID", typeof(string), "");

        #endregion //Properties

        #region Methods

        public Stroke ToStroke()
        {
            var points = new StylusPointCollection();
            foreach(var stylusPointDTO in StrokePoints)
            {
                points.Add(stylusPointDTO.ToStylusPoint());
            }

            var stroke = new Stroke(points) {DrawingAttributes = StrokeDrawingAttributes.ToDrawingAttributes()};
            stroke.SetStrokeUniqueID(StrokeID);

            return stroke;
        }

        #endregion //Methods
    }
}
