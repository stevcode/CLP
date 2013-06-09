using System;
using Catel.Data;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPProofPage : CLPPage
    {
        #region Variables
        #endregion

        #region Constructor & destructor
        public CLPProofPage() :base()
        {
            this.PageHistory.Freeze();    

        }
        
    protected CLPProofPage(SerializationInfo info, StreamingContext context)
           : base(info, context)
       {
       }
        #endregion

        #region Properties
       
        /// <summary>
        /// Gets the CLPProofPage history.
        /// </summary>
        public override ICLPHistory PageHistory
        {
            get { return GetValue<CLPProofHistory>(ProofPageHistoryProperty); }
            set { SetValue(ProofPageHistoryProperty, value); }
        }

        public static volatile  PropertyData ProofPageHistoryProperty = RegisterProperty("ProofPageHistory", typeof(CLPProofHistory), () => new CLPProofHistory());
        
        #endregion

        #region Methods


        #endregion
    }
}
