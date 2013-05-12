using System;
using System.Runtime.Serialization;
using System.Windows.Input;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class StylusPointDTO : DataObjectBase<StylusPointDTO>
    {
        public StylusPointDTO(StylusPoint source)
        {
            X = source.X;
            Y = source.Y;
            PressureFactor = source.PressureFactor;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected StylusPointDTO(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public double X
        {
            get { return GetValue<double>(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public static readonly PropertyData XProperty = RegisterProperty("X", typeof(double), 0.0);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public double Y
        {
            get { return GetValue<double>(YProperty); }
            set { SetValue(YProperty, value); }
        }

        public static readonly PropertyData YProperty = RegisterProperty("Y", typeof(double), 0.0);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public float PressureFactor
        {
            get { return GetValue<float>(PressureFactorProperty); }
            set { SetValue(PressureFactorProperty, value); }
        }

        public static readonly PropertyData PressureFactorProperty = RegisterProperty("PressureFactor", typeof(float), 0.0);

        public StylusPoint ToStylusPoint()
        {
            return new StylusPoint(X, Y) { PressureFactor = PressureFactor };
        }
    }
}
