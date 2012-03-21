using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Catel.Data;
using System.Runtime.Serialization;
using System.Windows.Ink;
using System.Windows.Media;

namespace Classroom_Learning_Partner.Model
{
    public interface ICLPPageObject
    {
        string PageID { get; set; }
        string ParentID { get; set; }
        DateTime CreationDate { get; set; }
        string UniqueID { get; set; }
        ObservableCollection<string> PageObjectStrokes { get; set; }
        bool CanAcceptStrokes { get; set; }
        Point Position { get; set; }
        double Height { get; set; }
        double Width { get; set; }

        string PageObjectType { get; set; }

        CLPPageObjectBase Duplicate();
    }

    /// <summary>
    /// CLPPageObjectBase Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    abstract public class CLPPageObjectBase : DataObjectBase<CLPPageObjectBase>, ICLPPageObject
    {
        #region Variables
        #endregion

        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPPageObjectBase()
        {
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            ParentID = "";
            PageObjectStrokes = new ObservableCollection<string>();
            CanAcceptStrokes = false;
            Height = 10;
            Width = 10;
            Position = new Point(10, 10);
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
        /// PageObject's personal collection of strokes.
        /// </summary>
        public ObservableCollection<string> PageObjectStrokes
        {
            get { return GetValue<ObservableCollection<string>>(PageObjectStrokesProperty); }
            protected set { SetValue(PageObjectStrokesProperty, value); }
        }

        /// <summary>
        /// Register the PageObjectStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageObjectStrokesProperty = RegisterProperty("PageObjectStrokes", typeof(ObservableCollection<string>), new ObservableCollection<string>());

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

        #endregion

        #region Methods

        public abstract string PageObjectType { get; }

        public abstract CLPPageObjectBase Duplicate();

        public virtual void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {

        }

        protected virtual void ProcessStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            StrokeCollection strokesToRemove = new StrokeCollection();
            StrokeCollection PageObjectActualStrokes = CLPPage.StringsToStrokes(PageObjectStrokes);
            foreach (Stroke objectStroke in PageObjectActualStrokes)
            {

                string objectStrokeUniqueID = objectStroke.GetPropertyData(CLPPage.StrokeIDKey).ToString();
                foreach (Stroke pageStroke in removedStrokes)
                {
                    string pageStrokeUniqueID = pageStroke.GetPropertyData(CLPPage.StrokeIDKey).ToString();
                    if (objectStrokeUniqueID == pageStrokeUniqueID)
                    {
                        strokesToRemove.Add(objectStroke);
                    }
                }
            }

            foreach (Stroke stroke in strokesToRemove)
            {
                string stringStroke = CLPPage.StrokeToString(stroke);
                PageObjectStrokes.Remove(stringStroke);
            }


            foreach (Stroke stroke in addedStrokes)
            {
                Stroke newStroke = stroke.Clone();
                Matrix transform = new Matrix();
                transform.Translate(-Position.X, -Position.Y);
                newStroke.Transform(transform, true);

                PageObjectStrokes.Add(CLPPage.StrokeToString(newStroke));
            }
        }

        #endregion
    }

    //[Serializable]
    //abstract public class CLPPageObjectBase
    //{
    //    protected CLPPageObjectBase()
    //    {
    //        MetaData.SetValue("CreationDate", DateTime.Now.ToString());
    //        MetaData.SetValue("UniqueID", Guid.NewGuid().ToString());
    //    }

    //    private MetaDataContainer _metaData = new MetaDataContainer();
    //    public MetaDataContainer MetaData
    //    {
    //        get
    //        {
    //            return _metaData;
    //        }
    //    }

    //    private ObservableCollection<string> _pageObjectStrokes = new ObservableCollection<string>();
    //    public ObservableCollection<string> PageObjectStrokes
    //    {
    //        get
    //        {
    //            return _pageObjectStrokes;
    //        }
    //        protected set
    //        {
    //            _pageObjectStrokes = value;
    //        }
    //    }

    //    private Point _position;
    //    public Point Position
    //    {
    //        get
    //        {
    //            return _position;
    //        }
    //        set
    //        {
    //            _position = value;
    //        }
    //    }

    //    private double _height;
    //    public double Height
    //    {
    //        get
    //        {
    //            return _height;
    //        }
    //        set
    //        {
    //            _height = value;
    //        }
    //    }

    //    private double _width;
    //    public double Width
    //    {
    //        get
    //        {
    //            return _width;
    //        }
    //        set
    //        {
    //            _width = value;
    //        }
    //    }

    //    //can this be controlled by position in list?
    //    private int _zIndex;
    //    public int ZIndex
    //    {
    //        get
    //        {
    //            return _zIndex;
    //        }
    //        set
    //        {
    //            _zIndex = value;
    //        }
    //    }

    //    public virtual CLPPageObjectBase Copy()
    //    {
    //        return null;
    //    }

    //    public string UniqueID
    //    {
    //        get
    //        {
    //            return MetaData.GetValue("UniqueID");
    //        }
    //    }
    //}
}
