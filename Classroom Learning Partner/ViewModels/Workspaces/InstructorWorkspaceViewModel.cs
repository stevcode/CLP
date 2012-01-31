using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.ViewModels.Displays;
using System;

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
    public class InstructorWorkspaceViewModel : ViewModelBase, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the InstructorWorkspaceViewModel class.
        /// </summary>
        public InstructorWorkspaceViewModel()
        {
            Display = LinkedDisplay;
            LinkedDisplay.IsActive = true;
            LinkedDisplay.IsOnProjector = true;
            GridDisplay.IsActive = false;
            GridDisplay.IsOnProjector = false;
        }

        private SideBarViewModel _sideBar = new SideBarViewModel();
        public SideBarViewModel SideBar
        {
            get
            {
                return _sideBar;
            }
        }

        private LinkedDisplayViewModel _linkedDisplay = new LinkedDisplayViewModel();
        public LinkedDisplayViewModel LinkedDisplay
        {
            get
            {
                return _linkedDisplay;
            }
        }

        private GridDisplayViewModel _gridDisplay = new GridDisplayViewModel();
        public GridDisplayViewModel GridDisplay
        {
            get
            {
                return _gridDisplay;
            }
        }

        /// <summary>
        /// The <see cref="Display" /> property's name.
        /// </summary>
        public const string DisplayPropertyName = "Display";

        private ViewModelBase _display = null;

        /// <summary>
        /// Sets and gets the Display property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ViewModelBase Display
        {
            get
            {
                return _display;
            }

            set
            {
                if (_display == value)
                {
                    return;
                }

                _display = value;
                RaisePropertyChanged(DisplayPropertyName);
            }
        }

        public void Dispose()
        {
            //LinkedDisplay.Dispose();
        }
    }
}