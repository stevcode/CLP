using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;
using Path = Catel.IO.Path;

namespace CLP.Entities
{
    public class Notebook : AEntityBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="Notebook" /> from scratch.
        /// </summary>
        public Notebook()
        {
            CreationDate = DateTime.Now;
            ID = Guid.NewGuid().ToString();
            SingleDisplay = new SingleDisplay(this);
            SingleDisplayID = SingleDisplay.ID;
        }

        /// <summary>
        /// Initializes <see cref="Notebook" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public Notebook(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Unique Identifier for the <see cref="Notebook" />.
        /// </summary>
        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }

        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string));

        /// <summary>
        /// Date and Time the <see cref="Notebook" /> was created.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        /// <summary>
        /// Date and Time the <see cref="Notebook" /> was last saved.
        /// </summary>
        /// <remarks>
        /// Type set to DateTime? (i.e. nullable DateTime) to allow NULL in database if LastSavedDate hasn't been set yet.
        /// </remarks>
        public DateTime? LastSavedDate
        {
            get { return GetValue<DateTime?>(LastSavedDateProperty); }
            set { SetValue(LastSavedDateProperty, value); }
        }

        public static readonly PropertyData LastSavedDateProperty = RegisterProperty("LastSavedDate", typeof(DateTime?));

        /// <summary>
        /// Name of the <see cref="Notebook" />.
        /// </summary>
        public string Name
        {
            get { return GetValue<string>(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly PropertyData NameProperty = RegisterProperty("Name", typeof(string), string.Empty);

        /// <summary>
        /// Overall Curriculum the <see cref="Notebook" /> employs. Curriculum of individual pages may vary.
        /// </summary>
        public string Curriculum
        {
            get { return GetValue<string>(CurriculumProperty); }
            set { SetValue(CurriculumProperty, value); }
        }

        public static readonly PropertyData CurriculumProperty = RegisterProperty("Curriculum", typeof(string), string.Empty);

        /// <summary>
        /// Collection of all the <see cref="CLPPage" />s in the <see cref="Notebook" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public virtual ObservableCollection<CLPPage> Pages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>
        /// Unique Identifier of the <see cref="Notebook" />'s <see cref="SingleDisplay" />.
        /// </summary>
        /// <remarks>
        /// Foreign Key.
        /// </remarks>
        public string SingleDisplayID
        {
            get { return GetValue<string>(SingleDisplayIDProperty); }
            set { SetValue(SingleDisplayIDProperty, value); }
        }

        public static readonly PropertyData SingleDisplayIDProperty = RegisterProperty("SingleDisplayID", typeof(string));

        /// <summary>
        /// The <see cref="SingleDisplay" /> of the <see cref="Notebook" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        public virtual SingleDisplay SingleDisplay
        {
            get { return GetValue<SingleDisplay>(SingleDisplayProperty); }
            set { SetValue(SingleDisplayProperty, value); }
        }

        public static readonly PropertyData SingleDisplayProperty = RegisterProperty("SingleDisplay", typeof(SingleDisplay));

        /// <summary>
        /// List of the <see cref="IDisplay" />s in the <see cref="Notebook" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        [XmlIgnore]
        [ExcludeFromSerialization]
        public virtual ObservableCollection<IDisplay> Displays
        {
            get { return GetValue<ObservableCollection<IDisplay>>(DisplaysProperty); }
            set { SetValue(DisplaysProperty, value); }
        }

        public static readonly PropertyData DisplaysProperty = RegisterProperty("Displays", typeof(ObservableCollection<IDisplay>), () => new ObservableCollection<IDisplay>());

        #endregion //Properties

        #region Methods

        public void AddCLPPageToNotebook(CLPPage page)
        {
            page.NotebookID = ID;
            page.PageNumber = Pages.Any() ? Pages.Last().PageNumber + 1 : 1;

            Pages.Add(page);
            SingleDisplay.AddPageToDisplay(page);
            //GenerateSubmissionViews(page.ID);
        }

        public void AddDisplayToNotebook(IDisplay display)
        {
            display.NotebookID = ID;
            display.DisplayNumber = Displays.Any() ? Displays.Last().DisplayNumber + 1 : 1;
            Displays.Add(display);
        }

        public void InsertPageAt(int index, CLPPage page)
        {
            page.NotebookID = ID;

            if(index != 0)
            {
                var previousPage = Pages.ElementAtOrDefault(index - 1);
                if(previousPage != null)
                {
                    page.PageNumber = previousPage.PageNumber + 1;
                }
            }
            // TODO: Else Load previous page from Cache/Database if exists

            Pages.Insert(index, page);
            SingleDisplay.AddPageToDisplay(page);
            //GenerateSubmissionViews(page.UniqueID);
            //GeneratePageIndexes();

            for(var i = index + 1; i < Pages.Count; i++)
            {
                Pages[i].PageNumber++;
            }
        }

        public void RemovePageAt(int index)
        {
            if(Pages.Count > index && index >= 0)
            {
                //Submissions.Remove(Pages[index].UniqueID);
                Pages.RemoveAt(index);
            }
            if(Pages.Count == 0)
            {
                AddCLPPageToNotebook(new CLPPage());
            }
            //GeneratePageIndexes();
        }

        #endregion //Methods

        #region Cache

        public void ToXML(string fileName)
        {
            LastSavedDate = DateTime.Now;
            var fileInfo = new FileInfo(fileName);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using (Stream stream = new FileStream(fileName, FileMode.Create))
            {
                var xmlSerializer = SerializationFactory.GetXmlSerializer();
                xmlSerializer.Serialize(this, stream);
                ClearIsDirtyOnAllChilds();
            }
        }

        public void SaveNotebook(string folderPath)
        {
            var fileName = Path.Combine(folderPath, "notebook.xml");
            ToXML(fileName);

            var pagesFolderPath = Path.Combine(folderPath, "Pages");
            if(!Directory.Exists(pagesFolderPath))
            {
                Directory.CreateDirectory(pagesFolderPath);
            }
            foreach(var page in Pages)
            {
                var pageFilePath = Path.Combine(pagesFolderPath, "Page " + page.PageNumber + " - " + page.ID + ".xml");
                page.ToXML(pageFilePath);
            }

            var displaysFolderPath = Path.Combine(folderPath, "Displays");
            if(!Directory.Exists(displaysFolderPath))
            {
                Directory.CreateDirectory(displaysFolderPath);
            }
        }

        #endregion //Cache
    }
}