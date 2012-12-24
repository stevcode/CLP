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

        #endregion // Properties

        #region Methods

        public override void DoInterpretation()
        {
            List<Grouping> groupings = new List<Grouping>();
            groupings.Add(InkGrouping());
            groupings.Add(DistanceClustering());
            groupings.Add(BasicGrouping());
            StringBuilder interpretation = new StringBuilder();
            foreach (Grouping grouping in groupings) {
                interpretation.AppendLine(grouping.toString());
            }
            StoredAnswer = interpretation.ToString();
        }

        #region GenericGrouping

        private class Grouping
        {
            private string type;
            private List<Dictionary<string, List<ICLPPageObject>>> groups;

            public Grouping(string typeOfGrouping) {
                type = typeOfGrouping;
                groups = new List<Dictionary<string, List<ICLPPageObject>>>();
            }

            public void AddGroup(List<ICLPPageObject> group) {
                groups.Add(OrganizeGroupOfPageObjectsByType(group));
            }

            public string toString() {
                StringBuilder answer = new StringBuilder(type);
                answer.Append(": ");
                answer.Append(groups.Count);
                answer.Append(" Groups - ");
                foreach (Dictionary<string, List<ICLPPageObject>> dicOfGroup in groups) {
                    foreach (string key in dicOfGroup.Keys) {
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

        private Grouping InkGrouping()
        {
            Grouping group = new Grouping("Ink Grouping");
            //CLPInkShapeRegion inkShapeRegion = new CLPInkShapeRegion(ParentPage);
            return group;
        }

        #region Distance Grouping

        private Grouping DistanceClustering() {
            HashSet<DistanceGroup> groups = new HashSet<DistanceGroup>();
            foreach (ICLPPageObject po in ParentPage.PageObjects)
            {
                if (validOption.Contains(po.PageObjectType)) {
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
            Console.WriteLine("Average 0: " + combineTheseGroups[0].average());
            Console.WriteLine("Average 1: " + combineTheseGroups[1].average());
            Console.WriteLine("SmallestDistanceGroup: " + smallestDistanceGroups);
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

        private static readonly HashSet<string> validOption = new HashSet<string> {
            {CLPSnapTileContainer.Type},
            {CLPStrokePathContainer.Type},
            {CLPShape.Type}
        };

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
