using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPRegion : ACLPPageObjectBase
    {
        #region Constructors

        public CLPRegion(ObservableCollection<string> pageObjectIDs, double xPosition, double yPosition, double height, double width, ICLPPage page)
            : base(page)
        {
            PageObjectObjectParentIDs = pageObjectIDs;
            XPosition = xPosition;
            YPosition = yPosition;
            Height = height;
            Width = width;
            CanAcceptStrokes = false;
            CanAcceptPageObjects = true;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPRegion(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Overrides of ACLPPageObjectBase

        public override string PageObjectType
        {
            get { return "CLPRegion"; }
        }

        public override ICLPPageObject Duplicate()
        {
            var newRegion = Clone() as CLPRegion;
            if(newRegion != null) {
                newRegion.UniqueID = Guid.NewGuid().ToString();
                newRegion.ParentPage = ParentPage;
                return newRegion;
            }
            return null;
        }

        #endregion //Overrides of ACLPPageObjectBase


    }
}
