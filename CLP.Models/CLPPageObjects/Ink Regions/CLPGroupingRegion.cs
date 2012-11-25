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
            StoredAnswer = BasicGrouping();
            DistanceClustering();
        }

        private string BasicGrouping() {
            Dictionary<String, int> groups = new Dictionary<String, int>();
            String answer = "Basic Grouping: ";
            foreach (ICLPPageObject po in ParentPage.PageObjects) {
                if (po.UniqueID != UniqueID && HitTest(po, .8)) {
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

            answer += groups.Keys.Count + " Groups - ";
            foreach (String key in groups.Keys)
            {
                answer += key +" : " + groups[key] + "; ";
            }
            return answer;
        }

        private void DistanceClustering() {
            foreach (ICLPPageObject po1 in ParentPage.PageObjects)
            {
                if (validOption.Contains(po1.PageObjectType)) {
                    Console.WriteLine("PO1: " + po1.GetType() + " X: " + po1.XPosition + " Y: " + po1.YPosition);
                    foreach (ICLPPageObject po2 in ParentPage.PageObjects) {
                        if (validOption.Contains(po2.PageObjectType)) {
                            Console.WriteLine("PO2: " + po2.GetType() + " X: " + po2.XPosition + " Y: " + po2.YPosition);
                            Console.WriteLine("distance: " + getDistanceBetweenPageObjects(po1, po2));
                        }
                    }
                }
            }
        }

        private static readonly HashSet<string> validOption = new HashSet<string> {
            {CLPSnapTileContainer.Type},
            {CLPStrokePathContainer.Type},
            {CLPShape.Type}
        };

        private double getDistanceBetweenPageObjects(ICLPPageObject pageObject1, ICLPPageObject pageObject2) {
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
    }
}
