using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;
using System.Windows.Media;
using System.Threading;

namespace CLP.Models
{
    [Serializable]
    public class CLPArray : DataObjectBase<CLPArray>, ICLPPageObject
    {

        #region Constructors

        public CLPArray(int rows, int columns, CLPPage page)
            : base()
        {
            IsGridOn = true;
            IsDivisionBehaviorOn = true;

            Rows = rows;
            Columns = columns;
            HorizontalDivs = new ObservableCollection<double>();
            VerticalDivs = new ObservableCollection<double>();
            HorizontalDivLabels = new ObservableCollection<Tuple<double,int>>();
            VerticalDivLabels = new ObservableCollection<Tuple<double, int>>();
            double Ratio = ((double)rows) / ((double)columns);


            XPosition = 10;
            YPosition = 10;
            
            Height = 700*Ratio;
            Width = 700;

            if(Height + YPosition > page.PageHeight - 150)
            {
                Height = page.PageHeight - YPosition - 150;
                Width = Height * ((double)Columns) / ((double)Rows);
            }

            double SquareSize = Width/columns;
            HorizontalGridDivs = new ObservableCollection<double>();
            for(int i = 1; i < rows; i++)
            {
                HorizontalGridDivs.Add(i * SquareSize);
            }
            VerticalGridDivs = new ObservableCollection<double>();
            for(int i = 1; i < columns; i++)
            {
                VerticalGridDivs.Add(i * SquareSize);
            }


            ParentPage = page;
            ParentPageID = page.UniqueID;
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            CanAcceptStrokes = true;
            CanAcceptPageObjects = false;

            CLPPageObjectBase.ApplyDistinctPosition(this);
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPArray(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public string PageObjectType
        {
            get { return "CLPArray"; }
        }

        /// <summary>
        /// Turns the grid on or off.
        /// </summary>
        public Boolean IsGridOn
        {
            get { return GetValue<Boolean>(IsGridOnProperty); }
            set { SetValue(IsGridOnProperty, value); }
        }

        public static readonly PropertyData IsGridOnProperty = RegisterProperty("IsGridOn", typeof(Boolean), true);

        /// <summary>
        /// Turns the division behavior on or off.
        /// </summary>
        public Boolean IsDivisionBehaviorOn
        {
            get { return GetValue<Boolean>(IsDivisionBehaviorOnProperty); }
            set { SetValue(IsDivisionBehaviorOnProperty, value); }
        }

        /// <summary>
        /// Register the isDivisionBehaviorOn property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsDivisionBehaviorOnProperty = RegisterProperty("IsDivisionBehaviorOn", typeof(Boolean), true);

        /// <summary>
        /// The number of rows in the array.
        /// </summary>
        public int Rows
        {
            get { return GetValue<int>(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        /// <summary>
        /// Register the Rows property so it is known in the class.
        /// </summary>
        public static readonly PropertyData RowsProperty = RegisterProperty("Rows", typeof(int), null);

        /// <summary>
        /// The number of columns in the array.
        /// </summary>
        public int Columns
        {
            get { return GetValue<int>(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// Register the Columns property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ColumnsProperty = RegisterProperty("Columns", typeof(int), null);

        /// <summary>
        /// Gets or sets the HorizontalGridDivs value.
        /// </summary>
        public ObservableCollection<double> HorizontalGridDivs
        {
            get { return GetValue<ObservableCollection<double>>(HorizontalGridDivsProperty); }
            set { SetValue(HorizontalGridDivsProperty, value); }
        }

        /// <summary>
        /// Register the HorizontalGridDivs property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HorizontalGridDivsProperty = RegisterProperty("HorizontalGridDivs", typeof(ObservableCollection<double>), null);

        /// <summary>
        /// Gets or sets the VerticalGridDivs value.
        /// </summary>
        public ObservableCollection<double> VerticalGridDivs
        {
            get { return GetValue<ObservableCollection<double>>(VerticalGridDivsProperty); }
            set { SetValue(VerticalGridDivsProperty, value); }
        }

        /// <summary>
        /// Register the VerticalGridDivs property so it is known in the class.
        /// </summary>
        public static readonly PropertyData VerticalGridDivsProperty = RegisterProperty("VerticalGridDivs", typeof(ObservableCollection<double>), null);

        /// <summary>
        /// Exact positions of where the horizontal divisions are placed on the array.
        /// </summary>
        public ObservableCollection<double> HorizontalDivs
        {
            get { return GetValue<ObservableCollection<double>>(HorizontalDivsProperty); }
            set { SetValue(HorizontalDivsProperty, value); }
        }

        /// <summary>
        /// Register the HorizontalDivs property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HorizontalDivsProperty = RegisterProperty("HorizontalDivs", typeof(ObservableCollection<double>), null);

        /// <summary>
        /// Exact position of where the vertical divisions are placed on the array.
        /// </summary>
        public ObservableCollection<double> VerticalDivs
        {
            get { return GetValue<ObservableCollection<double>>(VerticalDivsProperty); }
            set { SetValue(VerticalDivsProperty, value); }
        }

        /// <summary>
        /// Register the VerticalDivs property so it is known in the class.
        /// </summary>
        public static readonly PropertyData VerticalDivsProperty = RegisterProperty("VerticalDivs", typeof(ObservableCollection<double>), null);

        /// <summary>
        /// A list of labels for horizontal divisions - (position, value). -1 for unlabelled.
        /// </summary>
        public ObservableCollection<Tuple<double,int>> HorizontalDivLabels
        {
            get { return GetValue<ObservableCollection<Tuple<double,int>>>(HorizontalDivLabelsProperty); }
            set { SetValue(HorizontalDivLabelsProperty, value); }
        }

        /// <summary>
        /// Register the HorizontalDivLabels property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HorizontalDivLabelsProperty = RegisterProperty("HorizontalDivLabels", typeof(ObservableCollection<Tuple<double,int>>), null);

        /// <summary>
        /// A list of labels for vertical divisions - (position, value). -1 for unlabelled.
        /// </summary>
        public ObservableCollection<Tuple<double,int>> VerticalDivLabels
        {
            get { return GetValue<ObservableCollection<Tuple<double,int>>>(VerticalDivLabelsProperty); }
            set { SetValue(VerticalDivLabelsProperty, value); }
        }

        /// <summary>
        /// Register the VerticalDivLabels property so it is known in the class.
        /// </summary>
        public static readonly PropertyData VerticalDivLabelsProperty = RegisterProperty("VerticalDivLabels", typeof(ObservableCollection<Tuple<double,int>>), null);

        /// <summary>
        /// Register the ColumnDivs property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ColumnDivsProperty = RegisterProperty("ColumnDivs", typeof(ObservableCollection<int>), null);


 
 
        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public CLPPage ParentPage
        {
            get { return GetValue<CLPPage>(ParentPageProperty); }
            set
            {
                SetValue(ParentPageProperty, value);
            }
        }

        [NonSerialized]
        public static readonly PropertyData ParentPageProperty = RegisterProperty("ParentPage", typeof(CLPPage), null, includeInSerialization: false);

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

        public ICLPPageObject Duplicate()
        {
            CLPArray newArray = this.Clone() as CLPArray;
            newArray.UniqueID = Guid.NewGuid().ToString();
            newArray.ParentPage = ParentPage;
            return newArray;
        }

        public void OnRemoved()
        {

            foreach(ICLPPageObject po in GetPageObjectsOverPageObject())
            {
                po.OnRemoved();
                ParentPage.PageObjects.Remove(po);
            }
        }

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

        public void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            foreach(Stroke s in removedStrokes)
            {
                string strokeID = s.GetStrokeUniqueID();
                try
                {
                    PageObjectStrokeParentIDs.Remove(strokeID);
                }
                catch(System.Exception)
                {
                    Console.WriteLine("StrokeID not found in PageObjectStrokeParentIDs. StrokeID: " + strokeID);
                }
            }
        }

        public void ResetParts()
        {
            Parts = 0;
        }

        public StrokeCollection GetStrokesOverPageObject()
        {
            var strokes =
                from strokeID in PageObjectStrokeParentIDs
                from stroke in ParentPage.InkStrokes
                where stroke.GetStrokeUniqueID() == strokeID
                select stroke;

            StrokeCollection inkStrokes = new StrokeCollection(strokes);
            return inkStrokes;
        }


        public bool PageObjectIsOver(ICLPPageObject pageObject, double percentage)
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

        public void AcceptObjects(ObservableCollection<ICLPPageObject> addedPageObjects, ObservableCollection<ICLPPageObject> removedPageObjects)
        {
        }

        public ObservableCollection<ICLPPageObject> GetPageObjectsOverPageObject()
        {
            var pageObjects =
                from pageObjectID in PageObjectObjectParentIDs
                from pageObject in ParentPage.PageObjects
                where pageObject.UniqueID == pageObjectID
                select pageObject;

            ObservableCollection<ICLPPageObject> pageObjectsOver = new ObservableCollection<ICLPPageObject>(pageObjects);
            return pageObjectsOver;
        }

        #endregion //Methods
    }
}