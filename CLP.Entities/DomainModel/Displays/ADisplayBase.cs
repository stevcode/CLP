using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities.Ann
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
            ID = Guid.NewGuid().ToCompactID();
        }

        /// <summary>
        /// Initializes <see cref="ADisplayBase" /> from parent <see cref="Notebook" />.
        /// </summary>
        public ADisplayBase(Notebook notebook)
            : this() { NotebookID = notebook.ID; }

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
        public int DisplayNumber
        {
            get { return GetValue<int>(DisplayNumberProperty); }
            set { SetValue(DisplayNumberProperty, value); }
        }

        public static readonly PropertyData DisplayNumberProperty = RegisterProperty("DisplayNumber", typeof(int), 0);

        /// <summary>
        /// Unique Identifier of the <see cref="IDisplay" />'s parent <see cref="Notebook" />.
        /// </summary>
        public string NotebookID
        {
            get { return GetValue<string>(NotebookIDProperty); }
            set { SetValue(NotebookIDProperty, value); }
        }

        public static readonly PropertyData NotebookIDProperty = RegisterProperty("NotebookID", typeof(string), string.Empty);

        /// <summary>
        /// List of the composite IDs of the <see cref="CLPPage" />s in the <see cref="IDisplay" />.
        /// </summary>
        public List<string> CompositePageIDs
        {
            get { return GetValue<List<string>>(CompositePageIDsProperty); }
            set { SetValue(CompositePageIDsProperty, value); }
        }

        public static readonly PropertyData CompositePageIDsProperty = RegisterProperty("CompositePageIDs", typeof(List<string>), () => new List<string>());

        /// <summary>
        /// List of the <see cref="CLPPage" />s in the <see cref="IDisplay" />.
        /// </summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
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

        public abstract void ToXML(string filePath);

        public abstract void Save(string folderPath);

        #endregion //Methods
    }
}