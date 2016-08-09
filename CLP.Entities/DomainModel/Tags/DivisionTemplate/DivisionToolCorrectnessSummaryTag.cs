using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class DivisionToolCorrectnessSummaryTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="DivisionToolCorrectnessSummaryTag" /> from scratch.</summary>
        public DivisionToolCorrectnessSummaryTag() { }

        /// <summary>Initializes <see cref="DivisionToolCorrectnessSummaryTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="DivisionToolCorrectnessSummaryTag" /> belongs to.</param>
        public DivisionToolCorrectnessSummaryTag(CLPPage parentPage, Origin origin, Correctness correctness)
            : base(parentPage, origin)
        {
            IsSingleValueTag = true;

            Correctness = correctness;
        }

        /// <summary>Initializes <see cref="DivisionToolCorrectnessSummaryTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public DivisionToolCorrectnessSummaryTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Type of correctness.</summary>
        public Correctness Correctness
        {
            get { return GetValue<Correctness>(CorrectnessProperty); }
            set { SetValue(CorrectnessProperty, value); }
        }

        public static readonly PropertyData CorrectnessProperty = RegisterProperty("Correctness", typeof (Correctness));

        /// <summary>Number of RepresentationCorrectnessTags set to Correct.</summary>
        public int CorrectCount
        {
            get { return GetValue<int>(CorrectCountProperty); }
            set { SetValue(CorrectCountProperty, value); }
        }

        public static readonly PropertyData CorrectCountProperty = RegisterProperty("CorrectCount", typeof (int), 0);

        /// <summary>Number of RepresentationCorrectnessTags set to PartiallyCorrect.</summary>
        public int PartiallyCorrectCount
        {
            get { return GetValue<int>(PartiallyCorrectCountProperty); }
            set { SetValue(PartiallyCorrectCountProperty, value); }
        }

        public static readonly PropertyData PartiallyCorrectCountProperty = RegisterProperty("PartiallyCorrectCount", typeof (int), 0);

        /// <summary>Number of RepresentationCorrectnessTags set to Incorrect.</summary>
        public int IncorrectCount
        {
            get { return GetValue<int>(IncorrectCountProperty); }
            set { SetValue(IncorrectCountProperty, value); }
        }

        public static readonly PropertyData IncorrectCountProperty = RegisterProperty("IncorrectCount", typeof (int), 0);

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.DivisionTool; }
        }

        public override string FormattedName
        {
            get { return "Division Tool Correctness Summary"; }
        }

        public override string FormattedValue
        {
            get
            {
                return string.Format("Overall Correctness: {0}{1}{2}{3}",
                                     Correctness,
                                     CorrectCount == 0
                                         ? string.Empty
                                         : string.Format("\n{0} RepresentationCorrectnessTag(s) set to Correct.", CorrectCount),
                                     PartiallyCorrectCount == 0
                                         ? string.Empty
                                         : string.Format("\n{0} RepresentationCorrectnessTag(s) set to Partially Correct.", PartiallyCorrectCount),
                                     IncorrectCount == 0
                                         ? string.Empty
                                         : string.Format("\n{0} RepresentationCorrectnessTag(s) set to Incorrect.", IncorrectCount));
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}