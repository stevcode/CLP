using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class StampGroupingsTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="StampGroupingsTag" /> from scratch.</summary>
        public StampGroupingsTag() { }

        /// <summary>Initializes <see cref="StampGroupingsTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="StampGroupingsTag" /> belongs to.</param>
        public StampGroupingsTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        /// <summary>Initializes <see cref="StampGroupingsTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public StampGroupingsTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Pairs a list of IDs for StampedObjects with the same parent Stamp to the Parts value they share.</summary>
        public Dictionary<int, List<string>> ParentStampGroupings
        {
            get { return GetValue<Dictionary<int, List<string>>>(ParentStampGroupingsProperty); }
            set { SetValue(ParentStampGroupingsProperty, value); }
        }

        public static readonly PropertyData ParentStampGroupingsProperty = RegisterProperty("ParentStampGroupings",
                                                                                            typeof (Dictionary<int, List<string>>),
                                                                                            () => new Dictionary<int, List<string>>());

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Stamp; }
        }

        public override string FormattedName
        {
            get { return string.Format("Stamp Groupings{0}", false ? " **Trouble**" : string.Empty); }
        }

        public override string FormattedValue
        {
            get
            {
                return
                    string.Format("{0}{1}",
                                  !ParentStampGroupings.Any() ? string.Empty : "Parent Stamp Groupings:\n",
                                  !ParentStampGroupings.Any()
                                      ? string.Empty
                                      : string.Join("\n", ParentStampGroupings.Select(x => x.Key + " group(s) of " + x.Value.Count))).TrimEnd('\n');
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}