using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Windows.Input;

namespace CLP.Models
{
    public enum StrokeProperty
    {
        CreationTime
    }

    [DataContract]
    [Serializable]
    public class StrokeDTO
    {
        #region Constructors

        public StrokeDTO(Stroke source)
        {
            StrokePoints = new List<StylusPointDTO>();
            ExtendedProperties = new Dictionary<StrokeProperty, object>();

            foreach(var point in source.StylusPoints)
            {
                StrokePoints.Add(new StylusPointDTO(point));
            }

            StrokeDrawingAttributes = new DrawingAttributesDTO(source.DrawingAttributes);
            StrokeID = source.GetStrokeUniqueID();
        }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Individual Points of a stroke.
        /// </summary>
        [DataMember]
        public List<StylusPointDTO> StrokePoints { get; set; }

        /// <summary>
        /// Drawing Attributes for the Stroke (Height/Width/Color/etc).
        /// </summary>
        [DataMember]
        public DrawingAttributesDTO StrokeDrawingAttributes { get; set; }

        /// <summary>
        /// Unique ID of the Stroke.
        /// </summary>
        [DataMember]
        public string StrokeID { get; set; }

        /// <summary>
        /// Unique ID of the Person who made the stroke.
        /// </summary>
        [DataMember]
        public string StrokeOwnerID { get; set; }

        /// <summary>
        /// Futureproof serialization by allowing additional properties to be added to a stroke using Extended Properties.
        /// </summary>
        [DataMember]
        public Dictionary<StrokeProperty, object> ExtendedProperties { get; set; }

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
