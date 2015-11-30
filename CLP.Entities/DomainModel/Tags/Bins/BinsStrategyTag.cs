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

        public BinsStrategyTag(CLPPage parentPage, Origin origin, int binCount, int dealBy, List<int> dealt)
    : base(parentPage, origin)
        {
            BinCount = binCount;
            DealBy = dealBy;
            Dealt = dealt;
        }

        public int BinCount
        {
            get; set; }

        public int DealBy
        {
            get; set;
        }

        public List<int> Dealt
        {
            get; set;
        }


        public override Category Category
        {
            get
            {
                return Category.Bin;
            }
        }

        public override string FormattedName
        {
            get { return "Bins Strategy"; }
        }

        public override string FormattedValue
        {
            get
            {
                var dealtStr = (Dealt.Count == 0) ? "" : Dealt[0].ToString();
                if (Dealt.Distinct().Skip(1).Any())
                {
                    for (int i = 1; i < Dealt.Count; i++)
                    {
                        dealtStr = dealtStr + ", " + Dealt[i].ToString();
                    }
                }
                return "Code: BINS deal [" + BinCount + " DB " + DealBy + " D: " + dealtStr + "]";
            }
        }
    }
}
