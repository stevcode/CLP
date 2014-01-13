using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models.CLPPageObjects
{

    [Serializable]
    public class CLPFactorCard : CLPArray
    {
        #region Constructors

        public CLPFactorCard(int rows, int columns, ICLPPage page)
            : base(rows, columns, page)
        {
        }
        
        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPFactorCard(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}
