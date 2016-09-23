using System;
using System.IO;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;

namespace CLP.Entities
{
    [Serializable]
    public class NotebookSet : AEntityBase
    {
        #region Constructor

        // TODO: Perhaps just replace with ClassRoster
        /// <summary>Initializes <see cref="NotebookSet" /> from scratch.</summary>
        public NotebookSet()
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToCompactID();
        }

        #endregion // Constructor

        #region Properties

        /// <summary>Unique ID for the notebook set, propgated down to all notebooks in the set.</summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        // TODO: Account for .clpconnected notebooks with different names.
        /// <summary>Name of the contained notebooks.</summary>
        public string NotebookName
        {
            get { return GetValue<string>(NotebookNameProperty); }
            set { SetValue(NotebookNameProperty, value); }
        }

        public static readonly PropertyData NotebookNameProperty = RegisterProperty("NotebookName", typeof(string), string.Empty);

        /// <summary>Date and Time the <see cref="NotebookSet" /> was created.</summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>Associated FileInfo of the notebookSet's container.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public FileInfo FilePathFileInfo
        {
            get { return GetValue<FileInfo>(FilePathFileInfoProperty); }
            set { SetValue(FilePathFileInfoProperty, value); }
        }

        public static readonly PropertyData FilePathFileInfoProperty = RegisterProperty("FilePathFileInfo", typeof(FileInfo));

        #endregion // Properties
    }
}