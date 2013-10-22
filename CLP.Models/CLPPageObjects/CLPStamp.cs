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
    [Serializable, KnownType(typeof(CLPImage))]
    public class CLPStamp : ModelBase, ICLPPageObject
    {
        #region Constants

        public static double HandleHeight { get { return 35; } }    

        public static double PartsHeight { get { return 70; } }

        #endregion //Constants

        #region Constructors

        public CLPStamp(ICLPPage page, string imageID, bool isCollectionStamp = false)
        {           
            CreationDate = DateTime.Now;
            StampCopy = new CLPStampCopy(page, imageID)
            {
                IsInternalPageObject = true,
                Width = isCollectionStamp ? 125 : 75,
                Height = isCollectionStamp ? 125 : 75
            };
            ParentPage = page;
            ParentPageID = page.UniqueID;
            UniqueID = Guid.NewGuid().ToString();

            CanAcceptStrokes = true;
            CanAcceptPageObjects = isCollectionStamp;
            IsCollectionStamp = isCollectionStamp;

            Height = StampCopy.Height + HandleHeight + PartsHeight;
            Width = StampCopy.Width;

            ACLPPageObjectBase.ApplyDistinctPosition(this);
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPStamp(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public string PageObjectType
        {
            get
            {
                return "CLPStamp";
            }
        }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Internally contained StampCopy
        /// </summary>
        public CLPStampCopy StampCopy
        {
            get { return GetValue<CLPStampCopy>(StampCopyProperty); }
            set { SetValue(StampCopyProperty, value); }
        }

        public static readonly PropertyData StampCopyProperty = RegisterProperty("StampCopy", typeof(CLPStampCopy));

        /// <summary>
        /// Sets Stamp to CollectionStamp.
        /// </summary>
        public bool IsCollectionStamp
        {
            get { return GetValue<bool>(IsCollectionStampProperty); }
            set { SetValue(IsCollectionStampProperty, value); }
        }

        public static readonly PropertyData IsCollectionStampProperty = RegisterProperty("IsCollectionStamp", typeof(bool), false);

        /// <summary>
        /// Whether or not the Parts were set in Authoring mode (and thus unchangable).
        /// </summary>
        public bool PartsAuthorGenerated
        {
            get { return GetValue<bool>(PartsAuthorGeneratedProperty); }
            set { SetValue(PartsAuthorGeneratedProperty, value); }
        }

        public static readonly PropertyData PartsAuthorGeneratedProperty = RegisterProperty("PartsAuthorGenerated", typeof(bool), false);

        /// <summary>
        /// The page the stamp resides on.
        /// </summary>
        public ICLPPage ParentPage
        {
            get { return GetValue<ICLPPage>(ParentPageProperty); }
            set 
            { 
                SetValue(ParentPageProperty, value);
                StampCopy.ParentPage = value;
                StampCopy.ParentPageID = value.UniqueID;
            }
        }

        [NonSerialized]
        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(ICLPPage), null, includeInSerialization: false);

        /// <summary>
        /// The uniqueID of the stamp's ParentPage.
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

        public static readonly PropertyData CanAcceptPageObjectsProperty = RegisterProperty("CanAcceptPageObjects", typeof(bool), false);

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
            set { SetValue(HeightProperty, value); }
        }

        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double), 100);

        /// <summary>
        /// Width of pageObject.
        /// </summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value); }
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

            return newStamp;
        }

        public virtual void OnAdded()
        {
        }

        public void OnRemoved()
        {
            if(StampCopy.IsStamped)
            {
                return;
            }

            foreach(var stroke in GetStrokesOverPageObject())
            {
                ParentPage.InkStrokes.Remove(stroke);
            }

            if(CanAcceptPageObjects)
            {
                foreach(var po in GetPageObjectsOverPageObject())
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
            StampCopy.Height = Height - HandleHeight - PartsHeight;
            StampCopy.Width = Width;
        }

        public void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
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

            var containerBoundingBox = new Rect(XPosition, YPosition + HandleHeight, StampCopy.Width, StampCopy.Height);      
            foreach(var stroke in addedStrokes.Where(stroke => stroke.HitTest(containerBoundingBox, 50)).Where(stroke => !PageObjectStrokeParentIDs.Contains(stroke.GetStrokeUniqueID()))) 
            {
                PageObjectStrokeParentIDs.Add(stroke.GetStrokeUniqueID());
            }
        }

        public StrokeCollection GetStrokesOverPageObject()
        {
            var strokes = from strokeID in PageObjectStrokeParentIDs
                          from stroke in ParentPage.InkStrokes
                          where stroke.GetStrokeUniqueID() == strokeID
                          select stroke;

            var inkStrokes = new StrokeCollection(strokes.Distinct());
            return inkStrokes;
        }

        public void RefreshStrokeParentIDs()
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

        public void AcceptObjects(IEnumerable<ICLPPageObject> addedPageObjects, IEnumerable<ICLPPageObject> removedPageObjects)
        {
            if(!CanAcceptPageObjects || !IsCollectionStamp)
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

        public ObservableCollection<ICLPPageObject> GetPageObjectsOverPageObject()
        {
            var pageObjects = from pageObjectID in PageObjectObjectParentIDs
                              from pageObject in ParentPage.PageObjects
                              where pageObject.UniqueID == pageObjectID
                              select pageObject;

            var pageObjectsOver = new ObservableCollection<ICLPPageObject>(pageObjects.Distinct());
            return pageObjectsOver;
        }

        public void RefreshPageObjectIDs()
        {
            if(!CanAcceptPageObjects || !IsCollectionStamp)
            {
                return;
            }

            PageObjectObjectParentIDs.Clear();

            var pageObjectsOverObject = from pageObject in ParentPage.PageObjects
                                        where PageObjectIsOver(pageObject, .90)
                                        select pageObject;

            AcceptObjects(pageObjectsOverObject, new List<ICLPPageObject>());
        }

        public bool PageObjectIsOver(ICLPPageObject pageObject, double percentage)
        {
            var areaObject = pageObject.Height * pageObject.Width;
            var top = Math.Max(YPosition + HandleHeight, pageObject.YPosition);
            var bottom = Math.Min(YPosition + Height - PartsHeight, pageObject.YPosition + pageObject.Height);
            var left = Math.Max(XPosition, pageObject.XPosition);
            var right = Math.Min(XPosition + Width, pageObject.XPosition + pageObject.Width);
            var deltaY = bottom - top;
            var deltaX = right - left;
            var intersectionArea = deltaY * deltaX;
            return deltaY >= 0 && deltaX >= 0 && intersectionArea / areaObject >= percentage;
        }

        public void EnforceAspectRatio(double aspectRatio) { }

        public void RefreshParts()
        {
            Parts = 0;
            foreach(var pageObject in GetPageObjectsOverPageObject())
            {
                Parts += pageObject.Parts;
            }
        }

        #endregion //Methods  
    }
}
