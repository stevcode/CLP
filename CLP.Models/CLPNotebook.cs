using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    /// <summary>
    /// CLPNotebook Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    public class CLPNotebook : SavableModelBase<CLPNotebook>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPNotebook()
        {
            CreationDate = DateTime.Now;
            UniqueID = Guid.NewGuid().ToString();
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

        #endregion //Constructors

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
        public ObservableCollection<ICLPPage> Pages
        {
            get { return GetValue<ObservableCollection<ICLPPage>>(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public static readonly PropertyData PagesProperty = RegisterProperty("Pages", typeof(ObservableCollection<ICLPPage>), () => new ObservableCollection<ICLPPage>());

        /// <summary>
        /// Gets a dictionary that maps the UniqueID of a page to a list of the submissions for that page.
        /// </summary>
        public Dictionary<string, ObservableCollection<ICLPPage>> Submissions
        {
            get { return GetValue<Dictionary<string, ObservableCollection<ICLPPage>>>(SubmissionsProperty); }
            set { SetValue(SubmissionsProperty, value); }
        }

        public static readonly PropertyData SubmissionsProperty = RegisterProperty("Submissions", typeof(Dictionary<string, ObservableCollection<ICLPPage>>), () => new Dictionary<string, ObservableCollection<ICLPPage>>());

        /// <summary>
        /// Name of notebook.
        /// </summary>
        public string NotebookName
        {
            get { return GetValue<string>(NotebookNameProperty); }
            set { SetValue(NotebookNameProperty, value); }
        }

        public static readonly PropertyData NotebookNameProperty = RegisterProperty("NotebookName", typeof(string));

        /// <summary>
        /// UniqueID assigned to the notebook.
        /// </summary>
        public string UniqueID
        {
            get { return GetValue<string>(UniqueIDProperty); }
            private set { SetValue(UniqueIDProperty, value); }
        }

        public static readonly PropertyData UniqueIDProperty = RegisterProperty("UniqueID", typeof(string), Guid.NewGuid().ToString());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

        #endregion

        #region Methods

        public void AddPage(CLPPage page)
        {
            page.ParentNotebookID = UniqueID;
            Pages.Add(page);
            GenerateSubmissionViews(page.UniqueID);
        }

        public void InsertPageAt(int index, CLPPage page)
        {
            Pages.Insert(index, page);
            GenerateSubmissionViews(page.UniqueID);
        }

        private void GenerateSubmissionViews(string pageUniqueID)
        {
            if(!Submissions.ContainsKey(pageUniqueID))
            {
                Submissions.Add(pageUniqueID, new ObservableCollection<ICLPPage>());
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
        }

        public ICLPPage GetPageAt(int pageIndex, int submissionIndex)
        {
            if(submissionIndex < -1) return null;
            if(submissionIndex == -1)
            {
                try
                { return Pages[pageIndex]; }
                catch(Exception)
                {
                    return null;
                }
            }

            try
            {
                return Submissions[Pages[pageIndex].UniqueID][submissionIndex];
            }
            catch(Exception)
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
                int groupCount = 0;
                foreach(var page in Submissions[pageID])
                {
                    if(submission.GroupName == page.GroupName)
                    {
                        groupCount++;
                        break;
                    }
                }

                int individualCount = 0;
                foreach(var page in Submissions[pageID])
                {
                    if(submission.SubmitterName == page.SubmitterName)
                    {
                        individualCount++;
                        break;
                    }
                }

                if(groupCount == 0 && submission.IsGroupSubmission)
                {
                    notebookPage.NumberOfGroupSubmissions++;
                }
                if(individualCount == 0 && !submission.IsGroupSubmission)
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
                if (notebookPage.IsGroupSubmission)
                {
                    notebookPage.NumberOfGroupSubmissions++;
                }
            }
        }

        #endregion
    }
}
