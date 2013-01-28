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
            Console.WriteLine("Interpretation");
            Console.WriteLine(StoredAnswer);
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
                    if (!strokeBounds.Equals(Rect.Empty))
                    {
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
        InkGroupingNode root = null;
        private CLPGrouping InkGrouping(List<ICLPPageObject> validObjectsForGrouping)
        {
            InkShapeRegion.ParentPage = ParentPage;
            CLPGrouping group = new CLPGrouping("Ink Grouping");

            // We need the ink shape region to be the same box as the grouping region, but without overloading the
            // properties of the parent class. Updates to the grouping region's size, etc. will not be seen by its
            // internal ink shape region.
            setInkShapeRegionAttributes();
            root = new InkGroupingNode(new Rect(XPosition, YPosition, Width, Height), createSideDictionary(null, null, null, null));
            InkShapeRegion.DoInterpretation();
            foreach (CLPNamedInkSet shape in InkShapeRegion.InkShapes)
            {
                if (!shape.InkShapeType.Equals("Other"))
                {
                    StrokeCollection shapeStrokes = CLPPage.BytesToStrokes(shape.InkShapeStrokes);
                    Rect shapeBounds = shapeStrokes.GetBounds();
                    //GetBounds = X,Y,Width,Height
                    Console.WriteLine(shape.InkShapeType + " Left: " + shapeBounds.Left + " Right: " + shapeBounds.Right +
                        " Top: " + shapeBounds.Top + " Bottom: " + shapeBounds.Bottom);

                    Tuple<double, double, double, double> attributes = getShapeAttributes(shape);
                    Console.WriteLine("ShapeAttrs: X: " + attributes.Item1 + " Y: " + attributes.Item2 +
    " Width: " + attributes.Item3 + " Height: " + attributes.Item4);

                    Console.WriteLine("Overall grouping region: X: " + XPosition + " Y: " + YPosition + " Width: " + Width + " Height: " + Height);

                    //No workable ojbect smaller than this - unrealistic that line this small would be separating
                    // Probably stray mark so get rid of
                    double minLineLength = 25;
                    if (shape.InkShapeType.Equals("Vertical"))
                    {
                        if (shapeBounds.Height > minLineLength) {
                        InsertNewInkNode(createSideDictionary(null, shape, null, null));
                        InsertNewInkNode(createSideDictionary(shape, null, null, null));
                        }
                    }
                    else if (shape.InkShapeType.Equals("Horizontal"))
                    {
                        if (shapeBounds.Width > minLineLength)
                        {
                            InsertNewInkNode(createSideDictionary(null, null, null, shape));
                            InsertNewInkNode(createSideDictionary(null, null, shape, null));
                        }
                    }
                    else
                    {
                        InsertNewInkNode(createSideDictionary(shape, shape, shape, shape));
                    }
                }
            }

            foreach (ICLPPageObject po in validObjectsForGrouping)
            {
                ClippedObject clipObj = new ClippedObject(po);
                Rect objBounds = new Rect(clipObj.XPosition, clipObj.YPosition, clipObj.Width, clipObj.Height);
                InkGroupingNode containRect = findInkGroupingNodeForObject(root, objBounds);
                containRect.objects.Add(po);
            }

            TraverseInkGroupingNodeTree(group, root);

            TraverseInkGroupingDebug(root);

            // We don't want to return a valid grouping region if the only objects appear in the root (a section unmarked
            // by ink).
            if (group.Groups.Count == 1 && root.objects.Count > 0)
            {
                group.Groups = new List<Dictionary<string, List<ICLPPageObject>>>();
            }
            return group;
        }

        private void InsertNewInkNode(Dictionary<Side, CLPNamedInkSet> sides)
        {
            Console.WriteLine("Insert New Node");
            InkGroupingNode node = new InkGroupingNode(Rect.Empty, sides);
            UpdateBounds(node);
            SetParentOfNodeAndFixBounds(node, root);
        }

        private void TraverseInkGroupingDebug(InkGroupingNode node)
        {
            Console.WriteLine("NodeParent: " + node.parent + "; NodeBounds: Left: " + node.bounds.Left +
                " Right: " + node.bounds.Right + " Top: " + node.bounds.Top + " Bottom: " + node.bounds.Bottom);
            foreach (ICLPPageObject po in node.objects)
            {
                Console.WriteLine("Obj: Left: " + po.XPosition + " Right: " + (po.Width + po.XPosition) +
                    " Top: " + po.YPosition + " Bottom: " + (po.Height + po.YPosition));
            }
            foreach (InkGroupingNode ign in node.children)
            {
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

        private void SetParentOfNodeAndFixBounds(InkGroupingNode node, InkGroupingNode potentialParent)
        {
            node.parent = potentialParent;
            Console.WriteLine("NodeParent: " + potentialParent.bounds);
            bool nodeStillExists = true;
            int childIndex = 0;
            while (childIndex < potentialParent.children.Count)
            {
                InkGroupingNode childNode = potentialParent.children[childIndex];
                Console.WriteLine("ChildNode: " + childNode.bounds);
                if (CanCombine(childNode, node))
                {
                    Console.WriteLine("Reg iteration");
                    CombineNodes(childNode, node);
                    nodeStillExists = false;
                    break;
                }
                else if (childNode.bounds.Contains(node.bounds))
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
                else if (childNode.bounds.IntersectsWith(node.bounds) &&
                    Rect.Intersect(node.bounds, childNode.bounds).Height * Rect.Intersect(node.bounds, childNode.bounds).Width > 0)
                {
                    Rect intersection = Rect.Intersect(node.bounds, childNode.bounds);
                    while (nodeStillExists && intersection.Height * intersection.Width > 0)
                    {
                        // New node's right side intersects another node
                        Console.WriteLine("newNode.Left: " + node.bounds.Left + " newNode.Right: " + node.bounds.Right);
                        Console.WriteLine("exisitingNode.Left: " + childNode.bounds.Left + " exisitingNode.Right: " + childNode.bounds.Right);

                        // left or right side of the new node intersects with another node
                        // -- without knowing which side has control its hard to 
                        if (node.bounds.Right > childNode.bounds.Left && node.bounds.Left < childNode.bounds.Left
                            || node.bounds.Right > childNode.bounds.Right && node.bounds.Left < childNode.bounds.Right)
                        {
                            // left side comes from one of the nodes
                            Console.WriteLine("left or right side intersecting");
                            if ((node.sides[Side.Left] != null && node.bounds.Left == intersection.Left) ||
                                (childNode.sides[Side.Left] != null && childNode.bounds.Left == intersection.Left))
                            {
                                nodeStillExists = HandleIntersection(node, childNode, Side.Left);
                                if (!nodeStillExists)
                                {
                                    break;
                                }
                                intersection = Rect.Intersect(node.bounds, childNode.bounds);
                            }
                            // Right side forming intersection
                            else if ((node.sides[Side.Right] != null && node.bounds.Right == intersection.Right) ||
                                (childNode.sides[Side.Right] != null && childNode.bounds.Right == intersection.Right))
                            {
                                nodeStillExists = HandleIntersection(node, childNode, Side.Right);
                                if (!nodeStillExists)
                                {
                                    break;
                                }
                                intersection = Rect.Intersect(node.bounds, childNode.bounds);
                            }
                            else {
                                Console.WriteLine("Umm");
                            }
                        }
                        // New node's top side intersects another node
                        else if (node.bounds.Top < childNode.bounds.Bottom && node.bounds.Bottom > childNode.bounds.Bottom ||
                            node.bounds.Bottom > childNode.bounds.Top && node.bounds.Top < childNode.bounds.Top)
                        {
                            Console.WriteLine("bottom or top side intersecting");
                            // top side comes from one of the nodes
                            if ((node.sides[Side.Top] != null && node.bounds.Top == intersection.Top) ||
                                (childNode.sides[Side.Top] != null && childNode.bounds.Top == intersection.Top))
                            {
                                nodeStillExists = HandleIntersection(node, childNode, Side.Top);
                                if (!nodeStillExists)
                                {
                                    break;
                                }
                                intersection = Rect.Intersect(node.bounds, childNode.bounds);
                            }
                            // Bottom side forming intersection
                            else if ((node.sides[Side.Bottom] != null && node.bounds.Bottom == intersection.Bottom) ||
                                (childNode.sides[Side.Bottom] != null && childNode.bounds.Bottom == intersection.Bottom))
                            {
                                nodeStillExists = HandleIntersection(node, childNode, Side.Bottom);
                                if (!nodeStillExists)
                                {
                                    break;
                                }
                                intersection = Rect.Intersect(node.bounds, childNode.bounds);
                            }
                            else
                            {
                                Console.WriteLine("Umm");
                            }
                        }
                        // Intersection is some bottom side combination with neither immediately adjoining
                        else
                        {
                            Console.WriteLine("Break");
                        }
                        intersection = Rect.Intersect(node.bounds, childNode.bounds);
                    }
                    if (!nodeStillExists)
                    {
                        break;
                    }
                    childIndex = 0;
                }
                else
                {
                    childIndex++;
                }
            }
            //Technically when it finds its child it will come back here and we could say the node doesnt exist
            // or just check for the right parent
            if (nodeStillExists && node.parent == potentialParent)
            {
                Console.WriteLine("Done. Added node to parent");
                potentialParent.children.Add(node);
            }
        }

        private bool CanCombine(InkGroupingNode n1, InkGroupingNode n2) {
            // We want to make sure that they have at least all of the same sides defined
            // There is an edge case where a really large node may be containing a smaller
            // node that is only splitting part of the node
            double differenceInArea = .5;
            double area1 = n1.bounds.Height * n1.bounds.Width;
            double area2 = n2.bounds.Height * n1.bounds.Width;
            double min = Math.Min(area1, area2);
            double max = (min == area1) ? area2 : area1;
            return HasSameAndPossiblyMoreDefinedSides(n1, n2) && min/max > differenceInArea;
        }

        private bool HandleIntersection(InkGroupingNode newNode, InkGroupingNode existingNode, Side side)
        {
            Rect intersection = Rect.Intersect(newNode.bounds, existingNode.bounds);
            Console.WriteLine("Intersection: Left: " + intersection.Left + " Right: " + intersection.Right + " Top: " +
                intersection.Top + " Bottom: " + intersection.Bottom);
            InkGroupingNode controlNode = (newNode.sides[side] != null && GetBoundOfRectangleWithSide(newNode.bounds, side) == GetBoundOfRectangleWithSide(intersection, side)) ? newNode : existingNode;
            InkGroupingNode changingNode = (controlNode == newNode) ? existingNode : newNode;
            Console.WriteLine("Side: " + side);
            Console.WriteLine("Control: " + controlNode.bounds);
            Console.WriteLine("Changing: " + changingNode.bounds);
            Console.WriteLine("Opp side: " + GetOppositeSide(side) + " Opposite Side null: " + (GetOppositeSide(side) == null));
            // This is the case where you have a square or the equivalent and the new line splits it
            if (changingNode.sides[side] != null && changingNode.sides[GetOppositeSide(side)] != null)
            {
                Console.WriteLine("changing node both appropriate sides defined");
                // split the changing node in two
                if (controlNode.sides[GetOppositeSide(side)] == null)
                {
                    splitNode(changingNode, controlNode, side);
                    return newNode == controlNode;
                }
                // two squares overlapping = control node right has a side defined
                else
                {
                    overlappingNodes(changingNode, controlNode, side);
                    return false;
                }
            }
            // intersection caused by defaults from a vertical or horizontal line or set by default from the
            // opposite axis ie. if the side is left or right - then it was set from top or bottom
            else
            {
                Console.WriteLine("Adjacent: " + GetAdjacentSide(side) + " Get Opposite Adj: " + GetOppositeSide(GetAdjacentSide(side)));
                // Node just needs to be trimmed
                if ((changingNode.sides[GetAdjacentSide(side)] == null && changingNode.sides[GetOppositeSide(GetAdjacentSide(side))] == null) ||
                    AdjacentSidesAllowBound(changingNode, side, GetBoundOfRectangleWithSide(intersection, side)))
                {
                    trimNode(changingNode, controlNode, side);
                    if (CanCombine(changingNode, controlNode))
                    {
                        // will have same parent so we dont need to worry about
                        CombineNodes(existingNode, newNode);
                        return false;
                    }
                    return true;
                }
                // In this case the top and bottom are being split by this new line
                else
                {
                    splitNode(changingNode, controlNode, side);
                    return newNode == controlNode;
                }
            }
        }

        private void trimNode(InkGroupingNode changingNode, InkGroupingNode controlNode, Side side) {
            // One side set by another vertical line'
            if ( changingNode.sides[GetOppositeSide(side)] == null) {
                changingNode.sides[GetOppositeSide(side)] = controlNode.sides[side];
            }
            else
            {
                changingNode.sides[side] = controlNode.sides[side];
            }
            Console.WriteLine("Trimming");
            UpdateBounds(changingNode);
            // Bounds are only getting smaller so parent has to be the same but the kids may 
            // have been part of the larger area.
            CheckIfChildOrUpdateParent(changingNode);
        }

        private void splitNode(InkGroupingNode changingNode, InkGroupingNode controlNode, Side side) {
            Console.WriteLine("Split changing node in 2");
            // destory all references to the node
            DestroyNode(changingNode);
            if (side == Side.Left || side == Side.Right)
            {
                //left side of changing node
                InsertNewInkNode(createSideDictionary(changingNode.sides[Side.Left],
                    controlNode.sides[side], changingNode.sides[Side.Top],
                    changingNode.sides[Side.Bottom]));
                //right side of changing node
                InsertNewInkNode(createSideDictionary(controlNode.sides[side],
                    changingNode.sides[Side.Right], changingNode.sides[Side.Top],
                    changingNode.sides[Side.Bottom]));
            }
            else {
                //top side of changing node
                InsertNewInkNode(createSideDictionary(changingNode.sides[Side.Left],
                    changingNode.sides[Side.Right], changingNode.sides[Side.Top],
                    controlNode.sides[side]));
                //bottom side of changing node
                InsertNewInkNode(createSideDictionary(changingNode.sides[Side.Left],
                    changingNode.sides[Side.Right], controlNode.sides[side],
                    changingNode.sides[Side.Bottom]));
            }
            // If the control's right side is null then changingNode's right side should
            // alse be the control's right side - this is all we can do for now because
            // we dont know about the top and bottom
            controlNode.sides[GetOppositeSide(side)] = changingNode.sides[GetOppositeSide(side)];
            UpdateBounds(controlNode);
            CheckIfChildOrUpdateParent(controlNode);
        }

        private void overlappingNodes(InkGroupingNode changingNode, InkGroupingNode controlNode, Side side)
        {
            Console.WriteLine("Two squares overlapping");
            DestroyNode(changingNode);
            DestroyNode(controlNode);
            if (side == Side.Left)
            {
                // Left Node
                InsertNewInkNode(createSideDictionary(changingNode.sides[Side.Left], controlNode.sides[Side.Left],
                    changingNode.sides[Side.Top], changingNode.sides[Side.Bottom]));
                // Right node
                InsertNewInkNode(createSideDictionary(changingNode.sides[Side.Right], controlNode.sides[Side.Right],
                    controlNode.sides[Side.Top], controlNode.sides[Side.Bottom]));
                // Middle node - a bit complicated because the top and bottom might 
                // be different so we're just going to create two nodes - one with
                // the controls top and botton and one with the changine node's
                InsertNewInkNode(createSideDictionary(controlNode.sides[Side.Left], changingNode.sides[Side.Right],
                    changingNode.sides[Side.Top], changingNode.sides[Side.Bottom]));
                InsertNewInkNode(createSideDictionary(controlNode.sides[Side.Left], changingNode.sides[Side.Right],
                    controlNode.sides[Side.Top], controlNode.sides[Side.Bottom]));
            }
            else if (side == Side.Right)
            {
                // Left Node
                InsertNewInkNode(createSideDictionary(controlNode.sides[Side.Left], changingNode.sides[Side.Left],
                    controlNode.sides[Side.Top], controlNode.sides[Side.Bottom]));
                // Right node
                InsertNewInkNode(createSideDictionary(controlNode.sides[Side.Right], changingNode.sides[Side.Right],
                    changingNode.sides[Side.Top], changingNode.sides[Side.Bottom]));
                // Middle node - a bit complicated because the top and bottom might 
                // be different so we're just going to create two nodes - one with
                // the controls top and botton and one with the changine node's
                InsertNewInkNode(createSideDictionary(changingNode.sides[Side.Left], controlNode.sides[Side.Right],
                    changingNode.sides[Side.Top], changingNode.sides[Side.Bottom]));
                InsertNewInkNode(createSideDictionary(changingNode.sides[Side.Left], controlNode.sides[Side.Right],
                    controlNode.sides[Side.Top], controlNode.sides[Side.Bottom]));
            }
            else if (side == Side.Top)
            {
                // Top Node
                InsertNewInkNode(createSideDictionary(changingNode.sides[Side.Left], changingNode.sides[Side.Right],
                    changingNode.sides[Side.Top], controlNode.sides[Side.Top]));
                // Bottom node
                InsertNewInkNode(createSideDictionary(controlNode.sides[Side.Left], controlNode.sides[Side.Right],
                    changingNode.sides[Side.Bottom], controlNode.sides[Side.Bottom]));
                // Middle node - a bit complicated because the left and right might 
                // be different so we're just going to create two nodes - one with
                // the controls left and right and one with the changine node's
                InsertNewInkNode(createSideDictionary(changingNode.sides[Side.Left], changingNode.sides[Side.Right],
                    controlNode.sides[Side.Top], changingNode.sides[Side.Bottom]));
                InsertNewInkNode(createSideDictionary(controlNode.sides[Side.Left], controlNode.sides[Side.Right],
                    controlNode.sides[Side.Top], changingNode.sides[Side.Bottom]));
            }
            else {
                // Top Node
                InsertNewInkNode(createSideDictionary(controlNode.sides[Side.Left], controlNode.sides[Side.Right],
                    controlNode.sides[Side.Top], changingNode.sides[Side.Top]));
                // Bottom node
                InsertNewInkNode(createSideDictionary(changingNode.sides[Side.Left], changingNode.sides[Side.Right],
                    controlNode.sides[Side.Bottom], changingNode.sides[Side.Bottom]));
                // Middle node - a bit complicated because the left and right might 
                // be different so we're just going to create two nodes - one with
                // the controls left and right and one with the changine node's
                InsertNewInkNode(createSideDictionary(changingNode.sides[Side.Left], changingNode.sides[Side.Right],
                    changingNode.sides[Side.Top], controlNode.sides[Side.Bottom]));
                InsertNewInkNode(createSideDictionary(controlNode.sides[Side.Left], controlNode.sides[Side.Right],
                    changingNode.sides[Side.Top], controlNode.sides[Side.Bottom]));
            }
        }

        private void CheckIfChildOrUpdateParent(InkGroupingNode node)
        {
            Console.WriteLine("Check Kids");
            int childIndex = 0;
            while (childIndex < node.children.Count) {
                InkGroupingNode child = node.children[childIndex];
                Console.WriteLine("Child: " + child.bounds);
                if (!node.bounds.Contains(child.bounds))
                {
                    node.children.Remove(child);
                    Console.WriteLine("Bye kiddo");
                    SetParentOfNodeAndFixBounds(child, root);
                }
                else {
                    childIndex++;
                }
            }
        }

        // Destroys all references to node
        private void DestroyNode(InkGroupingNode node)
        {
            if (node.parent.children.Contains(node))
            {
                node.parent.children.Remove(node);
            }

            foreach (InkGroupingNode child in node.children)
            {
                SetParentOfNodeAndFixBounds(child, root);
            }
        }

        private void CombineNodes(InkGroupingNode remainingNode, InkGroupingNode nodeToBeDeleted)
        {
            Console.WriteLine("Combining Nodes");
            remainingNode.sides[Side.Left] = (remainingNode.sides[Side.Left] == null) ?
                nodeToBeDeleted.sides[Side.Left] : remainingNode.sides[Side.Left];
            remainingNode.sides[Side.Right] = (remainingNode.sides[Side.Right] == null) ?
                nodeToBeDeleted.sides[Side.Right] : remainingNode.sides[Side.Right];
            remainingNode.sides[Side.Top] = (remainingNode.sides[Side.Top] == null) ?
                nodeToBeDeleted.sides[Side.Top] : remainingNode.sides[Side.Top];
            remainingNode.sides[Side.Bottom] = (remainingNode.sides[Side.Bottom] == null) ?
                nodeToBeDeleted.sides[Side.Bottom] : remainingNode.sides[Side.Bottom];

            UpdateBounds(remainingNode);

            DestroyNode(nodeToBeDeleted);
        }

        #region Bounds
        private void UpdateBounds(InkGroupingNode node)
        {
            double origLeft = getBound(node, Side.Left);
            double origRight = getBound(node, Side.Right);
            double origTop = getBound(node, Side.Top);
            double origBottom = getBound(node, Side.Bottom);
           
            double left;
            double right;
            double top;
            double bottom;

            double differenceThreshold = .75;

            if (origLeft < 0)
            {
                Console.WriteLine("Find Left");
                if (origTop >= 0 && origBottom < 0)
                {
                    left = getShapeAttributes(node.sides[Side.Top]).Item1;
                }
                else if (origTop < 0 && origBottom >= 0)
                {
                    left = getShapeAttributes(node.sides[Side.Bottom]).Item1;
                }
                else if (origTop >= 0 && origBottom >= 0)
                {
                    double bottomLeft = getShapeAttributes(node.sides[Side.Bottom]).Item1;
                    double topLeft = getShapeAttributes(node.sides[Side.Top]).Item1;
                    left = Math.Min(bottomLeft, topLeft);
                    // Theres a case where we might have a really small line trying to create multiple nodes with a
                    // really big line. We only want the max if their difference in lengths is not greater than the threshold
                    double min = Math.Min(bottomLeft, topLeft);
                    double max = Math.Max(bottomLeft, topLeft);
                    left = (min / max > differenceThreshold) ? min : max;
                }
                else
                {
                    left = XPosition;
                }
            }
            else
            {
                left = origLeft;
            }

            if (origRight < 0)
            {
                Console.WriteLine("Find Right");
                if (origTop >= 0 && origBottom < 0)
                {
                    right = GetRightCoordinate(node, Side.Top);
                }
                else if (origTop < 0 && origBottom >= 0)
                {
                    right = GetRightCoordinate(node, Side.Bottom);
                }
                else if (origTop >= 0 && origBottom >= 0)
                {
                    double bottomRight = GetRightCoordinate(node, Side.Bottom);
                    double topRight = GetRightCoordinate(node, Side.Top);
                    // Theres a case where we might have a really small line trying to create multiple nodes with a
                    // really big line. We only want the max if their difference in lengths is not greater than the threshold
                    double min = Math.Min(bottomRight, topRight);
                    double max = Math.Max(bottomRight, topRight);
                    right = (min / max > differenceThreshold) ? max : min;
                }
                else
                {
                    right = XPosition + Width;
                }
            }
            else
            {
                right = origRight;
            }

            if (origTop < 0)
            {
                Console.WriteLine("Find Top");
                if (origLeft >= 0 && origRight < 0)
                {
                    top = getShapeAttributes(node.sides[Side.Left]).Item2;
                }
                else if (origLeft < 0 && origRight >= 0)
                {
                    top = getShapeAttributes(node.sides[Side.Right]).Item2;
                }
                else if (origLeft >= 0 && origRight >= 0)
                {
                    double topLeft = getShapeAttributes(node.sides[Side.Left]).Item2;
                    double topRight = getShapeAttributes(node.sides[Side.Right]).Item2;
                    // Theres a case where we might have a really small line trying to create multiple nodes with a
                    // really big line. We only want the max if their difference in lengths is not greater than the threshold
                    double min = Math.Min(topLeft, topRight);
                    double max = Math.Max(topLeft, topRight);
                    top = (min / max > differenceThreshold) ? min : max;
                }
                else
                {
                    top = YPosition;
                }
            }
            else
            {
                top = origTop;
            }

            if (origBottom < 0)
            {
                Console.WriteLine("Find Bottom");
                if (origLeft >= 0 && origRight < 0)
                {
                    bottom = GetBottomCoordinate(node, Side.Left);
                }
                else if (origLeft < 0 && origRight >= 0)
                {
                    bottom = GetBottomCoordinate(node, Side.Right);
                }
                else if (origLeft >= 0 && origRight >= 0)
                {
                    double leftBottom = GetBottomCoordinate(node, Side.Left);
                    double rightBottom = GetBottomCoordinate(node, Side.Right);
                    // Theres a case where we might have a really small line trying to create multiple nodes with a
                    // really big line. We only want the max if their difference in lengths is not greater than the threshold
                    double min = Math.Min(leftBottom, rightBottom);
                    double max = Math.Max(leftBottom, rightBottom);
                    bottom = (min / max > differenceThreshold) ? max : min;
                }
                else
                {
                    bottom = YPosition + Height;
                }
            }
            else
            {
                bottom = origBottom;
            }

            Console.WriteLine("New Left: " + left + "; New Right: " + right + "; New Top: " + top + "; New Bottom: " + bottom);
            node.bounds = new Rect(left, top, right - left, bottom - top);
        }

        private double getBound(InkGroupingNode node, Side side)
        {
            if (node.sides[side] == null)
            {
                // No left through side
                return -1;
            }
            // check line options
            else if (isLine(node.sides[side]))
            {
                if (side == Side.Left || side == Side.Right)
                {
                    return getShapeAttributes(node.sides[side]).Item1;
                }
                else
                {
                    return getShapeAttributes(node.sides[side]).Item2;
                }
            }
            else if (((side == Side.Left || side == Side.Right) && node.sides[Side.Left] == node.sides[Side.Right]) ||
              ((side == Side.Top || side == Side.Bottom) && node.sides[Side.Top] == node.sides[Side.Bottom]))
            {
                if (side == Side.Left)
                {
                    return getShapeAttributes(node.sides[side]).Item1;
                }
                else if (side == Side.Right)
                {
                    return GetRightCoordinate(node, side);
                }
                else if (side == Side.Top)
                {
                    return getShapeAttributes(node.sides[side]).Item2;
                }
                else
                {
                    return GetBottomCoordinate(node, side);
                }
            }
            else
            {
                // This means that the bound is found from the opposite side
                if (side == Side.Left)
                {
                    return GetRightCoordinate(node, side);
                }
                else if (side == Side.Right)
                {
                    return getShapeAttributes(node.sides[side]).Item1;
                }
                else if (side == Side.Top)
                {
                    return GetBottomCoordinate(node, side);
                }
                else
                {
                    return getShapeAttributes(node.sides[side]).Item2;
                }
            }
        }

        private double GetRightCoordinate(InkGroupingNode node, Side side)
        {
            Tuple<double, double, double, double> attributes = getShapeAttributes(node.sides[side]);
            return attributes.Item1 + attributes.Item3;
        }

        private double GetBottomCoordinate(InkGroupingNode node, Side side)
        {
            Tuple<double, double, double, double> attributes = getShapeAttributes(node.sides[side]);
            return attributes.Item2 + attributes.Item4;
        }
        
        // Attributes for node based on shape
        // Returns Tuple <x, y, width, height> with the suggested attributes of each node
        // Using -1 as a null value since attributes can never be null
        private Tuple<double, double, double, double> getShapeAttributes(CLPNamedInkSet shape)
        {
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
                double y = (shapeBounds.Bottom + shapeBounds.Top) / 2;
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

        private bool AdjacentSidesAllowBound(InkGroupingNode node, Side side, double proposedBound)
        {
            // percentage of line that must still be present in shape to allow purely trimming
            double threshold = .80;
            // left
            CLPNamedInkSet adjacent1 = node.sides[GetAdjacentSide(side)];
            CLPNamedInkSet adjacent2 = node.sides[GetOppositeSide(GetAdjacentSide(side))];
            Rect bounds;
            if (adjacent1 != null && adjacent2 != null)
            {
                Console.WriteLine("Both adjacents present");
                Rect adj1Bounds = CLPPage.BytesToStrokes(adjacent1.InkShapeStrokes).GetBounds();
                Rect adj2Bounds = CLPPage.BytesToStrokes(adjacent2.InkShapeStrokes).GetBounds();
                if (side == Side.Left || side == Side.Top)
                {
                    bounds = Math.Max(GetBoundOfRectangleWithSide(adj1Bounds, side),
                        GetBoundOfRectangleWithSide(adj2Bounds, side)) == GetBoundOfRectangleWithSide(adj1Bounds, side)
                        ? adj1Bounds : adj2Bounds;
                }
                else
                {
                    bounds = Math.Min(GetBoundOfRectangleWithSide(adj1Bounds, side),
                        GetBoundOfRectangleWithSide(adj2Bounds, side)) == GetBoundOfRectangleWithSide(adj1Bounds, side)
                        ? adj1Bounds : adj2Bounds;
                }
            }
            // Only one defined
            else
            {
                Console.WriteLine("Only 1 adjacent");
                CLPNamedInkSet shape = (adjacent1 != null) ? adjacent1 : adjacent2;
                bounds = CLPPage.BytesToStrokes(shape.InkShapeStrokes).GetBounds();
            }

            if (side == Side.Left || side == Side.Top)
            {
                Console.WriteLine("Proposed bound: " + proposedBound + " Opposite bound: " + GetBoundOfRectangleWithSide(bounds, GetOppositeSide(side)) +
" Difference: " + (GetBoundOfRectangleWithSide(bounds, GetOppositeSide(side)) - proposedBound) +
    " Percentage of original: " + (GetBoundOfRectangleWithSide(bounds, GetOppositeSide(side)) - proposedBound) / bounds.Width);
                // checking if the new proposed bound is in the extra space we gave the line - then don't worry, then checking if its actually splitting that line
                return proposedBound > GetBoundOfRectangleWithSide(bounds, GetOppositeSide(side)) || (GetBoundOfRectangleWithSide(bounds, GetOppositeSide(side)) - proposedBound) / bounds.Width > threshold;
            }
            else {
                Console.WriteLine("Proposed bound: " + proposedBound + " Opposite bound: " + GetBoundOfRectangleWithSide(bounds, GetOppositeSide(side)) +
" Difference: " + (proposedBound - GetBoundOfRectangleWithSide(bounds, GetOppositeSide(side))) +
    " Percentage of original: " + (proposedBound - GetBoundOfRectangleWithSide(bounds, GetOppositeSide(side))) / bounds.Width);
                return proposedBound < GetBoundOfRectangleWithSide(bounds, GetOppositeSide(side)) || (proposedBound - GetBoundOfRectangleWithSide(bounds, GetOppositeSide(side))) / bounds.Width > threshold;
            }
        }

        private double GetBoundOfRectangleWithSide(Rect r, Side s) {
            if (s == Side.Top) {
                return r.Top;
            }
            else if (s == Side.Bottom) {
                return r.Bottom;
            }
            else if (s == Side.Left)
            {
                return r.Left;
            }
            else {
                return r.Right;
            }
        }

        #endregion

        private bool isLine(CLPNamedInkSet shape)
        {
            return (shape.InkShapeType == "Vertical" || shape.InkShapeType == "Horizontal");
        }

        #region Sides
        enum Side { Left, Top, Right, Bottom };

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

        //Have either the same sides defined or one has side defined and other is null - must share at least one side
        private bool HasSameAndPossiblyMoreDefinedSides(InkGroupingNode n1, InkGroupingNode n2)
        {
            int sharedSides = 0;
            for(int n = 0; n < 4; n++) {
                Side s = (Side)n;
                if (n1.sides[s] != null && n2.sides[s] != null)
                {
                    if (n1.sides[s] == n2.sides[s])
                    {
                        Console.WriteLine("Shared side: " + s);
                        sharedSides++;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return sharedSides > 0;
        }

        private Side GetOppositeSide(Side s)
        {
            return (Side)(((int)s + 2) % 4);
        }

        private Side GetAdjacentSide(Side s)
        {
            return (Side)(((int)s + 1) % 4);
        }

        #endregion

        private void setInkShapeRegionAttributes()
        {
            InkShapeRegion.Width = Width;
            InkShapeRegion.Height = Height;
            InkShapeRegion.XPosition = XPosition;
            InkShapeRegion.YPosition = YPosition;
        }

        private class InkGroupingNode
        {
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
                foreach (ClippedObject disObj in group.groupObjects)
                {
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

            public string printGroupObjects()
            {
                StringBuilder sb = new StringBuilder();
                foreach (ClippedObject co in groupObjects)
                {
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
