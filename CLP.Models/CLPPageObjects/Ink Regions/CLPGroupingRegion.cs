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

#region General

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

        private class ClippedObject
        {
            public ICLPPageObject po;
            public double Width;
            public double Height;
            public double XPosition;
            public double YPosition;

            public ClippedObject(ICLPPageObject po)
            {
                this.po = po;
                Rect visDimensions = findVisibleDimensions(po);
                XPosition = visDimensions.X;
                Width = visDimensions.Width;
                YPosition = visDimensions.Y;
                Height = visDimensions.Height;
                /*Console.WriteLine("Obj: x: " + XPosition + " oldX: " + po.XPosition + "; Y: " + YPosition + " oldY: " +
                    po.YPosition + "; width: " + Width + " old Width: " + po.Width + "; height: " + Height + " oldHeight: " +
                    po.Height);*/
                //Console.WriteLine("X of rect: " + visDimensions.X + "; Y of rect: " + visDimensions.Y);
            }

            // XPosition, XPosition+Width, YPosition, YPosition + Height OR left, right, top, bottom
            private Rect findVisibleDimensions(ICLPPageObject po)
            {
                Rect bounds = Rect.Empty;
                Rect strokeBounds = Rect.Empty;
                if (po.GetType().Equals(typeof(CLPStrokePathContainer)) && (po as CLPStrokePathContainer).InternalPageObject == null)
                {
                    ObservableCollection<ICLPPageObject> pageObjs = po.GetPageObjectsOverPageObject();
                    foreach (ICLPPageObject childObject in po.GetPageObjectsOverPageObject())
                    {
                        //Console.WriteLine("co");
                        Rect childDimensions = findVisibleDimensions(childObject);
                            bounds.Union(childDimensions);
                            //Console.WriteLine("X: " + bounds.X + " Y: " + bounds.Y + " Height: " + bounds.Height + " Width: " + bounds.Width);
                    }

                    Rect testRectSize = new Rect(0, 0, po.Width, po.Height);
                    foreach (Stroke s in CLPPage.BytesToStrokes((po as CLPStrokePathContainer).ByteStrokes))
                    {
                        //Console.WriteLine("Stroke X: " + s.GetBounds().X + " Y: " + s.GetBounds().Y + " Height: " + s.GetBounds().Height + " Width: " + s.GetBounds().Width);
                        if (s.HitTest(testRectSize, 3))
                        {
                                strokeBounds.Union(s.GetBounds());
                        //        Console.WriteLine("X: " + strokeBounds.X + " Y: " + strokeBounds.Y + " Height: " + strokeBounds.Height + " Width: " + strokeBounds.Width);
                        }
                    }
                    if (!strokeBounds.Equals(Rect.Empty)) {
                        strokeBounds.X = strokeBounds.X + po.XPosition;
                        strokeBounds.Y = strokeBounds.Y + po.YPosition;
                        bounds.Union(strokeBounds);
                    }
                }
                else
                {
                    bounds = new Rect(po.XPosition, po.YPosition, po.Width, po.Height);
                }
                return bounds;
            }
        }
#endregion

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
            InkShapeRegion.ParentPage = ParentPage;
            CLPGrouping group = new CLPGrouping("Ink Grouping");

            // We need the ink shape region to be the same box as the grouping region, but without overloading the
            // properties of the parent class. Updates to the grouping region's size, etc. will not be seen by its
            // internal ink shape region.
            setInkShapeRegionAttributes();
            InkGroupingNode root = new InkGroupingNode(new Rect(XPosition, YPosition, Width, Height), createSideDictionary(null, null, null, null));
            InkShapeRegion.DoInterpretation();
            foreach (CLPNamedInkSet shape in InkShapeRegion.InkShapes)
            {
                if (!shape.InkShapeType.Equals("Other")) {
                    StrokeCollection shapeStrokes =  CLPPage.BytesToStrokes(shape.InkShapeStrokes);
                    Rect shapeBounds = shapeStrokes.GetBounds();
                    //GetBounds = X,Y,Width,Height
                    Console.WriteLine(shape.InkShapeType + " Left: " + shapeBounds.Left + " Right: " + shapeBounds.Right + 
                        " Top: " + shapeBounds.Top + " Bottom: " + shapeBounds.Bottom);

                    Tuple<double, double, double, double> attributes = getShapeAttributes(shape);
                    Console.WriteLine("ShapeAttrs: X: " + attributes.Item1 + " Y: " + attributes.Item2 +
    " Width: " + attributes.Item3 + " Height: " + attributes.Item4);
                    Console.WriteLine("Overall grouping region: X: " + XPosition + " Y: " + YPosition + " Width: " + Width + " Height: " + Height
                        );

                    if (shape.InkShapeType.Equals("Vertical"))
                    {
                        Rect left = new Rect(XPosition, attributes.Item2, attributes.Item1 - XPosition, attributes.Item4);
                        InsertNewInkNode(left, root, createSideDictionary(null, shape, null, null));
                        Rect right = new Rect(attributes.Item1, attributes.Item2, Width - attributes.Item1, attributes.Item4);
                        InsertNewInkNode(right, root, createSideDictionary(shape, null, null, null));
                    }
                    else if (shape.InkShapeType.Equals("Horizontal")) {
                        Rect top = new Rect(attributes.Item1, YPosition, attributes.Item3, attributes.Item2 - YPosition);
                        InsertNewInkNode(top, root, createSideDictionary(null, null, null, shape));
                        Rect bottom = new Rect(attributes.Item1, attributes.Item2, attributes.Item3,
                            Height - attributes.Item2);
                        InsertNewInkNode(bottom, root, createSideDictionary(null, null, shape, null));
                    }
                    else {
                        InsertNewInkNode(shapeBounds, root, createSideDictionary(shape, shape, shape, shape));
                    }
                }
            }

            foreach (ICLPPageObject po in validObjectsForGrouping) {
                ClippedObject clipObj = new ClippedObject(po);
                Rect objBounds = new Rect(clipObj.XPosition, clipObj.YPosition, clipObj.Width, clipObj.Height);
                Console.WriteLine("Object bounds: Left: " + objBounds.Left + " Right: " + objBounds.Right +
                    " Top: " + objBounds.Top + " Bottom: " + objBounds.Bottom);
                InkGroupingNode containRect = findInkGroupingNodeForObject(root, objBounds);
                containRect.objects.Add(po);
            }

            TraverseInkGroupingNodeTree(group, root);

            TraverseInkGroupingDebug(root);
            
            // We don't want to return a valid grouping region if the only objects appear in the root (a section unmarked
            // by ink).
            if (group.Groups.Count == 1 && root.objects.Count > 0) {
                group.Groups = new List<Dictionary<string, List<ICLPPageObject>>>();
            }
            return group;
        }

        private Dictionary<Side, CLPNamedInkSet> createSideDictionary(CLPNamedInkSet left, CLPNamedInkSet right,
            CLPNamedInkSet top, CLPNamedInkSet bottom)
        {
            Dictionary<Side, CLPNamedInkSet> sides = new Dictionary<Side, CLPNamedInkSet>();
            sides.Add(Side.Left, left);
            sides.Add(Side.Right, right);
            sides.Add(Side.Top, top);
            sides.Add(Side.Bottom, bottom);
            return sides;
        }

        // Attributes for node based on shape
        // Returns Tuple <x, y, width, height> with the suggested attributes of each node
        // Using -1 as a null value since attributes can never be null
        private Tuple<double, double, double, double> getShapeAttributes(CLPNamedInkSet shape) {
            double lineThreshold = 1.25;
            Rect shapeBounds = CLPPage.BytesToStrokes(shape.InkShapeStrokes).GetBounds();
            if (shape.InkShapeType.Equals("Vertical"))
            {
                double x = (shapeBounds.Right + shapeBounds.Left) / 2;
                double y = Math.Max(YPosition, shapeBounds.Top - shapeBounds.Height * ((lineThreshold - 1) / 2));
                double height = shapeBounds.Height * lineThreshold;
                return new Tuple<double, double, double, double>(x, y, -1, height);
            }
            else if (shape.InkShapeType.Equals("Horizontal"))
            {
                double y = (shapeBounds.Bottom + shapeBounds.Top) /2;
                double x = Math.Max(XPosition, shapeBounds.Left - shapeBounds.Width * ((lineThreshold - 1) / 2));
                double width = shapeBounds.Width * lineThreshold;
                return new Tuple<double, double, double, double>(x, y, width, -1);
            }
            else
            {
                return new Tuple<double, double, double, double>(shapeBounds.Left, shapeBounds.Top,
                    shapeBounds.Width, shapeBounds.Height);
            }
            
        }

        private void InsertNewInkNode(Rect bounds, InkGroupingNode root, Dictionary<Side, CLPNamedInkSet> sides)
        {
            Console.WriteLine("Insert New Node: bounds: Left: " + bounds.Left + " Right: " + bounds.Right +
                " Top: " + bounds.Top + " Bottom: " + bounds.Bottom);
            InkGroupingNode node = new InkGroupingNode(bounds, sides);
            SetParentOfNodeAndFixBounds(node, root);
        }

        private void TraverseInkGroupingDebug(InkGroupingNode node) {
            Console.WriteLine("NodeParent: " + node.parent + "; NodeBounds: Left: " + node.bounds.Left +
                " Right: " + node.bounds.Right + " Top: " + node.bounds.Top + " Bottom: " + node.bounds.Bottom);
            foreach (ICLPPageObject po in node.objects) {
                Console.WriteLine("Obj: Left: " + po.XPosition + " Right: " + (po.Width + po.XPosition) +
                    " Top: " + po.YPosition + " Bottom: " + (po.Height + po.YPosition));
            }
            foreach (InkGroupingNode ign in node.children) {
                TraverseInkGroupingDebug(ign);
            }
        }

        private InkGroupingNode findInkGroupingNodeForObject(InkGroupingNode node, Rect objBounds)
        {
            double objThreshold = .5;
            foreach (InkGroupingNode n in node.children)
            {
                if (objBounds.IntersectsWith(n.bounds))
                {
                    Rect intersection = Rect.Intersect(n.bounds, objBounds);
                    //Console.WriteLine("Intersection bounds: Left: " + intersection.Left + " Right: " + intersection.Right +
                    //    " Top: " + intersection.Top + " Bottom: " + intersection.Bottom + "; Node: " + n.bounds);
                    if ((intersection.Height * intersection.Width) / (objBounds.Height * objBounds.Width) > objThreshold)
                    {
                        //Console.WriteLine("Hit");
                        return findInkGroupingNodeForObject(n, objBounds);
                    }
                }
            }
            return node;
        }

        private void TraverseInkGroupingNodeTree(CLPGrouping group, InkGroupingNode node)
        {
            if (node.objects.Count > 0)
            {
                group.AddGroup(node.objects);
            }
            foreach (InkGroupingNode n in node.children)
            {
                TraverseInkGroupingNodeTree(group, n);
            }
        }

        private void SetParentOfNodeAndFixBounds(InkGroupingNode node, InkGroupingNode potentialParent) {
            node.parent = potentialParent;
            Console.WriteLine("NodeParent: " + potentialParent.bounds);
            bool nodeStillExists = true;
            int childIndex = 0;
            while (childIndex < potentialParent.children.Count) {
                InkGroupingNode childNode = potentialParent.children[0];
                Console.WriteLine("ChildNode: " + childNode.bounds);
                if (childNode.bounds.Contains(node.bounds))
                {
                    SetParentOfNodeAndFixBounds(node, childNode);
                    break;
                }
                else if (node.bounds.Contains(childNode.bounds))
                {
                    Console.WriteLine("New Child: " + childNode.bounds);
                    childNode.parent = node;
                    node.children.Add(childNode);
                    potentialParent.children.Remove(childNode);
                }
                else if (childNode.bounds.IntersectsWith(node.bounds))
                {
                    Rect intersection = Rect.Intersect(node.bounds, childNode.bounds);
                    if (intersection.Height * intersection.Width > 0)
                    {
                        while (intersection.Height * intersection.Width > 0)
                        {
                            Console.WriteLine("Intersection bounds: Left: " + intersection.Left + " Right: " +
                            intersection.Right + " Top: " + intersection.Top + " Bottom: " + intersection.Bottom);
                            // New node's right side intersects another node
                            if (node.bounds.Left < childNode.bounds.Left && node.bounds.Right > childNode.bounds.Left)
                            {
                                nodeStillExists = nodeStillExists && HandleIntersection(node, childNode, Side.Right);
                                intersection = Rect.Intersect(node.bounds, childNode.bounds);
                            }
                            // New node's left side intersects another node
                            else if (childNode.bounds.Right > node.bounds.Left && node.bounds.Right > childNode.bounds.Right)
                            {
                                nodeStillExists = nodeStillExists && HandleIntersection(node, childNode, Side.Left);
                                intersection = Rect.Intersect(node.bounds, childNode.bounds);
                            }
                            // New node's bottom side intersects another node
                            else if (node.bounds.Bottom > childNode.bounds.Top)
                            {
                                nodeStillExists = nodeStillExists && HandleIntersection(node, childNode, Side.Bottom);
                                intersection = Rect.Intersect(node.bounds, childNode.bounds);
                            }
                            // New node's top side intersects another node
                            else if (childNode.bounds.Bottom > node.bounds.Top)
                            {
                                nodeStillExists = nodeStillExists && HandleIntersection(node, childNode, Side.Top);
                                intersection = Rect.Intersect(node.bounds, childNode.bounds);
                            }
                            if (!nodeStillExists)
                            {
                                break;
                            }
                        }
                        childIndex = 0;
                    }
                    else
                    {
                        childIndex++;
                    }
                }
                else {
                    childIndex++;
                }
            }
            if (nodeStillExists)
            {
                // In this case the parent node has the same sides defined as this new node except has more defaults
                // this means that the two nodes should be combined and the more confined bounds should be used.
                // The root node can't be removed and by default has all sides nu
                if (!isRootNode(potentialParent) && HasMoreSides(potentialParent, node)) {
                    CombineNodes(potentialParent, node);
                    CheckIfChildOrUpdateParent(potentialParent);
                } else {
                    potentialParent.children.Add(node);
                }
            }
        }

        private bool isRootNode(InkGroupingNode n) {
            return (n.sides[Side.Left] == null && n.sides[Side.Right] == null &&
                n.sides[Side.Top] == null && n.sides[Side.Bottom] == null);
        }

        private void CheckIfChildOrUpdateParent(InkGroupingNode node) {
            foreach (InkGroupingNode child in node.children)
            {
                if (!node.bounds.Contains(child.bounds))
                {
                    node.children.Remove(child);
                    SetParentOfNodeAndFixBounds(child, node.parent);
                }
            }
        }

        private bool HandleIntersection(InkGroupingNode newNode, InkGroupingNode otherNode, Side side) {
            Console.WriteLine("Side: " + side);
            Rect intersection = Rect.Intersect(newNode.bounds, otherNode.bounds);
            Console.WriteLine("Intersection bounds: Left: " + intersection.Left + " Right: " +
                intersection.Right + " Top: " + intersection.Top + " Bottom: " + intersection.Bottom);
            Console.WriteLine("Intersection area: " + intersection.Height * intersection.Width);
            // One and only one node has a side controlling 
            if (newNode.sides[side] != null ^ otherNode.sides[GetOppositeSide(side)] != null)
            {
                InkGroupingNode controlNode = (newNode.sides[side] != null) ? newNode : otherNode;
                InkGroupingNode changingNode = (newNode.sides[side] != null) ? otherNode : newNode;
                changingNode.sides[side] = controlNode.sides[GetOppositeSide(side)];
                if (HasSameSides(newNode, otherNode))
                {
                    CombineNodes(otherNode, newNode);
                    return false;
                }
                else
                {
                    UpdateBounds(changingNode);
                    CheckIfChildOrUpdateParent(changingNode);
                }
            } else {
                Console.WriteLine("Breakpoint");
            }
            return true;
        }

        private void CombineNodes(InkGroupingNode remainingNode, InkGroupingNode nodeToBeDeleted) {
            remainingNode.sides[Side.Left] = (remainingNode.sides[Side.Left] == null) ?
                nodeToBeDeleted.sides[Side.Left] : remainingNode.sides[Side.Left];
            remainingNode.sides[Side.Right] = (remainingNode.sides[Side.Right] == null) ?
                nodeToBeDeleted.sides[Side.Right] : remainingNode.sides[Side.Right];
            remainingNode.sides[Side.Top] = (remainingNode.sides[Side.Top] == null) ?
                nodeToBeDeleted.sides[Side.Top] : remainingNode.sides[Side.Top];
            remainingNode.sides[Side.Bottom] = (remainingNode.sides[Side.Bottom] == null) ?
                nodeToBeDeleted.sides[Side.Bottom] : remainingNode.sides[Side.Bottom];

            UpdateBounds(remainingNode);

            remainingNode.children.AddRange(nodeToBeDeleted.children);
        }

        private void UpdateBounds(InkGroupingNode node) {
            double left = getBound(node, Side.Left);
            double right = getBound(node, Side.Right);
            double top = getBound(node, Side.Top);
            double bottom = getBound(node, Side.Bottom);

            if (left < 0)
            {
                if (top >= 0 && bottom < 0) {
                    left = getShapeAttributes(node.sides[Side.Top]).Item1;
                } else if (top < 0 && bottom >= 0) {
                    left = getShapeAttributes(node.sides[Side.Bottom]).Item1;
                } else if (top >= 0 && bottom >= 0) {
                    Tuple<double, double, double, double> bottomAttrs = getShapeAttributes(node.sides[Side.Bottom]);
                    Tuple<double, double, double, double> topAttrs = getShapeAttributes(node.sides[Side.Top]);
                    left = Math.Min(bottomAttrs.Item1, topAttrs.Item1);
                } else {
                    left = XPosition;
                }
            }

            if (right < 0)
            {
                if (top >= 0 && bottom < 0)
                {
                    Tuple<double, double, double, double> topAttrs = getShapeAttributes(node.sides[Side.Top]);
                    right = topAttrs.Item1 + topAttrs.Item3;
                }
                else if (top < 0 && bottom >= 0)
                {
                    Tuple<double, double, double, double> bottomAttrs = getShapeAttributes(node.sides[Side.Bottom]);
                    right = bottomAttrs.Item1 + bottomAttrs.Item3;
                }
                else if (top >= 0 && bottom >= 0)
                {
                    Tuple<double, double, double, double> bottomAttrs = getShapeAttributes(node.sides[Side.Bottom]);
                    Tuple<double, double, double, double> topAttrs = getShapeAttributes(node.sides[Side.Top]);
                    right = Math.Max(bottomAttrs.Item1 + bottomAttrs.Item3, topAttrs.Item1 + topAttrs.Item3);
                }
                else
                {
                    right = XPosition + Width;
                }
            }
            if (top < 0)
            {
                if (left >= 0 && right < 0)
                {
                    top = getShapeAttributes(node.sides[Side.Left]).Item2;
                }
                else if (left < 0 && right >= 0)
                {
                    top = getShapeAttributes(node.sides[Side.Right]).Item2;
                }
                else if (left >= 0 && right >= 0)
                {
                    Tuple<double, double, double, double> leftAttrs = getShapeAttributes(node.sides[Side.Left]);
                    Tuple<double, double, double, double> rightAttrs = getShapeAttributes(node.sides[Side.Right]);
                    top = Math.Min(leftAttrs.Item2, rightAttrs.Item2);
                }
                else
                {
                    top = YPosition;
                }
            }

            if (bottom < 0)
            {
                if (left >= 0 && right < 0)
                {
                    Tuple<double, double, double, double> leftAttrs = getShapeAttributes(node.sides[Side.Left]);
                    bottom = leftAttrs.Item2 + leftAttrs.Item4;
                }
                else if (left < 0 && right >= 0)
                {
                    Tuple<double, double, double, double> rightAttrs = getShapeAttributes(node.sides[Side.Right]);
                    bottom = rightAttrs.Item2 + rightAttrs.Item4;
                }
                else if (left >= 0 && right >= 0)
                {
                    Tuple<double, double, double, double> leftAttrs = getShapeAttributes(node.sides[Side.Left]);
                    Tuple<double, double, double, double> rightAttrs = getShapeAttributes(node.sides[Side.Right]);
                    bottom = Math.Max(leftAttrs.Item2 + leftAttrs.Item4, rightAttrs.Item2 + rightAttrs.Item4);
                }
                else
                {
                    bottom = YPosition + Height;
                }
            }
            Console.WriteLine("New Left: " + left + "; New Right: " + right + "; New Top: " + top + "; New Bottom: " + bottom);
            node.bounds = new Rect(left, top, right - left, bottom - top);
        }

        private double getBound(InkGroupingNode node, Side side) {
            if (node.sides[side] == null) {
                // No left through side
                return -1;
            }
            // check not default options
            else if (isLine(node.sides[side]) ||
                ((side == Side.Left || side == Side.Right) && node.sides[Side.Left] == node.sides[Side.Right]) ||
                ((side == Side.Top || side == Side.Bottom) && node.sides[Side.Top] == node.sides[Side.Bottom]))
            {
                if (side == Side.Left)
                {
                    return getShapeAttributes(node.sides[side]).Item1;
                }
                else if (side == Side.Right) {
                    return GetRightCoordinate(node, side);
                }
                else if (side == Side.Top)
                {
                    return getShapeAttributes(node.sides[side]).Item2;
                }
                else {
                    return GetBottomCoordinate(node, side);
                }
            } else {
                // This means that the bound is found from the opposite side
                if (side == Side.Left)
                {
                    return GetRightCoordinate(node, side);
                }
                else if (side == Side.Right) {
                    return getShapeAttributes(node.sides[side]).Item1;
                }
                else if (side == Side.Top)
                {
                    return GetBottomCoordinate(node, side);
                }
                else {
                    return getShapeAttributes(node.sides[side]).Item2;
                }
            }
        }

        private double GetRightCoordinate(InkGroupingNode node, Side side) {
            // This means that the right side of the object is used as a left bound
            Tuple<double, double, double, double> attributes = getShapeAttributes(node.sides[side]);
            return attributes.Item1 + attributes.Item3;
        }

        private double GetBottomCoordinate(InkGroupingNode node, Side side)
        {
            // This means that the right side of the object is used as a left bound
            Tuple<double, double, double, double> attributes = getShapeAttributes(node.sides[side]);
            return attributes.Item2 + attributes.Item4;
        }

        private bool isLine(CLPNamedInkSet shape) {
            return (shape.InkShapeType == "Vertical" || shape.InkShapeType == "Horizontal");
        }

        private bool HasSameSides(InkGroupingNode n1, InkGroupingNode n2) {
            return n1.sides[Side.Left] == n2.sides[Side.Left] && n1.sides[Side.Right] == n2.sides[Side.Right] &&
                n1.sides[Side.Top] == n2.sides[Side.Top] && n1.sides[Side.Bottom] == n2.sides[Side.Bottom];
        }

        //N2 has either the same sides or has an additional side defined where N1's side is null
        private bool HasMoreSides(InkGroupingNode lessSides, InkGroupingNode moreSides) {
            return (lessSides.sides[Side.Left] == moreSides.sides[Side.Left] || (lessSides.sides[Side.Left] == null && moreSides.sides[Side.Left] != null)) &&
                (lessSides.sides[Side.Right] == moreSides.sides[Side.Right] || (lessSides.sides[Side.Right] == null && moreSides.sides[Side.Right] != null)) &&
                (lessSides.sides[Side.Top] == moreSides.sides[Side.Top] || (lessSides.sides[Side.Top] == null && moreSides.sides[Side.Top] != null)) &&
                (lessSides.sides[Side.Bottom] == moreSides.sides[Side.Bottom] || (lessSides.sides[Side.Bottom] == null && moreSides.sides[Side.Bottom] != null));
        }

        private Side GetOppositeSide(Side s) {
            return (Side)(((int)s + 2) % 4);
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
            public List<ICLPPageObject> objects;
            public Dictionary<Side, CLPNamedInkSet> sides;

            public InkGroupingNode(Rect bounds, Dictionary<Side, CLPNamedInkSet> sides)
            {
                children = new List<InkGroupingNode>();
                objects = new List<ICLPPageObject>();
                parent = null;
                this.bounds = bounds;
                this.sides = sides;
            }
        }

        enum Side { Left, Top, Right, Bottom };

        #endregion

        #region Distance Grouping

        private CLPGrouping DistanceClustering(List<ICLPPageObject> validGroupingObjects)
        {
            HashSet<DistanceGroup> groups = new HashSet<DistanceGroup>();
            foreach (ICLPPageObject po in validGroupingObjects)
            {
                groups.Add(new DistanceGroup(new ClippedObject(po)));
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
                    foreach (ClippedObject distOb in g.groupObjects) {
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
                foreach (ClippedObject disObj in group.groupObjects) {
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
                        foreach (ClippedObject poA in groupA.groupObjects)
                        {
                            foreach (ClippedObject poB in groupB.groupObjects)
                            {
                                double distance = getDistanceBetweenPageObjects(poA, poB);
                                if (distance < smallestDistance)
                                {
                                    smallestDistance = distance;
                                    //Console.WriteLine("smallestDistance: " + smallestDistance + "; POA: " + poA.po.UniqueID + "; POB: " + poB.po.UniqueID);
                                }
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

            double threshold = 2.5;
            double minValue = 10;
            //Console.WriteLine("SmallestDistanceGroups: " + smallestDistanceGroups + "; Groups1: " + combineTheseGroups[0].printGroupObjects() + " avg: " + combineTheseGroups[0].average() + "; Groups2: " + combineTheseGroups[1].printGroupObjects() + " avg: " + combineTheseGroups[1].average());
            if (Math.Max(combineTheseGroups[0].average(), minValue) * threshold >= smallestDistanceGroups &&
                Math.Max(combineTheseGroups[1].average(), minValue) * threshold >= smallestDistanceGroups)
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
            public List<ClippedObject> groupObjects;
            public List<double> metrics = new List<double>();

            public DistanceGroup(ClippedObject po)
            {
                groupObjects = new List<ClippedObject>();
                groupObjects.Add(po);
            }

            public void combineGroup(DistanceGroup group, double metricBetween)
            {
                //Console.WriteLine("Metric: " + metricBetween);
                foreach (ClippedObject po in group.groupObjects)
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

            public string printGroupObjects() {
                StringBuilder sb = new StringBuilder();
                foreach(ClippedObject co in groupObjects) {
                    sb.Append(co.po.UniqueID);
                    sb.Append(", ");
                }
                sb.Remove(sb.Length - 3, 2);
                return sb.ToString();
            }

        }

        private double getDistanceBetweenPageObjects(ClippedObject pageObject1, ClippedObject pageObject2)
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
