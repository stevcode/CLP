using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    [AllowNonSerializableMembers]
    abstract public class ACLPHistoryItemBase : ModelBase, ICLPHistoryItem
    {
        #region Constructors

        protected ACLPHistoryItemBase(ICLPPage parentPage)
        {
            ParentPage = parentPage;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected ACLPHistoryItemBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        // TODO: Steve - Find out if this gets serialized. It shouldn't be.
        public ICLPPage ParentPage { get; set; }

        public void Undo(bool isAnimationUndo = false)
        {
            if(ParentPage == null)
            {
                Logger.Instance.WriteToLog("No ParentPage in HistoryItem for Undo");
                return;
            }
            UndoAction(isAnimationUndo);
        }

        protected abstract void UndoAction(bool isAnimationUndo);

        public void Redo(bool isAnimationRedo = false)
        {
            if(ParentPage == null)
            {
                Logger.Instance.WriteToLog("No ParentPage in HistoryItem for Redo");
                return;
            }
            RedoAction(isAnimationRedo);
        }

        protected abstract void RedoAction(bool isAnimationRedo);

    }
}
