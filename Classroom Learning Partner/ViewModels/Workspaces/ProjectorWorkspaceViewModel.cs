using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.ViewModels.Displays;

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
    public class ProjectorWorkspaceViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the ProjectorWorkspaceViewModel class.
        /// </summary>
        public ProjectorWorkspaceViewModel()
        {
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