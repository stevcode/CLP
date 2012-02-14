using Catel.MVVM;
using System.Collections.ObjectModel;
using Classroom_Learning_Partner.Model;
using Catel.Data;

namespace Classroom_Learning_Partner.ViewModels.Displays
{
    [InterestedIn(typeof(SideBarViewModel))]
    public class GridDisplayViewModel : ViewModelBase, IDisplayViewModel
    {
        /// <summary>
        /// Initializes a new instance of the GridDisplayViewModel class.
        /// </summary>
        public GridDisplayViewModel()
            : base()
        {
            DisplayedPages = new ObservableCollection<CLPPageViewModel>();

            RemovePageFromGridDisplayCommand = new Command<CLPPageViewModel>(OnRemovePageFromGridDisplayCommandExecute);
        }

        public override string Title { get { return "GridDisplayVM"; } }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public ObservableCollection<CLPPageViewModel> DisplayedPages
        {
            get { return GetValue<ObservableCollection<CLPPageViewModel>>(DisplayedPagesProperty); }
            set { SetValue(DisplayedPagesProperty, value); }
        }

        /// <summary>
        /// Register the DisplayedPages property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DisplayedPagesProperty = RegisterProperty("DisplayedPages", typeof(ObservableCollection<CLPPageViewModel>));

        /// <summary>
        /// Gets the RemovePageFromGridDisplayCommand command.
        /// </summary>
        public Command<CLPPageViewModel> RemovePageFromGridDisplayCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the RemovePageFromGridDisplayCommand command is executed.
        /// </summary>
        private void OnRemovePageFromGridDisplayCommandExecute(CLPPageViewModel page)
        {
            DisplayedPages.Remove(page);

            //if (App.CurrentUserMode == App.UserMode.Instructor)
            //{
            //    if (App.Peer.Channel != null)
            //    {
            //        if (this.IsOnProjector)
            //        {
            //            string pageString = ObjectSerializer.ToString(pageViewModel.Page);
            //            App.Peer.Channel.RemovePageFromGridDisplay(pageString);
            //        }
            //    }
            //}
        }

        public string DisplayName
        {
            get { return "GridDisplay"; }
        }

        public bool IsActive { get; set; }
        public bool IsOnProjector { get; set; }


        public void AddPageToDisplay(CLPPageViewModel page)
        {
            DisplayedPages.Add(page);
        }

        protected override void OnViewModelPropertyChanged(IViewModel viewModel, string propertyName)
        {
            if (propertyName == "CurrentPage" && IsActive)
            {
                AddPageToDisplay((viewModel as SideBarViewModel).CurrentPage);
                //Steve - send to projector
                //App.Peer.Channel.AddPageToDisplay? if IsOnProjector
                // if (this.IsActive)
                //{
                //    if (App.CurrentUserMode == App.UserMode.Instructor)
                //    {
                //        if (App.Peer.Channel != null)
                //        {
                //            if (this.IsOnProjector)
                //            {
                //            //run this in background thread?
                //                string pageString = ObjectSerializer.ToString(pageViewModel.Page);
                //                App.Peer.Channel.AddPageToDisplay(pageString);
                //                
                //                //alternative?
                //                if (pageViewModel.Page.IsSubmission)
                //            {
                //                App.Peer.Channel.AddPageToDisplay(pageViewModel.Page.SubmissionID);
                //            }
                //            else
                //            {
                //                App.Peer.Channel.AddPageToDisplay(pageViewModel.Page.UniqueID);
                //            }
                //            }
                //        }
                //    }
                //}
            }

            base.OnViewModelPropertyChanged(viewModel, propertyName);
        }
    }
}