using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum NumberLineRepresentationIncorrectReasons
    {
        Incomplete,
        WrongNumberofJumps,
        WrongJumpSizes,
        ReversedGrouping,
        WrongLastMarkedTick
    }

    [Serializable]
    public class NumberLineRepresentationCorrectnessTag : ANumberLineBaseTag
    {
        #region Constructors

        public NumberLineRepresentationCorrectnessTag() { }

        public NumberLineRepresentationCorrectnessTag(CLPPage parentPage,
                                                      Origin origin,
                                                      string numberLineID,
                                                      int firstNumber,
                                                      int lastNumber,
                                                      int numberLineNumber,
                                                      Correctness correctness,
                                                      List<NumberLineRepresentationIncorrectReasons> reasons)
            : base(parentPage, origin, numberLineID, firstNumber, lastNumber, numberLineNumber)
        {
            Correctness = correctness;
            Reasons = reasons;
        }

        public NumberLineRepresentationCorrectnessTag(CLPPage parentPage,
                                                      Origin origin,
                                                      string numberLineID,
                                                      int firstNumber,
                                                      int lastNumber,
                                                      int numberLineNumber,
                                                      Correctness correctness)
            : base(parentPage, origin, numberLineID, firstNumber, lastNumber, numberLineNumber)
        {
            Correctness = correctness;
        }

        public NumberLineRepresentationCorrectnessTag(SerializationInfo info, StreamingContext context)
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

        /// <summary>Reason Number Line is wrong</summary>
        public List<NumberLineRepresentationIncorrectReasons> Reasons
        {
            get { return GetValue<List<NumberLineRepresentationIncorrectReasons>>(ReasonsProperty); }
            set { SetValue(ReasonsProperty, value); }
        }

        public static readonly PropertyData ReasonsProperty = RegisterProperty("Reasons",
                                                                               typeof (List<NumberLineRepresentationIncorrectReasons>),
                                                                               () => new List<NumberLineRepresentationIncorrectReasons>());

        /// <summary>List of <see cref="IHistoryAction" /> IDs used to generate this Tag.</summary>
        public List<string> HistoryActionIDs
        {
            get { return GetValue<List<string>>(HistoryActionIDsProperty); }
            set { SetValue(HistoryActionIDsProperty, value); }
        }

        public static readonly PropertyData HistoryActionIDsProperty = RegisterProperty("HistoryActionIDs", typeof (List<string>), () => new List<string>());

        #region Calculated Properties

        public List<IHistoryAction> HistoryActions
        {
            get { return ParentPage.History.HistoryActions.Where(h => HistoryActionIDs.Contains(h.ID)).OrderBy(h => h.HistoryActionIndex).ToList(); }
        }

        #endregion // Calculated Properties

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return "Number Line Representation Correctness"; }
        }

        public override string FormattedValue
        {
            get
            {
                var analysisCode = string.Format("NL [{0}: COR]", LastNumber);
                return string.Format("{0}{1}", Correctness, string.IsNullOrWhiteSpace(analysisCode) ? string.Empty : "\nCode: " + analysisCode);

                //return string.Format("Correctness for {0} to {1}.\n" + "Number Line {2} on page.\n" + "{3}{4}{5}",
                //                     FirstNumber,
                //                     LastNumber,
                //                     IsNumberLineStillOnPage ? "still" : "no longer",
                //                     Correctness,
                //                     Correctness == Correctness.Correct || Correctness == Correctness.Unknown ? string.Empty : " due to:\n" + string.Join("\n", Reasons),
                //                     string.IsNullOrWhiteSpace(analysisCode) ? string.Empty : "\nAnalysis Code: " + analysisCode);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}