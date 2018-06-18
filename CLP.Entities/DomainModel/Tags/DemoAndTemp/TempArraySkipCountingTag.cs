using System;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class TempArraySkipCountingTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="TempArraySkipCountingTag" /> from scratch.</summary>
        public TempArraySkipCountingTag() { }

        /// <summary>Initializes <see cref="TempArraySkipCountingTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="TempArraySkipCountingTag" /> belongs to.</param>
        public TempArraySkipCountingTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Coded ID for the Array.</summary>
        public string CodedID
        {
            get { return GetValue<string>(CodedIDProperty); }
            set { SetValue(CodedIDProperty, value); }
        }

        public static readonly PropertyData CodedIDProperty = RegisterProperty("CodedID", typeof(string), string.Empty);

        /// <summary>Ink Interpretations for each Row in the Array.</summary>
        public string RowInterpretations
        {
            get { return GetValue<string>(RowInterpretationsProperty); }
            set { SetValue(RowInterpretationsProperty, value); }
        }

        public static readonly PropertyData RowInterpretationsProperty = RegisterProperty("RowInterpretations", typeof(string), string.Empty);

        /// <summary>Results of heuristic adjustment of row interpretations.</summary>
        public string HeuristicsResults
        {
            get { return GetValue<string>(HeuristicsResultsProperty); }
            set { SetValue(HeuristicsResultsProperty, value); }
        }

        public static readonly PropertyData HeuristicsResultsProperty = RegisterProperty("HeuristicsResults", typeof(string), string.Empty);

        #region ATagBase Overrides

        public override Category Category => Category.Array;

        public override string FormattedName => "Array Skip Counting (Static Analysis)";

        public override string FormattedValue => $"ARR skip [{CodedID}: {RowInterpretations}]\n\t{HeuristicsResults}";

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}