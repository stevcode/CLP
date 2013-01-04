using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPGroupingRegion : ACLPInkRegion
    {
        #region Constructors

        public CLPGroupingRegion(CLPPage page)
            : base(page)
        {
            InkShapeRegion = new CLPInkShapeRegion(ParentPage);
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
        public CLPInkShapeRegion InkShapeRegion
        {
            get { return GetValue<CLPInkShapeRegion>(inkShapeRegionProperty); }
            set { SetValue(inkShapeRegionProperty, value); }
        }

        /// <summary>
        /// Register the InkShapeRegion property so it is known in the class.
        /// </summary>
        public static readonly PropertyData inkShapeRegionProperty = RegisterProperty("InkShapeRegion", typeof(CLPInkShapeRegion), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<CLPGrouping> Groupings
        {
            get { return GetValue<ObservableCollection<CLPGrouping>>(GroupingsProperty); }
            set { SetValue(GroupingsProperty, value); }
        }

        /// <summary>
        /// Register the Groupings property so it is known in the class.
        /// </summary>
        public static readonly PropertyData GroupingsProperty = RegisterProperty("Groupings", typeof(ObservableCollection<CLPGrouping>), () => new ObservableCollection<CLPGrouping>());

        #endregion // Properties
        #region Methods

        public override void DoInterpretation()
        {
            Groupings.Clear();
            List<ICLPPageObject> validGroupingObjects = new List<ICLPPageObject>();
            foreach (ICLPPageObject po in ParentPage.PageObjects)
            {
                if (ValidObjectForGrouping(po))
                {
                    validGroupingObjects.Add(po);
                }
            }
            AddGrouping(InkGrouping(validGroupingObjects), true, Groupings);
            AddGrouping(DistanceClustering(validGroupingObjects), true, Groupings);
            AddGrouping(BasicGrouping(validGroupingObjects), false, Groupings);
            StringBuilder interpretation = new StringBuilder();
            foreach (CLPGrouping grouping in Groupings)
            {
                interpretation.AppendLine(grouping.toString());
            }
            StoredAnswer = interpretation.ToString();
        }

        private void AddGrouping(CLPGrouping group, bool checkForContainers, ObservableCollection<CLPGrouping> groupingCollection)
        {
            if (group.Groups.Count > 0)
            {
                groupingCollection.Add(group);
                if (checkForContainers)
                {
                    List<CLPGrouping> containerGroups = DetectContainer(group);
                    if (containerGroups != null)
                    {
                        foreach (CLPGrouping container in containerGroups)
                        {
                            groupingCollection.Add(container);
                        }
                    }
                }
            }
        }

        private bool ValidObjectForGrouping(ICLPPageObject po)
        {
            return PageObjectIsOver(po, .8) && po.Parts >= 0 && po.GetType() != typeof(CLPStamp);
        }

        #region Containers
        private List<CLPGrouping> DetectContainer(CLPGrouping group)
        {
            List<string> possibleContainers = new List<string>();
            List<Dictionary<string, List<ICLPPageObject>>> groups = group.Groups;
            // Go through first group to make a base of possible containers
            foreach (string key in groups[0].Keys)
            {
                List<ICLPPageObject> objectsOfGroup = groups[0][key];
                if (IsContainer(objectsOfGroup))
                {
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
                List<CLPGrouping> containerGroups = new List<CLPGrouping>();
                foreach (String containerKey in possibleContainers)
                {
                    CLPGrouping containerGroup = new CLPGrouping("Container" + group.GetType(), containerKey);
                    List<Dictionary<string, List<ICLPPageObject>>> groupsAugmented = new List<Dictionary<string, List<ICLPPageObject>>>(groups);
                    foreach (Dictionary<string, List<ICLPPageObject>> grouping in groupsAugmented)
                    {
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

        private bool IsContainer(List<ICLPPageObject> possibleContainer)
        {
            return (possibleContainer.Count == 1 &&
                (possibleContainer[0].Parts == 0 || possibleContainer[0].Parts == 1));
        }
        #endregion

        private CLPGrouping BasicGrouping(List<ICLPPageObject> validGroupingObjects)
        {
            CLPGrouping group = new CLPGrouping("Basic Grouping");
            Dictionary<string, List<ICLPPageObject>> groupsByObject =
                CLPGrouping.OrganizeGroupOfPageObjectsByType(validGroupingObjects);
            foreach (string key in groupsByObject.Keys)
            {
                group.AddGroup(groupsByObject[key]);
            }
            return group;
        }

        #region Ink Grouping
        private CLPGrouping InkGrouping(List<ICLPPageObject> validObjectsForGrouping)
        {
            CLPGrouping group = new CLPGrouping("Ink Grouping");

            // We need the ink shape region to be the same box as the grouping region, but without overloading the
            // properties of the parent class. Updates to the grouping region's size, etc. will not be seen by its
            // internal ink shape region.
            setInkShapeRegionAttributes();
            InkGroupingNode root = new InkGroupingNode(null, new Rect(XPosition, YPosition, Width, Height));
            InkShapeRegion.DoInterpretation();
            foreach (CLPNamedInkSet shape in InkShapeRegion.InkShapes)
            {
                if (!shape.InkShapeType.Equals("Other")) {
                    Console.WriteLine(shape.InkShapeType + " " + CLPPage.BytesToStrokes(shape.InkShapeStrokes).GetBounds());


                    if (shape.InkShapeType.Equals("Vertical") || shape.InkShapeType.Equals("Horizontal"))
                    {
                    }
                    else {
                        Rect bounds = CLPPage.BytesToStrokes(shape.InkShapeStrokes).GetBounds();
                        InkGroupingNode parent = getParentNode(bounds, root);
                        InkGroupingNode node = new InkGroupingNode(parent, bounds);
                        parent.children.Add(node);
                    }
                }
            }
            foreach (ICLPPageObject po in validObjectsForGrouping) {
            }

            return group;
        }

        private InkGroupingNode getParentNode(Rect bounds, InkGroupingNode potentialParent) {
            InkGroupingNode parentNode = potentialParent;
            foreach (InkGroupingNode childNode in potentialParent.children) {
                if (childNode.bounds.Contains(bounds)) {
                    parentNode = getParentNode(bounds, childNode);
                }
            }
            return parentNode;
        }

        private void setInkShapeRegionAttributes()
        {
            InkShapeRegion.Width = Width;
            InkShapeRegion.Height = Height;
            InkShapeRegion.XPosition = XPosition;
            InkShapeRegion.YPosition = YPosition;
        }

        private class InkGroupingNode {
            public InkGroupingNode parent;
            public List<InkGroupingNode> children;
            public Rect bounds;

            public InkGroupingNode(InkGroupingNode parent, Rect bounds) {
                children = new List<InkGroupingNode>();
                this.parent = parent;
                this.bounds = bounds;
            }
        }

        #endregion

        #region Distance Grouping

        private CLPGrouping DistanceClustering(List<ICLPPageObject> validGroupingObjects)
        {
            HashSet<DistanceGroup> groups = new HashSet<DistanceGroup>();
            foreach (ICLPPageObject po in validGroupingObjects)
            {
                groups.Add(new DistanceGroup(new DistanceObject(po)));
            }
            Boolean canCombine = true;
            // Check to make sure that there are valid objects to group
            int count = 0;
            while (canCombine && groups.Count > 1)
            {
                /*Console.WriteLine("Iteration: " + count);
                count++;
                foreach(DistanceGroup g in groups){
                    Console.Write("Group:");
                    foreach (DistanceObject distOb in g.groupObjects) {
                        Console.Write(distOb.po.UniqueID + " ");
                    }
                    Console.WriteLine("; Average: " + g.average());
                }*/
                canCombine = combineGroups(groups);
            }
            CLPGrouping grouping = new CLPGrouping("Distance Grouping");
            foreach (DistanceGroup group in groups)
            {
                List<ICLPPageObject> poInGroup = new List<ICLPPageObject>();
                foreach (DistanceObject disObj in group.groupObjects) {
                    poInGroup.Add(disObj.po);
                }
                grouping.AddGroup(poInGroup);
            }
            return grouping;
        }

        private Boolean combineGroups(HashSet<DistanceGroup> groups)
        {
            double smallestDistanceGroups = Double.MaxValue;
            List<DistanceGroup> combineTheseGroups = new List<DistanceGroup>(2);
            foreach (DistanceGroup groupA in groups)
            {
                foreach (DistanceGroup groupB in groups)
                {
                    // Make sure not the same group
                    if (!groupA.Equals(groupB))
                    {
                        double smallestDistance = Double.MaxValue;
                        foreach (DistanceObject poA in groupA.groupObjects)
                        {
                            foreach (DistanceObject poB in groupB.groupObjects)
                            {
                                double distance = getDistanceBetweenPageObjects(poA, poB);
                                smallestDistance = (distance < smallestDistance) ? distance : smallestDistance;
                                //Console.WriteLine("smallestDistance: " + smallestDistance + "; POA: " + poA.po.UniqueID + "; POB: " + poB.po.UniqueID);
                            }
                        }
                        if (smallestDistance < smallestDistanceGroups)
                        {
                            smallestDistanceGroups = smallestDistance;
                            combineTheseGroups = new List<DistanceGroup>(2);
                            combineTheseGroups.Add(groupA);
                            combineTheseGroups.Add(groupB);
                        }
                    }
                }
            }

            double threshold = 2;
            //Console.WriteLine("SmallestDistanceGroups: " + smallestDistanceGroups + "; Groups1: " + combineTheseGroups[0].average() + "; Groups2: " + combineTheseGroups[1].average());
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

        private class DistanceGroup
        {
            public List<DistanceObject> groupObjects;
            public List<double> metrics = new List<double>();

            public DistanceGroup(DistanceObject po)
            {
                groupObjects = new List<DistanceObject>();
                groupObjects.Add(po);
            }

            public void combineGroup(DistanceGroup group, double metricBetween)
            {
                //Console.WriteLine("Metric: " + metricBetween);
                foreach (DistanceObject po in group.groupObjects)
                {
                    groupObjects.Add(po);
                    metrics.Add(metricBetween);
                }
            }

            public double average()
            {
                if (metrics.Count > 0)
                {
                    return metrics.Average();
                }
                else
                {
                    return double.MaxValue;
                }
            }

        }

        private class DistanceObject {
            public ICLPPageObject po;
            public double Width;
            public double Height;
            public double XPosition;
            public double YPosition;

            public DistanceObject(ICLPPageObject po)
            {
                this.po = po;
                Tuple<double, double, double, double> visWidth = findVisibleWidthHeight(po);
                XPosition = po.XPosition + visWidth.Item1;
                Width = visWidth.Item2 - visWidth.Item1;
                YPosition = po.YPosition + visWidth.Item3;
                Height = visWidth.Item4 - visWidth.Item3;
            }

            // XPosition, XPosition+Width, YPosition, YPosition + Height OR left, right, top, bottom
            private Tuple<double, double, double, double> findVisibleWidthHeight(ICLPPageObject po) {
                double visibleLeft = Double.MaxValue;
                double visibleRight = 0;
                double visibleTop = Double.MaxValue;
                double visibleBottom = 0;
                if (po.GetType().Equals(typeof(CLPStrokePathContainer)))
                {
                    foreach (ICLPPageObject childObject in po.GetPageObjectsOverPageObject())
                    {
                        Tuple<double, double, double, double> visibleWidthChild = findVisibleWidthHeight(childObject);
                        if (visibleLeft > visibleWidthChild.Item1)
                        {
                            visibleLeft = visibleWidthChild.Item1;
                        }
                        if (visibleRight < visibleWidthChild.Item2)
                        {
                            visibleRight = visibleWidthChild.Item2;
                        }
                        if (visibleTop > visibleWidthChild.Item3)
                        {
                            visibleTop = visibleWidthChild.Item3;
                        }
                        if (visibleBottom < visibleWidthChild.Item4)
                        {
                            visibleBottom = visibleWidthChild.Item4;
                        }
                    }
                    foreach (Stroke s in CLPPage.BytesToStrokes((po as CLPStrokePathContainer).ByteStrokes))
                    {
                        if (visibleLeft > s.GetBounds().Left)
                        {
                            visibleLeft = s.GetBounds().Left;
                        }
                        if (visibleRight < s.GetBounds().Right)
                        {
                            visibleRight = s.GetBounds().Right;
                        }
                        if (visibleTop > s.GetBounds().Top)
                        {
                            visibleTop = s.GetBounds().Top;
                        }
                        if (visibleBottom < s.GetBounds().Bottom)
                        {
                            visibleBottom = s.GetBounds().Bottom;
                        }
                    }
                }
                else
                {
                    visibleLeft = po.XPosition;
                    visibleRight = po.XPosition + po.Width;
                    visibleTop = po.YPosition;
                    visibleBottom = po.YPosition + po.Height;
                }
                return new Tuple<double, double, double, double>(visibleLeft, visibleRight, visibleTop, visibleBottom);
            }
        }

        private double getDistanceBetweenPageObjects(DistanceObject pageObject1, DistanceObject pageObject2)
        {
            double x = pageObject2.XPosition - pageObject1.XPosition;
            if (x > 0)
            {
                x -= pageObject1.Width;
                x = (x < 0) ? 0 : x;
            }
            else if (x < 0)
            {
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
