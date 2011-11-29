using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Windows;

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
    public class SideBarViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the NotebookSideBarViewModel class.
        /// </summary>
        public SideBarViewModel()
        {
            
            PageViewModels = App.CurrentNotebookViewModel.PageViewModels;
            //SubmissionViewModels = App.CurrentNotebookViewModel.SubmissionViewModels[
            OpenNotebookNames.Add(App.CurrentNotebookViewModel.Notebook.NotebookName);
            SelectedNotebookPage = PageViewModels[0];
        }

        #region Bindings

        private ObservableCollection<string> _openNotebookNames = new ObservableCollection<string>();
        public ObservableCollection<string> OpenNotebookNames
        {
            get
            {
                return _openNotebookNames;
            }
        }

        /// <summary>
        /// The <see cref="PageViewModels" /> property's name.
        /// </summary>
        public const string PageViewModelsPropertyName = "PageViewModels";

        private ObservableCollection<CLPPageViewModel> _pageViewModels = new ObservableCollection<CLPPageViewModel>();

        /// <summary>
        /// Sets and gets the PageViewModels property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<CLPPageViewModel> PageViewModels
        {
            get
            {
                return _pageViewModels;
            }

            set
            {
                if (_pageViewModels == value)
                {
                    return;
                }

                _pageViewModels = value;
                RaisePropertyChanged(PageViewModelsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SubmissionViewModels" /> property's name.
        /// </summary>
        public const string SubmissionViewModelsPropertyName = "SubmissionViewModels";

        private ObservableCollection<CLPPageViewModel> _submissionViewModels = new ObservableCollection<CLPPageViewModel>();

        /// <summary>
        /// Sets and gets the SubmissionViewModels property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<CLPPageViewModel> SubmissionViewModels
        {
            get
            {
                return _submissionViewModels;
            }

            set
            {
                if (_submissionViewModels == value)
                {
                    return;
                }

                _submissionViewModels = value;
                RaisePropertyChanged(SubmissionViewModelsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedSubmissionPage" /> property's name.
        /// </summary>
        public const string SelectedSubmissionPagePropertyName = "SelectedSubmissionPage";

        private CLPPageViewModel _selectedSubmissionPage;

        /// <summary>
        /// Sets and gets the SelectedSubmissionPage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public CLPPageViewModel SelectedSubmissionPage
        {
            get
            {
                return _selectedSubmissionPage;
            }

            set
            {
                if (_selectedSubmissionPage == value)
                {
                    return;
                }

                _selectedSubmissionPage = value;
                RaisePropertyChanged(SelectedSubmissionPagePropertyName);
                AppMessages.AddPageToDisplay.Send(_selectedSubmissionPage);
            }
        }

        /// <summary>
        /// The <see cref="SelectedNotebookPage" /> property's name.
        /// </summary>
        public const string SelectedNotebookPagePropertyName = "SelectedNotebookPage";

        private CLPPageViewModel _selectedNotebookPage;

        /// <summary>
        /// Sets and gets the SelectedPage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the MessengerInstance when it changes.
        /// </summary>
        public CLPPageViewModel SelectedNotebookPage
        {
            get
            {
                return _selectedNotebookPage;
            }

            set
            {
                if (_selectedNotebookPage == value)
                {
                    return;
                }

                

                if (value == null)
                {
                    //breakpoint to see if this is ever reached/needed
                    _selectedNotebookPage = PageViewModels[0];
                }
                else
                {
                    _selectedNotebookPage = value;
                    RaisePropertyChanged(SelectedNotebookPagePropertyName);
                    AppMessages.AddPageToDisplay.Send(_selectedNotebookPage); 
                }

                               
            }
        }

        /// <summary>
        /// The <see cref="SubmissionsSideBarVisibility" /> property's name.
        /// </summary>
        public const string SubmissionsSideBarVisibilityPropertyName = "SubmissionsSideBarVisibility";

        private Visibility _submissionsSideBarVisibility = Visibility.Visible;

        /// <summary>
        /// Sets and gets the SubmissionsSideBarVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility SubmissionsSideBarVisibility
        {
            get
            {
                return _submissionsSideBarVisibility;
            }

            set
            {
                if (_submissionsSideBarVisibility == value)
                {
                    return;
                }

                _submissionsSideBarVisibility = value;
                RaisePropertyChanged(SubmissionsSideBarVisibilityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ToggleSubmissionsButtonVisibility" /> property's name.
        /// </summary>
        public const string ToggleSubmissionsButtonVisibilityPropertyName = "ToggleSubmissionsButtonVisibility";

        private Visibility _toggleSubmissionsButtonVisibility = Visibility.Visible;

        /// <summary>
        /// Sets and gets the ToggleSubmissionsButtonVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility ToggleSubmissionsButtonVisibility
        {
            get
            {
                return _toggleSubmissionsButtonVisibility;
            }

            set
            {
                if (_toggleSubmissionsButtonVisibility == value)
                {
                    return;
                }

                _toggleSubmissionsButtonVisibility = value;
                RaisePropertyChanged(ToggleSubmissionsButtonVisibilityPropertyName);
            }
        }

        #endregion //Bindings
    }
}