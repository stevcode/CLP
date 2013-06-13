using System.Collections.Generic;

namespace CLP.Models
{
    public interface ICLPHistory
    {
        Stack<CLPHistoryItem> Future { get; set; }  
        Stack<CLPHistoryItem> MetaPast { get; set; }
        List<CLPHistoryItem> ExpectedEvents { get; set; }
         
        bool UseHistory { get; set; }
        bool SingleCutting { get; set; }
        
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
