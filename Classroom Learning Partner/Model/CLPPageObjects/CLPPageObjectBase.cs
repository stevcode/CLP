using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Catel.Data;
using System.Runtime.Serialization;

namespace Classroom_Learning_Partner.Model
{
    public interface ICLPPageObject
    {
        DateTime CreationDate { get; set; }
        string UniqueID { get; set; }
        ObservableCollection<string> PageObjectStrokes { get; }
        Point Position { get; set; }
        double Height { get; set; }
        double Width { get; set; }

        string PageObjectType { get; }

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
            PageObjectStrokes = new ObservableCollection<string>();
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
        /// <summary>
        /// Validates the fields.
        /// </summary>
        protected override void ValidateFields()
        {
            // TODO: Implement any field validation of this object. Simply set any error by using the SetFieldError method
        }

        /// <summary>
        /// Validates the business rules.
        /// </summary>
        protected override void ValidateBusinessRules()
        {
            // TODO: Implement any business rules of this object. Simply set any error by using the SetBusinessRuleError method
        }

        public abstract string PageObjectType { get; }

        public abstract CLPPageObjectBase Duplicate();

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
