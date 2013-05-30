using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    public class CLPProofHistory : CLPHistory
    {
        #region Constructor
        public CLPProofHistory() : base() { }

        protected CLPProofHistory(SerializationInfo info, StreamingContext context)
        : base(info, context){}

        #endregion //Constructor

        #region Properties
        public bool isPaused
        {
            get { return GetValue<bool>(isPausedProperty); }
            set { SetValue(isPausedProperty, value); }
        }
        public static readonly PropertyData isPausedProperty = RegisterProperty("isPaused", typeof(bool), false);
        #endregion//Properties

        #region Methods
        public override void Push(CLPHistoryItem item)
        {
            if(isPaused){ 
                item.wasPaused = true; 
            }
            if(!_useHistory || _frozen || IsExpected(item))
            {
                return;
            }
            if(_ingroup)
            {
                groupEvents.Push(item);
            }
            else
            {
                Past.Push(item);
                MetaPast.Push(item);
                //Future.Clear(); does not clear future items
                //unlike overridden method
            }
        }

        #endregion //Methods
    }
}
