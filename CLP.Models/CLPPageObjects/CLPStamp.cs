using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows;
using Catel.Data;
using System.Windows.Ink;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace CLP.Models
{
    [Serializable]
    public class CLPStamp : DataObjectBase<CLPStamp>, ICLPPageObject
    {
        public static double HANDLE_HEIGHT
        {
            get
            {
                return 35;
            }
        }

        public static double PARTS_SIDE
        {
            get
            {
                return 50;
            }
        }

        #region Constructors

        public CLPStamp(ICLPPageObject internalPageObject, CLPPage page)
            : base()
        {
            ParentPageID = page.UniqueID;
            StrokePathContainer = new CLPStrokePathContainer(internalPageObject, page);
            HandwritingRegionParts = new CLPHandwritingRegion(CLPHandwritingAnalysisType.NUMBER, page);

            Position = new Point(100, 100);

            Height = StrokePathContainer.Height + HANDLE_HEIGHT;
            Width = StrokePathContainer.Width;

            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            ParentID = "";
            PageObjectByteStrokes = new ObservableCollection<List<byte>>();
            CanAcceptStrokes = true;
            PageObjectObjects = new ObservableCollection<ICLPPageObject>();
            CanAcceptPageObjects = true;
            Parts = -1;
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
        public CLPStrokePathContainer StrokePathContainer
        {
            get { return GetValue<CLPStrokePathContainer>(StrokePathContainerProperty); }
            set { SetValue(StrokePathContainerProperty, value); }
        }

        /// <summary>
        /// Register the InternalPageObject property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StrokePathContainerProperty = RegisterProperty("StrokePathContainer", typeof(CLPStrokePathContainer), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPHandwritingRegion HandwritingRegionParts
        {
            get { return GetValue<CLPHandwritingRegion>(HandwritingRegionPartsProperty); }
            set { SetValue(HandwritingRegionPartsProperty, value); }
        }

        /// <summary>
        /// Register the HandwritingRegionParts property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HandwritingRegionPartsProperty = RegisterProperty("HandwritingRegionParts", typeof(CLPHandwritingRegion), null);

        #endregion //Properties

        #region Methods

        public string PageObjectType
        {
            get { return "CLPStamp"; }
        }

        public ICLPPageObject Duplicate()
        {
            CLPStamp newStamp = this.Clone() as CLPStamp;
            newStamp.UniqueID = Guid.NewGuid().ToString();

            return newStamp;
        }

        public void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            this.ProcessStrokes(addedStrokes, removedStrokes);
        }

        protected void ProcessStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
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
                transform.Translate(-XPosition, -YPosition - HANDLE_HEIGHT);
                newStroke.Transform(transform, true);

                PageObjectByteStrokes.Add(CLPPage.StrokeToByte(newStroke));
            }
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

        // Returns a boolean stating if the @percentage of the @pageObject is contained within the item.
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

        #endregion //Methods

        /// <summary>
        /// Gets or sets the property value.
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

        public static readonly PropertyData ParentPageIDProperty = RegisterProperty("ParentPageID", typeof(string), "");

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string ParentID
        {
            get { return GetValue<string>(ParentIDProperty); }
            set { SetValue(ParentIDProperty, value); }
        }

        /// <summary>
        /// Register the ParentID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ParentIDProperty = RegisterProperty("ParentID", typeof(string), "");

        /// <summary>
        /// Creation date of pageObject.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        /// <summary>
        /// Register the CreationDate property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime), null);

        /// <summary>
        /// UniqueID of pageObject.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            set { SetValue(UniqueIDProperty, value); }
        }

        /// <summary>
        /// Register the UniqueID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<List<byte>> PageObjectByteStrokes
        {
            get { return GetValue<ObservableCollection<List<byte>>>(PageObjectByteStrokesProperty); }
            set { SetValue(PageObjectByteStrokesProperty, value); }
        }

        /// <summary>
        /// Register the PageObjectByteStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageObjectByteStrokesProperty = RegisterProperty("PageObjectByteStrokes", typeof(ObservableCollection<List<byte>>), () => new ObservableCollection<List<byte>>());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool CanAcceptStrokes
        {
            get { return GetValue<bool>(CanAcceptStrokesProperty); }
            set { SetValue(CanAcceptStrokesProperty, value); }
        }

        /// <summary>
        /// Register the CanAcceptStrokes property so it is known in the class.
        /// </summary>
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
            set { SetValue(CanAcceptPageObjectsProperty, value); }
        }

        /// <summary>
        /// Register the CanAcceptPageObjects property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CanAcceptPageObjectsProperty = RegisterProperty("CanAcceptPageObjects", typeof(bool), true);

        /// <summary>
        /// Position of pageObject on page.
        /// </summary>
        public Point Position
        {
            get { return GetValue<Point>(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        /// <summary>
        /// Register the Position property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PositionProperty = RegisterProperty("Position", typeof(Point), new Point(10, 10));

        /// <summary>
        /// xPosition of pageObject on page, used for serialization.
        /// </summary>
        public double XPosition
        {
            get { return GetValue<double>(XPositionProperty); }
            set { SetValue(XPositionProperty, value); }
        }

        /// <summary>
        /// Register the XPosition property so it is known in the class.
        /// </summary>
        public static readonly PropertyData XPositionProperty = RegisterProperty("XPosition", typeof(double), 10.0);

        /// <summary>
        /// YPosition of pageObject on page, used for serialization.
        /// </summary>
        public double YPosition
        {
            get { return GetValue<double>(YPositionProperty); }
            set { SetValue(YPositionProperty, value); }
        }

        /// <summary>
        /// Register the YPosition property so it is known in the class.
        /// </summary>
        public static readonly PropertyData YPositionProperty = RegisterProperty("YPosition", typeof(double), 10.0);

        /// <summary>
        /// Height of pageObject.
        /// </summary>
        public double Height
        {
            get { return GetValue<double>(HeightProperty); }
            set { SetValue(HeightProperty, value);
            StrokePathContainer.Height = Height - HANDLE_HEIGHT;
            if (StrokePathContainer.InternalPageObject != null)
            {
                StrokePathContainer.InternalPageObject.Height = StrokePathContainer.Height;
            }
            }
        }

        /// <summary>
        /// Register the Height property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HeightProperty = RegisterProperty("Height", typeof(double), 100);

        /// <summary>
        /// Width of pageObject.
        /// </summary>
        public double Width
        {
            get { return GetValue<double>(WidthProperty); }
            set { SetValue(WidthProperty, value);
            StrokePathContainer.Width = Width;
            if (StrokePathContainer.InternalPageObject != null)
            {
                StrokePathContainer.InternalPageObject.Width = StrokePathContainer.Width;
            }
            }
        }

        /// <summary>
        /// Register the Width property so it is known in the class.
        /// </summary>
        public static readonly PropertyData WidthProperty = RegisterProperty("Width", typeof(double), 100);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsBackground
        {
            get { return GetValue<bool>(IsBackgroundProperty); }
            set { SetValue(IsBackgroundProperty, value); }
        }

        /// <summary>
        /// Register the IsBackground property so it is known in the class.
        /// </summary>
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
    }
}
