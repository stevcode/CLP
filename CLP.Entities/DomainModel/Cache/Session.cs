using System;
using System.Collections.Generic;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class Session : AInternalZipEntryFile
    {
        #region Constructor

        /// <summary>Initializes <see cref="Session" /> from scratch.</summary>
        public Session()
        {
            ID = Guid.NewGuid().ToCompactID();
        }

        #endregion // Constructor

        #region Properties

        /// <summary>Unique ID for the class.</summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>A title describing the session.</summary>
        public string SessionTitle
        {
            get { return GetValue<string>(SessionTitleProperty); }
            set { SetValue(SessionTitleProperty, value); }
        }

        public static readonly PropertyData SessionTitleProperty = RegisterProperty("SessionTitle", typeof(string), string.Empty);

        /// <summary>Date and Time the class session is meant to start.</summary>
        public DateTime StartTime
        {
            get { return GetValue<DateTime>(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly PropertyData StartTimeProperty = RegisterProperty("StartTime", typeof(DateTime));

        /// <summary>Unique ID of the first Selected Page when the Session starts.</summary>
        public string StartPageID
        {
            get { return GetValue<string>(StartPageIDProperty); }
            set { SetValue(StartPageIDProperty, value); }
        }

        public static readonly PropertyData StartPageIDProperty = RegisterProperty("StartPageID", typeof(string), string.Empty);

        /// <summary>Unique IDs of all the pages in the session, mapped to their page numbers.</summary>
        public Dictionary<string, decimal> PageIDs
        {
            get { return GetValue<Dictionary<string, decimal>>(PageIDsProperty); }
            set { SetValue(PageIDsProperty, value); }
        }

        public static readonly PropertyData PageIDsProperty = RegisterProperty("PageIDs", typeof(Dictionary<string, decimal>), () => new Dictionary<string, decimal>());

        /// <summary>Comments about the session.</summary>
        public string SessionComments
        {
            get { return GetValue<string>(SessionCommentsProperty); }
            set { SetValue(SessionCommentsProperty, value); }
        }

        public static readonly PropertyData SessionCommentsProperty = RegisterProperty("SessionComments", typeof(string), string.Empty);

        /// <summary>List of the IDs of the notebooks where the pages for this session can be found.</summary>
        public List<string> NotebookIDs
        {
            get { return GetValue<List<string>>(NotebookIDsProperty); }
            set { SetValue(NotebookIDsProperty, value); }
        }

        public static readonly PropertyData NotebookIDsProperty = RegisterProperty("NotebookIDs", typeof(List<string>), () => new List<string>());

        #endregion // Properties

        #region Overrides of AInternalZipEntryFile

        public override string DefaultInternalFileName => $"s;{StartTime:yyyy.MM.dd.HH.mm};{ID}";

        public override string GetFullInternalFilePathWithExtension(string parentNotebookName)
        {
            return $"{ZIP_SESSIONS_FOLDER_NAME}/{DefaultInternalFileName}.json";
        }

        #endregion
    }
}