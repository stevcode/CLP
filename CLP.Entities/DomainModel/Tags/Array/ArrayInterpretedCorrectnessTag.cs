using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum ArrayIncorrectReason
    {
        SwappedFactors,
        WrongFactors,
        MisusedGivens,
        Other
    }

    [Serializable]
    public class ArrayInterpretedCorrectnessTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ArrayInterpretedCorrectnessTag" /> from scratch.
        /// </summary>
        public ArrayInterpretedCorrectnessTag() { }

        /// <summary>
        /// Initializes <see cref="ArrayInterpretedCorrectnessTag" />.
        /// </summary>
        public ArrayInterpretedCorrectnessTag(CLPPage parentPage, Origin origin, Correctness correctness, ArrayIncorrectReason incorrectReason = ArrayIncorrectReason.Other)
            : base(parentPage, origin)
        {
            Correctness = correctness;
            ArrayIncorrectReason = incorrectReason;
        }

        /// <summary>
        /// Initializes <see cref="ArrayInterpretedCorrectnessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayInterpretedCorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Type of correctness.
        /// </summary>
        public Correctness Correctness
        {
            get { return GetValue<Correctness>(CorrectnessProperty); }
            set { SetValue(CorrectnessProperty, value); }
        }

        public static readonly PropertyData CorrectnessProperty = RegisterProperty("Correctness", typeof(Correctness));

        /// <summary>
        /// Reason the Interpreted Correctness was set to Incorrect.
        /// </summary>
        public ArrayIncorrectReason ArrayIncorrectReason
        {
            get { return GetValue<ArrayIncorrectReason>(ArrayIncorrectReasonProperty); }
            set { SetValue(ArrayIncorrectReasonProperty, value); }
        }

        public static readonly PropertyData ArrayIncorrectReasonProperty = RegisterProperty("ArrayIncorrectReason", typeof(ArrayIncorrectReason));

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Array; }
        }

        public override string FormattedValue
        {
            get { return string.Format("{0}{1}", 
                                       Correctness,
                                       Correctness == Correctness.Correct ||
                                       Correctness == Correctness.Unknown ? string.Empty : "Incorrect due to: " + ArrayIncorrectReason); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}