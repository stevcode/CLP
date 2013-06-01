using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class TagOptionValue : DataObjectBase
    {
        #region Constructor

        public TagOptionValue(string value, string iconPath)
        {
            IconUri = new Uri(iconPath, UriKind.Relative); 
            Value = value;
        }

        public TagOptionValue(string value)
        {
            IconUri = null;
            Value = value;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected TagOptionValue(SerializationInfo info, StreamingContext context)
            : base(info, context) { }


        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Uri IconUri
        {
            get { return GetValue<Uri>(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        /// <summary>
        /// Register the Icon property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IconProperty = RegisterProperty("Icon", typeof(Uri), new Uri("", UriKind.Relative));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string Value
        {
            get { return GetValue<string>(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Register the Value property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ValueProperty = RegisterProperty("Value", typeof(string), "");
        #endregion
    }
}
       
