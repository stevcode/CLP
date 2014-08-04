using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class ObjectTypesOnPage : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ObjectTypesOnPage" /> from scratch.
        /// </summary>
        public ObjectTypesOnPage() { }

        /// <summary>
        /// Initializes <see cref="ObjectTypesOnPage" /> from values.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ObjectTypesOnPage" /> belongs to.</param>
        public ObjectTypesOnPage(CLPPage parentPage, Origin origin, string currentUserID)
            : base(parentPage, origin)
        {
            IsSingleValueTag = true;

            foreach(var pageObject in parentPage.PageObjects.Where(pageObject => pageObject.OwnerID == parentPage.OwnerID))
            {
                if(pageObject is FuzzyFactorCard)
                {
                    ObjectTypes.Add("Division Templates");
                    continue;
                }

                if(pageObject is CLPArray)
                {
                    ObjectTypes.Add("Arrays");
                    continue;
                }

                if(pageObject is RemainderTiles)
                {
                    ObjectTypes.Add("Remainder Tiles");
                    continue;
                }

                if(pageObject is Stamp)
                {
                    ObjectTypes.Add("Stamps");
                    continue;
                }

                if(pageObject is Shape)
                {
                    ObjectTypes.Add("Shapes");
                    continue;
                }
            }

            ObjectTypes = ObjectTypes.Distinct().ToList();

           // ObjectTypes = parentPage.PageObjects.Where(pageObject => pageObject.OwnerID == parentPage.OwnerID).Select(pageObject => pageObject.GetType().Name).Distinct().ToList();
            if(parentPage.InkStrokes.Any(stroke => stroke.GetStrokeOwnerID() == parentPage.OwnerID))
            {
                ObjectTypes.Add(typeof(Stroke).Name);
            }
        }

        /// <summary>
        /// Initializes <see cref="ObjectTypesOnPage" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ObjectTypesOnPage(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// List of all the <see cref="Type" />s of the relevant <see cref="IPageObject" />s on the <see cref="CLPPage" /> as well as Ink.
        /// </summary>
        public List<string> ObjectTypes
        {
            get { return GetValue<List<string>>(ObjectTypesProperty); }
            set { SetValue(ObjectTypesProperty, value); }
        }

        public static readonly PropertyData ObjectTypesProperty = RegisterProperty("ObjectTypes", typeof(List<string>), () => new List<string>());

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