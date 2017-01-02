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
        public Notebook() { }

        /// <summary>Initializes <see cref="Notebook" /> with name and owner.</summary>
        public Notebook(NotebookSet notebookSet, Person owner)
        {
            Name = notebookSet.NotebookName;
            ID = notebookSet.NotebookID;
            CreationDate = notebookSet.CreationDate;
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

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string), string.Empty);

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

        /// <summary>Date and Time the <see cref="Notebook" /> was generated from the Author Notebook.</summary>
        public DateTime GenerationDate
        {
            get { return GetValue<DateTime>(GenerationDateProperty); }
            set { SetValue(GenerationDateProperty, value); }
        }

        public static readonly PropertyData GenerationDateProperty = RegisterProperty("GenerationDate", typeof(DateTime));

        /// <summary>Date and Time the <see cref="Notebook" /> was last saved.</summary>
        /// <remarks>Type set to DateTime? (i.e. nullable DateTime) to allow NULL in database if LastSavedDate hasn't been set yet.</remarks>
        public DateTime? LastSavedDate
        {
            get { return GetValue<DateTime?>(LastSavedDateProperty); }
            set { SetValue(LastSavedDateProperty, value); }
        }

        public static readonly PropertyData LastSavedDateProperty = RegisterProperty("LastSavedDate", typeof(DateTime?));

        /// <summary>Unique Identifier of the currently selected <see cref="CLPPage" />.</summary>
        public string CurrentPageID
        {
            get { return GetValue<string>(CurrentPageIDProperty); }
            set { SetValue(CurrentPageIDProperty, value); }
        }

        public static readonly PropertyData CurrentPageIDProperty = RegisterProperty("CurrentPageID", typeof(string), string.Empty);

        /// <summary>Version Index of the currently selected <see cref="CLPPage" />.</summary>
        public uint CurrentPageVersionIndex
        {
            get { return GetValue<uint>(CurrentPageVersionIndexProperty); }
            set { SetValue(CurrentPageVersionIndexProperty, value); }
        }

        public static readonly PropertyData CurrentPageVersionIndexProperty = RegisterProperty("CurrentPageVersionIndex", typeof(uint), 0);

        #region Calculated Properties

        /// <summary>List of all the HashIDs for each <see cref="CLPImage" /> that is in the notebook.</summary>
        public List<string> ImagePoolHashIDsForLoadedPages
        {
            get { return Pages.SelectMany(page => page.PageObjects).OfType<CLPImage>().Select(image => image.ImageHashID).ToList().Distinct().ToList(); }
        }

        #endregion // Calculated Properties

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

        /// <summary>The selected display in the list of the Notebook's Displays. SingleDisplay is displayed when null.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [ExcludeFromSerialization]
        public IDisplay CurrentDisplay
        {
            get { return GetValue<IDisplay>(CurrentDisplayProperty); }
            set { SetValue(CurrentDisplayProperty, value); }
        }

        public static readonly PropertyData CurrentDisplayProperty = RegisterProperty("CurrentDisplay", typeof(IDisplay));

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

        #region Overrides of ModelBase

        protected override void OnSerializing()
        {
            base.OnSerializing();

            LastSavedDate = DateTime.Now;
        }

        #endregion

        #region Storage

        public const string DEFAULT_INTERNAL_FILE_NAME = "notebook";

        public string NotebookSetDirectoryName => $"{Name};{ID}";

        public string NotebookOwnerDirectoryName => Owner.NotebookOwnerDirectoryName;

        public string NotebookPagesDirectoryPath => $"{ZIP_NOTEBOOKS_FOLDER_NAME}/{NotebookSetDirectoryName}/{NotebookOwnerDirectoryName}/{ZIP_NOTEBOOK_PAGES_FOLDER_NAME}/";

        public string NotebookSubmissionsDirectoryPath => $"{ZIP_NOTEBOOKS_FOLDER_NAME}/{NotebookSetDirectoryName}/{NotebookOwnerDirectoryName}/{ZIP_NOTEBOOK_SUBMISSIONS_FOLDER_NAME}/";

        #endregion // Storage

        #region Overrides of AInternalZipEntryFile

        public override string DefaultZipEntryName => DEFAULT_INTERNAL_FILE_NAME;

        public override string GetZipEntryFullPath(Notebook parentNotebook)
        {
            return $"{ZIP_NOTEBOOKS_FOLDER_NAME}/{NotebookSetDirectoryName}/{NotebookOwnerDirectoryName}/{DefaultZipEntryName}.json";
        }

        #endregion
    }
}