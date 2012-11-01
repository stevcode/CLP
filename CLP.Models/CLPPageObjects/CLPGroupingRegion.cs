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
    public class CLPGroupingRegion : CLPPageObjectBase
    {
        #region Constructors

        public CLPGroupingRegion(CLPPage page) : base(page)
        {
            GroupsString = "";
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPGroupingRegion(SerializationInfo info, StreamingContext context)
            : base(info, context) {
                Console.WriteLine("constructor deserialize");
        }

        #endregion // Constructors

        #region Properties

        public override string PageObjectType
        {
            get { return "CLPGroupingRegion"; }
        }

        /// <summary>
        /// Stored ink shapes as a string.
        /// </summary>
        public string GroupsString
        {
            get { return GetValue<string>(GroupsStringProperty); }
            set { SetValue(GroupsStringProperty, value); }
        }

        /// <summary>
        /// Register the InkShapesString property so it is known in the class.
        /// </summary>
        public static readonly PropertyData GroupsStringProperty = RegisterProperty("GroupsString", typeof(string), "");

        #endregion // Properties

        #region Methods

        public void DoInterpretation()
        {
            GroupsString = "testing - you should implement this";
        }

        public override ICLPPageObject Duplicate()
        {
            return this.Duplicate();
        }

        #endregion // Methods
    }
}
