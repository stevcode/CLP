using System;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NotebookSet : ASerializableBase
    {
        #region Constructor

        /// <summary>Initializes <see cref="NotebookSet" /> from scratch.</summary>
        public NotebookSet()
        {
            CreationDate = DateTime.Now;
            NotebookID = Guid.NewGuid().ToCompactID();
        }

        /// <summary>Initializes <see cref="Notebook" /> with name and owner.</summary>
        public NotebookSet(string notebookName)
            : this()
        {
            NotebookName = notebookName;
        }

        #endregion // Constructor

        #region Properties

        /// <summary>Name of the contained notebooks.</summary>
        public string NotebookName
        {
            get { return GetValue<string>(NotebookNameProperty); }
            set { SetValue(NotebookNameProperty, value); }
        }

        public static readonly PropertyData NotebookNameProperty = RegisterProperty("NotebookName", typeof(string), string.Empty);

        /// <summary>Unique ID for the notebook set, propgated down to all notebooks in the set.</summary>
        public string NotebookID
        {
            get { return GetValue<string>(NotebookIDProperty); }
            set { SetValue(NotebookIDProperty, value); }
        }

        public static readonly PropertyData NotebookIDProperty = RegisterProperty("NotebookID", typeof(string), string.Empty);

        /// <summary>Date and Time the <see cref="NotebookSet" /> was created.</summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>Flags that the NotebookSet is contained in a .clpconnected file.</summary>
        public bool IsConnectedNotebook
        {
            get { return GetValue<bool>(IsConnectedNotebookProperty); }
            set { SetValue(IsConnectedNotebookProperty, value); }
        }

        public static readonly PropertyData IsConnectedNotebookProperty = RegisterProperty("IsConnectedNotebook", typeof(bool), false);

        #endregion // Properties
    }
}