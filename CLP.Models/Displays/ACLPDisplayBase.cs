using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    abstract public class ACLPDisplayBase : ModelBase, ICLPDisplay
    {
        #region Constructors

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        protected ACLPDisplayBase()
        {
            UniqueID = Guid.NewGuid().ToString();
            CreationDate = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected ACLPDisplayBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties

        /// <summary>
        /// The Display's UniqueID
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            set { SetValue(UniqueIDProperty, value); }
        }

        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// UniqueID of the notebook the Display belongs to.
        /// </summary>
        public string ParentNotebookID
        {
            get { return GetValue<string>(ParentNotebookIDProperty); }
            set { SetValue(ParentNotebookIDProperty, value); }
        }

        public static readonly PropertyData ParentNotebookIDProperty = RegisterProperty("ParentNotebookID", typeof(string), string.Empty);

        /// <summary>
        /// Time the display was created.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>
        /// Times the Display was projected.
        /// </summary>
        public ObservableCollection<DateTime> ProjectionTimesHistory
        {
            get { return GetValue<ObservableCollection<DateTime>>(ProjectionTimesHistoryProperty); }
            set { SetValue(ProjectionTimesHistoryProperty, value); }
        }

        public static readonly PropertyData ProjectionTimesHistoryProperty = RegisterProperty("ProjectionTimesHistory", typeof(ObservableCollection<DateTime>), () => new ObservableCollection<DateTime>());

        /// <summary>
        /// The UniqueID's of the pages displayed by the Display.
        /// </summary>
        public ObservableCollection<string> DisplayPageIDs
        {
            get { return GetValue<ObservableCollection<string>>(DisplayPageIDsProperty); }
            set { SetValue(DisplayPageIDsProperty, value); }
        }

        public static readonly PropertyData DisplayPageIDsProperty = RegisterProperty("DisplayPageIDs", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        /// <summary>
        /// All pages in a Display that are not part of the Display's parent notebook.
        /// </summary>
        public List<ICLPPage> ForeignPages
        {
            get { return GetValue<List<ICLPPage>>(ForeignPagesProperty); }
            set { SetValue(ForeignPagesProperty, value); }
        }

        public static readonly PropertyData ForeignPagesProperty = RegisterProperty("ForeignPages", typeof(List<ICLPPage>), () => new List<ICLPPage>());

        #endregion //Properties

        #region Methods

        public abstract void AddPageToDisplay(ICLPPage page);

        public abstract void RemovePageFromDisplay(ICLPPage page);

        #endregion //Methods
    }
}
