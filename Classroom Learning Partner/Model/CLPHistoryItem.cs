using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classroom_Learning_Partner.Model
{
    public class CLPHistoryItem
    {

       
         public CLPHistoryItem(object obj, string itemType)
        {
             /*CLPHistoryItem Types:
              * UNDO
              * REDO
              * ADD
              * MOVE
              * ERASE
              * COPY
              */

            
            _CLPHistoryObjectReference = obj;
            _metaData.Add("CreationDate", new CLPAttribute("CreationDate", DateTime.Now.ToString()));
            _metaData.Add("UniqueID", new CLPAttribute("UniqueID", System.Guid.NewGuid().ToString()));
            _metaData.Add("ItemType", new CLPAttribute("ItemType", itemType));
        }
        private Dictionary<string, CLPAttribute> _metaData = new Dictionary<string, CLPAttribute>();
        public Dictionary<string, CLPAttribute> MetaData
        {
            get
            {
                return _metaData;
            }
        }
        private object _CLPHistoryObjectReference;
        public object CLPHistoryObjectReference
        {
            get
            {
                return _CLPHistoryObjectReference;
            }
            set
            {
                _CLPHistoryObjectReference = value;
            }
        }
       
        #region MetaData
        private String _itemType;
        public string ItemType
        {
            get
            {
                return MetaData["ItemType"].SelectedValue;
            }

        }

        #endregion //MetaData
        
       
    }
}
