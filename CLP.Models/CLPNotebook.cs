using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    /// <summary>
    /// CLPNotebook Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    public class CLPNotebook : SavableDataObjectBase<CLPNotebook>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPNotebook()
        {
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
            Pages = new ObservableCollection<CLPPage>();
            Submissions = new Dictionary<string, ObservableCollection<CLPPage>>();
            AddPage(new CLPPage());
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPNotebook(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Properties

        public String UserName
        {
            get { return GetValue<string>(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }
        public static readonly PropertyData UserNameProperty = RegisterProperty("UserName", typeof(String), "NoName");

        /// <summary>
        /// Gets the list of CLPPages in the notebook.
        /// </summary>
        public ObservableCollection<CLPPage> Pages
        {
            get { return GetValue<ObservableCollection<CLPPage>>(PagesProperty); }
            private set { SetValue(PagesProperty, value); }
        }

        /// <summary>
        /// Register the Pages property so it is known in the class.
        /// </summary>
        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<CLPPage>), () => new ObservableCollection<CLPPage>());

        /// <summary>
        /// Gets a dictionary that maps the UniqueID of a page to a list of the submissions for that page.
        /// </summary>
        public Dictionary<string, ObservableCollection<CLPPage>> Submissions
        {
            get { return GetValue<Dictionary<string, ObservableCollection<CLPPage>>>(SubmissionsProperty); }
            private set { SetValue(SubmissionsProperty, value); }
        }

        /// <summary>
        /// Register the Submissions property so it is known in the class.
        /// </summary>
        public static readonly PropertyData SubmissionsProperty = RegisterProperty("Submissions", typeof(Dictionary<string, ObservableCollection<CLPPage>>), () => new Dictionary<string, ObservableCollection<CLPPage>>());

        /// <summary>
        /// Name of notebook.
        /// </summary>
        public string NotebookName
        {
            get { return GetValue<string>(NotebookNameProperty); }
            set { SetValue(NotebookNameProperty, value); }
        }

        /// <summary>
        /// Register the NotebookName property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NotebookNameProperty = RegisterProperty("NotebookName", typeof(string), null);

        /// <summary>
        /// UniqueID assigned to the notebook.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            private set { SetValue(UniqueIDProperty, value); }
        }

        /// <summary>
        /// Register the UniqueID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        /// <summary>
        /// Register the CreationDate property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime), null);

        #endregion

        #region Methods

        public void GeneratePageIndexes()
        {
            int pageIndex = 1;
            foreach(var page in Pages)
            {
                page.PageIndex = pageIndex;
                pageIndex++;
            }
        }

        public void AddPage(CLPPage page)
        {
            page.ParentNotebookID = UniqueID;
            Pages.Add(page);
            GenerateSubmissionViews(page.UniqueID);
            GeneratePageIndexes();
        }

        public void InsertPageAt(int index, CLPPage page)
        {
            Pages.Insert(index, page);

            GenerateSubmissionViews(page.UniqueID);
            GeneratePageIndexes();
        }

        private void GenerateSubmissionViews(string pageUniqueID)
        {
            if(!Submissions.ContainsKey(pageUniqueID))
            {
                Submissions.Add(pageUniqueID, new ObservableCollection<CLPPage>());
            }
        }

        public void RemovePageAt(int index)
        {
            if(Pages.Count > index && index >= 0)
            {
                Submissions.Remove(Pages[index].UniqueID);
                Pages.RemoveAt(index);
            }
            if(Pages.Count == 0)
            {
                AddPage(new CLPPage());
            }
            GeneratePageIndexes();
        }

        public CLPPage GetPageAt(int pageIndex, int submissionIndex)
        {
            if(submissionIndex < -1) return null;
            if(submissionIndex == -1)
            {
                try
                { return Pages[pageIndex]; }
                catch(Exception e)
                {
                    return null;
                }
            }

            try
            {
                return Submissions[Pages[pageIndex].UniqueID][submissionIndex];
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public CLPPage GetNotebookPageByID(string pageUniqueID)
        {
            foreach(var page in Pages)
            {
                if(page.UniqueID == pageUniqueID)
                {
                    return page;
                }
            }

            return null;
        }

        public int GetNotebookPageIndex(CLPPage page)
        {
            if(page.IsSubmission)
            {
                return -1;
            }
            else
            {
                return Pages.IndexOf(page);
            }
        }

        public CLPPage GetSubmissionByID(string pageID)
        {
            CLPPage returnPage = null;
            foreach(var pageKey in Submissions.Keys)
            {
                foreach(var page in Submissions[pageKey])
                {
                    if(page.SubmissionID == pageID)
                    {
                        returnPage = page;
                        break;
                    }
                }
                if(returnPage != null)
                {
                    break;
                }
            }

            return returnPage;
        }

        public int GetSubmissionIndex(CLPPage page)
        {
            if(page.IsSubmission)
            {
                int submissionIndex = -1;
                foreach(string uniqueID in Submissions.Keys)
                {
                    foreach(CLPPage submission in Submissions[uniqueID])
                    {
                        if(submission.SubmissionID == page.SubmissionID)
                        {
                            submissionIndex = Submissions[uniqueID].IndexOf(submission);
                            break;
                        }
                    }
                }

                return submissionIndex;
            }
            else
            {
                return -1;
            }
        }

        public void AddStudentSubmission(string pageID, CLPPage submission)
        {
            CLPPage notebookPage = GetNotebookPageByID(pageID);
            if(Submissions.ContainsKey(pageID))
            {
                int count = 0;
                foreach(var page in Submissions[pageID])
                {
                    if(submission.SubmitterName == page.SubmitterName)
                    {
                        count++;
                        break;
                    }
                }
                if(count == 0)
                {
                    notebookPage.NumberOfSubmissions++;
                }
                Submissions[pageID].Add(submission);
            }
            else
            {
                ObservableCollection<CLPPage> pages = new ObservableCollection<CLPPage>();
                pages.Add(submission);
                Submissions.Add(pageID, pages);
                notebookPage.NumberOfSubmissions++;
            }
        }

        #endregion
    }
}
