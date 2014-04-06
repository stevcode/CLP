using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public abstract class ADisplayBase : AEntityBase, IDisplay
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="ADisplayBase" /> from scratch.
        /// </summary>
        public ADisplayBase()
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes <see cref="ADisplayBase" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ADisplayBase(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

         /// <summary>
        /// Unique Identifier for the <see cref="IDisplay" />.
        /// </summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>
        /// Date and Time the <see cref="IDisplay" /> was created.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>
        /// Index of the <see cref="IDisplay" /> in the notebook.
        /// </summary>
        public int Index
        {
            get { return GetValue<int>(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public static readonly PropertyData IndexProperty = RegisterProperty("Index", typeof(int), 0);

        /// <summary>
        /// Unique Identifier of the currently selected <see cref="CLPPage" />.
        /// </summary>
        public string CurrentPageID
        {
            get { return GetValue<string>(CurrentPageIDProperty); }
            set { SetValue(CurrentPageIDProperty, value); }
        }

        public static readonly PropertyData CurrentPageIDProperty = RegisterProperty("CurrentPageID", typeof(string));

        /// <summary>
        /// Currently selected <see cref="CLPPage" /> of the <see cref="IDisplay" />.
        /// </summary>
        public virtual CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage));

        /// <summary>
        /// List of <see cref="CLPPage" /> IDs in the <see cref="IDisplay" />.
        /// </summary>
        public ObservableCollection<string> PageIDs
        {
            get { return GetValue<ObservableCollection<string>>(PageIDsProperty); }
            set { SetValue(PageIDsProperty, value); }
        }

        public static readonly PropertyData PageIDsProperty = RegisterProperty("PageIDs", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        /// <summary>
        /// List of the <see cref="CLPPage" />s in the <see cref="IDisplay" />.
        /// </summary>
        public virtual ObservableCollection<CLPPage> Pages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        #endregion //Properties

        #region Methods

        public abstract void AddPageToDisplay(CLPPage page);

        public abstract void RemovePageFromDisplay(CLPPage page);

        #endregion //Methods
    }
}