using System;
using System.Runtime.Serialization;
using Catel.Data;
using System.Collections.ObjectModel;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPStrokePathContainer : CLPPageObjectBase
    {
        #region Constructors

        public CLPStrokePathContainer(ICLPPageObject internalPageObject)
            : base()
        {
            InternalPageObject = internalPageObject;
            IsStrokePathsVisible = false;

            if (internalPageObject == null)
            {
                Height = 100;
                Width = 100;
            }
            else
            {
                Height = InternalPageObject.Height;
                Width = InternalPageObject.Width;
            }
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPStrokePathContainer(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

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

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsStrokePathsVisible
        {
            get { return GetValue<bool>(IsStrokePathsVisibleProperty); }
            set { SetValue(IsStrokePathsVisibleProperty, value); }
        }

        /// <summary>
        /// Register the IsStrokePathsVisible property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsStrokePathsVisibleProperty = RegisterProperty("IsStrokePathsVisible", typeof(bool), false);

        public override string PageObjectType
        {
            get { return "CLPStrokePathContainer"; }
        }

        public override ICLPPageObject Duplicate()
        {
            CLPStrokePathContainer newContainer = this.Clone() as CLPStrokePathContainer;
            newContainer.UniqueID = Guid.NewGuid().ToString();

            return newContainer;
        }
    }
}
