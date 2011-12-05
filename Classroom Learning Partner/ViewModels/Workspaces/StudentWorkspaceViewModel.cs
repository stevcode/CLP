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
    public class StudentWorkspaceViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the StudentWorkspaceViewModel class.
        /// </summary>
        public StudentWorkspaceViewModel()
        {
            SideBar.SubmissionsSideBarVisibility = Visibility.Collapsed;
            SideBar.ToggleSubmissionsButtonVisibility = Visibility.Collapsed;
        }

        private SideBarViewModel _sideBar = new SideBarViewModel();
        public SideBarViewModel SideBar
        {
            get
            {
                return _sideBar;
            }
        }

        private LinkedDisplayViewModel _display = new LinkedDisplayViewModel();
        public LinkedDisplayViewModel Display
        {
            get
            {
                return _display;
            }
        }
    }
}