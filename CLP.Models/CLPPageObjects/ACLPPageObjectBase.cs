using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
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
            ParentID = "";
            PageObjectByteStrokes = new ObservableCollection<List<byte>>();
            CanAcceptStrokes = false;
            Height = 10;
            Width = 10;
            XPosition = 10;
            YPosition = 10;
            IsBackground = false;
            PageObjectObjects = new ObservableCollection<ICLPPageObject>();
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

        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(CLPPage), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string ParentPageID
        {
            get 
            {                 
                string tempValue = GetValue<string>(ParentPageIDProperty);
                if(tempValue != "")
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

        /// <summary>
        /// Register the ParentPageID property so it is known in the class.
        /// </summary>
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
        /// Serialized inkStrokes the pageObject has accepted.
        /// </summary>
        public ObservableCollection<List<byte>> PageObjectByteStrokes
        {
            get { return GetValue<ObservableCollection<List<byte>>>(PageObjectByteStrokesProperty); }
            set { SetValue(PageObjectByteStrokesProperty, value); }
        }

        public static readonly PropertyData PageObjectByteStrokesProperty = RegisterProperty("PageObjectByteStrokes", typeof(ObservableCollection<List<byte>>), () => new ObservableCollection<List<byte>>());

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
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<ICLPPageObject> PageObjectObjects
        {
            get { return GetValue<ObservableCollection<ICLPPageObject>>(PageObjectObjectsProperty); }
            set { SetValue(PageObjectObjectsProperty, value); }
        }

        /// <summary>
        /// Register the PageObjectObjects property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageObjectObjectsProperty = RegisterProperty("PageObjectObjects", typeof(ObservableCollection<ICLPPageObject>), () => new ObservableCollection<ICLPPageObject>());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool CanAcceptPageObjects
        {
            get { return GetValue<bool>(CanAcceptPageObjectsProperty); }
            set { SetValue(CanAcceptPageObjectsProperty, false); }
        }

        /// <summary>
        /// Register the CanAcceptPageObjects property so it is known in the class.
        /// </summary>
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
        /// True if placed while in Authoring Mode.
        /// 
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

        /// <summary>
        /// Register the Parts property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PartsProperty = RegisterProperty("Parts", typeof(int), -1);

        #endregion

        #region Methods

        public abstract string PageObjectType { get; }

        public abstract ICLPPageObject Duplicate();

        public virtual void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
        }

        protected virtual void ProcessStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            StrokeCollection strokesToRemove = new StrokeCollection();
            StrokeCollection PageObjectActualStrokes = CLPPage.BytesToStrokes(PageObjectByteStrokes);
            foreach(Stroke objectStroke in PageObjectActualStrokes)
            {
                string objectStrokeUniqueID = objectStroke.GetPropertyData(CLPPage.StrokeIDKey).ToString();
                foreach(Stroke pageStroke in removedStrokes)
                {
                    string pageStrokeUniqueID = pageStroke.GetPropertyData(CLPPage.StrokeIDKey).ToString();
                    if(objectStrokeUniqueID == pageStrokeUniqueID)
                    {
                        strokesToRemove.Add(objectStroke);
                    }
                }
            }

            foreach(Stroke stroke in strokesToRemove)
            {
                List<byte> b = CLPPage.StrokeToByte(stroke);

                /* Converting equal strokes to List<byte> arrays create List<byte> arrays with the same sequence of elements.
                 * The List<byte> arrays, however, are difference referenced objects, so the ByteStrokes.Remove will not work.
                 * This predicate searches for the first sequence match, instead of the first identical object, then removes
                 * that List<byte> array, which references the exact same object. */
                Func<List<byte>, bool> pred = (x) => { return x.SequenceEqual(b); };
                List<byte> eq = PageObjectByteStrokes.First<List<byte>>(pred);

                PageObjectByteStrokes.Remove(eq);
            }

            foreach(Stroke stroke in addedStrokes)
            {
                Stroke newStroke = stroke.Clone();
                Matrix transform = new Matrix();
                transform.Translate(-XPosition, -YPosition);
                newStroke.Transform(transform, true);

                PageObjectByteStrokes.Add(CLPPage.StrokeToByte(newStroke));
            }
        }

        public virtual bool HitTest(ICLPPageObject pageObject, double percentage)
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

        public void AcceptObject(ICLPPageObject pageObject)
        {
            //if (pageObject.Parts > 1) {
            PageObjectObjects.Add(pageObject);
            Parts += pageObject.Parts;
            //}
        }

        public void RemoveObject(ICLPPageObject pageObject)
        {
            PageObjectObjects.Remove(pageObject);
            Parts -= pageObject.Parts;
        }

        #endregion
    }
}
