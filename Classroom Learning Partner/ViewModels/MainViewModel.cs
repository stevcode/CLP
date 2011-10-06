using GalaSoft.MvvmLight;
using Classroom_Learning_Partner.Model;
using Classroom_Learning_Partner.ViewModels.Workspaces;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;

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
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {



        }

        #region Properties



        #endregion //Properties

        #region Bindings

        /// <summary>
        /// The <see cref="Workspace" /> property's name.
        /// </summary>
        public const string WorkspacePropertyName = "Workspace";

        private ViewModelBase _workspace = new BlankWorkspaceViewModel();

        /// <summary>
        /// Sets and gets the Workspace property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ViewModelBase Workspace
        {
            get
            {
                return _workspace;
            }

            set
            {
                if (_workspace == value)
                {
                    return;
                }

                _workspace = value;
                RaisePropertyChanged(WorkspacePropertyName);
            }
        }


        private RibbonViewModel _ribbon = new RibbonViewModel();
        public RibbonViewModel Ribbon
        {
            get
            {
                return _ribbon;
            }
        }

        #endregion //Bindings

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}