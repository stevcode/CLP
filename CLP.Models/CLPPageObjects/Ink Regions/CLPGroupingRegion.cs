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
            StringBuilder interpretation = new StringBuilder();
            interpretation.AppendLine(InkGrouping());
            interpretation.AppendLine(DistanceClustering());
            interpretation.AppendLine(BasicGrouping());
            StoredAnswer = interpretation.ToString();
        }

        private string BasicGrouping() {
            Dictionary<String, int> groups = new Dictionary<String, int>();
            StringBuilder answer = new StringBuilder("Basic Grouping: ");
            foreach (ICLPPageObject po in ParentPage.PageObjects) {
                if (po.UniqueID != UniqueID && PageObjectIsOver(po, .8)) {
                    String key;
                    if (po.GetType().Equals(typeof(CLPStrokePathContainer))) {
                        key = "CLPStamp" + (po as CLPStrokePathContainer).ParentID;
                    } else if (po.GetType().Equals(typeof(CLPSnapTileContainer))) {
                        key = "Tiles" + (po as CLPSnapTileContainer).NumberOfTiles;
                    }
                    else if (po.GetType().Equals(typeof(CLPShape)))
                    {
                        key = (po as CLPShape).ShapeType.ToString();
                    } else {
                        key = null;
                    }
                    if (key != null)
                    {
                        int parts = 1;
                        if (groups.ContainsKey(key))
                        {
                            parts = groups[key] + 1;
                            groups.Remove(key);
                        }
                        groups.Add(key, parts);
                    }
                }
            }

            answer.Append(groups.Keys.Count);
            answer.Append(" Groups - ");
            foreach (String key in groups.Keys)
            {
                answer.Append(key);
                answer.Append(" : ");
                answer.Append(groups[key]);
                answer.Append("; ");
            }
            return answer.ToString();
        }

        private string InkGrouping()
        {
            StringBuilder answer = new StringBuilder("Ink Grouping: ");
            //CLPInkShapeRegion inkShapeRegion = new CLPInkShapeRegion(ParentPage);
            return answer.ToString();
        }

        #region Distance Grouping

        private string DistanceClustering() {
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
            StringBuilder answer = new StringBuilder("Distance Grouping: ");
            answer.Append(groups.Count);
            answer.Append(" Groups - ");
            foreach (DistanceGroup group in groups)
            {
                answer.Append(group.groupToString());
                answer.Append("; ");
            }
            return answer.ToString();
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

            public void printGroup() {
                foreach (ICLPPageObject po in groupObjects)
                {
                    Console.Write((po as CLPShape).ShapeType.ToString() + ", ");
                }
                Console.WriteLine(" ");
            }

            public string groupToString() {
                string groupString = "";
                foreach (ICLPPageObject po in groupObjects)
                {
                    groupString += (po as CLPShape).ShapeType.ToString() + ", ";
                }
                groupString = groupString.Substring(0, groupString.Length - 2);
                return groupString;
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
