using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Catel.Data;

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
            Groupings.Add(InkGrouping());
            Groupings.Add(DistanceClustering());
            Groupings.Add(BasicGrouping());
            StringBuilder interpretation = new StringBuilder();
            foreach (CLPGrouping grouping in Groupings)
            {
                interpretation.AppendLine(grouping.toString());
            }
            StoredAnswer = interpretation.ToString();
        }

        private bool ValidObjectForGrouping(ICLPPageObject po)
        {
            return PageObjectIsOver(po, .8) && po.Parts >= 0 && po.GetType() != typeof(CLPStamp);
        }

        private CLPGrouping BasicGrouping()
        {
            CLPGrouping group = new CLPGrouping("Basic Grouping");
            List<ICLPPageObject> validGroupingObjects = new List<ICLPPageObject>();
            foreach (ICLPPageObject po in ParentPage.PageObjects) {
                if (ValidObjectForGrouping(po))
                {
                    validGroupingObjects.Add(po);
                }
            }

            Dictionary<string, List<ICLPPageObject>> groupsByObject =
                CLPGrouping.OrganizeGroupOfPageObjectsByType(validGroupingObjects);
            foreach (string key in groupsByObject.Keys)
            {
                group.AddGroup(groupsByObject[key]);
            }
            return group;
        }

        private CLPGrouping InkGrouping()
        {
            CLPGrouping group = new CLPGrouping("Ink Grouping");
            //CLPInkShapeRegion inkShapeRegion = new CLPInkShapeRegion(ParentPage);
            return group;
        }

        #region Distance Grouping

        private CLPGrouping DistanceClustering()
        {
            HashSet<DistanceGroup> groups = new HashSet<DistanceGroup>();
            foreach (ICLPPageObject po in ParentPage.PageObjects)
            {
                if (validOption.Contains(po.PageObjectType)) {
                    groups.Add(new DistanceGroup(po));
                }
            }
            Boolean canCombine = true;
            while (canCombine && groups.Count > 1) {
                canCombine = combineGroups(groups);
            }
            CLPGrouping grouping = new CLPGrouping("Distance Grouping");
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
            if (combineTheseGroups[0].average() * threshold > smallestDistanceGroups &&
                combineTheseGroups[1].average() * threshold > smallestDistanceGroups)
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
