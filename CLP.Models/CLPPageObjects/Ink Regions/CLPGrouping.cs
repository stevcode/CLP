﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPGrouping : DataObjectBase<CLPNamedInkSet>
    {

        #region Constructor
        public CLPGrouping(string typeOfGrouping)
        {
            GroupingType = typeOfGrouping;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string GroupingType
        {
            get { return GetValue<string>(GroupingTypeProperty); }
            set { SetValue(GroupingTypeProperty, value); }
        }

        /// <summary>
        /// Register the GroupingType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData GroupingTypeProperty = RegisterProperty("GroupingType", typeof(string), "");

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public List<Dictionary<string, List<ICLPPageObject>>> Groups
        {
            get { return GetValue<List<Dictionary<string, List<ICLPPageObject>>>>(GroupsProperty); }
            set { SetValue(GroupsProperty, value); }
        }

        /// <summary>
        /// Register the Groups property so it is known in the class.
        /// </summary>
        public static readonly PropertyData GroupsProperty = RegisterProperty("Groups", typeof(List<Dictionary<string, List<ICLPPageObject>>>), () => new List<Dictionary<string, List<ICLPPageObject>>>());

        #endregion

        #region Methods

        public void AddGroup(List<ICLPPageObject> group)
        {
            Groups.Add(OrganizeGroupOfPageObjectsByType(group));
        }

        public string toString()
        {
            StringBuilder answer = new StringBuilder(GroupingType);
            answer.Append(": ");
            answer.Append(Groups.Count);
            answer.Append(" Groups - ");
            foreach (Dictionary<string, List<ICLPPageObject>> dicOfGroup in Groups)
            {
                foreach (string key in dicOfGroup.Keys)
                {
                    List<ICLPPageObject> objectsOfGroup = dicOfGroup[key];
                    answer.Append(objectsOfGroup.Count);
                    answer.Append(" ");
                    answer.Append(key);
                    answer.Append(" of ");
                    answer.Append(objectsOfGroup[0].Parts);
                    answer.Append(" Parts");
                    answer.Append("; ");
                }
            }
            return answer.ToString();
        }

        public static Dictionary<string, List<ICLPPageObject>> OrganizeGroupOfPageObjectsByType(List<ICLPPageObject> group)
        {
            Dictionary<string, List<ICLPPageObject>> groupOrganized =
                new Dictionary<string, List<ICLPPageObject>>();
            foreach (ICLPPageObject po in group)
            {
                String key = GetObjectGroupingType(po);
                List<ICLPPageObject> objectsInGroup;
                if (groupOrganized.ContainsKey(key))
                {
                    objectsInGroup = groupOrganized[key];
                    groupOrganized.Remove(key);
                }
                else
                {
                    objectsInGroup = new List<ICLPPageObject>();
                }
                objectsInGroup.Add(po);
                groupOrganized.Add(key, objectsInGroup);
            }
            return groupOrganized;
        }

        /* Many objects such as tiles and stamps don't use their generic type of object for
         * grouping purposes. */
        private static string GetObjectGroupingType(ICLPPageObject po)
        {
            if (po.GetType().Equals(typeof(CLPStrokePathContainer)))
            {
                return "CLPStamp-" + (po as CLPStrokePathContainer).ParentID;
            }
            else if (po.GetType().Equals(typeof(CLPSnapTileContainer)))
            {
                return "Tiles" + (po as CLPSnapTileContainer).NumberOfTiles;
            }
            else if (po.GetType().Equals(typeof(CLPShape)))
            {
                return (po as CLPShape).ShapeType.ToString();
            }
            else
            {
                return po.GetType().ToString();
            }
        }

        #endregion
    }
}
