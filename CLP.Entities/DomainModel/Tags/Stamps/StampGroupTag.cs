using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class StampGroupTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="StampGroupTag" /> from scratch.</summary>
        public StampGroupTag() { }

        /// <summary>Initializes <see cref="StampGroupTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="StampGroupTag" /> belongs to.</param>
        public StampGroupTag(CLPPage parentPage, Origin origin, string parentStampID, int parts, List<string> stampedObjectIDs)
            : base(parentPage, origin)
        {
            ParentStampID = parentStampID;
            Parts = parts;
            StampedObjectIDs = stampedObjectIDs;
        }

        /// <summary>Initializes <see cref="StampGroupTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public StampGroupTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// ID of the Parent Stamp of the Stamped Objects in the grouping.
        /// </summary>
        public string ParentStampID
        {
            get { return GetValue<string>(ParentStampIDProperty); }
            set { SetValue(ParentStampIDProperty, value); }
        }

        public static readonly PropertyData ParentStampIDProperty = RegisterProperty("ParentStampID", typeof (string));

        /// <summary>
        /// Number of Parts each Stamped Object in the grouping represents.
        /// </summary>
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof (int));

        /// <summary>
        /// List of the IDs of the Stamped Objects in the grouping.
        /// </summary>
        public List<string> StampedObjectIDs
        {
            get { return GetValue<List<string>>(StampedObjectIDsProperty); }
            set { SetValue(StampedObjectIDsProperty, value); }
        }

        public static readonly PropertyData StampedObjectIDsProperty = RegisterProperty("StampedObjectIDs", typeof (List<string>));

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Stamp; }
        }

        public override string FormattedName
        {
            get { return "Stamp Group"; }
        }

        public override string FormattedValue
        {
            get
            {
                return StampedObjectIDs.Count + " group(s) of " + Parts;
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}