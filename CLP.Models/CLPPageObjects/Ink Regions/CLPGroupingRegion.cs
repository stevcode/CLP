using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Catel.Data;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Runtime.Serialization;

namespace CLP.Models
{
    [Serializable]
    public class CLPGroupingRegion : ACLPInkRegion
    {
        #region Constructors

        public CLPGroupingRegion(CLPPage page) : base(page)
        {
            StoredAnswer = "";
            inkShapeRegion = new CLPInkShapeRegion(ParentPage);
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPGroupingRegion(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion // Constructors

        #region Properties

        public override string PageObjectType
        {
            get { return "CLPGroupingRegion"; }
        }

        /// <summary>
        /// Stored interpreted answer.
        /// </summary>
        public string StoredAnswer
        {
            get { return GetValue<string>(StoredAnswerProperty); }
            set { SetValue(StoredAnswerProperty, value); }
        }

        /// <summary>
        /// Register the StoredAnswer property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StoredAnswerProperty = RegisterProperty("StoredAnswer", typeof(string), "");

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPInkShapeRegion inkShapeRegion
        {
            get { return GetValue<CLPInkShapeRegion>(inkShapeRegionProperty); }
            set { SetValue(inkShapeRegionProperty, value); }
        }

        /// <summary>
        /// Register the inkShapeRegion property so it is known in the class.
        /// </summary>
        public static readonly PropertyData inkShapeRegionProperty = RegisterProperty("inkShapeRegion", typeof(CLPInkShapeRegion), null);

        #endregion // Properties

        #region Methods

        public override void DoInterpretation()
        {
            List<Grouping> groupings = new List<Grouping>();
            AddGrouping(InkGrouping(), true, groupings);
            AddGrouping(DistanceClustering(), true, groupings);
            AddGrouping(BasicGrouping(), false, groupings);
            StringBuilder interpretation = new StringBuilder();
            foreach (Grouping grouping in groupings) {
                interpretation.AppendLine(grouping.toString());
            }
            StoredAnswer = interpretation.ToString();
        }

        private void AddGrouping(Grouping group, bool checkForContainers, List<Grouping> groupingCollection) {
            if (group.getGroups().Count > 0) {
                groupingCollection.Add(group);
                if (checkForContainers) {
                    List<Grouping> containerGroups = DetectContainer(group);
                    if (containerGroups != null)
                    {
                        groupingCollection.AddRange(containerGroups);
                    }
                }
            }
        }

        #region GenericGrouping

        private class Grouping
        {
            private string type;
            private List<Dictionary<string, List<ICLPPageObject>>> groups;
            private bool hasContainer;
            private string container;

            public Grouping(string typeOfGrouping, string container)  {
                type = typeOfGrouping;
                groups = new List<Dictionary<string, List<ICLPPageObject>>>();
                hasContainer = (container.Length > 0) ? true : false;
                this.container = container;
            }

            public Grouping(string typeOfGrouping) : this(typeOfGrouping, "")
            {
            }

            public void AddGroup(List<ICLPPageObject> group) {
                groups.Add(OrganizeGroupOfPageObjectsByType(group));
            }

            public void AddAllOrganizedGroups(List<Dictionary<string, List<ICLPPageObject>>> organizedGroups) {
                groups.AddRange(organizedGroups);
            }

            public string getTypeOfGroup() {
                return type;
            }

            public List<Dictionary<string, List<ICLPPageObject>>> getGroups() {
                return groups;
            }

            public string toString() {
                StringBuilder answer = new StringBuilder(type);
                answer.Append(": ");
                answer.Append(groups.Count);
                answer.AppendLine(" Groups - ");
                if (hasContainer) {
                    answer.Append("\t Container: ");
                    answer.AppendLine(container);
                }
                foreach (Dictionary<string, List<ICLPPageObject>> dicOfGroup in groups) {
                    answer.AppendLine("\t Group:");
                    foreach (string key in dicOfGroup.Keys) {
                        List<ICLPPageObject> objectsOfGroup = dicOfGroup[key];
                        answer.Append("\t\t");
                        answer.Append(objectsOfGroup.Count);
                        answer.Append(" ");
                        answer.Append(key);
                        answer.Append(" of ");
                        answer.Append(objectsOfGroup[0].Parts);
                        answer.Append(" Parts");
                        answer.AppendLine("; ");
                    }
                }
                return answer.ToString();
            }
        }

        private static Dictionary<string, List<ICLPPageObject>> OrganizeGroupOfPageObjectsByType(List<ICLPPageObject> group) {
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
        private static string GetObjectGroupingType(ICLPPageObject po) {
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

        private bool ValidObjectForGrouping(ICLPPageObject po) {
            return PageObjectIsOver(po, .8) && po.Parts >= 0 && po.GetType() != typeof(CLPStamp);
        }

        #endregion

        #region Containers
        private List<Grouping> DetectContainer(Grouping group)
        {
            List<string> possibleContainers = new List<string>();
            List<Dictionary<string, List<ICLPPageObject>>> groups = group.getGroups();
            // Go through first group to make a base of possible containers
            foreach (string key in groups[0].Keys)
            {
                List<ICLPPageObject> objectsOfGroup = groups[0][key];
                if (IsContainer(objectsOfGroup)) {
                    possibleContainers.Add(key);
                }
            }

            // Check if more groups, otherwise index error
            if (groups.Count > 1)
            {
                //Check all other groups to see if they support the same containers.
                foreach (Dictionary<string, List<ICLPPageObject>> dictOfGroup in groups.GetRange(1, groups.Count - 2))
                {
                    // Since we want to iterate through the possible containers, we must create a new list since
                    // we can't edit the list that we are looping through.
                    List<string> newPossibleContainers = new List<string>();
                    foreach (string possibleContainer in possibleContainers)
                    {
                        if (dictOfGroup.ContainsKey(possibleContainer) &&
                            IsContainer(dictOfGroup[possibleContainer]))
                        {
                            newPossibleContainers.Add(possibleContainer);
                        }
                    }
                    possibleContainers = newPossibleContainers;
                }
            }

            if (possibleContainers.Count > 0)
            {
                // Usually this list will only be one item, but we consider that multiple might occasionally
                // occur.
                List<Grouping> containerGroups = new List<Grouping>();
                foreach (String containerKey in possibleContainers) {
                    Grouping containerGroup = new Grouping("Container" + group.GetType(), containerKey);
                    List<Dictionary<string, List<ICLPPageObject>>> groupsAugmented = new List<Dictionary<string, List<ICLPPageObject>>>(groups);
                    foreach (Dictionary<string, List<ICLPPageObject>> grouping in groupsAugmented) {
                        grouping.Remove(containerKey);
                    }
                    containerGroup.AddAllOrganizedGroups(groupsAugmented);
                    containerGroups.Add(containerGroup);
                }  
                return containerGroups;
            }
            else
            {
                return null;
            }
        }

        private bool IsContainer(List<ICLPPageObject> possibleContainer){
            return (possibleContainer.Count == 1 &&
                (possibleContainer[0].Parts == 0 || possibleContainer[0].Parts == 1));
        }

        #endregion

        private Grouping BasicGrouping() {
            Grouping group = new Grouping("Basic Grouping");
            List<ICLPPageObject> validGroupingObjects = new List<ICLPPageObject>();
            foreach (ICLPPageObject po in ParentPage.PageObjects) {
                if (ValidObjectForGrouping(po))
                {
                    validGroupingObjects.Add(po);
                }
            }

            Dictionary<string, List<ICLPPageObject>> groupsByObject =
                OrganizeGroupOfPageObjectsByType(validGroupingObjects);
            foreach (string key in groupsByObject.Keys)
            {
                group.AddGroup(groupsByObject[key]);
            }
            return group;
        }

        #region Ink Grouping
        private Grouping InkGrouping()
        {
            // We need the ink shape region to be the same box as the grouping region, but without overloading the
            // properties of the parent class. Updates to the grouping region's size, etc. will not be seen by its
            // internal ink shape region.
            setInkShapeRegionAttributes();
            inkShapeRegion.DoInterpretation();
            Console.WriteLine("inkShapes" + inkShapeRegion.InkShapesString);
            Grouping group = new Grouping("Ink Grouping");
            return group;
        }

        private void setInkShapeRegionAttributes() {
            inkShapeRegion.Width = Width;
            inkShapeRegion.Height = Height;
            inkShapeRegion.XPosition = XPosition;
            inkShapeRegion.YPosition = YPosition;
        }
        #endregion

        #region Distance Grouping

        private Grouping DistanceClustering() {
            HashSet<DistanceGroup> groups = new HashSet<DistanceGroup>();
            foreach (ICLPPageObject po in ParentPage.PageObjects)
            {
                if (ValidObjectForGrouping(po))
                {
                    groups.Add(new DistanceGroup(po));
                }
            }
            Boolean canCombine = true;
            // Check to make sure that there are valid objects to group
            while (canCombine && groups.Count > 1) {
                canCombine = combineGroups(groups);
            }
            Grouping grouping = new Grouping("Distance Grouping");
            foreach (DistanceGroup group in groups)
            {
                grouping.AddGroup(group.groupObjects);
            }
            return grouping;
        }

        private Boolean combineGroups(HashSet<DistanceGroup> groups) {
            double smallestDistanceGroups = Double.MaxValue;
            List<DistanceGroup> combineTheseGroups = new List<DistanceGroup>(2);
            foreach (DistanceGroup groupA in groups) {
                foreach (DistanceGroup groupB in groups)
                {
                    // Make sure not the same group
                    if (!groupA.Equals(groupB)) {
                        double smallestDistance = Double.MaxValue;
                        foreach (ICLPPageObject poA in groupA.groupObjects) {
                            foreach (ICLPPageObject poB in groupB.groupObjects)
                            {
                                double distance = getDistanceBetweenPageObjects(poA, poB);
                                smallestDistance = (distance < smallestDistance) ? distance : smallestDistance;
                            }
                        }
                        if (smallestDistance < smallestDistanceGroups) {
                            smallestDistanceGroups = smallestDistance;
                            combineTheseGroups = new List<DistanceGroup>(2);
                            combineTheseGroups.Add(groupA);
                            combineTheseGroups.Add(groupB);
                        }
                    }
                }
            }

            double threshold = 2;
            if (combineTheseGroups[0].average() * threshold >= smallestDistanceGroups &&
                combineTheseGroups[1].average() * threshold >= smallestDistanceGroups)
            {
                DistanceGroup removeGroup = combineTheseGroups[0];
                groups.Remove(removeGroup);
                combineTheseGroups[1].combineGroup(removeGroup, smallestDistanceGroups);
                return true;
            }
            else
            {
                return false;
            }
        }

        private class DistanceGroup {
            public List<ICLPPageObject> groupObjects;
            public List<double> metrics = new List<double>();

            public DistanceGroup(ICLPPageObject po) {
                groupObjects = new List<ICLPPageObject>();
                groupObjects.Add(po);
            }

            public void combineGroup(DistanceGroup group, double metricBetween) {
                foreach (ICLPPageObject po in group.groupObjects)
                {
                    groupObjects.Add(po);
                    metrics.Add(metricBetween);
                }
            }

            public double average() {
                if (metrics.Count > 0)
                {
                    return metrics.Average();
                }
                else {
                    return double.MaxValue;
                }
            }
        }

        private double getDistanceBetweenPageObjects(ICLPPageObject pageObject1, ICLPPageObject pageObject2)
        {
            double x = pageObject2.XPosition - pageObject1.XPosition;
            if (x > 0)
            {
                x -= pageObject1.Width;
                x = (x < 0) ? 0 : x;
            } else if (x < 0) {
                x *= -1;
                x -= pageObject2.Width;
                x = (x < 0) ? 0 : x;
            }

            double y = pageObject2.YPosition - pageObject1.YPosition;
            if (y > 0)
            {
                y -= pageObject1.Height;
                y = (y < 0) ? 0 : y;
            }
            else if (y < 0)
            {
                y *= -1;
                y -= pageObject2.Height;
                y = (y < 0) ? 0 : y;
            }

            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

        #endregion // Methods

        #endregion
    }
}
