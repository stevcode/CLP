using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class ArrayYAxisDividerCorrectnessTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ArrayYAxisDividerCorrectnessTag" /> from scratch.
        /// </summary>
        public ArrayYAxisDividerCorrectnessTag() { }

        /// <summary>
        /// Initializes <see cref="ArrayYAxisDividerCorrectnessTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ArrayYAxisDividerCorrectnessTag" /> belongs to.</param>
        public ArrayYAxisDividerCorrectnessTag(CLPPage parentPage, Origin origin, Correctness correctness, List<ArrayAxisIncorrectReason> incorrectReasons)
            : base(parentPage, origin)
        {
            Correctness = correctness;
            ArrayIncorrectReasons = incorrectReasons;
        }

        /// <summary>
        /// Initializes <see cref="ArrayYAxisDividerCorrectnessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayYAxisDividerCorrectnessTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Type of correctness.</summary>
        public Correctness Correctness
        {
            get { return GetValue<Correctness>(CorrectnessProperty); }
            set { SetValue(CorrectnessProperty, value); }
        }

        public static readonly PropertyData CorrectnessProperty = RegisterProperty("Correctness", typeof(Correctness));

        /// <summary>Reason the Interpreted Correctness was set to Incorrect.</summary>
        public List<ArrayAxisIncorrectReason> ArrayIncorrectReasons
        {
            get { return GetValue<List<ArrayAxisIncorrectReason>>(ArrayIncorrectReasonsProperty); }
            set { SetValue(ArrayIncorrectReasonsProperty, value); }
        }

        public static readonly PropertyData ArrayIncorrectReasonsProperty = RegisterProperty("ArrayIncorrectReasons",
                                                                                             typeof(List<ArrayAxisIncorrectReason>));

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