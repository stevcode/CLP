using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;

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
            CLPService = new CLPServiceAgent();
            //claire
            AppMessages.GetInkCanvas.Register(this, (inkCanvas) =>
            {
                this.InkCanvas = inkCanvas;
                foreach (CLPPageViewModel page in PageViewModels)
                {
                    page.HistoryVM.InkCanvas = this.InkCanvas;
                }
            });
            //end claire
        }

        private ICLPServiceAgent CLPService { get; set; }

        public CLPNotebookViewModel(CLPNotebook notebook)
        {
            
            _currentPageViewModel = new CLPPageViewModel(this);
            //claire
            AppMessages.GetInkCanvas.Register(this, (inkCanvas) =>
            {
                this.InkCanvas = inkCanvas;
                foreach (CLPPageViewModel page in PageViewModels)
                {
                    page.HistoryVM.InkCanvas = this.InkCanvas;
                }
            });
            //end claire
            _notebook = notebook;
            foreach (CLPPage page in Notebook.Pages)
            {

                CLPPageViewModel pageVM = new CLPPageViewModel(page, this);
                
                PageViewModels.Add(pageVM);

                //create blank submissions key/value pairs
                ObservableCollection<CLPPageViewModel> pages = new ObservableCollection<CLPPageViewModel>();
                SubmissionViewModels.Add(page.UniqueID, pages);
            }
            foreach (string submissionUniqueID in Notebook.Submissions.Keys)
            {
                foreach (CLPPage submission in Notebook.Submissions[submissionUniqueID])
                {
                    CLPPageViewModel submissionVM = new CLPPageViewModel(submission, this);
                    SubmissionViewModels[submissionUniqueID].Add(submissionVM);
                }
            }

            
        }
        #endregion //Constructors

        #region Properties

        private CLPNotebook _notebook;
        public CLPNotebook Notebook
        {
            get { return _notebook; }
        }
        private System.Windows.Controls.InkCanvas _inkCanvas;
        public System.Windows.Controls.InkCanvas InkCanvas
        {
            get 
            {
                return _inkCanvas;
            }
            set
            {
                _inkCanvas = value;
            }
        }
        #endregion //Properties

        #region Bindings

        private readonly ObservableCollection<CLPPageViewModel> _pageViewModels = new ObservableCollection<CLPPageViewModel>();
        public ObservableCollection<CLPPageViewModel> PageViewModels
        {
            get { return _pageViewModels; }
        }

        // <key, value> = <NotebookPageUniqueID, List of Submissions>
        private Dictionary<string, ObservableCollection<CLPPageViewModel>> _submissionViewModels = new Dictionary<string, ObservableCollection<CLPPageViewModel>>();
        public Dictionary<string, ObservableCollection<CLPPageViewModel>> SubmissionViewModels
        {
            get
            {
                return _submissionViewModels;
            }
        }

        /// <summary>
        /// The <see cref="CurrentPageViewModel" /> property's name.
        /// </summary>
        public const string CurrentPageViewModelPropertyName = "CurrentPageViewModel";

        private CLPPageViewModel _currentPageViewModel;

        /// <summary>
        /// Sets and gets the CurrentPageViewModel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public CLPPageViewModel CurrentPageViewModel
        {
            get
            {
                return _currentPageViewModel;
            }

            set
            {
                if (_currentPageViewModel == value)
                {
                    return;
                }

                _currentPageViewModel = value;
                RaisePropertyChanged(CurrentPageViewModelPropertyName);
            }
        }

        #endregion //Bindings

        #region Methods

        public void InsertPage(int index, CLPPageViewModel pageViewModel)
        {
            PageViewModels.Insert(index, pageViewModel);

            GenerateSubmissionViews(pageViewModel.Page.UniqueID);

            pageViewModel.HistoryVM.InkCanvas = this.InkCanvas;
        }

        private void GenerateSubmissionViews(string pageUniqueID)
        {
            if (!SubmissionViewModels.ContainsKey(pageUniqueID))
            {
                SubmissionViewModels.Add(pageUniqueID, new ObservableCollection<CLPPageViewModel>());
            }
        }

        public void RemovePageAt(int index)
        {
            if (PageViewModels.Count > index)
            {
                CLPPageViewModel viewModel = PageViewModels[index];
                PageViewModels.Remove(viewModel);
                SubmissionViewModels.Remove(viewModel.Page.UniqueID);
                if (PageViewModels.Count == 0)
                {
                    CLPService.AddPageAt(new CLPPage(), 0, -1);
                }
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

        public CLPPageViewModel GetPageByID(string pageUniqueID)
        {
            foreach (var pageViewModel in PageViewModels)
            {
                if (pageViewModel.Page.UniqueID == pageUniqueID)
                {
                    return pageViewModel;
                }
            }

            return null;
        }

        public int GetNotebookPageIndex(CLPPageViewModel pageViewModel)
        {
            if (pageViewModel.Page.IsSubmission)
            {
                return -1;
            }
            else
            {
                return PageViewModels.IndexOf(pageViewModel);
            }
        }

        public int GetSubmissionIndex(CLPPageViewModel pageViewModel)
        {
            if (pageViewModel.Page.IsSubmission)
            {
                int submissionIndex = -1;
                foreach (string uniqueID in SubmissionViewModels.Keys)
                {
                    foreach (CLPPageViewModel submissionViewModel in SubmissionViewModels[uniqueID])
                    {
                        if (submissionViewModel.Page.UniqueID == pageViewModel.Page.UniqueID)
                        {
                            submissionIndex = SubmissionViewModels[uniqueID].IndexOf(submissionViewModel);
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

            GetPageByID(pageID).NumberOfSubmissions++;

            Notebook.AddStudentSubmission(pageID, submission.Page);
        }

        #endregion //Methods
    }
}