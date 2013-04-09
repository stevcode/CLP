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
            this.Rows = rows;
            this.Columns = columns;
            this.HorizontalDivs = new ObservableCollection<double>();
            this.VerticalDivs = new ObservableCollection<double>();
            this.RowDivs = new ObservableCollection<int>();
            this.ColumnDivs = new ObservableCollection<int>();
            double Ratio = ((double)rows) / ((double)columns);

            HandwritingRegionParts = new CLPHandwritingRegion(CLPHandwritingAnalysisType.NUMBER, page);
            HandwritingRegionParts.IsInternalPageObject = true;
            HandwritingRegionParts.IsBackground = true;
            HandwritingRegionParts.Height = 100*Ratio + 20;

            XPosition = 10;
            YPosition = 10;
            
            Height = 750*Ratio;
            Width = 750;

            if(Height + this.YPosition > page.PageHeight)
            {
                Height = page.PageHeight - this.YPosition;
                Width = Height * ((double)this.Columns) / ((double)this.Rows);
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
        /// A list of divisions in the rows of the array - based on labels
        /// </summary>
        public ObservableCollection<int> RowDivs
        {
            get { return GetValue<ObservableCollection<int>>(RowDivsProperty); }
            set { SetValue(RowDivsProperty, value); }
        }

        /// <summary>
        /// Register the RowDivs property so it is known in the class.
        /// </summary>
        public static readonly PropertyData RowDivsProperty = RegisterProperty("RowDivs", typeof(ObservableCollection<int>), null);

        /// <summary>
        /// A list of divisions in the columns in the array - based on labels
        /// </summary>
        public ObservableCollection<int> ColumnDivs
        {
            get { return GetValue<ObservableCollection<int>>(ColumnDivsProperty); }
            set { SetValue(ColumnDivsProperty, value); }
        }

        /// <summary>
        /// Register the ColumnDivs property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ColumnDivsProperty = RegisterProperty("ColumnDivs", typeof(ObservableCollection<int>), null);


        /// <summary>
        /// Internally contained Handwriting Region.
        /// </summary>
        public CLPHandwritingRegion HandwritingRegionParts
        {
            get { return GetValue<CLPHandwritingRegion>(HandwritingRegionPartsProperty); }
            set { SetValue(HandwritingRegionPartsProperty, value); }
        }

        public static readonly PropertyData HandwritingRegionPartsProperty = RegisterProperty("HandwritingRegionParts", typeof(CLPHandwritingRegion), null);

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
        public CLPPage ParentPage
        {
            get { return GetValue<CLPPage>(ParentPageProperty); }
            set
            {
                SetValue(ParentPageProperty, value);
                HandwritingRegionParts.ParentPage = value;
                HandwritingRegionParts.ParentPageID = value.UniqueID;
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
                HandwritingRegionParts.Width = Width + 20;
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
            if(newArray.HandwritingRegionParts != null)
            {
                newArray.HandwritingRegionParts.ParentPage = ParentPage;
            }
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
                HandwritingRegionParts.PageObjectStrokeParentIDs.Clear();

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
                    HandwritingRegionParts.PageObjectStrokeParentIDs.Remove(strokeID);
                }
                catch(System.Exception)
                {
                    Console.WriteLine("StrokeID not found in PageObjectStrokeParentIDs. StrokeID: " + strokeID);
                }
            }

            Rect rectParts = new Rect(XPosition, YPosition,
                HandwritingRegionParts.Width, HandwritingRegionParts.Height);

            StrokeCollection handwritingRegionStrokesAdd = new StrokeCollection();
            foreach(Stroke stroke in addedStrokes)
            {
                if(stroke.HitTest(rectParts, 3))
                {
                    //TODO: Steve - this doesn't do anything because HandwritingRegionParts hasn't accepted strokes yet, move down.
                    if(PartsAuthorGenerated)
                    {
                        ClearHandWritingPartsStrokes();
                    }
                    else
                    {
                        handwritingRegionStrokesAdd.Add(stroke);
                        PartsAutoGenerated = false;
                    }
                }
                PageObjectStrokeParentIDs.Add(stroke.GetStrokeUniqueID());
            }

            HandwritingRegionParts.AcceptStrokes(handwritingRegionStrokesAdd, new StrokeCollection());
            UpdatePartsFromHandwritingRegion();
        }

        public void ResetParts()
        {
            Parts = 0;
            PartsAutoGenerated = false;
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

        public void UpdatePartsFromHandwritingRegion()
        {
            HandwritingRegionParts.DoInterpretation();
            int num;
            bool success = int.TryParse(HandwritingRegionParts.StoredAnswer, out num);
            if(success)
            {
                Parts = num;
            }
            // Set back to null otherwise you may accidentally keep reading the old value
            HandwritingRegionParts.StoredAnswer = null;
            Console.WriteLine("After interpret Parts: " + Parts);
            PartsInterpreted = (HandwritingRegionParts.GetStrokesOverPageObject().Count > 0) ? true : false;
        }

        public void ClearHandWritingPartsStrokes()
        {
            //Console.WriteLine("hw strokes : " + HandwritingRegionParts.GetStrokesOverPageObject().Count);
            foreach(Stroke stroke in HandwritingRegionParts.GetStrokesOverPageObject())
            {
                ParentPage.InkStrokes.Remove(stroke);
            }
            PartsInterpreted = false;

            //TODO: Steve - call RefreshStrokeParentIDs() here?
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
                    if(!PageObjectObjectParentIDs.Contains(pageObject.UniqueID) && !pageObject.GetType().Equals(typeof(CLPStamp)))
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
                    ClearHandWritingPartsStrokes();
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

            ObservableCollection<ICLPPageObject> pageObjectsOver = new ObservableCollection<ICLPPageObject>(pageObjects);
            return pageObjectsOver;
        }

        #endregion //Methods
    }
}