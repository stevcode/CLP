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
            BasicGrouping();
            StoredAnswer = "Hmm";
        }

        private void BasicGrouping() {
            Dictionary<String, int> groups = new Dictionary<String, int>();
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

            Console.WriteLine(groups.Keys.Count + " Groups");
            foreach (String key in groups.Keys)
            {
                Console.WriteLine(key +" : " + groups[key]);
            }
        }

        #endregion // Methods
    }
}
