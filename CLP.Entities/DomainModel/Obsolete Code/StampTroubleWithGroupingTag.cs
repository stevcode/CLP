using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class StampTroubleWithGroupingTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="StampTroubleWithGroupingTag" /> from scratch.</summary>
        public StampTroubleWithGroupingTag() { }

        /// <summary>Initializes <see cref="StampTroubleWithGroupingTag" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="StampTroubleWithGroupingTag" /> belongs to.</param>
        public StampTroubleWithGroupingTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Wrong number of groups count.</summary>
        public int NumberOfGroupsWrongCount
        {
            get { return GetValue<int>(NumberOfGroupsWrongCountProperty); }
            set
            {
                SetValue(NumberOfGroupsWrongCountProperty, value);
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData NumberOfGroupsWrongCountProperty = RegisterProperty("NumberOfGroupsWrongCount", typeof (int), 0);

        /// <summary>Wrong group size count.</summary>
        public int GroupSizeWrongCount
        {
            get { return GetValue<int>(GroupSizeWrongCountProperty); }
            set
            {
                SetValue(GroupSizeWrongCountProperty, value);
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData GroupSizeWrongCountProperty = RegisterProperty("GroupSizeWrongCount", typeof (int), 0);

        /// <summary>Both group size and number of groups wrong count.</summary>
        public int NumberOfGroupsWrongAndGroupSizeWrongCount
        {
            get { return GetValue<int>(NumberOfGroupsWrongAndGroupSizeWrongCountProperty); }
            set
            {
                SetValue(NumberOfGroupsWrongAndGroupSizeWrongCountProperty, value);
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData NumberOfGroupsWrongAndGroupSizeWrongCountProperty = RegisterProperty("NumberOfGroupsWrongAndGroupSizeWrongCount", typeof (int), 0);

        /// <summary>Group size and number of groups swapped count.</summary>
        public int NumberOfGroupsAndGroupSizeSwappedCount
        {
            get { return GetValue<int>(NumberOfGroupsAndGroupSizeSwappedCountProperty); }
            set
            {
                SetValue(NumberOfGroupsAndGroupSizeSwappedCountProperty, value);
                RaisePropertyChanged("FormattedValue");
            }
        }

        public static readonly PropertyData NumberOfGroupsAndGroupSizeSwappedCountProperty = RegisterProperty("NumberOfGroupsAndGroupSizeSwappedCount", typeof (int), 0);

        #region ATagBase Overrides

        public override Category Category => Category.Stamp;

        public override string FormattedName => "Trouble With Grouping";

        public override string FormattedValue
        {
            get
            {
                return string.Format("{0}{1}{2}{3}",
                                     NumberOfGroupsWrongCount == 0 ? string.Empty : string.Format("Wrong number of groups {0} time(s).\n", NumberOfGroupsWrongCount),
                                     GroupSizeWrongCount == 0 ? string.Empty : string.Format("Wrong group size {0} time(s).\n", GroupSizeWrongCount),
                                     NumberOfGroupsWrongAndGroupSizeWrongCount == 0
                                         ? string.Empty
                                         : string.Format("Wrong number of groups and group size {0} time(s).\n", NumberOfGroupsWrongAndGroupSizeWrongCount),
                                     NumberOfGroupsAndGroupSizeSwappedCount == 0
                                         ? string.Empty
                                         : string.Format("Number of groups and group size switched {0} time(s).\n", NumberOfGroupsAndGroupSizeSwappedCount)).TrimEnd('\n');
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}