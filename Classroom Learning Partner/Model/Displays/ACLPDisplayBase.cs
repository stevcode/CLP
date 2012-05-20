using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Catel.Data;
using System.Runtime.Serialization;

namespace Classroom_Learning_Partner.Model.Displays
{
    public enum DisplayTypes
    {
        Mirror,
        Grid,
        Column,
        Canvas
    }

    public interface ICLPDisplay
    {
        string UniqueID { get; set; }
        string ParentNotebookUniqueID { get; set; }
        DateTime CreationDate { get; set; }
        DateTime LastProjectionTime { get; set; }
        ObservableCollection<DateTime> ProjectionTimesHistory { get; set; }

        ObservableCollection<Tuple<bool, string>> DisplayPages { get; set; }
        ObservableCollection<CLPPage> ForeignPages { get; set; }

        DisplayTypes DisplayType { get; }
    }

    abstract public class ACLPDisplayBase : DataObjectBase<ACLPDisplayBase>
    {

        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public ACLPDisplayBase()
        {
            UniqueID = Guid.NewGuid().ToString();
            CreationDate = DateTime.Now;
            ProjectionTimesHistory = new ObservableCollection<DateTime>();
            DisplayPages = new ObservableCollection<Tuple<bool, string>>();
            ForeignPages = new ObservableCollection<CLPPage>();
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
        /// Gets or sets the property value.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            set { SetValue(UniqueIDProperty, value); }
        }

        /// <summary>
        /// Register the UniqueID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public string ParentNotebookUniqueID
        {
            get { return GetValue<string>(ParentNotebookUniqueIDProperty); }
            set { SetValue(ParentNotebookUniqueIDProperty, value); }
        }

        /// <summary>
        /// Register the ParentNotebookUniqueID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ParentNotebookUniqueIDProperty = RegisterProperty("ParentNotebookUniqueID", typeof(string), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        /// <summary>
        /// Register the CreationDate property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DateTime LastProjectionTime
        {
            get { return GetValue<DateTime>(LastProjectionTimeProperty); }
            set { SetValue(LastProjectionTimeProperty, value); }
        }

        /// <summary>
        /// Register the LastProjectionTime property so it is known in the class.
        /// </summary>
        public static readonly PropertyData LastProjectionTimeProperty = RegisterProperty("LastProjectionTime", typeof(DateTime), null);

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<DateTime> ProjectionTimesHistory
        {
            get { return GetValue<ObservableCollection<DateTime>>(ProjectionTimesHistoryProperty); }
            private set { SetValue(ProjectionTimesHistoryProperty, value); }
        }

        /// <summary>
        /// Register the ProjectionTimesHistory property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ProjectionTimesHistoryProperty = RegisterProperty("ProjectionTimesHistory", typeof(ObservableCollection<DateTime>), () => new ObservableCollection<DateTime>());

        /// <summary>
        /// Gets or sets the property value. The Tuble is <bool isPageInCurrentNotebook, string pageID>.
        /// </summary>
        public ObservableCollection<Tuple<bool, string>> DisplayPages
        {
            get { return GetValue<ObservableCollection<Tuple<bool, string>>>(DisplayPagesProperty); }
            set { SetValue(DisplayPagesProperty, value); }
        }

        /// <summary>
        /// Register the DisplayPages property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DisplayPagesProperty = RegisterProperty("DisplayPages", typeof(ObservableCollection<Tuple<bool, string>>), () => new ObservableCollection<Tuple<bool, string>>());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<CLPPage> ForeignPages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(ForeignPagesProperty); }
            set { SetValue(ForeignPagesProperty, value); }
        }

        /// <summary>
        /// Register the ForeignPages property so it is known in the class.
        /// </summary>
        public static readonly PropertyData ForeignPagesProperty = RegisterProperty("ForeignPages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        #endregion //Properties
    }
}
