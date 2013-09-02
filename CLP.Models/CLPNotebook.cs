using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            MirrorDisplay = new CLPMirrorDisplay {ParentNotebookID = UniqueID};
            MirrorDisplay.AddPageToDisplay(Pages.FirstOrDefault());
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

        /// <summary>
        /// DateTime the notebook was created.
        /// </summary>
        public DateTime CreationDate
        {
            get { return GetValue<DateTime>(CreationDateProperty); }
            private set { SetValue(CreationDateProperty, value); }
        }

        public static readonly PropertyData CreationDateProperty = RegisterProperty("CreationDate", typeof(DateTime));

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
        /// Name of notebook.
        /// </summary>
        public string NotebookName
        {
            get { return GetValue<string>(NotebookNameProperty); }
            set { SetValue(NotebookNameProperty, value); }
        }

        public static readonly PropertyData NotebookNameProperty = RegisterProperty("NotebookName", typeof(string));

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
        /// The MirrorDisplay of the notebook.
        /// </summary>
        public CLPMirrorDisplay MirrorDisplay
        {
            get { return GetValue<CLPMirrorDisplay>(MirrorDisplayProperty); }
            private set { SetValue(MirrorDisplayProperty, value); }
        }

        public static readonly PropertyData MirrorDisplayProperty = RegisterProperty("MirrorDisplay", typeof(CLPMirrorDisplay));

        /// <summary>
        /// A list of all other displays in the notebook.
        /// </summary>
        public ObservableCollection<ICLPDisplay> Displays
        {
            get { return GetValue<ObservableCollection<ICLPDisplay>>(DisplaysProperty); }
            set { SetValue(DisplaysProperty, value); }
        }

        public static readonly PropertyData DisplaysProperty = RegisterProperty("Displays", typeof(ObservableCollection<ICLPDisplay>), () => new ObservableCollection<ICLPDisplay>());

        #endregion

        #region Methods

        public void AddDisplay(ICLPDisplay display)
        {
            display.ParentNotebookID = UniqueID;
            Displays.Add(display);
        }

        public void AddPage(ICLPPage page)
        {
            page.ParentNotebookID = UniqueID;
            Pages.Add(page);
            GenerateSubmissionViews(page.UniqueID);
        }

        public void InsertPageAt(int index, ICLPPage page)
        {
            page.ParentNotebookID = UniqueID;
            Pages.Insert(index, page);
            GenerateSubmissionViews(page.UniqueID);
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
            if(submissionIndex < -1)
                return null;
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

        private void GenerateSubmissionViews(string pageUniqueID)
        {
            if(!Submissions.ContainsKey(pageUniqueID))
            {
                Submissions.Add(pageUniqueID, new ObservableCollection<ICLPPage>());
            }
        }

        public ICLPPage GetNotebookPageByID(string pageUniqueID)
        {
            return Pages.FirstOrDefault(page => page.UniqueID == pageUniqueID);
        }

        public int GetNotebookPageIndex(ICLPPage page)
        {
            return page.SubmissionType == SubmissionType.None ? Pages.IndexOf(page) : -1;
        }

        public ICLPPage GetSubmissionByID(string pageID)
        {
            ICLPPage returnPage = null;
            foreach(var pageKey in Submissions.Keys)
            {
                foreach(var page in Submissions[pageKey].Where(page => page.SubmissionID == pageID)) 
                {
                    returnPage = page;
                    break;
                }
                if(returnPage != null)
                {
                    break;
                }
            }

            return returnPage;
        }

        public int GetSubmissionIndex(ICLPPage page)
        {
            if(page.SubmissionType != SubmissionType.None)
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

            return -1;
        }

        public void AddStudentSubmission(string pageID, ICLPPage submission)
        {
            var notebookPage = GetNotebookPageByID(pageID);
            if(Submissions.ContainsKey(pageID))
            {
                int groupCount = 0;
                foreach(var page in Submissions[pageID])
                {
                    if(submission.GroupSubmitter.GroupName == page.GroupSubmitter.GroupName)
                    {
                        groupCount++;
                        break;
                    }
                }

                int individualCount = 0;
                foreach(var page in Submissions[pageID])
                {
                    if(submission.Submitter.FullName == page.Submitter.FullName)
                    {
                        individualCount++;
                        break;
                    }
                }

                if(groupCount == 0 && submission.SubmissionType == SubmissionType.Group)
                {
                    notebookPage.NumberOfGroupSubmissions++;
                }
                if(individualCount == 0 && submission.SubmissionType == SubmissionType.Single)
                {
                    notebookPage.NumberOfSubmissions++;
                }
                Submissions[pageID].Add(submission);
            }
            else
            {
                var pages = new ObservableCollection<ICLPPage>();
                pages.Add(submission);
                Submissions.Add(pageID, pages);
                notebookPage.NumberOfSubmissions++;
                if(notebookPage.SubmissionType == SubmissionType.Group)
                {
                    notebookPage.NumberOfGroupSubmissions++;
                }
            }
        }

        #endregion
    }
}
