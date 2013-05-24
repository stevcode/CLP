using System;
using System.Runtime.Serialization;
using Catel.Data;
using System.Collections.ObjectModel;
using System.Collections.Generic;

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
                InternalPageObject.IsInternalPageObject = true;
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

        //[OnDeserialized]
        //private void OnDeserialized()
        //{
        //    if (InternalPageObject != null)
        //    {
        //        InternalPageObject.ParentPage = ParentPage;
        //    }
        //}

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

        public static readonly PropertyData IsStampedProperty = RegisterProperty("IsStamped", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<List<byte>> ByteStrokes
        {
            get { return GetValue<ObservableCollection<List<byte>>>(ByteStrokesProperty); }
            set { SetValue(ByteStrokesProperty, value); }
        }

        public static readonly PropertyData ByteStrokesProperty = RegisterProperty("ByteStrokes", typeof(ObservableCollection<List<byte>>), () => new ObservableCollection<List<byte>>());

        /// <summary>
        /// Ink Strokes serialized via Data Transfer Object, StrokeDTO.
        /// </summary>
        public ObservableCollection<StrokeDTO> SerializedStrokes
        {
            get { return GetValue<ObservableCollection<StrokeDTO>>(SerializedStrokesProperty); }
            set { SetValue(SerializedStrokesProperty, value); }
        }

        public static readonly PropertyData SerializedStrokesProperty = RegisterProperty("SerializedStrokes", typeof(ObservableCollection<StrokeDTO>), () => new ObservableCollection<StrokeDTO>());


        public override string PageObjectType
        {
            get { return Type; }
        }

        public override ICLPPageObject Duplicate()
        {
            CLPStrokePathContainer newContainer = this.Clone() as CLPStrokePathContainer;
            newContainer.UniqueID = Guid.NewGuid().ToString();
            newContainer.ParentPage = ParentPage;

            return newContainer;
        }

        public override void OnRemoved()
        {
            foreach (ICLPPageObject po in GetPageObjectsOverPageObject())
            {
                po.OnRemoved();
                ParentPage.PageObjects.Remove(po);
            }
        }
    }
}
