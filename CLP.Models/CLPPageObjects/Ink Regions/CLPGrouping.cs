using System;
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

        public CLPGrouping(string typeOfGrouping, string container)
        {
            HasContainer = (container.Length > 0) ? true : false;
            Container = container;
            GroupingType = typeOfGrouping;
        }

        public CLPGrouping(string typeOfGrouping)
            : this(typeOfGrouping, "")
        { }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPGrouping(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
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

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool HasContainer
        {
            get { return GetValue<bool>(HasContainerProperty); }
            set { SetValue(HasContainerProperty, value); }
        }

        /// <summary>
        /// Register the HasContainer property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HasContainerProperty = RegisterProperty("HasContainer", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string Container
        {
            get { return GetValue<string>(ContainerProperty); }
            set { SetValue(ContainerProperty, value); }
        }

        /// <summary>
        /// Register the Container property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ContainerProperty = RegisterProperty("Container", typeof(string), "");

        #endregion

        #region Methods

        public void AddGroup(List<ICLPPageObject> group)
        {
            Groups.Add(OrganizeGroupOfPageObjectsByType(group));
        }

        public string toFormattedString()
        {
            StringBuilder answer = new StringBuilder(GroupingType);
            answer.Append(": ");
            answer.Append(Groups.Count);
            answer.AppendLine(" Groups - ");
            if (HasContainer)
            {
                answer.Append("\t Container: ");
                answer.AppendLine(Container);
            }
            foreach (Dictionary<string, List<ICLPPageObject>> dicOfGroup in Groups)
            {
                answer.Append("\t");
                answer.Append("Group: ");
                Console.WriteLine("Key Count: " + dicOfGroup.Keys.Count);
                foreach (string key in dicOfGroup.Keys)
                {
                    List<ICLPPageObject> objectsOfGroup = dicOfGroup[key];
                    answer.Append(objectsOfGroup.Count);
                    answer.Append(" ");
                    answer.Append(key);
                    answer.Append(" of ");
                    answer.Append(objectsOfGroup[0].Parts);
                    answer.Append(" Parts, ");
                }
                answer.Remove(answer.Length - 3, 2);
                answer.AppendLine(";");
            }
            return answer.ToString();
        }

        public string toNonFormattedString()
        {
            StringBuilder answer = new StringBuilder(GroupingType);
            answer.Append(": ");
            answer.Append(Groups.Count);
            answer.Append(" Groups - ");
            if (HasContainer)
            {
                answer.Append("Container: ");
                answer.Append(Container);
            }
            foreach (Dictionary<string, List<ICLPPageObject>> dicOfGroup in Groups)
            {
                answer.Append(" Group: ");
                foreach (string key in dicOfGroup.Keys)
                {
                    List<ICLPPageObject> objectsOfGroup = dicOfGroup[key];
                    answer.Append(objectsOfGroup.Count);
                    answer.Append(" ");
                    answer.Append(key);
                    answer.Append(" of ");
                    answer.Append(objectsOfGroup[0].Parts);
                    answer.Append(" Parts, ");
                }
                answer.Remove(answer.Length - 3, 2);
                answer.Append(";");
            }
            return answer.ToString();
        }

        public void AddAllOrganizedGroups(List<Dictionary<string, List<ICLPPageObject>>> organizedGroups)
        {
            Groups.AddRange(organizedGroups);
        }


        public static Dictionary<string, List<ICLPPageObject>> OrganizeGroupOfPageObjectsByType(List<ICLPPageObject> group)
        {
            Console.WriteLine("Group length: " + group.Count);
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
