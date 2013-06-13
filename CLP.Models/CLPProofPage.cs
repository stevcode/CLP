using System;
using Catel.Data;
using System.Runtime.Serialization;

namespace CLP.Models
{
    [Serializable]
    public class CLPProofPage : CLPPage
    {
        #region Constructors

        public CLPProofPage()
        {
            PageHistory.Freeze();    
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

        // TODO: Steve - is volatile necessary?
        public static volatile PropertyData ProofPageHistoryProperty = RegisterProperty("ProofPageHistory", typeof(CLPProofHistory), () => new CLPProofHistory());
        
        #endregion

    }
}
