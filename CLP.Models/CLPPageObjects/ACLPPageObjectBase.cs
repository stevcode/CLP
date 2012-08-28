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
        public CLPPageObjectBase()
        {
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            ParentID = "";
            PageObjectByteStrokes = new ObservableCollection<byte[]>();
            CanAcceptStrokes = false;
            Height = 10;
            Width = 10;
            XPosition = 10;
            YPosition = 10;
            IsBackground = false;
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
        /// Gets or sets the property value.
        /// </summary>
        public string PageID
        {
            get { return GetValue<string>(PageIDProperty); }
            set { SetValue(PageIDProperty, value); }
        }

        /// <summary>
        /// Register the PageID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageIDProperty = RegisterProperty("PageID", typeof(string), "");

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
        public ObservableCollection<byte[]> PageObjectByteStrokes
        {
            get { return GetValue<ObservableCollection<byte[]>>(PageObjectByteStrokesProperty); }
            set { SetValue(PageObjectByteStrokesProperty, value); }
        }

        /// <summary>
        /// Register the PageObjectByteStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageObjectByteStrokesProperty = RegisterProperty("PageObjectByteStrokes", typeof(ObservableCollection<byte[]>), () => new ObservableCollection<byte[]>());

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
            set { SetValue(HeightProperty, value); }
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
            set { SetValue(WidthProperty, value); }
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
                byte[] b = CLPPage.StrokeToByte(stroke);

                /* Converting equal strokes to byte[] arrays create byte[] arrays with the same sequence of elements.
                 * The byte[] arrays, however, are difference referenced objects, so the ByteStrokes.Remove will not work.
                 * This predicate searches for the first sequence match, instead of the first identical object, then removes
                 * that byte[] array, which references the exact same object. */
                Func<byte[], bool> pred = (x) => { return x.SequenceEqual(b); };
                byte[] eq = PageObjectByteStrokes.First<byte[]>(pred);

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

        #endregion
    }
}
