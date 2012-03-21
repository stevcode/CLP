using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;
using System.Windows.Ink;
using System.Windows;
using Classroom_Learning_Partner.Model.CLPPageObjects;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// CLPPage Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    [AllowNonSerializableMembers]
    public class CLPPage : DataObjectBase<CLPPage>
    {
        #region Variables

        public static Guid StrokeIDKey = new Guid("00000000-0000-0000-0000-000000000001");
        public static Guid Immutable = new Guid("00000000-0000-0000-0000-000000000002");
        public static Guid ParentPageID = new Guid("00000000-0000-0000-0000-000000000003");

        #endregion

        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPPage()
        {
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            InkStrokes = new StrokeCollection();
            Strokes = new ObservableCollection<string>();
            PageObjects = new ObservableCollection<ICLPPageObject>();
            PageHistory = new CLPHistory();
            PageIndex = -1;
            PageTopics = new ObservableCollection<string>();
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPPage(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        #endregion

        protected override void OnDeserialized()
        {
            InkStrokes = StringsToStrokes(Strokes);
            base.OnDeserialized();
        }

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string ParentNotebookID
        {
            get { return GetValue<string>(ParentNotebookIDProperty); }
            set { SetValue(ParentNotebookIDProperty, value); }
        }

        /// <summary>
        /// Register the ParentNotebookID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ParentNotebookIDProperty = RegisterProperty("ParentNotebookID", typeof(string), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public StrokeCollection InkStrokes
        {
            get { return GetValue<StrokeCollection>(InkStrokesProperty); }
            private set { SetValue(InkStrokesProperty, value); }
        }

        /// <summary>
        /// Register the InkStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InkStrokesProperty = RegisterProperty("InkStrokes", typeof(StrokeCollection), new StrokeCollection()); //, true, false

        /// <summary>
        /// Gets a list of stringified strokes on the page.
        /// </summary>
        public ObservableCollection<string> Strokes
        {
            get { return GetValue<ObservableCollection<string>>(StrokesProperty); }
            private set { SetValue(StrokesProperty, value); }
        }

        /// <summary>
        /// Register the Strokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StrokesProperty = RegisterProperty("Strokes", typeof(ObservableCollection<string>), new ObservableCollection<string>());

        /// <summary>
        /// Gets a list of pageObjects on the page.
        /// </summary>
        public ObservableCollection<ICLPPageObject> PageObjects
        {
            get { return GetValue<ObservableCollection<ICLPPageObject>>(PageObjectsProperty); }
            private set { SetValue(PageObjectsProperty, value); }
        }

        /// <summary>
        /// Register the PageObjects property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageObjectsProperty = RegisterProperty("PageObjects", typeof(ObservableCollection<ICLPPageObject>), new ObservableCollection<ICLPPageObject>());

        /// <summary>
        /// Gets the CLPPage history.
        /// </summary>
        public CLPHistory PageHistory
        {
            get { return GetValue<CLPHistory>(PageHistoryProperty); }
            private set { SetValue(PageHistoryProperty, value); }
        }

        /// <summary>
        /// Register the PageHistory property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageHistoryProperty = RegisterProperty("PageHistory", typeof(CLPHistory), new CLPHistory());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IsSubmission
        {
            get { return GetValue<bool>(IsSubmissionProperty); }
            set { SetValue(IsSubmissionProperty, value); }
        }

        /// <summary>
        /// Register the IsSubmissions property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsSubmissionProperty = RegisterProperty("IsSubmission", typeof(bool), false);

        /// <summary>
        /// UniqueID of the page.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            private set { SetValue(UniqueIDProperty, value); }
        }

        /// <summary>
        /// Register the UniqueID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int PageIndex
        {
            get { return GetValue<int>(PageIndexProperty); }
            set { SetValue(PageIndexProperty, value); }
        }

        /// <summary>
        /// Register the PageIndex property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageIndexProperty = RegisterProperty("PageIndex", typeof(int), -1);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<string> PageTopics
        {
            get { return GetValue<ObservableCollection<string>>(PageTopicsProperty); }
            set { SetValue(PageTopicsProperty, value); }
        }

        /// <summary>
        /// Register the PageTopics property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageTopicsProperty = RegisterProperty("PageTopics", typeof(ObservableCollection<string>), new ObservableCollection<string>());

        /// <summary>
        /// Exact time and date the page was created.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            private set { SetValue(CreationDateProperty, value); }
        }

        /// <summary>
        /// Register the CreationDate property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime), null);

        /// <summary>
        /// Unique submission ID for submitted pages.
        /// </summary>
        public string SubmissionID
        {
            get { return GetValue<string>(SubmissionIDProperty); }
            set { SetValue(SubmissionIDProperty, value); }
        }

        /// <summary>
        /// Register the SubmissionID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SubmissionIDProperty = RegisterProperty("SubmissionID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// Name of the submitter on a submitted page.
        /// </summary>
        public string SubmitterName
        {
            get { return GetValue<string>(SubmitterNameProperty); }
            set { SetValue(SubmitterNameProperty, value); }
        }

        /// <summary>
        /// Register the SubmitterName property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SubmitterNameProperty = RegisterProperty("SubmitterName", typeof(string), null);

        /// <summary>
        /// Time the page was submitted.
        /// </summary>
        public DateTime SubmissionTime
        {
            get { return GetValue<DateTime>(SubmissionTimeProperty); }
            set { SetValue(SubmissionTimeProperty, value); }
        }

        /// <summary>
        /// Register the SubmissionTime property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SubmissionTimeProperty = RegisterProperty("SubmissionTime", typeof(DateTime), null);

        #endregion

        #region Methods

        public static Stroke StringToStroke(string stroke)
        {
            StrokeCollectionConverter converter = new StrokeCollectionConverter();
            StrokeCollection sc = new StrokeCollection();
            sc = (StrokeCollection)converter.ConvertFromString(stroke);
            return sc[0];
        }

        public static string StrokeToString(Stroke stroke)
        {
            StrokeCollection sc = new StrokeCollection();
            sc.Add(stroke);
            StrokeCollectionConverter converter = new StrokeCollectionConverter();
            string stringStroke = (string)converter.ConvertToString(sc);
            return stringStroke;
        }

        /**
         * Helper method that converts a ObservableCollection of strings to a StrokeCollection
         */
        public static StrokeCollection StringsToStrokes(ObservableCollection<string> strings)
        {
            StrokeCollection strokes = new StrokeCollection();
            foreach (string s in strings)
            {
                strokes.Add(StringToStroke(s));
            }
            return strokes;
        }

        /**
         * Helper method that converts a StrokeCollection to an ObservableCollection of strings
         */
        public static ObservableCollection<string> StrokesToStrings(StrokeCollection strokes)
        {
            ObservableCollection<string> strings = new ObservableCollection<string>();
            foreach (Stroke stroke in strokes)
            {
                strings.Add(StrokeToString(stroke));
            }
            return strings;
        }

        #endregion
    }

    ///// <summary>
    ///// 
    ///// </summary>
    //[Serializable]
    //public class CLPPage
    //{
    //    #region StrokeKeys for Stroke MetaData

    //    public static Guid StrokeIDKey = new Guid("00000000-0000-0000-0000-000000000001");
    //    public static Guid Mutable = new Guid("00000000-0000-0000-0000-000000000002");

    //    #endregion //StrokeKeys

    //    #region Constructors

    //    public CLPPage()
    //    {
    //        MetaData.SetValue("CreationDate", DateTime.Now.ToString());
    //        MetaData.SetValue("UniqueID", Guid.NewGuid().ToString());
    //        IsSubmission = false;
    //    }

    //    #endregion //Constructors

    //    #region Properties

    //    private ObservableCollection<string> _strokes = new ObservableCollection<string>();
    //    public ObservableCollection<string> Strokes
    //    {
    //        get
    //        {
    //            return _strokes;
    //        }
    //    }

    //    private ObservableCollection<CLPPageObjectBase> _pageObjects = new ObservableCollection<CLPPageObjectBase>();
    //    public ObservableCollection<CLPPageObjectBase> PageObjects
    //    {
    //        get
    //        {
    //            return _pageObjects;
    //        }
    //    }

    //    private MetaDataContainer _metaData = new MetaDataContainer();
    //    public MetaDataContainer MetaData
    //    {
    //        get
    //        {
    //            return _metaData;
    //        }
    //    }

    //    private CLPHistory _pageHistory = new CLPHistory();
    //    public CLPHistory PageHistory
    //    {
    //        get
    //        {
    //            return _pageHistory;
    //        }
    //    }

    //    #endregion //Properties

    //    #region MetaData

    //    public string UniqueID
    //    {
    //        get
    //        {
    //            return MetaData.GetValue("UniqueID");
    //        }
    //        set
    //        {
    //            MetaData.SetValue("UniqueID", value);
    //        }
    //    }

    //    public string SubmissionID
    //    {
    //        get
    //        {
    //            return MetaData.GetValue("SubmissionID");
    //        }
    //        set
    //        {
    //            MetaData.SetValue("SubmissionID", value);
    //        }
    //    }

    //    public bool IsSubmission
    //    {
    //        get
    //        {
    //            if (MetaData.GetValue("IsSubmission") == "True")
    //            {
    //                return true;
    //            }
    //            else
    //            {
    //                return false;
    //            }
    //        }
    //        set
    //        {
    //            if (value)
    //            {
    //                MetaData.SetValue("IsSubmission", "True");
    //                if (MetaData.GetValue("SubmissionID") == "NULL_KEY")
    //                {
    //                    MetaData.SetValue("SubmissionID", Guid.NewGuid().ToString());
    //                } 
    //            }
    //            else
    //            {
    //                MetaData.SetValue("IsSubmission", "False");
    //            }
    //        }
    //    }

    //    public string SubmitterName
    //    {
    //        get
    //        {
    //            return MetaData.GetValue("SubmitterName");
    //        }
    //        set
    //        {
    //            MetaData.SetValue("SubmitterName", value);
    //        }
    //    }

    //    #endregion //MetaData
    //}
}
