using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    abstract public class CLPPageObjectBase
    {
        protected CLPPageObjectBase()
        {
            _metaData.Add("CreationDate", new CLPAttribute("CreationDate", DateTime.Now.ToString()));
            _metaData.Add("UniqueID", new CLPAttribute("UniqueID", Guid.NewGuid().ToString()));
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

        private ObservableCollection<string> _pageObjectStrokes = new ObservableCollection<string>();
        [DataMember]
        public ObservableCollection<string> PageObjectStrokes
        {
            get
            {
                return _pageObjectStrokes;
            }
            protected set
            {
                _pageObjectStrokes = value;
            }
        }

        private Point _position;
        [DataMember]
        public Point Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        private double _height;
        [DataMember]
        public double Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }

        private double _width;
        [DataMember]
        public double Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        //can this be controlled by position in list?
        private int _zIndex;
        [DataMember]
        public int ZIndex
        {
            get
            {
                return _zIndex;
            }
            set
            {
                _zIndex = value;
            }
        }

        public virtual CLPPageObjectBase Copy()
        {
            return null;
        }


    }
}
