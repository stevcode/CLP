using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class AnswerRepresentationSequence : AAnalysisTagBase
    {
        #region Constructors

        public AnswerRepresentationSequence() { }

        public AnswerRepresentationSequence(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion //Constructors

        /// <summary>Sequence of Final Answers and Representations.</summary>
        public List<string> Sequence
        {
            get { return GetValue<List<string>>(SequenceProperty); }
            set { SetValue(SequenceProperty, value); }
        }

        public static readonly PropertyData SequenceProperty = RegisterProperty("Sequence", typeof(List<string>), () => new List<string>());

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.Representation;

        public override string FormattedName => "Answer-Representation Sequence";

        public override string FormattedValue
        {
            get
            {
                var sequence = string.Join(", ", Sequence);
                var analysisCodes = string.Join(", ", AnalysisCodes);
                var codedSection = AnalysisCodes.Any() ? $"\nCodes: {analysisCodes}" : string.Empty;

                return $"{sequence}{codedSection}";
            }
        }

        #endregion //ATagBase Overrides

        #region Static Methods

        public static void AttemptTagGeneration(CLPPage page, List<ISemanticEvent> semanticEvents)
        {

            foreach (var semanticEvent in semanticEvents)
            {
                

            }

            var tag = new AnswerRepresentationSequence(page, Origin.StudentPageGenerated);
            page.AddTag(tag);
        }

        #endregion // Static Methods
    }
}
