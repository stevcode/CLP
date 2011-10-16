﻿using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class CLPNotebookViewModel : ViewModelBase
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CLPNotebookViewModel class.
        /// </summary>
        public CLPNotebookViewModel() : this(new CLPNotebook())
        {
        }

        public CLPNotebookViewModel(CLPNotebook notebook)
        {
        }

        #endregion //Constructors

        #region Properties

        private CLPNotebook _notebook;
        public CLPNotebook Notebook
        {
            get { return _notebook; }
        }

        #endregion //Properties

        #region Bindings

        private readonly ObservableCollection<CLPPageViewModel> _pageViewModels = new ObservableCollection<CLPPageViewModel>();
        public ObservableCollection<CLPPageViewModel> PageViewModels
        {
            get { return _pageViewModels; }
        }

        private Dictionary<string, ObservableCollection<CLPPageViewModel>> _submissionViewModels = new Dictionary<string, ObservableCollection<CLPPageViewModel>>();
        public Dictionary<string, ObservableCollection<CLPPageViewModel>> SubmissionViewModels
        {
            get
            {
                return _submissionViewModels;
            }
        }

        #endregion //Bindings

        #region Methods

        public void InsertPage(int index, CLPPageViewModel pageViewModel)
        {
            PageViewModels.Insert(index, pageViewModel);
            Notebook.Pages.Insert(index, pageViewModel.Page);

            Notebook.Submissions.Add(pageViewModel.Page.UniqueID, new ObservableCollection<CLPPage>());


            GenerateSubmissionViews(pageViewModel.Page);
        }

        public void RemovePageAt(int index)
        {
            if (PageViewModels.Count > index)
            {
                CLPPageViewModel viewModel = PageViewModels[index];
                PageViewModels.Remove(viewModel);

                _submissionViewModels.Remove(viewModel.Page.UniqueID);

                CLPPage page = viewModel.Page;
                Notebook.Pages.Remove(page);
                Notebook.Submissions.Remove(page.UniqueID);

            }
        }

        public CLPPageViewModel GetPage(int pageIndex, int submissionIndex)
        {
            if (submissionIndex < -1) return null;
            if (submissionIndex == -1)
            {
                try
                { return PageViewModels[pageIndex]; }
                catch (Exception e)
                {
                    return null;
                }
            }

            try
            {
                return SubmissionViewModels[PageViewModels[pageIndex].Page.UniqueID][submissionIndex];
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public int GetPageIndex(CLPPageViewModel pageView)
        {
            //re-write to account for category of submission
            //if (pageView is SubmissionViewModel)
            //    return PageViewModels.IndexOf((pageView as SubmissionViewModel).OriginalPageViewModel);
            //else
            //    return PageViewModels.IndexOf(pageView);
            return 0;
        }

        public int GetSubmissionIndex(CLPPageViewModel pageView)
        {
            //re-write to account for category of submission
            //if (pageView is SubmissionViewModel)
            //{
            //    int submissionIndex = -2;
            //    SubmissionViewModel submissionView = pageView as SubmissionViewModel;
            //    CLPPageViewModel originalPageView = submissionView.OriginalPageViewModel;
            //    if (SubmissionViews.ContainsKey(originalPageView))
            //        submissionIndex = SubmissionViews[originalPageView].IndexOf(submissionView);
            //    if (submissionIndex < 0) submissionIndex = -2;
            //    return submissionIndex;
            //}
            //else
            //{
            //    return -1;
            //}
            return 0;
        }

        public void AddStudentSubmission(string pageID, CLPPageViewModel submission)
        {

            if (SubmissionViewModels.ContainsKey(pageID))
            {
                SubmissionViewModels[pageID].Add(submission);
            }
            else
            {
                ObservableCollection<CLPPageViewModel> pages = new ObservableCollection<CLPPageViewModel>();
                pages.Add(submission);
                SubmissionViewModels.Add(pageID, pages);
            }

            Notebook.AddStudentSubmission(pageID, submission.Page);
        }

        #endregion //Methods
    }
}