using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;

namespace CLP.Entities
{
    [Serializable]
    public class Notebook : AInternalZipEntryFile
    {
        #region Constructors

        /// <summary>Initializes <see cref="Notebook" /> from scratch.</summary>
        public Notebook()
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToCompactID();
        }

        /// <summary>Initializes <see cref="Notebook" /> with name and owner.</summary>
        /// <param name="notebookName">The name of the notebook.</param>
        /// <param name="owner">The <see cref="Person" /> who owns the notebook.</param>
        public Notebook(string notebookName, Person owner)
            : this()
        {
            Name = notebookName;
            Owner = owner;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Unique Identifier for the <see cref="Notebook" />.</summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>Unique Identifier for the <see cref="Person" /> who owns the <see cref="Notebook" />.</summary>
        public string OwnerID
        {
            get { return GetValue<string>(OwnerIDProperty); }
            set { SetValue(OwnerIDProperty, value); }
        }

        public static readonly PropertyData OwnerIDProperty = RegisterProperty("OwnerID", typeof(string), string.Empty);

        /// <summary><see cref="Person" /> who owns the <see cref="Notebook" />.</summary>
        public Person Owner
        {
            get { return GetValue<Person>(OwnerProperty); }
            set { SetValue(OwnerProperty, value); }
        }

        public static readonly PropertyData OwnerProperty = RegisterProperty("Owner", typeof(Person), propertyChangedEventHandler: OnOwnerChanged);

        private static void OnOwnerChanged(object sender, AdvancedPropertyChangedEventArgs e)
        {
            if (!e.IsNewValueMeaningful ||
                e.NewValue == null)
            {
                return;
            }

            var notebook = sender as Notebook;
            var newOwner = e.NewValue as Person;
            if (notebook == null ||
                newOwner == null)
            {
                return;
            }

            notebook.OwnerID = newOwner.ID;
        }

        /// <summary>Name of the <see cref="Notebook" />.</summary>
        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string), string.Empty);

        /// <summary>Date and Time the <see cref="Notebook" /> was created.</summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>Date and Time the <see cref="Notebook" /> was last saved.</summary>
        /// <remarks>Type set to DateTime? (i.e. nullable DateTime) to allow NULL in database if LastSavedDate hasn't been set yet.</remarks>
        public DateTime? LastSavedDate
        {
            get { return GetValue<DateTime?>(LastSavedDateProperty); }
            set { SetValue(LastSavedDateProperty, value); }
        }

        public static readonly PropertyData LastSavedDateProperty = RegisterProperty("LastSavedDate", typeof(DateTime?));

        /// <summary>List of all the HashIDs for each <see cref="CLPImage" /> that is in the notebook.</summary>
        public List<string> ImagePoolHashIDs
        {
            get { return Pages.SelectMany(page => page.PageObjects).OfType<CLPImage>().Select(image => image.ImageHashID).ToList().Distinct().ToList(); }
        }

        /// <summary>Unique Identifier of the currently selected <see cref="CLPPage" />.</summary>
        public string CurrentPageID
        {
            get { return GetValue<string>(CurrentPageIDProperty); }
            set { SetValue(CurrentPageIDProperty, value); }
        }

        public static readonly PropertyData CurrentPageIDProperty = RegisterProperty("CurrentPageID", typeof(string));

        /// <summary>Unique Identifier of the <see cref="Person" /> who owns the currently selected <see cref="CLPPage" />.</summary>
        public string CurrentPageOwnerID
        {
            get { return GetValue<string>(CurrentPageOwnerIDProperty); }
            set { SetValue(CurrentPageOwnerIDProperty, value); }
        }

        public static readonly PropertyData CurrentPageOwnerIDProperty = RegisterProperty("CurrentPageOwnerID", typeof(string));

        /// <summary>Version Index of the currently selected <see cref="CLPPage" />.</summary>
        public uint CurrentPageVersionIndex
        {
            get { return GetValue<uint>(CurrentPageVersionIndexProperty); }
            set { SetValue(CurrentPageVersionIndexProperty, value); }
        }

        public static readonly PropertyData CurrentPageVersionIndexProperty = RegisterProperty("CurrentPageVersionIndex", typeof(uint));

        #region Non-Serialized

        /// <summary>Currently selected <see cref="CLPPage" />.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public CLPPage CurrentPage
        {
            get { return GetValue<CLPPage>(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly PropertyData CurrentPageProperty = RegisterProperty("CurrentPage", typeof(CLPPage), propertyChangedEventHandler: OnCurrentPageChanged);

        private static void OnCurrentPageChanged(object sender, AdvancedPropertyChangedEventArgs e)
        {
            if (!e.IsNewValueMeaningful ||
                e.NewValue == null)
            {
                return;
            }

            var notebook = sender as Notebook;
            var page = e.NewValue as CLPPage;
            if (notebook == null ||
                page == null)
            {
                return;
            }

            notebook.CurrentPageID = page.ID;
            notebook.CurrentPageVersionIndex = page.VersionIndex;
        }

        /// <summary>Collection of all the <see cref="CLPPage" />s in the <see cref="Notebook" />.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public ObservableCollection<CLPPage> Pages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>List of the <see cref="IDisplay" />s in the <see cref="Notebook" />.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public ObservableCollection<IDisplay> Displays
        {
            get { return GetValue<ObservableCollection<IDisplay>>(DisplaysProperty); }
            set { SetValue(DisplaysProperty, value); }
        }

        public static readonly PropertyData DisplaysProperty = RegisterProperty("Displays", typeof(ObservableCollection<IDisplay>), () => new ObservableCollection<IDisplay>());

        #endregion // Non-Serialized

        #endregion // Properties

        // TODO: Either here or in NotebookSet, add property for IsLoadedLocally vs IsLoadedOverNetwork, use as flag to decide how it saves.

        #region Methods

        public CLPPage GetPageByCompositeKeys(string pageID, string pageOwnerID, string differentiationLevel, uint versionIndex, bool searchDatabaseAndCache = false)
        {
            // TODO: Move to DataService once loading/saving works. Possibly load from Container.clp file if not already in memory?
            var notebookPage = Pages.FirstOrDefault(x => x.ID == pageID && x.OwnerID == pageOwnerID && x.DifferentiationLevel == differentiationLevel && x.VersionIndex == versionIndex);
            if (notebookPage != null)
            {
                return notebookPage;
            }

            notebookPage = Pages.FirstOrDefault(x => x.ID == pageID && x.DifferentiationLevel == differentiationLevel);
            return notebookPage == null ? null : notebookPage.Submissions.FirstOrDefault(x => x.OwnerID == pageOwnerID && x.VersionIndex == versionIndex);
        }

        #endregion //Methods

        #region Storage

        public string InternalZipFileDirectoryName
        {
            get
            {
                var ownerType = OwnerID == Person.AUTHOR_ID ? "A" : Owner.IsStudent ? "S" : "T";
                return $"{ownerType};{Owner.FullName};{Owner.ID}";
            }
        }

        #endregion // Storage

        #region Overrides of ModelBase

        protected override void OnSerializing()
        {
            base.OnSerializing();

            LastSavedDate = DateTime.Now;
        }

        #endregion

        #region Overrides of AInternalZipEntryFile

        public override string DefaultInternalFileName => "notebook";

        public override string GetFullInternalFilePathWithExtension(string parentNotebookName)
        {
            return $"{ZIP_NOTEBOOKS_FOLDER_NAME}/{parentNotebookName}/{DefaultInternalFileName}.json";
        }

        #endregion
    }
}