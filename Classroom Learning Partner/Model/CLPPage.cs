using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;
using ProtoBuf;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// CLPPage Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [ProtoContract]
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
            InkStrokes = new StrokeCollection();
            Strokes = new ObservableCollection<string>();
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

        protected override void OnDeserialized()
        {
            InkStrokes = StringsToStrokes(Strokes);
            base.OnDeserialized();
        }

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
        /// Gets or sets the property value.
        /// </summary>
        public StrokeCollection InkStrokes
        {
            get { return GetValue<StrokeCollection>(InkStrokesProperty); }
            set { SetValue(InkStrokesProperty, value); }
        }

        /// <summary>
        /// Register the InkStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData InkStrokesProperty = RegisterProperty("InkStrokes", typeof(StrokeCollection), () => new StrokeCollection()); //, true, false

        /// <summary>
        /// Gets a list of stringified strokes on the page.
        /// </summary>
        public ObservableCollection<string> Strokes
        {
            get { return GetValue<ObservableCollection<string>>(StrokesProperty); }
            set { SetValue(StrokesProperty, value); }
        }

        /// <summary>
        /// Register the Strokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StrokesProperty = RegisterProperty("Strokes", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

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
        /// Gets a list of pageObjects image data, if it exisits, on the page. Only used for compression during serialization 
        /// </summary>
        //[ProtoMember(20, AsReference= true, OverwriteList = true)]
        public List<String> PageObjectsSer
        {
            get { return GetValue<List<String>>(PageObjectsSerProperty); }
            set { SetValue(PageObjectsSerProperty, value); }
        }

        /// <summary>
        /// Register the PageObjectsSer property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PageObjectsSerProperty = RegisterProperty("PageObjectsSer", typeof(List<String>), () => new List<String>());


        /// <summary>
        /// Gets a list of pageObjects image data, if it exisits, on the page. Only used for compression during serialization 
        /// </summary>
        //public List<string> PageStrokesSer
        //{
        //    get { return GetValue<List<string>>(PageStrokesSerProperty); }
        //    set { SetValue(PageObjectsSerProperty, value); }
        //}

        ///// <summary>
        ///// Register the PageObjectsSer property so it is known in the class.
        ///// </summary>
        //public static readonly PropertyData PageStrokesSerProperty = RegisterProperty("PageStrokesSer", typeof(List<string>), new List<string>());





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
            foreach(string s in strings)
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
            foreach(Stroke stroke in strokes)
            {
                strings.Add(StrokeToString(stroke));
            }

            //This will reduce size of strokes by more than half. convert to this method soon! - steve
            //var m_stream = new MemoryStream();
            //strokes.Save(m_stream, true);
            //byte[] b_strokes = m_stream.ToArray();

            //string s_allstrings = "";
            //foreach (var strin in strings)
            //{
            //    s_allstrings += strin;
            //}

            //byte[] x = ASCIIEncoding.Unicode.GetBytes(s_allstrings);

            //Console.WriteLine("obscoll byte length: " + (x.Length).ToString());
            //Console.WriteLine("array byte length: " + (b_strokes.Length).ToString());

            return strings;
        }


        /**
        * Helper method that places images in the same list so that protobuf serializaiton can optimaize serialized string 
        */
        [ProtoBeforeSerialization]
        public void serializePageObjectsHelper()
        {
            Strokes = CLPPage.StrokesToStrings(InkStrokes);
            PageObjectsSer = new List<string>();
            //PageStrokesSer = new List<string>();
            foreach(ICLPPageObject obj in PageObjects)
            {
                if(obj is CLPPageObjects.CLPStamp)
                {
                    CLPPageObjects.CLPStamp stamp = (CLPPageObjects.CLPStamp)obj;
                    //Check to see if 
                    if(stamp.StrokePathContainer.InternalPageObject is CLPPageObjects.CLPImage)
                    {
                        CLPPageObjects.CLPImage im = (CLPPageObjects.CLPImage)stamp.StrokePathContainer.InternalPageObject;
                        PageObjectsSer.Add(Convert.ToBase64String(im.ByteSource));

                        im.ByteSource = null;
                    }
                    //copy strokes over
                    //However, may also be holding a shape .. 
                    else
                    {
                        if(obj.PageObjectStrokes.Count > 0)
                        {
                            //PageObjectsSer.Add(PageStrokesSer.Count.ToString());
                            PageObjectsSer.Add(obj.PageObjectStrokes.Count.ToString());
                            PageObjectsSer.AddRange(obj.PageObjectStrokes);
                            obj.PageObjectStrokes.Clear(); //= new ObservableCollection<string>();
                        }
                        else
                        {
                            //no strokes in stamp
                            PageObjectsSer.Add("");
                        }
                    }


                }


                else if(obj is CLPPageObjects.CLPImage)
                {
                    CLPPageObjects.CLPImage im = (CLPPageObjects.CLPImage)obj;
                    PageObjectsSer.Add(Convert.ToBase64String(im.ByteSource));
                    im.ByteSource = null;
                }
                else if(obj is CLPPageObjects.CLPStrokePathContainer)
                {
                    CLPPageObjects.CLPStrokePathContainer container = (CLPPageObjects.CLPStrokePathContainer)obj;
                    //Check to see if 
                    if(container.InternalPageObject is CLPPageObjects.CLPImage)
                    {
                        CLPPageObjects.CLPImage im = (CLPPageObjects.CLPImage)container.InternalPageObject;
                        PageObjectsSer.Add(Convert.ToBase64String(im.ByteSource));
                        im.ByteSource = null;
                    }
                    else
                    {
                        if(container.PageObjectStrokes.Count > 0)
                        {
                            //PageObjectsSer.Add(PageStrokesSer.Count.ToString());
                            PageObjectsSer.Add(container.PageObjectStrokes.Count.ToString());
                            PageObjectsSer.AddRange(container.PageObjectStrokes);
                            container.PageObjectStrokes.Clear();// = new ObservableCollection<string>();
                        }
                        else
                        {
                            PageObjectsSer.Add("");
                        }
                    }
                }
                else
                {
                    PageObjectsSer.Add("");

                }


            }



        }


        [ProtoAfterSerialization]
        public void afterSerialization()
        {
            deserializePageObjectsHelper();
        }

        [ProtoAfterDeserialization]
        public void afterDeserialization()
        {
            deserializePageObjectsHelper();
            InkStrokes = StringsToStrokes(Strokes);
        }

        public void deserializePageObjectsHelper()
        {
            int currentIndex = 0;
            for(int i = 0; i < PageObjects.Count; i++)
            {
                if(!PageObjectsSer[currentIndex].Equals(""))
                {
                    if(PageObjects[i] is CLPPageObjects.CLPStamp)
                    {
                        CLPPageObjects.CLPStamp stamp = ((CLPPageObjects.CLPStamp)PageObjects[i]);
                        if(stamp.StrokePathContainer.InternalPageObject is CLPPageObjects.CLPImage)
                        {
                            CLPPageObjects.CLPImage im = (CLPPageObjects.CLPImage)stamp.StrokePathContainer.InternalPageObject;
                            im.ByteSource = Convert.FromBase64String(PageObjectsSer[currentIndex]);
                            im.LoadImageFromByteSource(im.ByteSource);
                            currentIndex++;
                        }
                        else
                        {
                            //int startIndex = Convert.ToInt32(PageObjectsSer[i]);
                            int count = Convert.ToInt32(PageObjectsSer[currentIndex]);
                            if(count > 0)
                            {
                                stamp.PageObjectStrokes = new ObservableCollection<string>(PageObjectsSer.GetRange(currentIndex + 1, count));
                                CLPPage.StringsToStrokes(stamp.PageObjectStrokes);
                                currentIndex += count + 1;
                            }
                        }


                    }


                    else if(PageObjects[i] is CLPPageObjects.CLPStrokePathContainer)
                    {
                        CLPPageObjects.CLPStrokePathContainer container = (CLPPageObjects.CLPStrokePathContainer)PageObjects[i];
                        if(container.InternalPageObject is CLPPageObjects.CLPImage)
                        {
                            CLPPageObjects.CLPImage curImage = (CLPPageObjects.CLPImage)container.InternalPageObject;
                            curImage.ByteSource = Convert.FromBase64String(PageObjectsSer[currentIndex]);
                            curImage.LoadImageFromByteSource(curImage.ByteSource);
                            currentIndex++;
                        }
                        else
                        {
                            int count = Convert.ToInt32(PageObjectsSer[currentIndex]);
                            if(count > 0)
                            {
                                foreach(string s in (PageObjectsSer.GetRange(currentIndex + 1, count)))
                                {
                                    container.PageObjectStrokes.Add(s);
                                }
                                currentIndex += count + 1;

                            }
                        }
                    }
                    else //CLPImage
                    {
                        CLPPageObjects.CLPImage curImage = ((CLPPageObjects.CLPImage)PageObjects[i]);
                        curImage.ByteSource = Convert.FromBase64String(PageObjectsSer[currentIndex]);
                        curImage.LoadImageFromByteSource(curImage.ByteSource);
                        currentIndex++;
                    }
                }
                else
                {
                    currentIndex++;
                }

            }
            //PageObjectsSer.Clear();
            //PageStrokesSer.Clear();

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

