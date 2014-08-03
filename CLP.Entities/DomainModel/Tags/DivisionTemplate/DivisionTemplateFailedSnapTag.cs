using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionTemplateFailedSnapTag : ATagBase
    {
        public enum AcceptedValues
        {
            SnappedArrayTooLarge,
            SnappedIncorrectDimension,
            SnappedWrongOrientation,
        }

        #region Constructors

        /// <summary>
        /// Initializes <see cref="DivisionTemplateFailedSnapTag" /> from scratch.
        /// </summary>
        public DivisionTemplateFailedSnapTag() { }

        /// <summary>
        /// Initializes <see cref="DivisionTemplateFailedSnapTag" /> from <see cref="AcceptedValues" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateFailedSnapTag" /> belongs to.</param>
        /// <param name="value">The value of the <see cref="DivisionTemplateFailedSnapTag" />, parsed from <see cref="AcceptedValues" />.</param>
        public DivisionTemplateFailedSnapTag(CLPPage parentPage, Origin origin, AcceptedValues value, int numberOfAttempts)
            : base(parentPage, origin)
        {
            Value = value;
            NumberOfAttempts = numberOfAttempts;
        }

        /// <summary>
        /// Initializes <see cref="DivisionTemplateFailedSnapTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateFailedSnapTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Value of the Starred Tag.
        /// </summary>
        public AcceptedValues Value
        {
            get { return GetValue<AcceptedValues>(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof(AcceptedValues));

        /// <summary>
        /// Number of times a type of failed snap was attempted.
        /// </summary>
        public int NumberOfAttempts
        {
            get { return GetValue<int>(NumberOfAttemptsProperty); }
            set { SetValue(NumberOfAttemptsProperty, value); }
        }

        public static readonly PropertyData NumberOfAttemptsProperty = RegisterProperty("NumberOfAttempts", typeof(int), 0);

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.DivisionTemplate; }
        }

        public override string FormattedValue
        {
            get { return string.Format("{0} {1} time(s).", Value, NumberOfAttempts); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}