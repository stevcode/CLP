using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{

    [Serializable]
    public class CLPFactorCard : CLPArray
    {
        #region Constructors

        public CLPFactorCard(int rows, int columns, ICLPPage page)
            : base(rows, columns, page)
        {
            IsDivisionBehaviorOn = false;
            IsSnappable = false;
            IsGridOn = false;
        }
        
        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPFactorCard(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        public override string PageObjectType
        {
            get { return "CLPFactorCard"; }
        }

        #region Properties

        /// <summary>
        /// Whether the top label is displayed or shown as a "?".
        /// </summary>
        public bool IsTopLabelVisible
        {
            get
            {
                return GetValue<bool>(IsTopLabelVisibleProperty);
            }
            set
            {
                SetValue(IsTopLabelVisibleProperty, value);
            }
        }

        /// <summary>
        /// Register the IsTopLabelVisible property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsTopLabelVisibleProperty = RegisterProperty("IsTopLabelVisible", typeof(bool), false);
        
        #endregion //Properties

    }
}
