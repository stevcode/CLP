using System;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace CLP.Models
{
    [DataContract]
    [Serializable]
    public class StylusPointDTO
    {
        public StylusPointDTO(StylusPoint source)
        {
            X = source.X;
            Y = source.Y;
            PressureFactor = source.PressureFactor;
        }

        /// <summary>
        /// X coordinate of the point.
        /// </summary>
        [DataMember]
        public double X { get; set; }

        /// <summary>
        /// Y coordinate of the point.
        /// </summary>
        [DataMember]
        public double Y { get; set; }

        /// <summary>
        /// Amount of pressure exerted by Stylus, determining variable width of stroke at this point.
        /// </summary>
        [DataMember]
        public float PressureFactor { get; set; }

        public StylusPoint ToStylusPoint()
        {
            return new StylusPoint(X, Y) { PressureFactor = PressureFactor };
        }
    }
}
