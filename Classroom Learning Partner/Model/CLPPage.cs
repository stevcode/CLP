using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DataContract]
    public class CLPPage
    {
        #region Constructors

        public CLPPage()
        {
            _metaData.Add("CreationDate", new CLPAttribute("CreationDate", DateTime.Now.ToString()));
            _metaData.Add("UniqueID", new CLPAttribute("UniqueID", Guid.NewGuid().ToString()));
        }

        #endregion //Constructors

        #region Properties

        private ObservableCollection<string> _strokes = new ObservableCollection<string>();
        [DataMember]
        public ObservableCollection<string> Strokes
        {
            get
            {
                return _strokes;
            }
        }

        private ObservableCollection<CLPPageObjectBase> _pageObjects = new ObservableCollection<CLPPageObjectBase>();
        [DataMember]
        public ObservableCollection<CLPPageObjectBase> PageObjects
        {
            get
            {
                return _pageObjects;
            }
        }

        private Dictionary<string, CLPAttribute> _metaData = new Dictionary<string, CLPAttribute>();
        [DataMember]
        public Dictionary<string, CLPAttribute> MetaData
        {
            get
            {
                return _metaData;
            }
        }

        #endregion
    }
}
