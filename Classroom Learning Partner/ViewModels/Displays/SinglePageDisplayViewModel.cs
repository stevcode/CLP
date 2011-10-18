using GalaSoft.MvvmLight;

namespace Classroom_Learning_Partner.ViewModels.Displays
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
    public class SinglePageDisplayViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the SinglePageDisplayViewModel class.
        /// </summary>
        public SinglePageDisplayViewModel()
        {
        }

        /// <summary>
        /// The <see cref="PageViewModel" /> property's name.
        /// </summary>
        public const string PageViewModelPropertyName = "PageViewModel";

        private CLPPageViewModel _pageViewModel = new CLPPageViewModel();

        /// <summary>
        /// Sets and gets the PageViewModel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public CLPPageViewModel PageViewModel
        {
            get
            {
                return _pageViewModel;
            }

            set
            {
                if (_pageViewModel == value)
                {
                    return;
                }

                _pageViewModel = value;
                RaisePropertyChanged(PageViewModelPropertyName);
            }
        }
    }
}