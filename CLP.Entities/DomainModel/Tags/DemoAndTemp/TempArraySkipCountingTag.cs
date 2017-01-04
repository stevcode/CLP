using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Demo
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

        /// <summary>Initializes <see cref="TempArraySkipCountingTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public TempArraySkipCountingTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Coded ID for the Array.</summary>
        public string CodedID
        {
            get { return GetValue<string>(CodedIDProperty); }
            set { SetValue(CodedIDProperty, value); }
        }

        public static readonly PropertyData CodedIDProperty = RegisterProperty("CodedID", typeof (string), string.Empty);

        /// <summary>Ink Interpretations for each Row in the Array.</summary>
        public string RowInterpretations
        {
            get { return GetValue<string>(RowInterpretationsProperty); }
            set { SetValue(RowInterpretationsProperty, value); }
        }

        public static readonly PropertyData RowInterpretationsProperty = RegisterProperty("RowInterpretations", typeof (string), string.Empty);

        /// <summary>Results of heuristic adjustment of row interpretations.</summary>
        public string HeuristicsResults
        {
            get { return GetValue<string>(HeuristicsResultsProperty); }
            set { SetValue(HeuristicsResultsProperty, value); }
        }

        public static readonly PropertyData HeuristicsResultsProperty = RegisterProperty("HeuristicsResults", typeof(string), string.Empty);

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Array; }
        }

        public override string FormattedName
        {
            get { return "Array Skip Counting"; }
        }

        public override string FormattedValue
        {
            get { return string.Format("ARR skip [{0}: {1}]\n{2}", CodedID, RowInterpretations, HeuristicsResults); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}