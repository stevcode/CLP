using System;
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
    abstract public class CLPPageObjectBase : DataObjectBase<CLPPageObjectBase>, ICLPPageObject
    {
        #region Constructor

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPPageObjectBase(CLPPage page)
        {
            ParentPage = page;
            ParentPageID = page.UniqueID;
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            CanAcceptPageObjects = true;
            Parts = -1;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPPageObjectBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties

        /// <summary>
        /// The page the pageObject is on.
        /// </summary>
        public CLPPage ParentPage
        {
            get { return GetValue<CLPPage>(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        [NonSerialized]
        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(CLPPage), null, includeInSerialization:false);

        /// <summary>
        /// The UniqueID of the ParentPage
        /// </summary>
        public string ParentPageID
        {
            get
            {
                string tempValue = GetValue<string>(ParentPageIDProperty);
                if (tempValue != "")
                {
                    return tempValue;
                }
                else
                {
                    SetValue(ParentPageIDProperty, ParentPage.UniqueID);
                    return ParentPage.UniqueID;
                }
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
            set
            { SetValue(ParentIDProperty, value);}
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

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime), null);

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
            set { SetValue(CanAcceptPageObjectsProperty, false); }
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
        /// -1 for undefined.
        /// </summary>
        public int Parts
        {
            get {
                int parts = GetValue<int>(PartsProperty);
                if (parts < 1 && !ParentID.Equals(""))
                {
                    //Should only be one
                    foreach (ICLPPageObject po in ParentPage.PageObjects)
                    {
                        if (po.UniqueID.Equals(ParentID)) {
                            Parts = po.Parts;
                        }
                    }
                }
                return GetValue<int>(PartsProperty);
            }
            set { SetValue(PartsProperty, value); }
        }

        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof(int), -1);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsInternalPageObject
        {
            get { return GetValue<bool>(IsInternalPageObjectProperty); }
            set { SetValue(IsInternalPageObjectProperty, value); }
        }

        public static readonly PropertyData IsInternalPageObjectProperty = RegisterProperty("IsInternalPageObject", typeof(bool), false);

        #endregion

        #region Methods

        public abstract string PageObjectType { get; }

        public abstract ICLPPageObject Duplicate();

        public virtual void OnRemoved() { }

        public virtual void RefreshStrokeParentIDs()
        {
            if(CanAcceptStrokes)
            {
                PageObjectStrokeParentIDs.Clear();

                Rect rect = new Rect(XPosition, YPosition, Width, Height);
                var strokesOverObject =
                    from stroke in ParentPage.InkStrokes
                    where stroke.HitTest(rect, 3)
                    select stroke;

                AcceptStrokes(new StrokeCollection(strokesOverObject), new StrokeCollection());
            }
        }

        public virtual void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            if (CanAcceptStrokes)
            {
                foreach(Stroke s in removedStrokes)
                {
                    string strokeID = s.GetStrokeUniqueID();
                    try
                    {
                        PageObjectStrokeParentIDs.Remove(strokeID);
                    }
                    catch(System.Exception ex)
                    {
                        Console.WriteLine("StrokeID not found in PageObjectStrokeParentIDs. StrokeID: " + strokeID);
                    }
                }

                foreach(Stroke s in addedStrokes)
                {
                    PageObjectStrokeParentIDs.Add(s.GetStrokeUniqueID());
                }
            }
        }

        public virtual StrokeCollection GetStrokesOverPageObject()
        {
            var strokes =
                from strokeID in PageObjectStrokeParentIDs
                from stroke in ParentPage.InkStrokes
                where stroke.GetStrokeUniqueID() == strokeID
                select stroke;

            StrokeCollection inkStrokes = new StrokeCollection(strokes.ToList());
            return inkStrokes;
        }

        public virtual bool PageObjectIsOver(ICLPPageObject pageObject, double percentage)
        {
            double areaObject = pageObject.Height * pageObject.Width;
            double top = Math.Max(YPosition, pageObject.YPosition);
            double bottom = Math.Min(YPosition + Height, pageObject.YPosition + pageObject.Height);
            double left = Math.Max(XPosition, pageObject.XPosition);
            double right = Math.Min(XPosition + Width, pageObject.XPosition + pageObject.Width);
            double deltaY = bottom - top;
            double deltaX = right - left;
            double intersectionArea = deltaY * deltaX;
            return deltaY >= 0 && deltaX >= 0 && intersectionArea / areaObject >= percentage;
        }

        public virtual void AcceptObjects(List<string> addedPageObjectIDs, ObservableCollection<ICLPPageObject> removedPageObjects)
        {
            if (CanAcceptPageObjects)
            {
                foreach (ICLPPageObject pageObject in removedPageObjects)
                {
                    if (PageObjectObjectParentIDs.Contains(pageObject.UniqueID))
                    {
                        Parts = (Parts - pageObject.Parts > 0) ? Parts - pageObject.Parts : 0;
                        PageObjectObjectParentIDs.Remove(pageObject.UniqueID);
                    }
                }

                var pageObjectsAdd =
                    from pageObjectID in addedPageObjectIDs
                    from pageObject in ParentPage.PageObjects
                    where (pageObject.UniqueID).Equals(pageObjectID) && !pageObject.GetType().Equals(typeof(CLPStamp))
                    select pageObject;
                foreach (ICLPPageObject pageObject in pageObjectsAdd)
                {
                    if (!PageObjectObjectParentIDs.Contains(pageObject.UniqueID))
                    {
                        Parts += pageObject.Parts;
                        PageObjectObjectParentIDs.Add(pageObject.UniqueID);
                    }
                }
            }
        }
        
        public virtual ObservableCollection<ICLPPageObject> GetPageObjectsOverPageObject()
        {
            var pageObjects =
                from pageObjectID in PageObjectObjectParentIDs
                from pageObject in ParentPage.PageObjects
                where pageObject.UniqueID == pageObjectID
                select pageObject;

            ObservableCollection<ICLPPageObject> pageObjectsOver = new ObservableCollection<ICLPPageObject>(pageObjects.ToList());
            return pageObjectsOver;
        }

        #endregion

        #region Utility Methods

        public static void ApplyDistinctPosition(ICLPPageObject placedPageObject)
        {
            bool isAtHorizontalEdge = false;
            bool isAtVerticalEdge = false;

            foreach(ICLPPageObject pageObject in placedPageObject.ParentPage.PageObjects)
            {
                if (pageObject.GetType() == placedPageObject.GetType() && pageObject.GetType() != null && pageObject.UniqueID != placedPageObject.UniqueID)
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
