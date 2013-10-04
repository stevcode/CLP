﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    abstract public class ACLPPageObjectBase : ModelBase, ICLPPageObject
    {
        #region Constructors

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        protected ACLPPageObjectBase(ICLPPage page)
        {
            ParentPage = page;
            ParentPageID = page.UniqueID;
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            Parts = -1;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected ACLPPageObjectBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// The page the pageObject is on.
        /// </summary>
        public ICLPPage ParentPage
        {
            get { return GetValue<ICLPPage>(ParentPageProperty); }
            set 
            { 
                SetValue(ParentPageProperty, value); 
                SetValue(ParentPageIDProperty, value.UniqueID);
            }
        }

        [NonSerialized]
        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(ICLPPage), null, includeInSerialization: false);

        /// <summary>
        /// The UniqueID of the ParentPage
        /// </summary>
        public string ParentPageID
        {
            get
            {
                var tempValue = GetValue<string>(ParentPageIDProperty);
                if (tempValue != "")
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
        /// UniqueID of the pageObject's parent pageObject, if it has one.
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
        /// UniqueIDs of the strokes above a pageObject.
        /// </summary>
        public ObservableCollection<string> PageObjectStrokeParentIDs
        {
            get { return GetValue<ObservableCollection<string>>(PageObjectStrokeParentIDsProperty); }
            set { SetValue(PageObjectStrokeParentIDsProperty, value); }
        }

        public static readonly PropertyData PageObjectStrokeParentIDsProperty = RegisterProperty("PageObjectStrokeParentIDs", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        /// <summary>
        /// Whether or not the pageObject can accept strokes
        /// </summary>
        public bool CanAcceptStrokes
        {
            get { return GetValue<bool>(CanAcceptStrokesProperty); }
            set { SetValue(CanAcceptStrokesProperty, value); }
        }

        public static readonly PropertyData CanAcceptStrokesProperty = RegisterProperty("CanAcceptStrokes", typeof(bool), false);

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

        public static readonly PropertyData CanAcceptPageObjectsProperty = RegisterProperty("CanAcceptPageObjects", typeof(bool), false);

        /// <summary>
        /// xPosition of pageObject on page
        /// </summary>
        public double XPosition
        {
            get { return GetValue<double>(XPositionProperty); }
            set { SetValue(XPositionProperty, value); }
        }

        public static readonly PropertyData XPositionProperty = RegisterProperty("XPosition", typeof(double), 10.0);

        /// <summary>
        /// YPosition of pageObject on page
        /// </summary>
        public double YPosition
        {
            get { return GetValue<double>(YPositionProperty); }
            set { SetValue(YPositionProperty, value); }
        }

        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof(double), 10.0);

        /// <summary>
        /// Height of pageObject.
        /// </summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double), 10.0);

        /// <summary>
        /// Width of pageObject.
        /// </summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double), 10.0);

        /// <summary>
        /// True if placed while in Authoring Mode.
        /// </summary>
        public bool IsBackground
        {
            get { return GetValue<bool>(IsBackgroundProperty); }
            set { SetValue(IsBackgroundProperty, value); }
        }

        public static readonly PropertyData IsBackgroundProperty = RegisterProperty("IsBackground", typeof(bool), false);

        //TODO: Steve - Remove
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
        /// Represents the number of "parts" a pageObject represents.
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
        public bool IsInternalPageObject
        {
            get { return GetValue<bool>(IsInternalPageObjectProperty); }
            set { SetValue(IsInternalPageObjectProperty, value); }
        }

        public static readonly PropertyData IsInternalPageObjectProperty = RegisterProperty("IsInternalPageObject", typeof(bool), false);

        /// <summary>
        /// Background color of a pageObject.
        /// </summary>
        public string BackgroundColor
        {
            get { return GetValue<string>(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public static readonly PropertyData BackgroundColorProperty = RegisterProperty("BackgroundColor", typeof(string), Colors.Transparent.ToString());

        #endregion

        #region Methods

        public abstract string PageObjectType { get; }

        public abstract ICLPPageObject Duplicate();

        public virtual void OnRemoved() { }

        public virtual void OnMoved() 
        {
            AddRemovePageObjectFromOtherObjects();
        }

        //TODO: make this static in viewModel?
        private void AddRemovePageObjectFromOtherObjects(bool isHistory=true)
        {
            foreach(var container in ParentPage.PageObjects)
            {
                if(!container.CanAcceptPageObjects ||
                   ParentID.Equals(container.UniqueID) ||
                   UniqueID.Equals(container.UniqueID))
                {
                    continue;
                }
                var addObjects = new ObservableCollection<ICLPPageObject>();
                var removeObjects = new ObservableCollection<ICLPPageObject>();

                if(container.PageObjectIsOver(this, .50))
                {
                    addObjects.Add(this);
                }
                else
                {
                    removeObjects.Add(this);
                }

                container.AcceptObjects(addObjects, removeObjects);
            }
        }

        public virtual void OnResized() {}

        public virtual void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            if(!CanAcceptStrokes)
            {
                return;
            }

            foreach(var strokeID in removedStrokes.Select(stroke => stroke.GetStrokeUniqueID())) 
            {
                try
                {
                    PageObjectStrokeParentIDs.Remove(strokeID);
                }
                catch(Exception)
                {
                    Console.WriteLine("StrokeID not found in PageObjectStrokeParentIDs. StrokeID: " + strokeID);
                }
            }

            foreach(var stroke in addedStrokes)
            {
                PageObjectStrokeParentIDs.Add(stroke.GetStrokeUniqueID());
            }
        }

        public virtual StrokeCollection GetStrokesOverPageObject()
        {
            var strokes = from strokeID in PageObjectStrokeParentIDs
                          from stroke in ParentPage.InkStrokes
                          where stroke.GetStrokeUniqueID() == strokeID
                          select stroke;

            var inkStrokes = new StrokeCollection(strokes.Distinct());
            return inkStrokes;
        }

        public virtual void RefreshStrokeParentIDs()
        {
            if(!CanAcceptStrokes)
            {
                return;
            }

            PageObjectStrokeParentIDs.Clear();

            var rect = new Rect(XPosition, YPosition, Width, Height);
            var strokesOverObject = from stroke in ParentPage.InkStrokes
                                    where stroke.HitTest(rect, 50)
                                    select stroke;

            AcceptStrokes(new StrokeCollection(strokesOverObject), new StrokeCollection());
        }

        public virtual void AcceptObjects(IEnumerable<ICLPPageObject> addedPageObjects, IEnumerable<ICLPPageObject> removedPageObjects)
        {
            if(!CanAcceptPageObjects)
            {
                return;
            }

            foreach(var pageObject in removedPageObjects.Where(pageObject => PageObjectObjectParentIDs.Contains(pageObject.UniqueID))) 
            {
                Parts = (Parts - pageObject.Parts > 0) ? Parts - pageObject.Parts : 0;
                PageObjectObjectParentIDs.Remove(pageObject.UniqueID);
            }

            foreach(var pageObject in addedPageObjects.Where(pageObject => !PageObjectObjectParentIDs.Contains(pageObject.UniqueID) &&
                                                                            pageObject.GetType() == typeof(CLPStampCopy))) 
            {
                Parts += pageObject.Parts;
                PageObjectObjectParentIDs.Add(pageObject.UniqueID);
            }
        }

        public virtual ObservableCollection<ICLPPageObject> GetPageObjectsOverPageObject()
        {
            var pageObjects = from pageObjectID in PageObjectObjectParentIDs
                              from pageObject in ParentPage.PageObjects
                              where pageObject.UniqueID == pageObjectID
                              select pageObject;

            var pageObjectsOver = new ObservableCollection<ICLPPageObject>(pageObjects.Distinct());
            return pageObjectsOver;
        }

        public virtual void RefreshPageObjectIDs()
        {
            if(!CanAcceptPageObjects)
            {
                return;
            }

            PageObjectObjectParentIDs.Clear();

            var pageObjectsOverObject = from pageObject in ParentPage.PageObjects
                                        where PageObjectIsOver(pageObject, .90)
                                        select pageObject;

            AcceptObjects(pageObjectsOverObject, new List<ICLPPageObject>());
        }

        public virtual bool PageObjectIsOver(ICLPPageObject pageObject, double percentage)
        {
            var areaObject = pageObject.Height * pageObject.Width;
            var area = Height * Width;
            var top = Math.Max(YPosition, pageObject.YPosition);
            var bottom = Math.Min(YPosition + Height, pageObject.YPosition + pageObject.Height);
            var left = Math.Max(XPosition, pageObject.XPosition);
            var right = Math.Min(XPosition + Width, pageObject.XPosition + pageObject.Width);
            var deltaY = bottom - top;
            var deltaX = right - left;
            var intersectionArea = deltaY * deltaX;
            return deltaY >= 0 && deltaX >= 0 && (intersectionArea / areaObject >= percentage || intersectionArea / area >= percentage);
        }

        public virtual List<ICLPPageObject> Cut(Stroke cuttingStroke)
        {
            return new List<ICLPPageObject>();
        }

        //aspectRatio is Width/Height
        public virtual void EnforceAspectRatio(double aspectRatio)
        {
            Width = Height * aspectRatio;

            if(Width + XPosition > ParentPage.PageWidth)
            {
                Width = ParentPage.PageWidth - XPosition;
                Height = Width / aspectRatio;
            }
        }

        #endregion

        #region Utility Methods

        public static void ApplyDistinctPosition(ICLPPageObject placedPageObject)
        {
            bool isAtHorizontalEdge = false;
            bool isAtVerticalEdge = false;

            foreach(ICLPPageObject pageObject in placedPageObject.ParentPage.PageObjects)
            {
                if (pageObject.GetType() == placedPageObject.GetType() && pageObject.UniqueID != placedPageObject.UniqueID)
                {
                    double xDelta = Math.Abs(pageObject.XPosition - placedPageObject.XPosition);
                    double yDelta = Math.Abs(pageObject.YPosition - placedPageObject.YPosition);

                    if (xDelta < 20 && yDelta <20)
                    {
                        if(xDelta < 20)
                        {
                            if (placedPageObject.XPosition + 21 + placedPageObject.Width < placedPageObject.ParentPage.PageWidth)
                            {
                                placedPageObject.XPosition += 21;
                            }
                            else
                            {
                                placedPageObject.XPosition = placedPageObject.ParentPage.PageWidth - placedPageObject.Width;
                                isAtHorizontalEdge = true;
                            }
                        }

                        if(yDelta < 20)
                        {
                            if(placedPageObject.YPosition + 21 + placedPageObject.Height < placedPageObject.ParentPage.PageHeight)
                            {
                                placedPageObject.YPosition += 21;
                            }
                            else
                            {
                                placedPageObject.YPosition = placedPageObject.ParentPage.PageHeight - placedPageObject.Height;
                                isAtVerticalEdge = true;
                            }
                        }

                        if (!isAtHorizontalEdge && !isAtVerticalEdge)
                        {
                            ApplyDistinctPosition(placedPageObject);
                        }
                    }
                }
            }
        }

        #endregion //Utility Methods
    }
}
