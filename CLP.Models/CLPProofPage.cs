using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.Xml.Serialization;
using Catel;
using Catel.Data;

namespace CLP.Models
{
    public class CLPProofPage : CLPPage
    {
        #region Variables
        #endregion

        #region Constructor & destructor
        public CLPProofPage() :base()
        {
            this.PageHistory.Freeze();    

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
