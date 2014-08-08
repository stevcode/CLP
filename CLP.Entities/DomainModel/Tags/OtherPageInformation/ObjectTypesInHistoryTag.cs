using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class ObjectTypesInHistoryTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="ObjectTypesInHistoryTag" /> from scratch.</summary>
        public ObjectTypesInHistoryTag() { }

        /// <summary>Initializes <see cref="ObjectTypesInHistoryTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ObjectTypesInHistoryTag" /> belongs to.</param>
        /// <param name="origin"></param>
        public ObjectTypesInHistoryTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin)
        {
            IsSingleValueTag = true;

            foreach (
                var pageObject in
                    parentPage.PageObjects.Where(pageObject => pageObject.OwnerID == parentPage.OwnerID)
                              .Concat(parentPage.History.TrashedPageObjects.Where(pageObject => pageObject.OwnerID == parentPage.OwnerID)))
            {
                if (pageObject is FuzzyFactorCard)
                {
                    ObjectTypes.Add("Division Templates");
                    continue;
                }

                if (pageObject is CLPArray)
                {
                    ObjectTypes.Add("Arrays");
                    continue;
                }

                if (pageObject is RemainderTiles)
                {
                    ObjectTypes.Add("Remainder Tiles");
                    continue;
                }

                if (pageObject is Stamp)
                {
                    ObjectTypes.Add("Stamps");
                    continue;
                }

                if (pageObject is Shape)
                {
                    ObjectTypes.Add("Shapes");
                    continue;
                }
            }

            ObjectTypes = ObjectTypes.Distinct().ToList();

            if (parentPage.InkStrokes.Any(stroke => stroke.GetStrokeOwnerID() == parentPage.OwnerID) ||
                parentPage.History.TrashedInkStrokes.Any(stroke => stroke.GetStrokeOwnerID() == parentPage.OwnerID))
            {
                ObjectTypes.Add("Ink");
            }

            if (parentPage.InkStrokes.Any(stroke => stroke.GetStrokeOwnerID() == parentPage.OwnerID && stroke.DrawingAttributes.IsHighlighter) ||
                parentPage.History.TrashedInkStrokes.Any(stroke => stroke.GetStrokeOwnerID() == parentPage.OwnerID && stroke.DrawingAttributes.IsHighlighter))
            {
                ObjectTypes.Add("Highlighter");
            }
        }

        /// <summary>Initializes <see cref="ObjectTypesInHistoryTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ObjectTypesInHistoryTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>List of all the <see cref="Type" />s of the relevant <see cref="IPageObject" />s on the <see cref="CLPPage" /> as well as Ink.</summary>
        public List<string> ObjectTypes
        {
            get { return GetValue<List<string>>(ObjectTypesProperty); }
            set { SetValue(ObjectTypesProperty, value); }
        }

        public static readonly PropertyData ObjectTypesProperty = RegisterProperty("ObjectTypes", typeof (List<string>), () => new List<string>());

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.OtherPageInformation; }
        }

        public override string FormattedValue
        {
            get { return string.Join("\n", ObjectTypes); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}