using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public abstract class AInternalZipEntryFile : ASerializableBase
    {
        #region Constants

        public const string CONTAINER_EXTENSION = "clp";

        public const string ZIP_IMAGES_FOLDER_NAME = "images";
        public const string ZIP_NOTEBOOKS_FOLDER_NAME = "notebooks";
        public const string ZIP_SESSIONS_FOLDER_NAME = "sessions";

        public const string ZIP_NOTEBOOK_DISPLAYS_FOLDER_NAME = "displays";
        public const string ZIP_NOTEBOOK_PAGES_FOLDER_NAME = "pages";
        public const string ZIP_NOTEBOOK_PAGE_THUMBNAILS_FOLDER_NAME = "thumbnails";
        public const string ZIP_NOTEBOOK_SUBMISSIONS_FOLDER_NAME = "submissions";
        public const string ZIP_NOTEBOOK_SUBMISSION_THUMBNAILS_FOLDER_NAME = "thumbnails";

        #endregion // Constants

        #region Constructor

        protected AInternalZipEntryFile()
        {
            IsSavedLocally = false;
            IsSavedOverTheNetwork = false;
        }

        #endregion // Constructor

        #region Properties

        /// <summary>Associated FilePath of the <see cref="AInternalZipEntryFile" />'s container zip file.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public string ContainerZipFilePath { get; set; }

        /// <summary>Has the file been saved locally to disc?</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public bool IsSavedLocally { get; set; }

        /// <summary>Has the file been saved over the network?</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public bool IsSavedOverTheNetwork { get; set; }

        /// <summary><see cref="AInternalZipEntryFile" />'s internal file name without extension.</summary>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public abstract string DefaultZipEntryName { get; }

        #endregion // Properties

        #region Methods

        public abstract string GetZipEntryFullPath(Notebook parentNotebook);

        #endregion // Methods
    }
}