using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.ViewModels.Displays;
using System.Windows;

namespace Classroom_Learning_Partner.ViewModels.Workspaces
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
    public class AuthoringWorkspaceViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the AuthoringWorkspaceViewModel class.
        /// </summary>
        public AuthoringWorkspaceViewModel()
        {
            SideBar.SubmissionsSideBarVisibility = Visibility.Collapsed;
            SideBar.ToggleSubmissionsButtonVisibility = Visibility.Collapsed;
        }

        //TODO make left/right/top/bottom orientations...switch to single Workspace view with canvas?

        private SideBarViewModel _sideBar = new SideBarViewModel();
        public SideBarViewModel SideBar
        {
            get
            {
                return _sideBar;
            }
        }

        private SinglePageDisplayViewModel _display = new SinglePageDisplayViewModel();
        public SinglePageDisplayViewModel Display
        {
            get
            {
                return _display;
            }
        }
    }
}