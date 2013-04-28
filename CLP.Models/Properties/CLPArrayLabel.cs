using System;


namespace CLP.Models
{
    [Serializable]
    public class CLPArrayLabel
    {
        public CLPArrayLabel(double Position, int Value)
        {
            this.Position = Position;
            this.Value = Value;
        }

        /// <summary>
        /// Gets or sets the Position value.
        /// </summary>
        public double Position
        {
            get { return GetValue<double>(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        /// <summary>
        /// Register the Position property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PositionProperty = RegisterProperty("Position", typeof(double), null);

        /// <summary>
        /// Value of the label - number to be displayed. If null a question mark will be displayed.
        /// </summary>
        public int Value
        {
            get { return GetValue<int>(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Register the Value property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof(int), null);
    }
}