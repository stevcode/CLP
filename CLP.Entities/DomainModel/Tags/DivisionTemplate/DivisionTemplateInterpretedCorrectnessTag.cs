using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum DivisionTemplateIncorrectReason
    {
        WrongProduct,
        WrongFactor,
        Incomplete,
        Other
    }

    [Serializable]
    public class DivisionTemplateInterpretedCorrectnessTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="DivisionTemplateInterpretedCorrectnessTag" /> from scratch.
        /// </summary>
        public DivisionTemplateInterpretedCorrectnessTag() { }

        /// <summary>
        /// Initializes <see cref="DivisionTemplateInterpretedCorrectnessTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionTemplateInterpretedCorrectnessTag" /> belongs to.</param>
        public DivisionTemplateInterpretedCorrectnessTag(CLPPage parentPage,
                                                         Origin origin,
                                                         Correctness correctness,
                                                         DivisionTemplateIncorrectReason incorrectReason = DivisionTemplateIncorrectReason.Other)
            : base(parentPage, origin)
        {
            Correctness = correctness;
            DivisionTemplateIncorrectReason = incorrectReason;
        }

        /// <summary>
        /// Initializes <see cref="ArrayInterpretedCorrectnessTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionTemplateInterpretedCorrectnessTag(SerializationInfo info, StreamingContext context)
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
        public DivisionTemplateIncorrectReason DivisionTemplateIncorrectReason
        {
            get { return GetValue<DivisionTemplateIncorrectReason>(DivisionTemplateIncorrectReasonProperty); }
            set { SetValue(DivisionTemplateIncorrectReasonProperty, value); }
        }

        public static readonly PropertyData DivisionTemplateIncorrectReasonProperty = RegisterProperty("DivisionTemplateIncorrectReason", typeof(DivisionTemplateIncorrectReason));

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.DivisionTemplate; }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("{0}{1}",
                                     Correctness,
                                     Correctness == Correctness.Correct || Correctness == Correctness.Unknown ? string.Empty : "Incorrect due to: " + DivisionTemplateIncorrectReason);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}