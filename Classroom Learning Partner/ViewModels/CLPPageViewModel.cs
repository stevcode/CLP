using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;

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
    public class CLPPageViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the CLPPageViewModel class.
        /// </summary>
        public CLPPageViewModel()
        {
        }

        private CLPPage _page;
        public CLPPage Page
        {
            get
            {
                return _page;
            }
            set
            {
                _page = value;
            }
        }
    }
}