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
            MetaData.SetValue("CreationDate", DateTime.Now.ToString());
            MetaData.SetValue("UniqueID", Guid.NewGuid().ToString());
            MetaData.SetValue("ItemType", itemType);
        }

         private MetaDataContainer _metaData = new MetaDataContainer();
         public MetaDataContainer MetaData
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

        public string ItemType
        {
            get
            {
                return MetaData.GetValue("ItemType");
            }

        }

        #endregion //MetaData
        
       
    }
}
