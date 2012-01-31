using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classroom_Learning_Partner.Model
{
    [Serializable]
    public class CLPHistoryItem
    {
         public CLPHistoryItem(string itemType)
        {
             /*CLPHistoryItem Types:
              * UNDO
              * REDO
              * ADD
              * MOVE
              * ERASE
              * COPY
              */

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
       
        #region MetaData

        public string ItemType
        {
            get
            {
                return MetaData.GetValue("ItemType");
            }

        }

        public string ObjectID
        {
            get
            {
                return MetaData.GetValue("ObjectID");
            }
            set
            {
                MetaData.SetValue("ObjectID", value);
            }
        }

        public string OldValue
        {
            get
            {
                return MetaData.GetValue("OldValue");
            }
            set
            {
                MetaData.SetValue("OldValue", value);
            }
        }

        public string NewValue
        {
            get
            {
                return MetaData.GetValue("NewValue");
            }
            set
            {
                MetaData.SetValue("NewValue", value);
            }
        }

        #endregion //MetaData
        
       
    }
}
