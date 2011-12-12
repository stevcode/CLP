using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CLPPage
    {
        #region StrokeKeys for Stroke MetaData

        public static Guid StrokeIDKey = new Guid("00000000-0000-0000-0000-000000000001");
        public static Guid Mutable = new Guid("00000000-0000-0000-0000-000000000002");
        
        #endregion //StrokeKeys

        #region Constructors

        public CLPPage()
        {
            MetaData.SetValue("CreationDate", DateTime.Now.ToString());
            MetaData.SetValue("UniqueID", Guid.NewGuid().ToString());
            IsSubmission = false;
        }

        #endregion //Constructors

        #region Properties

        private ObservableCollection<string> _strokes = new ObservableCollection<string>();
        public ObservableCollection<string> Strokes
        {
            get
            {
                return _strokes;
            }
        }

        private ObservableCollection<CLPPageObjectBase> _pageObjects = new ObservableCollection<CLPPageObjectBase>();
        public ObservableCollection<CLPPageObjectBase> PageObjects
        {
            get
            {
                return _pageObjects;
            }
        }

        private MetaDataContainer _metaData = new MetaDataContainer();
        public MetaDataContainer MetaData
        {
            get
            {
                return _metaData;
            }
        }

        private CLPHistory _pageHistory = new CLPHistory();
        public CLPHistory PageHistory
        {
            get
            {
                return _pageHistory;
            }
        }

        #endregion //Properties

        #region MetaData

        public string UniqueID
        {
            get
            {
                return MetaData.GetValue("UniqueID");
            }
        }

        public bool IsSubmission
        {
            get
            {
                if (MetaData.GetValue("IsSubmission") == "True")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    MetaData.SetValue("IsSubmission", "True");
                }
                else
                {
                    MetaData.SetValue("IsSubmission", "False");
                }
            }
        }

        public string SubmitterName
        {
            get
            {
                return MetaData.GetValue("SubmitterName");
            }
            set
            {
                MetaData.SetValue("SubmitterName", value);
            }
        }

        #endregion //MetaData
    }
}
