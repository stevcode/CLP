﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Data;
using System.Collections.ObjectModel;

namespace CLP.Models
{
    [Serializable]
    public class CLPStampCopy : ACLPPageObjectBase
    {
        public static double PartsHeight
        {
            get
            {
                return 20;
            }
        }

        #region Constructors

        public CLPStampCopy(ICLPPage page, string imageID)
            : base(page)
        {
            ImageID = imageID;
            Height = 75;
            Width = 75;
            CanAcceptStrokes = false;
            CanAcceptPageObjects = false;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        private CLPStampCopy(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public override string PageObjectType { get { return "CLPStampCopy"; } }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Whether or not the stamp copy has been stamped on a page.
        /// </summary>
        public bool IsStamped
        {
            get { return GetValue<bool>(IsStampedProperty); }
            set { SetValue(IsStampedProperty, value); }
        }

        public static readonly PropertyData IsStampedProperty = RegisterProperty("IsStamped", typeof(bool), false);

        /// <summary>
        /// Whether or not the StampCopy is a copy of a Collection Stamp.
        /// </summary>
        public bool IsCollectionCopy
        {
            get { return GetValue<bool>(IsCollectionCopyProperty); }
            set { SetValue(IsCollectionCopyProperty, value); }
        }

        public static readonly PropertyData IsCollectionCopyProperty = RegisterProperty("IsCollectionCopy", typeof(bool), false);

        /// <summary>
        /// UniqueID of the image, whose bytes are stored in the page's ImagePool.
        /// </summary>
        public string ImageID
        {
            get { return GetValue<string>(ImageIDProperty); }
            set { SetValue(ImageIDProperty, value); }
        }

        public static readonly PropertyData ImageIDProperty = RegisterProperty("ImageID", typeof(string), string.Empty);

        /// <summary>
        /// Ink Strokes serialized via Data Transfer Object, StrokeDTO.
        /// </summary>
        public ObservableCollection<StrokeDTO> SerializedStrokes
        {
            get { return GetValue<ObservableCollection<StrokeDTO>>(SerializedStrokesProperty); }
            set { SetValue(SerializedStrokesProperty, value); }
        }

        public static readonly PropertyData SerializedStrokesProperty = RegisterProperty("SerializedStrokes", typeof(ObservableCollection<StrokeDTO>), () => new ObservableCollection<StrokeDTO>());

        #endregion

        #region Methods

        public override ICLPPageObject Duplicate()
        {
            var newContainer = Clone() as CLPStampCopy;
            newContainer.UniqueID = Guid.NewGuid().ToString();
            newContainer.ParentPage = ParentPage;

            return newContainer;
        }

        public override void OnRemoved()
        {
            foreach (var po in GetPageObjectsOverPageObject())
            {
                po.OnRemoved();
                ParentPage.PageObjects.Remove(po);
            }
        }

        private void RefreshParts()
        {
            var count = GetPageObjectsOverPageObject().Sum(pageObject => pageObject.Parts);
            Parts = count;
        }

        #region Overrides of ACLPPageObjectBase

        public override void AcceptObjects(IEnumerable<ICLPPageObject> addedPageObjects, IEnumerable<ICLPPageObject> removedPageObjects)
        {
            if(!CanAcceptPageObjects)
            {
                return;
            }

            foreach(var pageObject in removedPageObjects.Where(pageObject => PageObjectObjectParentIDs.Contains(pageObject.UniqueID)))
            {
                PageObjectObjectParentIDs.Remove(pageObject.UniqueID);
            }

            foreach(var pageObject in addedPageObjects.Where(pageObject => !PageObjectObjectParentIDs.Contains(pageObject.UniqueID) &&
                                                                           pageObject.GetType() == typeof(CLPStampCopy) &&
                                                                           !(pageObject as CLPStampCopy).IsCollectionCopy))
            {
                PageObjectObjectParentIDs.Add(pageObject.UniqueID);
            }

            RefreshParts();
        }

        #endregion

        #endregion //Methods
    }
}
