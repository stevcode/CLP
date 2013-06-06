using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    public class CLPProofHistory : CLPHistory
    {      

        public enum CLPProofPageAction
        {
            Play,
            Rewind,
            Forward,
            Record,
            Pause
        }

        #region Constructor

        public CLPProofHistory() {}

        protected CLPProofHistory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructor

        #region Properties

        public CLPProofPageAction ProofPageAction
        {
            get { return GetValue<CLPProofPageAction>(ProofPageActionProperty); }
            set { SetValue(ProofPageActionProperty, value); }
        }

        /// <summary>
        /// Register the ShapeType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ProofPageActionProperty = RegisterProperty("ProofPageAction", typeof(CLPProofPageAction), CLPProofPageAction.Pause);

        public bool IsPaused
        {
            get { return GetValue<bool>(IsPausedProperty); }
            set { SetValue(IsPausedProperty, value); }
        }
        
        public static readonly PropertyData IsPausedProperty = RegisterProperty("IsPaused", typeof(bool), false);

        #endregion//Properties

        #region Methods
        public override void Push(CLPHistoryItem item)
        {
            if(IsPaused){ 
                item.wasPaused = true; 
            }
            if(SingleCutting){
                item.singleCut = true;
            }
            if(!UseHistory || _frozen || IsExpected(item))
            {
                return;
            }
            /*if(_ingroup)
            {
                groupEvents.Push(item);
            }*/
            else
            {
                Past.Push(item);
                MetaPast.Push(item);
                //Future.Clear(); does not clear future items unlike overridden method
            }
        }

        public override void EndEventGroup()
        {
            _ingroup = false;
            if(groupEvents.Count > 0)
            {
                CLPHistoryItem group = AggregateItems(groupEvents);
                Past.Push(group);
                MetaPast.Push(group);
                //Future.Clear(); does not clear future items unlike overridden method
            }
        }
        
        public override void Undo(CLPPage page) {
            if(MetaPast.Count>0){
                Freeze();
                CLPHistoryItem lastAction = MetaPast.Pop();
                lastAction.Undo(page);
                Future.Push(lastAction);
                Unfreeze();
            }
        }

        public override void Redo(CLPPage page) {
            if(Future.Count>0){
                Freeze();
                CLPHistoryItem nextAction = Future.Pop();
                nextAction.Redo(page);
                MetaPast.Push(nextAction);
                Unfreeze();
            }
        }
        #endregion //Methods
    }
}
