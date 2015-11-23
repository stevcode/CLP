using System;
using System.Collections.Generic;
using System.Linq;
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

        public ArrayRepresentationCorrectnessTag(CLPPage parentPage, Origin origin, Correctness correctness, List<IHistoryAction> historyActions)
            : base(parentPage, origin)
        {
            Correctness = correctness;
            HistoryActionIDs = historyActions.Select(h => h.ID).ToList();
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

        public static readonly PropertyData ArrayIncorrectReasonsProperty = RegisterProperty("ArrayIncorrectReasons", typeof(List<ArrayRepresentationIncorrectReason>), () => new List<ArrayIncorrectReasons>());

        /// <summary>List of <see cref="IHistoryAction" /> IDs used to generate this Tag.</summary>
        public List<string> HistoryActionIDs
        {
            get { return GetValue<List<string>>(HistoryActionIDsProperty); }
            set { SetValue(HistoryActionIDsProperty, value); }
        }

        public static readonly PropertyData HistoryActionIDsProperty = RegisterProperty("HistoryActionIDs", typeof(List<string>), () => new List<string>());

        #region Calculated Properties

        public List<IHistoryAction> HistoryActions
        {
            get { return ParentPage.History.HistoryActions.Where(h => HistoryActionIDs.Contains(h.ID)).OrderBy(h => h.HistoryActionIndex).ToList(); }
        }

        #endregion // Calculated Properties

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Array; }
        }

        public override string FormattedName
        {
            get { return "Array Representation Correctness"; }
        }

        public override string FormattedValue
        {
            get
            {
                var analysisCode = HistoryActionIDs.Any() ? "ARR [4x8: COR]" : "ARR [5x8: COR]";
                return string.Format("{0}{1}", Correctness, string.IsNullOrWhiteSpace(analysisCode) ? string.Empty : "\nCode: " + analysisCode);

                //return string.Format("{0}{1}{3}",
                //                     Correctness,
                //                     Correctness == Correctness.Correct || Correctness == Correctness.Unknown
                //                         ? string.Empty
                //                         : " due to: " + string.Join(", ", ArrayIncorrectReasons),
                //                     string.IsNullOrWhiteSpace(analysisCode) ? string.Empty : "\nAnalysis Code: " + analysisCode);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}