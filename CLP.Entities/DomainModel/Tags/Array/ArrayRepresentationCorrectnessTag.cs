using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum ArrayRepresentationIncorrectReason
    {
        SwappedFactors,
        WrongFactors,
        ProductAsFactor,
        OneDimensionCorrect,
        Other
    }

    [Serializable]
    public class ArrayRepresentationCorrectnessTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ArrayRepresentationCorrectnessTag" /> from scratch.
        /// </summary>
        public ArrayRepresentationCorrectnessTag() { }

        /// <summary>
        /// Initializes <see cref="ArrayRepresentationCorrectnessTag" />.
        /// </summary>
        public ArrayRepresentationCorrectnessTag(CLPPage parentPage, Origin origin, Correctness correctness, List<ArrayRepresentationIncorrectReason> incorrectReasons)
            : base(parentPage, origin)
        {
            Correctness = correctness;
            ArrayIncorrectReasons = incorrectReasons;
        }

        /// <summary>
        /// Initializes <see cref="ArrayRepresentationCorrectnessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayRepresentationCorrectnessTag(SerializationInfo info, StreamingContext context)
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
        public List<ArrayRepresentationIncorrectReason> ArrayIncorrectReasons
        {
            get { return GetValue<List<ArrayRepresentationIncorrectReason>>(ArrayIncorrectReasonsProperty); }
            set { SetValue(ArrayIncorrectReasonsProperty, value); }
        }

        public static readonly PropertyData ArrayIncorrectReasonsProperty = RegisterProperty("ArrayIncorrectReasons", typeof(List<ArrayRepresentationIncorrectReason>));

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Array; }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("{0}{1}",
                                     Correctness,
                                     Correctness == Correctness.Correct || Correctness == Correctness.Unknown
                                         ? string.Empty
                                         : " due to: " + string.Join(", ", ArrayIncorrectReasons));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}