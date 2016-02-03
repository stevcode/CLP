using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class RepresentationsUsedTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="RepresentationsUsedTag" /> from scratch.</summary>
        public RepresentationsUsedTag() { }

        /// <summary>Initializes <see cref="RepresentationsUsedTag" /> from values.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ObjectTypesOnPageTag" /> belongs to.</param>
        /// <param name="origin"></param>
        /// <param name="currentUserID"></param>
        public RepresentationsUsedTag(CLPPage parentPage, Origin origin, List<string> allRepresentations, List<string> deletedCodedRepresentations, List<string> finalCodedRepresentations)
            : base(parentPage, origin)
        {
            IsSingleValueTag = true;

            AllRepresentations = allRepresentations;
            DeletedCodedRepresentations = deletedCodedRepresentations;
            FinalCodedRepresentations = finalCodedRepresentations;
        }

        /// <summary>Initializes <see cref="RepresentationsUsedTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public RepresentationsUsedTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Distinct list of all representations used through the history of the page.</summary>
        public List<string> AllRepresentations
        {
            get { return GetValue<List<string>>(AllRepresentationsProperty); }
            set { SetValue(AllRepresentationsProperty, value); }
        }

        public static readonly PropertyData AllRepresentationsProperty = RegisterProperty("AllRepresentations", typeof (List<string>), () => new List<string>());

        /// <summary>Coded values for all deleted representations.</summary>
        public List<string> DeletedCodedRepresentations
        {
            get { return GetValue<List<string>>(DeletedCodedRepresentationsProperty); }
            set { SetValue(DeletedCodedRepresentationsProperty, value); }
        }

        public static readonly PropertyData DeletedCodedRepresentationsProperty = RegisterProperty("DeletedCodedRepresentations", typeof (List<string>), () => new List<string>());

        /// <summary>Coded values for all final representations.</summary>
        public List<string> FinalCodedRepresentations
        {
            get { return GetValue<List<string>>(FinalCodedRepresentationsProperty); }
            set { SetValue(FinalCodedRepresentationsProperty, value); }
        }

        public static readonly PropertyData FinalCodedRepresentationsProperty = RegisterProperty("FinalCodedRepresentations", typeof (List<string>), () => new List<string>());

        #region ATagBase Overrides

        public string AnalysisCode
        {
            get { return string.Format("MR [{0}]", string.Join(", ", AllRepresentations)); }
        }

        public override Category Category
        {
            get { return Category.OtherPageInformation; }
        }

        public override string FormattedName
        {
            get { return "Representations Used"; }
        }

        public override string FormattedValue
        {
            get
            {
                if (!AllRepresentations.Any())
                {
                    var isStrokesUsed = ParentPage.InkStrokes.Any() || ParentPage.History.TrashedInkStrokes.Any();
                    return isStrokesUsed ? "Ink Only" : "Blank Page";
                }

                var deletedSection = !DeletedCodedRepresentations.Any() ? string.Empty : string.Format("Deleted Representation(s):\n{0}", string.Join("\n", DeletedCodedRepresentations));
                var finalSection = !FinalCodedRepresentations.Any() ? string.Empty : string.Format("Final Representation(s):\n{0}", string.Join("\n", FinalCodedRepresentations));
                var codeSection = AllRepresentations.Count > 1 ? string.Format("\nCode: {0}", AnalysisCode) : string.Empty;
                return string.Format("{0}{1}{2}", deletedSection, finalSection, codeSection);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}