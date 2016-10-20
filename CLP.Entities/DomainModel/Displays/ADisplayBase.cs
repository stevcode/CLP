using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;

namespace CLP.Entities
{
    [Serializable]
    public abstract class ADisplayBase : AInternalZipEntryFile, IDisplay
    {
        #region Constructors

        /// <summary>Initializes <see cref="ADisplayBase" /> from scratch.</summary>
        protected ADisplayBase()
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToCompactID();
        }

        /// <summary>Initializes <see cref="ADisplayBase" /> from parent <see cref="Notebook" />.</summary>
        protected ADisplayBase(Notebook notebook)
            : this()
        {
            NotebookID = notebook.ID;
            ParentNotebook = notebook;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Unique Identifier for the <see cref="IDisplay" />.</summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>Date and Time the <see cref="IDisplay" /> was created.</summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>Index of the <see cref="IDisplay" /> in the notebook.</summary>
        public int DisplayNumber
        {
            get { return GetValue<int>(DisplayNumberProperty); }
            set { SetValue(DisplayNumberProperty, value); }
        }

        public static readonly PropertyData DisplayNumberProperty = RegisterProperty("DisplayNumber", typeof(int), 0);

        /// <summary>Unique Identifier of the <see cref="IDisplay" />'s parent <see cref="Notebook" />.</summary>
        public string NotebookID
        {
            get { return GetValue<string>(NotebookIDProperty); }
            set { SetValue(NotebookIDProperty, value); }
        }

        public static readonly PropertyData NotebookIDProperty = RegisterProperty("NotebookID", typeof(string), string.Empty);

        /// <summary>Parent notebook the Display belongs to.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public Notebook ParentNotebook
        {
            get { return GetValue<Notebook>(ParentNotebookProperty); }
            set { SetValue(ParentNotebookProperty, value); }
        }

        public static readonly PropertyData ParentNotebookProperty = RegisterProperty("ParentNotebook", typeof(Notebook));

        /// <summary>List of the composite IDs of the <see cref="CLPPage" />s in the <see cref="IDisplay" />.</summary>
        public List<string> CompositePageIDs
        {
            get { return GetValue<List<string>>(CompositePageIDsProperty); }
            set { SetValue(CompositePageIDsProperty, value); }
        }

        public static readonly PropertyData CompositePageIDsProperty = RegisterProperty("CompositePageIDs", typeof(List<string>), () => new List<string>());

        /// <summary>List of the <see cref="CLPPage" />s in the <see cref="IDisplay" />.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public virtual ObservableCollection<CLPPage> Pages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>Toggles visibility of the <see cref="IDisplay" /> in the list of <see cref="IDisplay" />s.</summary>
        public bool IsHidden
        {
            get { return GetValue<bool>(IsHiddenProperty); }
            set { SetValue(IsHiddenProperty, value); }
        }

        public static readonly PropertyData IsHiddenProperty = RegisterProperty("IsHidden", typeof(bool), false);

        #endregion //Properties

        #region Methods

        public virtual void AddPageToDisplay(CLPPage page)
        {
            Pages.Add(page);
            ParentNotebook.CurrentPage = Pages.FirstOrDefault();
        }

        public virtual void RemovePageFromDisplay(CLPPage page)
        {
            Pages.Remove(page);
            if (Pages.Any())
            {
                ParentNotebook.CurrentPage = Pages.FirstOrDefault();
            }
        }

        #region Cache

        public virtual void ToXML(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using (Stream stream = new FileStream(filePath, FileMode.Create))
            {
                var xmlSerializer = SerializationFactory.GetXmlSerializer();
                xmlSerializer.Serialize(this, stream);
                ClearIsDirtyOnAllChilds();
            }
        }

        public virtual void Save(string folderPath)
        {
            var displayTypeIdentifier = this is GridDisplay ? "grid" : "column";

            var fileName = displayTypeIdentifier + ";" + DisplayNumber + ";" + ID + ".xml";
            CompositePageIDs.Clear();
            foreach (var compositeID in Pages.Select(page => page.ID + ";" + page.OwnerID + ";" + page.DifferentiationLevel + ";" + page.VersionIndex))
            {
                CompositePageIDs.Add(compositeID);
            }
            var filePath = Path.Combine(folderPath, fileName);
            ToXML(filePath);
        }

        #endregion //Cache

        #endregion //Methods

        #region Overrides of AInternalZipEntryFile

        public override string DefaultZipEntryName => "grid";

        public override string GetZipEntryFullPath(string parentNotebookName)
        {
            return $"{ZIP_NOTEBOOKS_FOLDER_NAME}/{parentNotebookName}/{ZIP_NOTEBOOK_DISPLAYS_FOLDER_NAME}/{DefaultZipEntryName}.json";
        }

        #endregion
    }
}