using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLP.Entities
{
    [Serializable]
    class BinsStrategyTag : ATagBase
    {

        public BinsStrategyTag() { }

        public BinsStrategyTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }


        public override Category Category
        {
            get
            {
                return Category.Bin;
            }
        }

        public override string FormattedValue
        {
            get
            {
                var x = 0;
                return "BINS 3 DB 2";
            }
        }
    }
}
