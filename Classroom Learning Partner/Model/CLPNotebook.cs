using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Classroom_Learning_Partner.Model
{
    [Serializable]
    public class CLPNotebook
    {
        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        public CLPNotebook()
        {
            CLPPage page = new CLPPage();
            _pages.Add(page);

            _metaData.Add("CreationDate", new CLPAttribute("CreationDate", DateTime.Now.ToString()));
            _metaData.Add("UniqueID", new CLPAttribute("UniqueID", Guid.NewGuid().ToString()));
        }

        #endregion //Constructors

        #region Properties

        private Dictionary<string, CLPAttribute> _metaData = new Dictionary<string, CLPAttribute>();
        public Dictionary<string, CLPAttribute> MetaData
        {
            get
            {
                return _metaData;
            }
        }

        private ObservableCollection<CLPPage> _pages = new ObservableCollection<CLPPage>();
        public ObservableCollection<CLPPage> Pages
        {
            get
            {
                return _pages;
            }
        }

        // Dictionary<UniqueID of Page, List of associated submissions for Page>
        private Dictionary<string, ObservableCollection<CLPPage>> _submissions = new Dictionary<string, ObservableCollection<CLPPage>>();
        public Dictionary<string, ObservableCollection<CLPPage>> Submissions
        {
            get
            {
                return _submissions;
            }
        }

        #region MetaData

        public string UniqueID
        {
            get
            {
                return MetaData["UniqueID"].SelectedValue;
            }

        }

        public string Name
        {
            get
            {
                if (MetaData.ContainsKey("Name"))
	            {
		             return MetaData["Name"].SelectedValue;
	            }
                else
                {
                    MetaData.Add("Name",new CLPAttribute("Name", "NoName"));
                    return "NoName";
                }
                
            }
            set
            {
                if (MetaData.ContainsKey("Name"))
	            {
                    MetaData["Name"] = new CLPAttribute("Name", value);
	            }
                else
	            {
                    MetaData.Add("Name",new CLPAttribute("Name", value));
	            }
            }
        }

        #endregion //MetaData

        #endregion //Properties

        #region Submissions

        public void AddStudentSubmission(string pageID, CLPPage page)
        {
            if (_submissions.ContainsKey(pageID))
            {
                _submissions[pageID].Add(page);
            }
            else
            {
                ObservableCollection<CLPPage> pages = new ObservableCollection<CLPPage>();
                pages.Add(page);
                _submissions.Add(pageID, pages);
            }
        }

        #endregion

        #region Public Interface

        public static CLPNotebook LoadNotebookFromFile(string filePath)
        {
            CLPNotebook notebook = new CLPNotebook();

            if (File.Exists(filePath))
            {
                BinaryFormatter binFormat = new BinaryFormatter();

                using (var file = File.OpenRead(filePath))
                {
                    notebook = (CLPNotebook)binFormat.Deserialize(file);
                }
            }

            return notebook;
        }

        public static void SaveNotebookToFile(string filePath, CLPNotebook notebook)
        {
            BinaryFormatter binFormat = new BinaryFormatter();

            using (var file = File.Create(filePath))
            {
                binFormat.Serialize(file, notebook);
            }
        }

        

        #endregion //Public Interface
    }
}

