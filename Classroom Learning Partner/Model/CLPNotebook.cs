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

            MetaData.SetValue("CreationDate", DateTime.Now.ToString());
            MetaData.SetValue("UniqueID", Guid.NewGuid().ToString());
        }

        #endregion //Constructors

        #region Properties

        private MetaDataContainer _metaData = new MetaDataContainer();
        public MetaDataContainer MetaData
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
                return MetaData.GetValue("UniqueID");
            }

        }

        public string NotebookName
        {
            get
            {
                return MetaData.GetValue("NotebookName"); 
            }
            set
            {
                MetaData.SetValue("NotebookName", value);
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

        public void InsertPage(int index, CLPPage page)
        {
            Pages.Insert(index, page);

            GenerateSubmissionViews(page.UniqueID);
        }

        private void GenerateSubmissionViews(string pageUniqueID)
        {
            if (!Submissions.ContainsKey(pageUniqueID))
            {
                Submissions.Add(pageUniqueID, new ObservableCollection<CLPPage>());
            }
        }

        public void RemovePageAt(int index)
        {
            if (Pages.Count > index)
            {
                CLPPage page = Pages[index];
                Pages.Remove(page);
                Submissions.Remove(page.UniqueID);
            }
        }

        #endregion //Public Interface
        #region properties

        #endregion
    }
}

