using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;

namespace CLP.Entities
{
    [Serializable]
    public abstract class AInternalZipEntryFile : AEntityBase
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
        [JsonIgnore]
        [ExcludeFromSerialization]
        public string ContainerZipFilePath { get; set; }

        /// <summary>Has the file been saved locally to disc?</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public bool IsSavedLocally { get; set; }

        /// <summary>Has the file been saved over the network?</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public bool IsSavedOverTheNetwork { get; set; }

        /// <summary><see cref="AInternalZipEntryFile" />'s internal file name without extension.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public abstract string DefaultZipEntryName { get; }

        #endregion // Properties

        #region Methods

        public abstract string GetZipEntryFullPath(Notebook parentNotebook);

        #endregion // Methods

        #region Overrides of ModelBase

        protected override void OnPropertyObjectCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.OnPropertyObjectCollectionChanged(sender, e);

            if (!IsDirty)
            {
                return;
            }

            //IsSavedLocally = false;
            //IsSavedOverTheNetwork = false;


        }

        // TODO: Take into account and ignore Person.IsConnected changes
        protected override void OnPropertyObjectCollectionItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyObjectCollectionItemPropertyChanged(sender, e);

            if (!IsDirty ||
                e.PropertyName == nameof(IsDirty))
            {
                return;
            }
            //IsSavedLocally = false;
            //IsSavedOverTheNetwork = false;
        }

        /// <summary>
        /// Determines whether a specific property change should update <c>IsDirty</c> to <c>true</c>.
        /// </summary>
        /// <returns><c>true</c> if <c>IsDirty</c> should be set to <c>true</c> when the specified property has changed, <c>false</c> otherwise.</returns>
        protected override bool ShouldPropertyChangeUpdateIsDirty(string propertyName)
        {
            return true;
        }

        #endregion

        #region Overrides of ObservableObject

        // Because all CLPPages are in a list on Notebook.Pages (or Submissions), OnPropertyObjectCollectionItemPropertyChanged above catches
        // changes to top level properties of CLPPage that change (such as Width or Height) and are handled above. This should only be used to
        // handle top-level changes on ClassRoster, Notebook, NotebookSet, or Session.
        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (!IsDirty ||
                e.PropertyName == nameof(IsDirty))
            {
                return;
            }

            //IsSavedLocally = false;
            //IsSavedOverTheNetwork = false;

            //Debug.WriteLine($"Property Changed: {e.PropertyName}, Old Value: {e.OldValue}, New Value: {e.NewValue}");
        }

        #endregion
    }
}