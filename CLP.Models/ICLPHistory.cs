using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLP.Models
{
    public interface ICLPHistory
    {
        Stack<CLPHistoryItem> Future { get; set; }  
        Stack<CLPHistoryItem> MetaPast { get; set; }
         List<CLPHistoryItem> ExpectedEvents { get; set; }
         
         bool _useHistory { get; set; }
         bool singleCutting { get; set; }
        
        //Forget everything that happened; lock the current state as the starting state.
         void ClearHistory();

         void Freeze();

         void Unfreeze();

         void Push(CLPHistoryItem item);

         void BeginEventGroup();

         void EndEventGroup();

         CLPHistoryItem AggregateItems(Stack<CLPHistoryItem> itemStack); 

         void Undo(CLPPage page);

        void Redo(CLPPage page);

         //bool IsExpected(CLPHistoryItem item);
    }
}
