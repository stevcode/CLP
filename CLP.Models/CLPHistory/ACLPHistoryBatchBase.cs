using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    abstract public class ACLPHistoryBatchBase : ModelBase, IHistoryBatch
    {
        #region Constructors

        protected ACLPHistoryBatchBase(ICLPPage parentPage)
        {
            ParentPage = parentPage;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected ACLPHistoryBatchBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion //Constructors

        #region Properties

        // TODO: Steve - Find out if this gets serialized. It shouldn't be.
        public ICLPPage ParentPage { get; set; }

        /// <summary>
        /// The ICLPHistoryItems that make up this batch.
        /// </summary>
        public ObservableCollection<ICLPHistoryItem> HistoryItems
        {
            get { return GetValue<ObservableCollection<ICLPHistoryItem>>(HistoryItemsProperty); }
            set { SetValue(HistoryItemsProperty, value); }
        }

        public static readonly PropertyData HistoryItemsProperty = RegisterProperty("HistoryItems", typeof(ObservableCollection<ICLPHistoryItem>), () => new ObservableCollection<ICLPHistoryItem>());

        public bool IsEmptyBatch
        {
            get { return HistoryItems.Count == 0; }
        }

        public bool IsSingleBatch
        {
            get { return HistoryItems.Count == 1; }
        }
        
        #endregion //Properties

        #region Methods

        public virtual void Undo()
        {
            foreach(var clpHistoryItem in HistoryItems)
            {
                clpHistoryItem.Undo();
            }
        }

        public virtual void Redo()
        {
            foreach(var clpHistoryItem in HistoryItems)
            {
                clpHistoryItem.Redo();
            }
        }

        public abstract void AddToBatch(object target, object tag = null);

        #endregion //Methods
    }
}
