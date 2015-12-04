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
        public RepresentationsUsedTag()
        { }

        /// <summary>Initializes <see cref="RepresentationsUsedTag" /> from values.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ObjectTypesOnPageTag" /> belongs to.</param>
        /// <param name="origin"></param>
        /// <param name="currentUserID"></param>
        public RepresentationsUsedTag(CLPPage parentPage, Origin origin, List<string> representations)
            : base(parentPage, origin)
        {
            IsSingleValueTag = true;

            Representations = representations;
        }

        /// <summary>Initializes <see cref="RepresentationsUsedTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public RepresentationsUsedTag(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        #endregion //Constructors

        #region Properties

        /// <summary>List of all the representations of the relevant <see cref="IPageObject" />s on the <see cref="CLPPage" />.</summary>
        public List<string> Representations
        {
            get { return GetValue<List<string>>(RepresentationsProperty); }
            set { SetValue(RepresentationsProperty, value); }
        }

        public static readonly PropertyData RepresentationsProperty = RegisterProperty("Representations", typeof(List<string>));

        public List<string> FinalRepresentations
        {
            get
            {
                if (ParentPage == null)
                {
                    return new List<string>();
                }

                var representations = ParentPage.PageObjects.Select(p => p.CodedName).Distinct().ToList();

                return representations;
            }
        } 

        #region ATagBase Overrides

        public string AnalysisCode
        {
            get
            {
                return string.Format("MR [{0}]", string.Join(", ", Representations));
            }
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
                var finalRepresentations = FinalRepresentations;
                var friendlyRepresentations =
                    Representations.Select(r => finalRepresentations.Contains(r) ? string.Format("{0}, Final Representation", Codings.FriendlyObjects[r]) : Codings.FriendlyObjects[r]).ToList();
                if (!friendlyRepresentations.Any())
                {
                    var isStrokesUsed = ParentPage.InkStrokes.Any() || ParentPage.History.TrashedInkStrokes.Any();
                    if (isStrokesUsed)
                    {
                        friendlyRepresentations.Add("Ink Only");
                    }
                    else
                    {
                        friendlyRepresentations.Add("Blank Page");
                    }
                }
                var codeString = Representations.Count > 1 ? string.Format("\nCode: {0}", AnalysisCode) : string.Empty;
                return string.Format("{0}{1}", string.Join("\n", friendlyRepresentations), codeString);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}