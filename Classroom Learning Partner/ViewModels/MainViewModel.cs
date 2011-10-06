using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public const string clpText = "Classroom Learning Partner - ";

        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }

            set
            {
                if (_welcomeTitle == value)
                {
                    return;
                }

                _welcomeTitle = value;
                RaisePropertyChanged(WelcomeTitlePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Workspace" /> property's name.
        /// </summary>
        public const string WorkspacePropertyName = "Workspace";

        //private BaseWorkspaceViewModel _workspace = new BaseWorkspaceViewModel();

        ///// <summary>
        ///// Sets and gets the Workspace property.
        ///// Changes to that property's value raise the PropertyChanged event. 
        ///// This property's value is broadcasted by the MessengerInstance when it changes.
        ///// </summary>
        //public BaseWorkspaceViewModel Workspace
        //{
        //    get
        //    {
        //        return _workspace;
        //    }

        //    set
        //    {
        //        if (_workspace == value)
        //        {
        //            return;
        //        }

        //        var oldValue = _workspace;
        //        _workspace = value;
        //        RaisePropertyChanged(WorkspacePropertyName, oldValue, value, true);
        //    }
        //}

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {

        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}