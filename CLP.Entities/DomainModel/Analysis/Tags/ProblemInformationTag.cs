using System;
using System.Linq;

namespace CLP.Entities
{
    [Serializable]
    public class ProblemInformationTag : AAnalysisTagBase
    {
        #region Constructors

        public ProblemInformationTag() { }

        public ProblemInformationTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion // Constructors

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.MetaData;

        public override string FormattedName => "Problem Information";

        public override string FormattedValue
        {
            get
            {
                var analysisCodes = string.Join("\n", QueryCodes.Select(c => c.FormattedValue));
                return QueryCodes.Any() ? $"Codes:\n{analysisCodes}" : "No Codes";
            }
        }

        #endregion //ATagBase Overrides

        #region Static Methods

        public static ProblemInformationTag AttemptTagGeneration(CLPPage page)
        {
            var tag = new ProblemInformationTag(page, Origin.Author);
            var wordProblemTag = page.Tags.OfType<MetaDataTag>().FirstOrDefault(t => t.TagName == MetaDataTag.NAME_WORD_PROBLEM);
            if (wordProblemTag != null)
            {
                AnalysisCode.AddIsWordProblem(tag, wordProblemTag.TagContents == MetaDataTag.VALUE_TRUE);
            }

            var pageDefTags = page.Tags.OfType<IDefinition>().ToList();
            AnalysisCode.AddPageDefinition(tag, pageDefTags);

            page.AddTag(tag);

            return tag;
        }

        #endregion // Static Methods
    }
}