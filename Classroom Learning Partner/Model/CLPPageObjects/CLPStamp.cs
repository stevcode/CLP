using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows;
using Catel.Data;
using System.Windows.Ink;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPStamp : CLPPageObjectBase
    {
        #region Constructors

        public CLPStamp(ICLPPageObject internalPageObject)
            : base()
        {
            InternalPageObject = internalPageObject;
            CanAcceptStrokes = true;

            Position = new Point(100, 100);
            if (InternalPageObject == null)
            {
                Height = 150;
                Width = 150;
            }
            else
            {
                Height = InternalPageObject.Height + 50;
                Width = InternalPageObject.Width;
            }
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPStamp(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ICLPPageObject InternalPageObject
        {
            get { return GetValue<ICLPPageObject>(InternalPageObjectProperty); }
            set { SetValue(InternalPageObjectProperty, value); }
        }

        /// <summary>
        /// Register the InternalPageObject property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InternalPageObjectProperty = RegisterProperty("InternalPageObject", typeof(ICLPPageObject), null);

        #endregion //Properties

        #region Methods

        public override string PageObjectType
        {
            get { return "CLPStamp"; }
        }

        public override CLPPageObjectBase Duplicate()
        {
            CLPStamp newStamp = this.Clone() as CLPStamp;
            newStamp.UniqueID = Guid.NewGuid().ToString();

            return newStamp;
        }

        public override void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            this.ProcessStrokes(addedStrokes, removedStrokes);
        }

        #endregion //Methods
    }
}
