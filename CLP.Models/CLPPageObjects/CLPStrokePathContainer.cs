using System;
using System.Runtime.Serialization;
using Catel.Data;
using System.Collections.ObjectModel;

namespace CLP.Models
{
    [Serializable]
    public class CLPStrokePathContainer : CLPPageObjectBase
    {

        public static string Type = "CLPStrokePathContainer";

        #region Constructors

        public CLPStrokePathContainer(ICLPPageObject internalPageObject, CLPPage page)
            : base(page)
        {
            InternalPageObject = internalPageObject;
            IsStrokePathsVisible = false;
            IsStamped = false;

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

        public static readonly PropertyData IsStrokePathsVisibleProperty = RegisterProperty("IsStrokePathsVisible", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsStamped
        {
            get { return GetValue<bool>(IsStampedProperty); }
            set { SetValue(IsStampedProperty, value); }
        }

        /// <summary>
        /// Register the IsStamped property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsStampedProperty = RegisterProperty("IsStamped", typeof(bool), false);

        public override string PageObjectType
        {
            get { return Type; }
        }

        public override ICLPPageObject Duplicate()
        {
            CLPStrokePathContainer newContainer = this.Clone() as CLPStrokePathContainer;
            newContainer.UniqueID = Guid.NewGuid().ToString();

            return newContainer;
        }
    }
}
