using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

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
            OpenNotebookNames.Add(App.CurrentNotebookViewModel.Notebook.Name);
            SelectedPage = PageViewModels[0];
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
            }
        }

        /// <summary>
        /// The <see cref="SelectedPage" /> property's name.
        /// </summary>
        public const string SelectedPagePropertyName = "SelectedPage";

        private CLPPageViewModel _selectedPage;

        /// <summary>
        /// Sets and gets the SelectedPage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the MessengerInstance when it changes.
        /// </summary>
        public CLPPageViewModel SelectedPage
        {
            get
            {
                return _selectedPage;
            }

            set
            {
                if (_selectedPage == value)
                {
                    return;
                }

                _selectedPage = value;

                if (_selectedPage == null)
                {
                    SelectedPage = PageViewModels[0];
                }
                else
                {
                    RaisePropertyChanged(SelectedPagePropertyName);
                    AppMessages.AddPageToDisplay.Send(_selectedPage);
                }
                if (App.CurrentNotebookViewModel.SubmissionViewModels.ContainsKey(_selectedPage.Page.UniqueID))
                {
                    SubmissionViewModels = App.CurrentNotebookViewModel.SubmissionViewModels[_selectedPage.Page.UniqueID];
                }
                
            }
        }

        #endregion //Bindings
    }
}