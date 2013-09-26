﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    [Serializable, KnownType(typeof(CLPImage))]
    public class CLPStamp : ModelBase, ICLPPageObject
    {
        #region Constants

        public static double HandleHeight
        {
            get
            {
                return 35;
            }
        }    

        public static double PartsHeight
        {
            get
            {
                return 75;
            }
        }

        #endregion //Constants

        #region Constructors

        public CLPStamp(ICLPPageObject internalPageObject, ICLPPage page, bool isContainerStamp = false)
        { 
            StrokePathContainer = new CLPStrokePathContainer(internalPageObject, page) {IsInternalPageObject = true};

            Height = StrokePathContainer.Height + HandleHeight + PartsHeight;
            Width = StrokePathContainer.Width;

            ParentPage = page;
            ParentPageID = page.UniqueID;
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            ParentID = "";
            CanAcceptStrokes = true;
            CanAcceptPageObjects = isContainerStamp;
            if(isContainerStamp)
            {
                Parts = 0;
                PartsAutoGenerated = true;
                PartsAuthorGenerated = true;
            }
            ACLPPageObjectBase.ApplyDistinctPosition(this);
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

        public string PageObjectType
        {
            get { return "CLPStamp"; }
        }

        /// <summary>
        /// Internally contained StrokePathContainer
        /// </summary>
        public CLPStrokePathContainer StrokePathContainer
        {
            get { return GetValue<CLPStrokePathContainer>(StrokePathContainerProperty); }
            set { SetValue(StrokePathContainerProperty, value); }
        }

        public static readonly PropertyData StrokePathContainerProperty = RegisterProperty("StrokePathContainer", typeof(CLPStrokePathContainer));

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool PartsAutoGenerated
        {
            get { return GetValue<bool>(PartsAutoGeneratedProperty); }
            set { SetValue(PartsAutoGeneratedProperty, value); }
        }

        public static readonly PropertyData PartsAutoGeneratedProperty = RegisterProperty("PartsAutoGenerated", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool PartsAuthorGenerated
        {
            get { return GetValue<bool>(PartsAuthorGeneratedProperty); }
            set { SetValue(PartsAuthorGeneratedProperty, value); }
        }

        public static readonly PropertyData PartsAuthorGeneratedProperty = RegisterProperty("PartsAuthorGenerated", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool PartsInterpreted
        {
            get { return GetValue<bool>(PartsInterpretedProperty); }
            set { SetValue(PartsInterpretedProperty, value); }
        }

        public static readonly PropertyData PartsInterpretedProperty = RegisterProperty("PartsInterpreted", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ICLPPage ParentPage
        {
            get { return GetValue<ICLPPage>(ParentPageProperty); }
            set 
            { 
                SetValue(ParentPageProperty, value);
                StrokePathContainer.ParentPage = value;
                StrokePathContainer.ParentPageID = value.UniqueID;
            }
        }

        [NonSerialized]
        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(ICLPPage), null, includeInSerialization: false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string ParentPageID
        {
            get
            {
                var tempValue = GetValue<string>(ParentPageIDProperty);
                if(tempValue != "")
                {
                    return tempValue;
                }
                SetValue(ParentPageIDProperty, ParentPage.UniqueID);
                return ParentPage.UniqueID;
            }
            set { SetValue(ParentPageIDProperty, value); }
        }

        public static readonly PropertyData ParentPageIDProperty = RegisterProperty("ParentPageID", typeof(string), "");

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string ParentID
        {
            get { return GetValue<string>(ParentIDProperty); }
            set { SetValue(ParentIDProperty, value); }
        }

        public static readonly PropertyData ParentIDProperty = RegisterProperty("ParentID", typeof(string), "");

        /// <summary>
        /// Creation date of pageObject.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>
        /// UniqueID of pageObject.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            set { SetValue(UniqueIDProperty, value); }
        }

        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<string> PageObjectStrokeParentIDs
        {
	        get { return GetValue<ObservableCollection<string>>(PageObjectStrokeParentIDsProperty); }
	        set { SetValue(PageObjectStrokeParentIDsProperty, value); }
        }

        public static readonly PropertyData PageObjectStrokeParentIDsProperty = RegisterProperty("PageObjectStrokeParentIDs", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());
        
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool CanAcceptStrokes
        {
            get { return GetValue<bool>(CanAcceptStrokesProperty); }
            set { SetValue(CanAcceptStrokesProperty, value); }
        }

        public static readonly PropertyData CanAcceptStrokesProperty = RegisterProperty("CanAcceptStrokes", typeof(bool), true);

        /// <summary>
        /// UniqueIDs of the pageObjects above a pageObject.
        /// </summary>
        public ObservableCollection<string> PageObjectObjectParentIDs
        {
            get { return GetValue<ObservableCollection<string>>(PageObjectObjectParentIDsProperty); }
            set { SetValue(PageObjectObjectParentIDsProperty, value); }
        }

        public static readonly PropertyData PageObjectObjectParentIDsProperty = RegisterProperty("PageObjectObjectParentIDs", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool CanAcceptPageObjects
        {
            get { return GetValue<bool>(CanAcceptPageObjectsProperty); }
            set { SetValue(CanAcceptPageObjectsProperty, value); }
        }

        public static readonly PropertyData CanAcceptPageObjectsProperty = RegisterProperty("CanAcceptPageObjects", typeof(bool), true);

        /// <summary>
        /// xPosition of pageObject on page.
        /// </summary>
        public double XPosition
        {
            get { return GetValue<double>(XPositionProperty); }
            set { SetValue(XPositionProperty, value); }
        }

        public static readonly PropertyData XPositionProperty = RegisterProperty("XPosition", typeof(double), 50.0);

        /// <summary>
        /// YPosition of pageObject on page.
        /// </summary>
        public double YPosition
        {
            get { return GetValue<double>(YPositionProperty); }
            set { SetValue(YPositionProperty, value); }
        }

        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof(double), 50.0);

        /// <summary>
        /// Height of pageObject.
        /// </summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set 
            { 
                SetValue(HeightProperty, value);
                StrokePathContainer.Height = Height - HandleHeight - PartsHeight;
                if (StrokePathContainer.InternalPageObject != null)
                {
                    StrokePathContainer.InternalPageObject.Height = StrokePathContainer.Height;
                }
            }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double), 100);

        /// <summary>
        /// Width of pageObject.
        /// </summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set 
            { 
                SetValue(WidthProperty, value);
                StrokePathContainer.Width = Width;
                if (StrokePathContainer.InternalPageObject != null)
                {
                    StrokePathContainer.InternalPageObject.Width = StrokePathContainer.Width;
                }
            }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double), 100);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsBackground
        {
            get { return GetValue<bool>(IsBackgroundProperty); }
            set { SetValue(IsBackgroundProperty, value); }
        }

        public static readonly PropertyData IsBackgroundProperty = RegisterProperty("IsBackground", typeof(bool), false);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int Parts
        {
            get { return GetValue<int>(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof(int), 0);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool CanAdornersShow
        {
            get { return GetValue<bool>(CanAdornersShowProperty); }
            set { SetValue(CanAdornersShowProperty, value); }
        }

        public static readonly PropertyData CanAdornersShowProperty = RegisterProperty("CanAdornersShow", typeof(bool), true);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsInternalPageObject
        {
            get { return GetValue<bool>(IsInternalPageObjectProperty); }
            set { SetValue(IsInternalPageObjectProperty, value); }
        }

        /// <summary>
        /// Register the IsInternalPageObject property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsInternalPageObjectProperty = RegisterProperty("IsInternalPageObject", typeof(bool), false);

        #endregion //Properties

        #region Methods

        public List<ICLPPageObject> Cut(Stroke cuttingStroke)
        {
            return new List<ICLPPageObject>();
        }

        public ICLPPageObject Duplicate()
        {
            var newStamp = Clone() as CLPStamp;
            if (newStamp == null) return null;

            newStamp.UniqueID = Guid.NewGuid().ToString();
            newStamp.ParentPage = ParentPage;
            if(newStamp.StrokePathContainer != null)
            {
                newStamp.StrokePathContainer.ParentPage = ParentPage;
            }
            return newStamp;
        }

        public virtual void OnRemoved()
        {
            if(StrokePathContainer.IsStamped)
            {
                return;
            }

            foreach(Stroke stroke in GetStrokesOverPageObject())
            {
                ParentPage.InkStrokes.Remove(stroke);
            }

            if(CanAcceptPageObjects)
            {
                foreach(ICLPPageObject po in GetPageObjectsOverPageObject())
                {
                    //TODO: Steve - Make CLPPage level method RemovePageObject to guarantee OnRemoved() is called.
                    po.OnRemoved();
                    ParentPage.PageObjects.Remove(po);
                }
            }
        }

        public void OnMoved()
        {
        }

        public void OnResized()
        {
        }

        public virtual void RefreshStrokeParentIDs()
        {
            if(!CanAcceptStrokes)
            {
                return;
            }

            PageObjectStrokeParentIDs.Clear();

            var rect = new Rect(XPosition, YPosition, Width, Height);
            var strokesOverObject = 
                from stroke in ParentPage.InkStrokes
                where stroke.HitTest(rect, 50)
                select stroke;

            AcceptStrokes(new StrokeCollection(strokesOverObject), new StrokeCollection());
        }

        public void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            foreach(Stroke s in removedStrokes)
            {
                string strokeID = s.GetStrokeUniqueID();
                try
                {
                    PageObjectStrokeParentIDs.Remove(strokeID);
                }
                catch(Exception)
                {
                    Console.WriteLine("StrokeID not found in PageObjectStrokeParentIDs. StrokeID: " + strokeID);
                }
            }

            var containerBoundingBox = new Rect(XPosition, YPosition + HandleHeight,
                StrokePathContainer.Width, StrokePathContainer.Height);      

            foreach(Stroke stroke in addedStrokes.Where(stroke => stroke.HitTest(containerBoundingBox, 50)))
            {
                if(!PageObjectStrokeParentIDs.Contains(stroke.GetStrokeUniqueID()))
                {
                    PageObjectStrokeParentIDs.Add(stroke.GetStrokeUniqueID());
                }
            }
        }

        public StrokeCollection GetStrokesOverPageObject()
        {
            var strokes =
                from strokeID in PageObjectStrokeParentIDs
                from stroke in ParentPage.InkStrokes
                where stroke.GetStrokeUniqueID() == strokeID
                select stroke;

            var inkStrokes = new StrokeCollection(strokes);
            return inkStrokes;
        }

        public void ResetParts()
        {
            Parts = 0;
            PartsAutoGenerated = false;
        }

        public bool PageObjectIsOver(ICLPPageObject pageObject, double percentage)
        {
            double areaObject = pageObject.Height * pageObject.Width;
            double top = Math.Max(YPosition + HandleHeight, pageObject.YPosition);
            double bottom = Math.Min(YPosition + Height - PartsHeight, pageObject.YPosition + pageObject.Height);
            double left = Math.Max(XPosition, pageObject.XPosition);
            double right = Math.Min(XPosition + Width, pageObject.XPosition + pageObject.Width);
            double deltaY = bottom - top;
            double deltaX = right - left;
            double intersectionArea = deltaY * deltaX;
            return deltaY >= 0 && deltaX >= 0 && intersectionArea / areaObject >= percentage;
        }

        public void AcceptObjects(ObservableCollection<ICLPPageObject> addedPageObjects, ObservableCollection<ICLPPageObject> removedPageObjects)
        {
            bool changed = false;
            if(CanAcceptPageObjects)
            {
                foreach(ICLPPageObject pageObject in removedPageObjects)
                {
                    if(PageObjectObjectParentIDs.Contains(pageObject.UniqueID))
                    {
                        changed = true;
                        Parts = (Parts - pageObject.Parts > 0) ? Parts - pageObject.Parts : 0;
                        PageObjectObjectParentIDs.Remove(pageObject.UniqueID);
                    }
                }

                foreach(ICLPPageObject pageObject in addedPageObjects)
                {
                    if(!PageObjectObjectParentIDs.Contains(pageObject.UniqueID) && pageObject.GetType() != typeof(CLPStamp))
                    {
                        changed = true;
                        Parts += pageObject.Parts;
                        PageObjectObjectParentIDs.Add(pageObject.UniqueID);
                    }
                }

                if(changed)
                {
                    PartsAutoGenerated = true;
                    PartsInterpreted = false;
                }
            }
        }

        public ObservableCollection<ICLPPageObject> GetPageObjectsOverPageObject()
        {
            var pageObjects =
                from pageObjectID in PageObjectObjectParentIDs
                from pageObject in ParentPage.PageObjects
                where pageObject.UniqueID == pageObjectID
                select pageObject;

            var pageObjectsOver = new ObservableCollection<ICLPPageObject>(pageObjects);
            return pageObjectsOver;
        }

        #endregion //Methods

        public void EnforceAspectRatio(double aspectRatio) {}
    }
}
