using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Classroom_Learning_Partner.Model
{
    [Serializable]
    [DataContract]
    class CLPNotebook
    {
        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        public CLPNotebook()
        {
            CLPPage page = new CLPPage();
            _pages.Add(page);

            var creationDate = new List<string>();
            creationDate.Add(DateTime.Now.ToString());
            _metaData.Add("CreationDate", creationDate);
        }

        #endregion //Constructors

        #region Properties

        private Dictionary<string, List<string>> _metaData = new Dictionary<string, List<string>>();
        [DataMember]
        public Dictionary<string, List<string>> MetaData
        {
            get
            {
                return _metaData;
            }
        }

        private ObservableCollection<CLPPage> _pages = new ObservableCollection<CLPPage>();
        [DataMember]
        public ObservableCollection<CLPPage> Pages
        {
            get
            {
                return _pages;
            }
        }

        // Dictionary<UniqueID of Page, List of associated submissions for Page>
        private Dictionary<string, ObservableCollection<CLPPage>> _submissions = new Dictionary<string, ObservableCollection<CLPPage>>();
        [DataMember]
        public Dictionary<string, ObservableCollection<CLPPage>> Submissions
        {
            get
            {
                return _submissions;
            }
        }

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
            BinaryFormatter binFormat = new BinaryFormatter();
            CLPNotebook notebook = new CLPNotebook();

            if (File.Exists(filePath))
            {
                using (FileStream fStream = new FileStream(filePath, FileMode.Open))
                {
                    notebook = (CLPNotebook)binFormat.Deserialize(fStream);
                }
            }

            return notebook;
        }

        #endregion //Public Interface
    }
}

