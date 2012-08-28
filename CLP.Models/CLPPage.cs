using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;

namespace CLP.Models
{
    public enum CLPPageOrientation
    {
        Portrait,
        Landscape,
        Custom
    }

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
        public const double LANDSCAPE_HEIGHT = 816;
        public const double LANDSCAPE_WIDTH = 1056;
        public const double PORTRAIT_HEIGHT = 1056;
        public const double PORTRAIT_WIDTH = 816;

        #endregion

        #region Constructor & destructor

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPPage()
        {
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            ByteStrokes = new ObservableCollection<byte[]>();
            PageObjects = new ObservableCollection<ICLPPageObject>();
            PageHistory = new CLPHistory();
            PageIndex = -1;
            PageTopics = new ObservableCollection<string>();
            NumberOfSubmissions = 0;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPPage(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public double PageHeight
        {
            get { return GetValue<double>(PageHeightProperty); }
            set { SetValue(PageHeightProperty, value); }
        }

        /// <summary>
        /// Register the PageHeight property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageHeightProperty = RegisterProperty("PageHeight", typeof(double), LANDSCAPE_HEIGHT);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public double PageWidth
        {
            get { return GetValue<double>(PageWidthProperty); }
            set { SetValue(PageWidthProperty, value); }
        }

        /// <summary>
        /// Register the PageWidth property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageWidthProperty = RegisterProperty("PageWidth", typeof(double), LANDSCAPE_WIDTH);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public int NumberOfSubmissions
        {
            get { return GetValue<int>(NumberOfSubmissionsProperty); }
            set { SetValue(NumberOfSubmissionsProperty, value); }
        }

        /// <summary>
        /// Register the NumberOfSubmissions property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NumberOfSubmissionsProperty = RegisterProperty("NumberOfSubmissions", typeof(int), 0);

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
        /// Gets a list of the serialized strokes for a page.
        /// </summary>
        public ObservableCollection<byte[]> ByteStrokes
        {
            get { return GetValue<ObservableCollection<byte[]>>(ByteStrokesProperty); }
            set { SetValue(ByteStrokesProperty, value); }
        }

        /// <summary>
        /// Register the ByteStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ByteStrokesProperty = RegisterProperty("ByteStrokes", typeof(ObservableCollection<byte[]>), () => new ObservableCollection<byte[]>());

        /// <summary>
        /// Gets a list of pageObjects on the page.
        /// </summary>
        public ObservableCollection<ICLPPageObject> PageObjects
        {
            get { return GetValue<ObservableCollection<ICLPPageObject>>(PageObjectsProperty); }
            set { SetValue(PageObjectsProperty, value); }
        }

        /// <summary>
        /// Register the PageObjects property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageObjectsProperty = RegisterProperty("PageObjects", typeof(ObservableCollection<ICLPPageObject>), () => new ObservableCollection<ICLPPageObject>());

        /// <summary>
        /// Gets the CLPPage history.
        /// </summary>
        public CLPHistory PageHistory
        {
            get { return GetValue<CLPHistory>(PageHistoryProperty); }
            set { SetValue(PageHistoryProperty, value); }
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
            set { SetValue(UniqueIDProperty, value); }
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
        public static readonly PropertyData PageTopicsProperty = RegisterProperty("PageTopics", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        /// <summary>
        /// Exact time and date the page was created.
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

        public static Stroke ByteToStroke(byte[] byteStroke)
        {
            var m_stream = new MemoryStream(byteStroke);
            StrokeCollection sc = new StrokeCollection(m_stream);

            return sc[0];
        }

        public static byte[] StrokeToByte(Stroke stroke)
        {
            StrokeCollection sc = new StrokeCollection();
            sc.Add(stroke);

            var m_stream = new MemoryStream();
            sc.Save(m_stream, true);
            byte[] byteStroke = m_stream.ToArray();

            return byteStroke;
        }

        /**
         * Helper method that converts a ObservableCollection of byte[] to a StrokeCollection
         */
        public static StrokeCollection BytesToStrokes(ObservableCollection<byte[]> byteStrokes)
        {
            StrokeCollection strokes = new StrokeCollection();
            foreach(byte[] s in byteStrokes)
            {
                strokes.Add(ByteToStroke(s));
            }

            return strokes;
        }

        /**
         * Helper method that converts a StrokeCollection to an ObservableCollection of byte[]
         */
        public static ObservableCollection<byte[]> StrokesToBytes(StrokeCollection strokes)
        {
            ObservableCollection<byte[]> byteStrokes = new ObservableCollection<byte[]>();
            foreach(Stroke stroke in strokes)
            {
                byteStrokes.Add(StrokeToByte(stroke));
            }

            return byteStrokes;
        }

        #endregion
    }

}

