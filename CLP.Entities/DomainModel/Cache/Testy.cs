using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace CLP.Entities
{
    public class Testy : ModelBase
    {
        public Testy() { }

        /// <summary>SUMMARY</summary>
        public Dictionary<string, decimal> PageIDs
        {
            get { return GetValue<Dictionary<string, decimal>>(PageIDsProperty); }
            set { SetValue(PageIDsProperty, value); }
        }

        public static readonly PropertyData PageIDsProperty = RegisterProperty("PageIDs", typeof(Dictionary<string, decimal>), () => new Dictionary<string, decimal>());
        
    }
}
